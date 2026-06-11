# Art Assets

This folder contains generated first-pass art assets for XianXiaMod.

## Folders

- `Generated/`: raw generated chroma-key source sheets.
- `Cleaned/`: post-processed transparent source sheets.
- `Final/`: sliced, aligned, transparent PNG assets sized for first-pass tModLoader integration.
- `Specs/`: CSV/YAML manifests used to generate and process assets.
- `Reference/`: selected style anchors and future approved references.

## Current Batch

- Batch: `v01`
- Strategy: first usable pass, later manual pixel refinement expected.
- Source style: Terraria-like 2D pixel art, transparent-ready chroma-key sheets, strong outline, limited palette.
- Final outputs: 130 PNG files generated from `Assets/Specs/art_asset_manifest.csv`.

## Validation

The current final batch has been checked for:

- Expected dimensions from manifest.
- RGBA PNG format.
- Transparent corners for non-tile/non-wall assets.
- Source sheet availability.

Known caveat: these assets are generated first-pass art. Some sprites may need manual pixel cleanup, animation refinement, or redraw before shipping.
