# Design System Specification: High-End Editorial Intelligence

## 1. Overview & Creative North Star
The "Creative North Star" for this design system is **The Digital Architect.** 

Moving beyond a generic utility app, this system treats meeting data as a premium editorial experience. We reject the "boxed-in" layout of traditional mobile apps in favor of **Structural Fluidity**. By leveraging intentional asymmetry, expansive negative space, and a sophisticated layering of tonal surfaces, we create an environment that feels intelligent and authoritative. The interface doesn't just display information; it curates it. We use the contrast between the sharp, geometric precision of *Plus Jakarta Sans* and the functional clarity of *Inter* to signal a product that is both visionary and deeply reliable.

---

### 2. Colors: The Tonal Landscape
We move away from the "flat blue" trope. This system utilizes a sophisticated light-mode palette where depth is communicated through temperature and "weight" rather than lines.

*   **Primary Architecture:** Use `primary` (#005FAA) for high-intent actions and `primary_container` (#0078D4) for tonal highlights.
*   **The "No-Line" Rule:** Direct 1px borders are strictly prohibited for sectioning. To define boundaries, use a shift from `surface` (#FAF9F8) to `surface_container_low` (#F4F3F2). Boundaries must feel "felt," not "seen."
*   **Surface Hierarchy & Nesting:** Treat the UI as stacked sheets of fine vellum. 
    *   *Base Layer:* `surface`
    *   *Sectional Layer:* `surface_container`
    *   *Interactive Card:* `surface_container_lowest` (Pure White)
*   **The Glass & Gradient Rule:** For elements that require "Intelligence" (AI summaries, active recording), use a backdrop-blur (20px+) on `surface_container_lowest` at 80% opacity. Apply a subtle linear gradient from `primary` to `primary_container` at a 135° angle for hero CTAs to give them a "soul."

---

### 3. Typography: Editorial Authority
Typography is our primary tool for hierarchy. We use a dual-font approach to balance "Executive Vision" with "Technical Precision."

*   **Display & Headlines (Plus Jakarta Sans):** These are the "Voice" of the app. Use `display-lg` and `headline-md` with tight letter-spacing (-0.02em) to create a bold, editorial look. Large headlines should often sit asymmetrically to the left, creating a sophisticated "white space" anchor.
*   **Body & Labels (Inter):** The "Workhorse." *Inter* is used for all functional data. Use `body-md` for meeting transcripts and `label-sm` (all caps, +0.05em tracking) for metadata like timestamps or "AI-Generated" tags.
*   **Hierarchy Tip:** Never use color alone to show importance. Use the scale jump from `headline-sm` to `body-sm` to create an immediate visual "hook."

---

### 4. Elevation & Depth: Tonal Layering
We abandon traditional drop shadows in favor of **Atmospheric Depth.**

*   **The Layering Principle:** To lift a card, place a `surface_container_lowest` (#FFFFFF) element onto a `surface_container` (#EFEEED) background. The 4% brightness delta is enough for the human eye to perceive elevation without visual clutter.
*   **Ambient Shadows:** For floating action buttons or modal sheets, use a "Ghost Shadow."
    *   *Values:* `0px 12px 32px`
    *   *Color:* `on_surface` (#1A1C1C) at **4% opacity**. It should feel like a soft hum, not a dark stain.
*   **The Ghost Border Fallback:** If a container requires more definition (e.g., in high-glare environments), use `outline_variant` at **15% opacity**. It must remain nearly invisible.

---

### 5. Components
All components must adhere to the **Roundedness Scale** (Default: `0.5rem`, Hero Cards: `lg` or `xl`).

*   **Buttons:**
    *   *Primary:* Gradient fill (`primary` to `primary_container`), `full` roundedness, `headline-sm` type.
    *   *Secondary:* `surface_container_high` background, no border, `on_surface` text.
*   **Intelligence Chips:** Use `secondary_container` with `on_secondary_container` text. These should represent AI-extracted keywords or topics. No icons—let the typography breathe.
*   **Meeting Cards:** 
    *   *Style:* Use `surface_container_lowest` with `xl` (1.5rem) corner radius. 
    *   *Rule:* No dividers. Separate the "Meeting Title" from the "Participants" using a `1.5` (0.375rem) spacing unit and a color shift to `on_surface_variant`.
*   **Input Fields:** 
    *   *State:* Inactive fields should be `surface_container_low`. On focus, shift to `surface_container_lowest` with a 2px `primary` bottom-border (no full box).
*   **The "AI Pulse" Component:** A signature element for this app. A semi-transparent `surface_tint` circle with a `24` spacing scale blur, placed behind active AI transcription text to signify "Intelligence in progress."

---

### 6. Do’s and Don’ts

**Do:**
*   **Do** use asymmetrical margins. A wider left margin (e.g., `spacing-8`) with a tighter right margin can make a list of meetings feel like a curated magazine.
*   **Do** use `surface_bright` for peak highlights in data visualizations.
*   **Do** embrace the "Empty State." If there are no meetings, use a single `display-md` word ("Quiet") rather than a cluttered illustration.

**Don’t:**
*   **Don’t** use a 1px divider to separate list items. Use a `spacing-4` vertical gap or a subtle background toggle between `surface` and `surface_container_low`.
*   **Don’t** use pure black (#000000). Always use `on_surface` (#1A1C1C) to maintain the premium, soft-touch feel.
*   **Don’t** use high-contrast "Danger" reds for everything. Use `error_container` with `on_error_container` to keep the professional, calm aesthetic even during errors.