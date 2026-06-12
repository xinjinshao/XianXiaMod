# Art Asset TODO List

This document tracks all art assets that currently use placeholder textures and need unique final art.

## Placeholder Tiles (7 crafting stations)
All use `Content/Tiles/Stations/AlchemyCauldronTile.png` as placeholder.

| Tile | Wiki ID | Spec Size | Wiki Prompt | Priority |
|------|---------|-----------|-------------|----------|
| EarthClayFurnaceTile | `earth_clay_furnace` | 48x48 | small clay alchemy furnace, red ember mouth, jade herbal vapor | P1 |
| SimpleTalismanTableTile | `simple_talisman_table` | 48x32 | small wooden talisman table, paper sheets, cinnabar ink dish | P1 |
| StarPatternCauldronTile | `star_pattern_cauldron` | 64x64 | dark blue star-pattern cauldron, void crystal rim, violet flame | P2 |
| ThunderPatternForgeTile | `thunder_pattern_forge` | 64x48 | thunder forge, cloud iron anvil, purple blue lightning veins | P2 |
| SectTrialAltarTile | `sect_trial_altar` | 64x48 | ancient sect trial altar, white stone, sword slot, jade token socket | P2 |
| HeavenFireFurnaceTile | `heaven_fire_furnace` | 64x64 | white jade heaven fire furnace, golden divine flame, floating decree fragments | P3 |
| DaoSeveringAltarTile | `dao_severing_altar` | 80x48 | dao severing altar, black and white broken ring, tiny void crack | P3 |

## Missing Buff Icons (3 buffs)
Use `Content/Buffs/QiGatheringBuff.png` as placeholder.

| Buff | Wiki ID | Spec Size | Wiki Prompt | Priority |
|------|---------|-----------|-------------|----------|
| SpringReturnRegenBuff | `spring_return_regen` | 32x32 | jade pill with leaf pattern, green and cyan | P1 |
| StarAbyssCorrosionBuff | `star_abyss_corrosion` | 32x32 | dark blue star-eye, corruption purple cracks | P2 |
| ArchiveLockBuff | `archive_lock` | 32x32 | golden ring lock, archive lines | P2 |

## Missing Item Icons (10+ items)
Use `Content/Items/Generated/ArtifactBlankShard.png` as placeholder.

| Item | Wiki ID | Spec Size | Priority |
|------|---------|-----------|----------|
| SpringReturnRegenPill (buff item) | `spring_return_regen_pill` | 32x32 | P1 |
| StarAbyssCorrosionResist (buff item) | `star_abyss_corrosion_resist` | 32x32 | P2 |
| ArchiveLockBreaker (buff item) | `archive_lock_breaker` | 32x32 | P2 |
| GreenwoodInscriptionNeedle | `greenwood_inscription_needle` | 32x32 | P1 |
| FurnaceInscriptionNeedle | `furnace_inscription_needle` | 32x32 | P1 |
| ThunderInscriptionNeedle | `thunder_inscription_needle` | 32x32 | P2 |
| StarAbyssInscriptionNeedle | `star_abyss_inscription_needle` | 32x32 | P2 |
| BrokenHeavenInscriptionNeedle | `broken_heaven_inscription_needle` | 32x32 | P2 |
| InscriptionRemovalStone | `inscription_removal_stone` | 32x32 | P1 |
| SmallArtifactPendant | `small_artifact_pendant` | 32x32 | P2 |
| FurnaceAshSpiritContract | `furnace_ash_spirit_contract` | 32x32 | P2 |
| StarAbyssLarvaContract | `star_abyss_larva_contract` | 32x32 | P2 |
| NascentSoulCloneTalisman | `nascent_soul_clone_talisman` | 32x32 | P2 |
| CelestialPuppetToken | `celestial_puppet_token` | 32x32 | P3 |
| ArchivedImmortalSoulContract | `archived_immortal_soul_contract` | 32x32 | P3 |
| SpiritHerbSeeds | `spirit_herb_seeds` | 32x32 | P2 |
| SectTrialHint | `sect_trial_hint` | 32x32 | P2 |
| HeavenDaoRouteHint | `heaven_dao_route_hint` | 32x32 | P3 |
| EndgameRouteFrame | `endgame_route_frame` | 32x32 | P3 |

## Missing Projectile Sprites (5 projectiles)
Use `Content/Projectiles/SpiritBoltProjectile.png` as placeholder.

| Projectile | Wiki ID | Spec Size | Priority |
|------------|---------|-----------|----------|
| TalismanFireProjectile | `talisman_fire` | 16x16 | P2 |
| StarBladeProjectile | `star_blade` | 32x16 | P2 |
| MoonboneSwordQiProjectile | `moonbone_sword_qi` | 48x16 | P2 |
| VineWhipProjectile | `vine_whip` | 32x8 | P2 |
| JudgmentBeamProjectile | `judgment_beam` | 32x128 | P3 |

## Missing UI Elements (1 element)

| Element | Wiki ID | Spec Size | Priority |
|---------|---------|-----------|----------|
| cultivation_tier_icons | `cultivation_tier_icons` | 8 frames x 32x32 | P2 |

## Missing Object Sprites (5 biome objects)
These exist in `Assets/Final/` but not yet placed or used in worldgen.

| Object | Wiki ID | Spec Size | Final Path | Priority |
|--------|---------|-----------|------------|----------|
| SwordTablet | `sword_tablet` | 32x48 | Assets/Final/sword_tablet/ | P2 |
| BrokenHeavenTablet | `broken_heaven_tablet` | 32x64 | Assets/Final/broken_heaven_tablet/ | P2 |
| ArchiveLightPillar | `archive_light_pillar` | 32x96 | Assets/Final/archive_light_pillar/ | P3 |
| SingingThunderStone | `singing_thunder_stone` | 24x32 | Assets/Final/singing_thunder_stone/ | P2 |
| RiftMembrane | `rift_membrane` | 32x32 | Assets/Final/rift_membrane/ | P2 |

## Placeholder Strategy

All items without final art use this placeholder file:
`Content/Items/Generated/ArtifactBlankShard.png`

All buffs without final art use:
`Content/Buffs/QiGatheringBuff.png`

All tiles without final art use:
`Content/Tiles/Stations/AlchemyCauldronTile.png`

All projectiles without final art use:
`Content/Projectiles/SpiritBoltProjectile.png`

Each placeholder is documented with `// TODO: ART_PLACEHOLDER - see Docs/ART_TODO.md` in the corresponding C# file.

## Total: ~40 assets needed
- 7 crafting station tiles
- 3 buff icons
- 19 item icons  
- 5 projectile sprites
- 1 UI element (8 frames)
- 5 biome object placements
