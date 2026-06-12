# Art Asset Replacement Status

This file originally tracked placeholder textures. The implemented content now has unique client-loadable PNG assets for every current `ModItem`, `ModTile`, `ModProjectile`, and `ModBuff`.

## Completed Replacements

### Crafting Stations

- `EarthClayFurnaceTile` and `EarthClayFurnace`
- `SimpleTalismanTableTile` and `SimpleTalismanTable`
- `StarPatternCauldronTile` and `StarPatternCauldron`
- `ThunderPatternForgeTile` and `ThunderPatternForge`
- `SectTrialAltarTile` and `SectTrialAltar`
- `HeavenFireFurnaceTile` and `HeavenFireFurnace`
- `DaoSeveringAltarTile` and `DaoSeveringAltar`

### Buff Icons

- `SpringReturnRegenBuff`
- `StarAbyssCorrosionBuff`
- `ArchiveLockBuff`

### Hand Generated Item Icons

- Inscription needles, removal stone, contracts, route hints, seeds, utility items, rare drops, masks, pets, trophies, decorations, and vanity rewards in `Content/Items/HandGenerated`.

### Biome Object Tiles

- `SwordTabletTile`
- `BrokenHeavenTabletTile`
- `ArchiveLightPillarTile`
- `SingingThunderStoneTile`
- `RiftMembraneTile`

### Required Projectile Textures

- `BossArrayFieldProjectile`
- `BossSpiritBoltProjectile`
- `TribulationLightningProjectile`

## Validation

`Tools/verify_png_assets.py` now verifies that every current `ModItem`, `ModTile`, `ModProjectile`, and `ModBuff` class has its matching client texture. This catches the class of tModLoader client load errors caused by missing art resources.

## Future Design-Only Entries

The following wiki concepts are not currently represented by concrete C# classes, so there is no runtime placeholder to replace yet:

- `TalismanFireProjectile`
- `StarBladeProjectile`
- `MoonboneSwordQiProjectile`
- `VineWhipProjectile`
- `JudgmentBeamProjectile`
- `cultivation_tier_icons`

When these features are implemented, add the matching PNG at the same path as the new class and rerun `python Tools\verify_png_assets.py`.
