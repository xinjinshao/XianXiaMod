# Art Runtime Refresh Rules

Last updated: 2026-06-17

This file supplements `Docs/ART_ASSET_GENERATION_PLAN.md` for the non-Boss redesign pass.

## Atlas Safety

- Batch generation may use an atlas only when it is a fixed grid atlas.
- Every asset must be centered in its own large cell.
- Neighboring visible pixels must be separated by at least 100px of pure `#ff00ff` gutter.
- Outer atlas borders must keep enough empty `#ff00ff` padding.
- Extraction must use the manifest grid cell coordinates. Do not use connected-component guessing for final atlas slicing.
- If any top or bottom fragment from asset A appears in asset B, treat the whole atlas as contaminated. Regenerate with fewer assets or larger cells.

## Runtime Edge Rules

- Non-Boss runtime PNGs in `Content/` must use hard pixel edges.
- Ordinary item icons, buffs, NPCs, enemies, equipment, stations, object tiles, and most projectiles must not keep feathered alpha halos.
- Run `python Tools\harden_runtime_png_edges.py` after refreshing non-Boss runtime PNGs.
- Boss body sprites are excluded from the hardening tool by default.

## Animation Outputs

- Enemies must provide a base sprite and a 6-frame vertical animation sheet. The sheet must be synced to `Content/NPCs/Enemies/*.png`.
- Town NPC bodies must provide a 4-frame vertical animation sheet. The sheet must be synced to `Content/NPCs/Town/*.png`.
- Town NPC heads stay as single 32x32 head textures.
- Projectiles must provide a base projectile texture and a `__motion_sheet__` preview in `Assets/Final/<asset>/`.
- Weapons, equipment, active items, and station icons must provide the base icon plus a `__use_sheet__` preview in `Assets/Final/<asset>/`.
- Simple materials may keep a single runtime icon, but should still have a lightweight `__use_sheet__` or inspect-style preview for wiki display.

## Required Verification

Run these after every non-Boss art refresh:

```powershell
python Tools\audit_art_redesign_quality.py
python Tools\verify_art_quality.py
python Tools\verify_png_assets.py
python Tools\verify_content_contract.py
python Tools\verify_localization_keys.py
dotnet build XianXia.csproj
powershell -ExecutionPolicy Bypass -File Tools\tmodloader_smoke_test.ps1
```

Also refresh:

- `Assets/Final/contact_sheet_v01.png`
- `Assets/Final/ContactSheets/*`
- `Wiki/Art_Gallery.md`
- Any wiki overview or entry page that embeds affected art.
