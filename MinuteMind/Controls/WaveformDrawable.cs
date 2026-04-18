using System;

namespace MinuteMind.Controls;

public class WaveformDrawable :IDrawable
{
    // array of 0.0 t0 1.0 values, oe per bar. The VM will ccalucclate and make these.
    public float[] Levels { get; set; } = new float[13];
    public Color BarColor { get; set; } = Color.FromArgb("#005FAA");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var barWidth = 6f;
        var gap = 6f;
        var totalWidth = Levels.Length * (barWidth + gap) - gap;

        //cennter the whole group of bars horizontally
        var startX = (dirtyRect.Width - totalWidth) / 2;
        var maxHeight = dirtyRect.Height;

        for (var i = 0; i < Levels.Length; i++)
        {
            // Clamp so bars never fully disappear (min 10% height)
            var level = Math.Clamp(Levels[i], 0.1f, 1.0f);
            var barHeight = maxHeight * level;
            var x = startX + i * (barWidth + gap);
            // Center each bar vertically so it grows from the middle
            var y = (maxHeight - barHeight) / 2;

            // Higher level = more opaque, lower = more transparent
            canvas.FillColor = BarColor.WithAlpha(0.2f + level * 0.8f);
            canvas.FillRoundedRectangle(x, y, barWidth, barHeight, barWidth / 2);
        }

    }
}
