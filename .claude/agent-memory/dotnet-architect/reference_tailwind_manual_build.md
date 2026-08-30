---
name: tailwind-manual-build
description: Tailwind CSS for LIN.Notes.Web is built manually, not via MSBuild
metadata:
  type: reference
---

Tailwind v3.4.17 is compiled by hand for this repo (no build-time integration).

**How to apply:** after editing markup classes or `wwwroot/css/app.css`, run from
`D:/LIN/LIN Services/Clientes/LIN.Notes.Web/LIN.Notes.Web/LIN.Notes.Web`:

```
npx tailwindcss -i wwwroot/css/app.css -o wwwroot/css/tailwind.css
```

- Input: `wwwroot/css/app.css` (has `@tailwind` directives + `--gim-*` tokens + hand CSS).
- Output committed: `wwwroot/css/tailwind.css` (this is the only tailwind link kept in
  `Components/App.razor`).
- `content` globs also scan the separate shared-components repo at
  `D:/LIN/LIN Services/Components/LIN.Notes.Shared/`.
- Note: repo root is doubly nested — `.../LIN.Notes.Web/LIN.Notes.Web/` holds the sln;
  `.../LIN.Notes.Web/LIN.Notes.Web/LIN.Notes.Web/` is the host project.
- Hand-written utility classes meant to be applied via string interpolation (e.g.
  `note-tint-{n}`) must be plain CSS outside `@layer utilities` or Tailwind tree-shakes them.
