using System.Text;
using System.Text.Json;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class PdfExportService : IPdfExportService
{
    public Task<string> ExportAsync(Meeting meeting)
    {
        return Task.Run(() =>
        {
            var minutes = Try<MeetingMinutes>(meeting.MinutesJson) ?? new MeetingMinutes();
            var transcript = Try<List<TranscriptSegment>>(meeting.TranscriptJson) ?? [];

            var name = new string(
                (meeting.Title + "_" + DateTime.Now.ToString("yyyyMMdd"))
                .Select(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_').ToArray());
            var path = Path.Combine(FileSystem.AppDataDirectory, name + ".pdf");

            var doc = new Pdf(595f, 842f, 40f);
            doc.Render(meeting, minutes, transcript);
            doc.Save(path);
            return path;
        });
    }

    static T? Try<T>(string? json)
        => string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
}

// Pure-C# PDF 1.4 writer — no native dependencies, works on Android ARM64 and all platforms.
// Uses standard Type1 fonts (Helvetica / Helvetica-Bold) built into every PDF reader.
file sealed class Pdf
{
    // Colours
    const float BR = 0f,    BG = 0.373f, BB = 0.667f;  // primary blue
    const float LR = 0.91f, LG = 0.95f,  LB = 0.98f;  // light blue (summary bg)
    const float GR = 0.25f, GG = 0.28f,  GB = 0.32f;  // body text
    const float DR = 0.40f, DG = 0.40f,  DB = 0.40f;  // dim / meta text
    const float SR = 0.87f, SG = 0.87f,  SB = 0.87f;  // separator / rule

    readonly float _w, _h, _m, _cw;
    readonly List<StringBuilder> _pages = new();
    StringBuilder _cur;
    float _y;

    public Pdf(float w, float h, float margin)
    {
        _w = w; _h = h; _m = margin; _cw = w - margin * 2;
        _cur = new StringBuilder();
        _pages.Add(_cur);
        _y = margin;
    }

    // ─────────────────────────── Content ────────────────────────────────────

    public void Render(Meeting meeting, MeetingMinutes min, List<TranscriptSegment> tscript)
    {
        var date = meeting.Date != default ? meeting.Date : meeting.CreatedAt;

        // Blue header band
        Rect(0, 0, _w, 82, BR, BG, BB);
        _y = 17;
        Text(meeting.Title, 22, bold: true, 1f, 1f, 1f);
        _y = 52;
        Text($"{date:MMMM dd, yyyy}  |  {meeting.Duration:mm\\:ss} duration",
             11, r: 0.78f, g: 0.88f, b: 1f);
        _y = 100;

        // Summary box
        if (!string.IsNullOrWhiteSpace(min.Summary))
        {
            var lines = Wrap(Safe(min.Summary), 11f, _cw - 24).ToList();
            float bh = lines.Count * Lh(11f) + 30;
            if (_y + bh <= _h - 48)
                Rect(_m, _y, _cw, bh, LR, LG, LB);
            _y += 10;
            RawLine("SUMMARY", 9, bold: true, BR, BG, BB, _m + 12);
            _y += Lh(9f) + 3;
            foreach (var ln in lines)
            {
                NeedRoom(Lh(11f));
                RawLine(ln, 11, false, GR, GG, GB, _m + 12);
                _y += Lh(11f);
            }
            _y += 16;
        }

        if (min.Attendees.Count > 0)
        {
            Section("Attendees");
            Body(string.Join(", ", min.Attendees), 11);
            _y += 18;
        }

        if (min.DiscussionPoints.Count > 0)
        {
            Section("Key Discussion Points");
            foreach (var pt in min.DiscussionPoints) { Bullet(pt); _y += 4; }
            _y += 14;
        }

        if (min.Decisions.Count > 0)
        {
            Section("Decisions");
            foreach (var d in min.Decisions)
            {
                Body($"[{d.Category}]", 9, bold: true, BR, BG, BB);
                _y += 2;
                Body(d.Text, 11);
                _y += 8;
            }
            _y += 10;
        }

        if (min.ActionItems.Count > 0)
        {
            Section("Action Items");
            foreach (var a in min.ActionItems)
            {
                var chk = a.IsCompleted ? "[done] " : "[ ]  ";
                Bullet(chk + a.Description);
                if (!string.IsNullOrEmpty(a.Assignee) || !string.IsNullOrEmpty(a.DueDate))
                    Body($"  {a.Assignee}{(string.IsNullOrEmpty(a.DueDate) ? "" : "  -  due " + a.DueDate)}",
                         9, r: DR, g: DG, b: DB);
                _y += 4;
            }
            _y += 14;
        }

        if (tscript.Count > 0)
        {
            Section("Full Transcript");
            foreach (var seg in tscript)
            {
                if (string.IsNullOrWhiteSpace(seg.Text)) continue;
                Body($"[{seg.TimestampDisplay}]  {seg.Speaker}:", 9, bold: true, DR, DG, DB);
                _y += 2;
                Body(seg.Text, 10);
                _y += 8;
            }
        }
    }

    // ─────────────────────────── Drawing ────────────────────────────────────

    void Section(string title)
    {
        NeedRoom(30);
        Rect(_m - 9, _y, 3, 20, BR, BG, BB);
        Text(title, 13, bold: true, BR, BG, BB);
        _y += Lh(13f) + 6;
    }

    void Bullet(string text)
    {
        var lines = Wrap(Safe(text), 11f, _cw - 18).ToList();
        bool first = true;
        foreach (var ln in lines)
        {
            NeedRoom(Lh(11f));
            float yb = _h - _y - 11f * 0.8f;
            if (first)
            {
                _cur.Append($"{BR:F3} {BG:F3} {BB:F3} rg\n");
                _cur.Append($"BT /F1 11 Tf 1 0 0 1 {_m:F1} {yb:F1} Tm (-) Tj ET\n");
                first = false;
            }
            _cur.Append($"{GR:F3} {GG:F3} {GB:F3} rg\n");
            _cur.Append($"BT /F1 11 Tf 1 0 0 1 {(_m + 14):F1} {yb:F1} Tm ({Esc(ln)}) Tj ET\n");
            _y += Lh(11f);
        }
    }

    void Body(string text, float sz, bool bold = false,
              float r = GR, float g = GG, float b = GB)
        => Text(text, sz, bold, r, g, b);

    void Text(string text, float sz, bool bold = false,
              float r = GR, float g = GG, float b = GB)
    {
        foreach (var ln in Wrap(Safe(text), sz, _cw))
        {
            NeedRoom(Lh(sz));
            RawLine(ln, sz, bold, r, g, b, _m);
            _y += Lh(sz);
        }
    }

    void RawLine(string line, float sz, bool bold, float r, float g, float b, float x)
    {
        float yb = _h - _y - sz * 0.8f;
        _cur.Append($"{r:F3} {g:F3} {b:F3} rg\n");
        _cur.Append($"BT /{(bold ? "F2" : "F1")} {sz:F0} Tf 1 0 0 1 {x:F1} {yb:F1} Tm ({Esc(line)}) Tj ET\n");
    }

    void Rect(float x, float yTop, float w, float h, float r, float g, float b)
    {
        float yp = _h - yTop - h;
        _cur.Append($"{r:F3} {g:F3} {b:F3} rg\n");
        _cur.Append($"{x:F1} {yp:F1} {w:F1} {h:F1} re f\n");
    }

    void NeedRoom(float h)
    {
        if (_y + h <= _h - 48) return;
        _cur = new StringBuilder();
        _pages.Add(_cur);
        _y = _m;
    }

    static float Lh(float sz) => sz * 1.45f;

    // ─────────────────────────── PDF serialisation ──────────────────────────

    public void Save(string path)
    {
        int n   = _pages.Count;
        int f1  = 2 * n + 3;
        int f2  = 2 * n + 4;
        int tot = f2;

        // Write footer on every page (total page count is now known)
        for (int i = 0; i < n; i++)
        {
            var p = _pages[i];
            p.Append($"{SR:F3} {SR:F3} {SR:F3} RG 0.5 w {_m:F1} 38 m {(_m + _cw):F1} 38 l S\n");
            p.Append($"{DR:F3} {DG:F3} {DB:F3} rg\n");
            p.Append($"BT /F1 9 Tf 1 0 0 1 {_m:F1} 26 Tm ({Esc($"MinuteMind  |  {DateTime.Now:MMM dd, yyyy}")}) Tj ET\n");
            p.Append($"BT /F1 9 Tf 1 0 0 1 {(_m + _cw - 48):F1} 26 Tm ({Esc($"Page {i + 1} of {n}")}) Tj ET\n");
        }

        var buf = new List<byte>(1 << 17);
        var off = new int[tot + 1];

        void W(string s) => buf.AddRange(Encoding.ASCII.GetBytes(s));
        void WB(byte[] b) => buf.AddRange(b);

        W("%PDF-1.4\n");

        off[1] = buf.Count;
        W("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        var kids = string.Join(" ", Enumerable.Range(0, n).Select(i => $"{3 + 2 * i} 0 R"));
        off[2] = buf.Count;
        W($"2 0 obj\n<< /Type /Pages /Kids [{kids}] /Count {n} >>\nendobj\n");

        for (int i = 0; i < n; i++)
        {
            int pid = 3 + 2 * i, cid = 4 + 2 * i;
            off[pid] = buf.Count;
            W($"{pid} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {_w:F0} {_h:F0}] " +
              $"/Contents {cid} 0 R /Resources << /Font << /F1 {f1} 0 R /F2 {f2} 0 R >> >> >>\nendobj\n");
            var cb = Encoding.Latin1.GetBytes(_pages[i].ToString());
            off[cid] = buf.Count;
            W($"{cid} 0 obj\n<< /Length {cb.Length} >>\nstream\n");
            WB(cb);
            W("endstream\nendobj\n");
        }

        off[f1] = buf.Count;
        W($"{f1} 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");
        off[f2] = buf.Count;
        W($"{f2} 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");

        int xp = buf.Count;
        W($"xref\n0 {tot + 1}\n");
        W("0000000000 65535 f\r\n");
        for (int i = 1; i <= tot; i++) W($"{off[i]:D10} 00000 n\r\n");
        W($"trailer\n<< /Size {tot + 1} /Root 1 0 R >>\nstartxref\n{xp}\n%%EOF\n");

        File.WriteAllBytes(path, buf.ToArray());
    }

    // ─────────────────────────── Helpers ────────────────────────────────────

    static IEnumerable<string> Wrap(string text, float sz, float maxW)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        int cap = Math.Max(1, (int)(maxW / (sz * 0.55f)));
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        foreach (var w in words)
        {
            if (sb.Length + w.Length + (sb.Length > 0 ? 1 : 0) > cap && sb.Length > 0)
            {
                yield return sb.ToString(); sb.Clear();
            }
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(w);
        }
        if (sb.Length > 0) yield return sb.ToString();
    }

    static string Esc(string s) =>
        s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    // Replace non-Latin1 and C#-11-problematic Unicode characters with ASCII equivalents.
    static string Safe(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (char c in s)
        {
            var mapped = (int)c switch
            {
                0x2022 => "*",     // bullet
                0x2018 => "\x27",  // left single quote  -> apostrophe
                0x2019 => "\x27",  // right single quote -> apostrophe
                0x201C => "\x22",  // left double quote  -> quotation mark
                0x201D => "\x22",  // right double quote -> quotation mark
                0x2013 => "-",     // en dash
                0x2014 => "--",    // em dash
                0x2026 => "...",   // ellipsis
                0x2192 => "->",    // right arrow
                0x2713 => "[x]",   // check mark
                _ => null
            };
            if (mapped != null)
                sb.Append(mapped);
            else if (c < 32 || (c > 126 && c < 160))
                sb.Append(' ');
            else if (c > 255)
                sb.Append('?');
            else
                sb.Append(c);
        }
        return sb.ToString();
    }
}
