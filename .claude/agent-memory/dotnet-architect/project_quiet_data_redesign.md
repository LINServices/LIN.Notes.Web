---
name: quiet-data-redesign
description: LIN.Notes.Web is being restyled to the "Quiet Data" design system in coordinated waves
metadata:
  type: project
---

The Blazor app `LIN.Notes.Web` is being migrated to an in-house design system called
"Quiet Data", done in numbered "olas" (waves) by separate agents/sessions.

**Why:** Large restyle; split into waves to keep each change reviewable and let the
token foundation land before pages consume it.

**How to apply:**
- Wave 1 (done 2026-08-29): shell + Topbar + SectionHero foundation. Files:
  `LIN.Notes.Web.Client/Layout/MainLayout.razor(.css)`, new `Layout/Topbar.razor`,
  new `Layout/SectionHero.razor`, `Client/Shared/AuthorizationControl.razor`,
  `LIN.Notes.Web/Components/App.razor`, note-tint utilities in `wwwroot/css/app.css`.
- `Login.razor` + `AuthLayout.razor` are the reference for correct style — do not touch.
- Wave 2 owns `Home.razor` / `Note.razor` / `NoteControl` and will consume the
  `.note-tint-0..4` classes defined in `wwwroot/css/app.css` (5 note colours kept as a
  documented exception to the "single accent" rule — re-tempered to a 6-8% wash + 2px band).
- Wave 3 owns `Movements.razor` (expenses): monochrome except the amount figure.
- Design tokens live in `LIN.Notes.Web/tailwind.config.js` + `wwwroot/css/app.css`
  (RGB-channel custom props `--gim-*`). Accent is amber `#eab308` via `current-*` scale.

See [[tailwind-manual-build]].
