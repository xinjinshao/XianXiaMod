from __future__ import annotations

import csv
import re
import shutil
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
FINAL = ROOT / "Assets" / "Final"
CONTENT = ROOT / "Content"


def pascal(asset_id: str) -> str:
    return "".join(part.capitalize() for part in asset_id.split("_"))


def write(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text.replace("\n", "\r\n"), encoding="utf-8")


def copy_asset(asset_id: str, output_type: str, class_name: str, folder: Path, suffix: str = "") -> None:
    src = FINAL / asset_id / f"{asset_id}__{output_type}__v01.png"
    if not src.exists():
        return
    dst = folder / f"{class_name}{suffix}.png"
    dst.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, dst)


def manifest_rows() -> list[dict[str, str]]:
    with (ROOT / "Assets" / "Specs" / "art_asset_manifest.csv").open(encoding="utf-8-sig", newline="") as f:
        return list(csv.DictReader(f))


def existing_class_names() -> set[str]:
    names: set[str] = set()
    for cs in ROOT.rglob("*.cs"):
        if "\\Tools\\" in str(cs) or "/Tools/" in str(cs):
            continue
        text = cs.read_text(encoding="utf-8")
        names.update(re.findall(r"\bclass\s+([A-Za-z0-9_]+)", text))
    return names


DISPLAY = {
    "greenwood_root": ("青木根", "Greenwood Root"),
    "furnace_slag_iron": ("炉渣铁", "Furnace Slag Iron"),
    "artifact_blank_shard": ("器胚碎片", "Artifact Blank Shard"),
    "tribulation_cloud_dew": ("劫云露", "Tribulation Cloud Dew"),
    "star_eclipse_crystal": ("星蚀晶", "Star Eclipse Crystal"),
    "sect_trial_token": ("宗门令", "Sect Trial Token"),
    "heaven_dao_fragment": ("天道碎片", "Heaven Dao Fragment"),
    "moonbone": ("月骸骨", "Moonbone"),
    "dao_severing_dust": ("斩道尘", "Dao Severing Dust"),
    "spring_return_pill": ("回春丹", "Spring Return Pill"),
    "qi_condensing_pill": ("凝气丹", "Qi Condensing Pill"),
    "foundation_pill": ("筑基丹", "Foundation Pill"),
    "tribulation_resisting_pill": ("抗劫丹", "Tribulation Resisting Pill"),
    "star_abyss_forbidden_talisman": ("星渊禁符", "Star Abyss Forbidden Talisman"),
    "garden_broken_key": ("守园残钥", "Garden Broken Key"),
    "old_furnace_ember": ("旧炉火种", "Old Furnace Ember"),
    "thunder_calling_jade": ("引雷玉", "Thunder Calling Jade"),
    "star_abyss_membrane": ("星渊胎膜", "Star Abyss Membrane"),
    "heaven_tablet_rubbing": ("天碑拓片", "Heaven Tablet Rubbing"),
    "moonbone_ritual_talisman": ("月骸祭符", "Moonbone Ritual Talisman"),
    "cloudpiercer_flying_sword": ("破云飞剑", "Cloudpiercer Flying Sword"),
    "thunder_pattern_sword_case": ("雷纹剑匣", "Thunder Pattern Sword Case"),
    "formless_sword_wheel": ("无相剑轮", "Formless Sword Wheel"),
    "moonbone_dharma_sword": ("月骸法剑", "Moonbone Dharma Sword"),
    "cinnabar_talisman_flame_item": ("朱砂符火", "Cinnabar Talisman Flame"),
    "greenwood_array_plate": ("青木阵盘", "Greenwood Array Plate"),
    "thunder_talisman_array_plate": ("雷符阵盘", "Thunder Talisman Array Plate"),
    "broken_heaven_decree": ("残天法旨", "Broken Heaven Decree"),
    "old_heaven_dao_scroll": ("旧天道残卷", "Old Heaven Dao Scroll"),
    "star_eclipse_arbalest": ("星蚀弩机", "Star Eclipse Arbalest"),
    "qi_gathering_pendant": ("聚气坠", "Qi Gathering Pendant"),
    "spiritwood_charm": ("灵木护符", "Spiritwood Charm"),
    "furnace_heart_ring": ("炉心戒", "Furnace Heart Ring"),
    "lightning_ward_jade": ("避雷玉佩", "Lightning Ward Jade"),
    "star_abyss_eye": ("星渊眼", "Star Abyss Eye"),
    "nascent_soul_jade_box": ("元婴玉匣", "Nascent Soul Jade Box"),
    "broken_heaven_crown_seal": ("残天冠印", "Broken Heaven Crown Seal"),
    "dao_severing_ring": ("斩道环", "Dao Severing Ring"),
}


BOSS_DATA = {
    "garden_warden": ("药宗守园人", "Garden Warden", 2800, 28, 10, "garden_broken_key", "greenwood_root"),
    "black_furnace_iron_golem": ("玄炉铁傀", "Black Furnace Iron Golem", 3200, 34, 18, "old_furnace_ember", "furnace_slag_iron"),
    "tribulation_cloud_avatar": ("劫云化身", "Tribulation Cloud Avatar", 4200, 30, 12, "thunder_calling_jade", "tribulation_cloud_dew"),
    "thunder_marsh_jiao": ("雷泽蛟", "Thunder Marsh Jiao", 18000, 58, 26, "thunder_calling_jade", "tribulation_cloud_dew"),
    "abyssal_star_womb": ("星渊胎主", "Abyssal Star Womb", 21000, 54, 30, "star_abyss_membrane", "star_eclipse_crystal"),
    "formless_sword_soul": ("无相剑魄", "Formless Sword Soul", 48000, 72, 38, "sect_trial_token", "sect_trial_token"),
    "greenwood_medicine_king_echo": ("青木药王残影", "Greenwood Medicine King Echo", 52000, 66, 34, "sect_trial_token", "greenwood_root"),
    "heaven_tablet_guardian": ("天碑守御", "Heaven Tablet Guardian", 86000, 82, 48, "heaven_tablet_rubbing", "heaven_dao_fragment"),
    "broken_heaven_inspector": ("残天监察使", "Broken Heaven Inspector", 96000, 92, 42, "heaven_tablet_rubbing", "heaven_dao_fragment"),
    "moonbone_immortal": ("月骸仙君", "Moonbone Immortal", 420000, 180, 80, "moonbone_ritual_talisman", "moonbone"),
    "old_heaven_dao_core": ("旧天道核心", "Old Heaven Dao Core", 650000, 220, 100, "moonbone_ritual_talisman", "dao_severing_dust"),
}


ENEMY_DATA = {
    "herb_garden_vine_spirit": (140, 24, 8, "greenwood_root"),
    "miasma_flower_moth": (90, 20, 4, "greenwood_root"),
    "furnace_ash_golem": (180, 28, 14, "furnace_slag_iron"),
    "iron_shard_spirit": (70, 22, 6, "artifact_blank_shard"),
    "tribulation_cloudling": (240, 42, 16, "tribulation_cloud_dew"),
    "thunder_pattern_hawk": (300, 48, 18, "tribulation_cloud_dew"),
    "star_eclipsed_cultivator": (360, 50, 20, "star_eclipse_crystal"),
    "star_abyss_larva": (260, 46, 18, "star_eclipse_crystal"),
    "obsessed_sword_cultivator": (850, 72, 34, "sect_trial_token"),
    "scripture_archive_echo": (720, 66, 28, "sect_trial_token"),
    "celestial_puppet": (1350, 88, 46, "heaven_dao_fragment"),
    "heaven_tablet_guard": (1500, 92, 54, "heaven_dao_fragment"),
    "moonbone_cultivator": (4200, 160, 72, "moonbone"),
    "archived_immortal_soul": (3600, 150, 64, "dao_severing_dust"),
}


BIOME_BY_ENEMY = {
    "herb_garden_vine_spirit": "GreenwoodHerbGardenBiome",
    "miasma_flower_moth": "GreenwoodHerbGardenBiome",
    "furnace_ash_golem": "SunkenFurnaceVeinBiome",
    "iron_shard_spirit": "SunkenFurnaceVeinBiome",
    "tribulation_cloudling": "ThunderMarshCloudsBiome",
    "thunder_pattern_hawk": "ThunderMarshCloudsBiome",
    "star_eclipsed_cultivator": "StarAbyssRiftBiome",
    "star_abyss_larva": "StarAbyssRiftBiome",
    "obsessed_sword_cultivator": "TenThousandSectsRuinsBiome",
    "scripture_archive_echo": "TenThousandSectsRuinsBiome",
    "celestial_puppet": "FallenHeavenPalaceBiome",
    "heaven_tablet_guard": "FallenHeavenPalaceBiome",
    "moonbone_cultivator": "MoonboneAbyssBiome",
    "archived_immortal_soul": "MoonboneAbyssBiome",
}


TILE_CLASSES = {
    "greenwood_soil_tile": ("GreenwoodSoilTile", "青木土", "Greenwood Soil", "greenwood_root"),
    "spirit_herb": ("SpiritHerbTile", "灵草", "Spirit Herb", "greenwood_root"),
    "furnace_slag_tile": ("FurnaceSlagTile", "炉渣石", "Furnace Slag", "furnace_slag_iron"),
    "black_furnace_wall": ("BlackFurnaceWall", "玄炉墙", "Black Furnace Wall", ""),
    "thunder_cloud_tile": ("ThunderCloudTile", "雷云块", "Thunder Cloud", ""),
    "star_abyss_crystal_tile": ("StarAbyssCrystalTile", "星渊晶岩", "Star Abyss Crystal", "star_eclipse_crystal"),
    "sect_ruin_brick": ("SectRuinBrickTile", "宗门石砖", "Sect Ruin Brick", ""),
    "fallen_heaven_jade_tile": ("FallenHeavenJadeTile", "坠天玉砖", "Fallen Heaven Jade", "heaven_dao_fragment"),
    "moonbone_tile": ("MoonboneTile", "月骸骨岩", "Moonbone Rock", "moonbone"),
}


BIOMES = [
    ("GreenwoodHerbGardenBiome", "青木药园", "Greenwood Herb Garden", ("GreenwoodSoilTile", "SpiritHerbTile"), 120),
    ("SunkenFurnaceVeinBiome", "沉炉矿脉", "Sunken Furnace Vein", ("FurnaceSlagTile",), 120),
    ("ThunderMarshCloudsBiome", "雷泽云层", "Thunder Marsh Clouds", ("ThunderCloudTile",), 100),
    ("StarAbyssRiftBiome", "星渊裂隙", "Star Abyss Rift", ("StarAbyssCrystalTile",), 140),
    ("TenThousandSectsRuinsBiome", "万宗遗址", "Ten Thousand Sects Ruins", ("SectRuinBrickTile",), 180),
    ("FallenHeavenPalaceBiome", "坠天宫阙", "Fallen Heaven Palace", ("FallenHeavenJadeTile",), 160),
    ("MoonboneAbyssBiome", "月骸天渊", "Moonbone Abyss", ("MoonboneTile",), 200),
]


def generate_materials(existing: set[str]) -> None:
    classes = []
    for asset_id, (zh, en) in DISPLAY.items():
        if asset_id in BOSS_DATA:
            continue
        row_type = next((r["output_type"] for r in manifest_rows() if r["asset_id"] == asset_id), None)
        if row_type != "item_icon":
            continue
        class_name = pascal(asset_id)
        if class_name in existing:
            continue
        copy_asset(asset_id, "item_icon", class_name, CONTENT / "Items" / "Generated")
        rare = "ItemRarityID.White"
        if any(k in asset_id for k in ["thunder", "star", "tribulation"]):
            rare = "ItemRarityID.LightRed"
        if any(k in asset_id for k in ["heaven", "broken"]):
            rare = "ItemRarityID.Yellow"
        if any(k in asset_id for k in ["moon", "dao", "old_heaven"]):
            rare = "ItemRarityID.Red"
        stack = 999 if any(k in asset_id for k in ["summon", "incense", "key", "ember", "jade", "membrane", "rubbing", "talisman"]) else 9999
        classes.append(f"""
public class {class_name} : ModItem
{{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {{
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = {stack};
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = {rare};
    }}
}}
""")
    write(CONTENT / "Items" / "Generated" / "GeneratedItems.cs", ITEMS_HEADER + "\n".join(classes))


ITEMS_HEADER = """using Terraria;\nusing Terraria.ID;\nusing Terraria.ModLoader;\n\nnamespace XianXia.Content.Items.Generated;\n"""


def generate_projectiles(existing: set[str]) -> None:
    classes = []
    for row in manifest_rows():
        if row["output_type"] != "projectile":
            continue
        class_name = pascal(row["asset_id"].replace("_proj", "_projectile"))
        if class_name in existing:
            continue
        copy_asset(row["asset_id"], "projectile", class_name, CONTENT / "Projectiles" / "Generated")
        width, height = row["width"], row["height"]
        classes.append(f"""
public class {class_name} : ModProjectile
{{
    public override void SetDefaults()
    {{
        Projectile.width = {width};
        Projectile.height = {height};
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }}

    public override void AI()
    {{
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
    }}
}}
""")
    write(CONTENT / "Projectiles" / "Generated" / "GeneratedProjectiles.cs", PROJECTILE_HEADER + "\n".join(classes))


PROJECTILE_HEADER = """using Microsoft.Xna.Framework;\nusing Terraria;\nusing Terraria.ModLoader;\n\nnamespace XianXia.Content.Projectiles.Generated;\n"""


def generate_tiles(existing: set[str]) -> None:
    classes = []
    for asset_id, (class_name, zh, en, drop) in TILE_CLASSES.items():
        if class_name in existing:
            continue
        output = "wall" if asset_id.endswith("_wall") else "tile"
        copy_asset(asset_id, output, class_name, CONTENT / "Tiles" / "Generated")
        drop_line = ""
        if drop:
            drop_line = f"        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.{pascal(drop)}>());"
        wall = "Wall" in class_name
        if wall:
            classes.append(f"""
public class {class_name} : ModWall
{{
    public override void SetStaticDefaults()
    {{
        Main.wallHouse[Type] = false;
        DustType = DustID.Stone;
        AddMapEntry(new Color(90, 82, 76), CreateMapEntryName());
    }}
}}
""")
        else:
            classes.append(f"""
public class {class_name} : ModTile
{{
    public override void SetStaticDefaults()
    {{
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
{drop_line}
    }}
}}
""")
    write(CONTENT / "Tiles" / "Generated" / "GeneratedTiles.cs", TILE_HEADER + "\n".join(classes))


TILE_HEADER = """using Microsoft.Xna.Framework;\nusing Terraria;\nusing Terraria.ID;\nusing Terraria.ModLoader;\n\nnamespace XianXia.Content.Tiles.Generated;\n"""


def generate_biomes() -> None:
    classes = []
    tile_count_terms = []
    for class_name, zh, en, tile_classes, threshold in BIOMES:
        terms = " + ".join(f"tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.{t}>()]" for t in tile_classes)
        prop = class_name[0].lower() + class_name[1:] + "TileCount"
        tile_count_terms.append((prop, terms))
        classes.append(f"""
public class {class_name} : ModBiome
{{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(90, 170, 150);

    public override bool IsBiomeActive(Player player)
    {{
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().{prop} >= {threshold};
    }}
}}
""")
    count_props = "\n".join(f"    public int {name};" for name, _ in tile_count_terms)
    count_assign = "\n".join(f"        {name} = {terms};" for name, terms in tile_count_terms)
    system = f"""
public class GeneratedBiomeTileCountSystem : ModSystem
{{
{count_props}

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {{
{count_assign}
    }}
}}
"""
    write(CONTENT / "Biomes" / "GeneratedBiomes.cs", BIOME_HEADER + system + "\n".join(classes))


BIOME_HEADER = """using System;\nusing Microsoft.Xna.Framework;\nusing Terraria;\nusing Terraria.ModLoader;\n\nnamespace XianXia.Content.Biomes;\n"""


def generate_enemies(existing: set[str]) -> None:
    classes = []
    for asset_id, (life, damage, defense, drop) in ENEMY_DATA.items():
        class_name = pascal(asset_id)
        if class_name in existing:
            continue
        copy_asset(asset_id, "base", class_name, CONTENT / "NPCs" / "Enemies" / "Generated")
        biome = BIOME_BY_ENEMY[asset_id]
        ai = "NPCAIStyleID.Fighter"
        flags = ""
        if any(k in asset_id for k in ["moth", "spirit", "hawk", "echo", "soul", "cloud"]):
            ai = "NPCAIStyleID.Bat"
            flags = "        NPC.noGravity = true;\n"
        classes.append(f"""
public class {class_name} : ModNPC
{{
    public override void SetDefaults()
    {{
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = {life};
        NPC.damage = {damage};
        NPC.defense = {defense};
        NPC.value = {max(60, life // 2)}f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = {ai};
        AIType = NPCID.CaveBat;
{flags}    }}

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {{
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.{biome}>() ? 0.18f : 0f;
    }}

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {{
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.{pascal(drop)}>(), 2, 1, 2));
    }}
}}
""")
    write(CONTENT / "NPCs" / "Enemies" / "Generated" / "GeneratedEnemies.cs", ENEMY_HEADER + "\n".join(classes))


ENEMY_HEADER = """using Terraria;\nusing Terraria.GameContent.ItemDropRules;\nusing Terraria.ID;\nusing Terraria.ModLoader;\n\nnamespace XianXia.Content.NPCs.Enemies.Generated;\n"""


def generate_bosses(existing: set[str]) -> None:
    classes = []
    for asset_id, (zh, en, life, damage, defense, summon, drop) in BOSS_DATA.items():
        class_name = pascal(asset_id)
        if class_name in existing:
            continue
        copy_asset(asset_id, "body", class_name, CONTENT / "NPCs" / "Bosses" / "Generated")
        copy_asset(asset_id, "boss_head", class_name, CONTENT / "NPCs" / "Bosses" / "Generated", "_Head_Boss")
        classes.append(f"""
[AutoloadBossHead]
public class {class_name} : ModNPC
{{
    public override void SetDefaults()
    {{
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = {life};
        NPC.damage = {damage};
        NPC.defense = {defense};
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }}

    public override void AI()
    {{
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {{
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {{
                NPC.EncourageDespawn(30);
                return;
            }}
        }}
        Vector2 desired = target.Center - NPC.Center;
        float speed = NPC.life < NPC.lifeMax / 2 ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);
    }}

    public override void OnKill() => DownedBossSystem.MarkDowned("{asset_id}");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {{
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.{pascal(drop)}>(), 1, 12, 24));
    }}
}}
""")
    write(CONTENT / "NPCs" / "Bosses" / "Generated" / "GeneratedBosses.cs", BOSS_HEADER + "\n".join(classes))


BOSS_HEADER = """using Microsoft.Xna.Framework;\nusing Terraria;\nusing Terraria.GameContent.ItemDropRules;\nusing Terraria.ID;\nusing Terraria.ModLoader;\nusing XianXia.Common.Systems;\n\nnamespace XianXia.Content.NPCs.Bosses.Generated;\n"""


def generate_summons(existing: set[str]) -> None:
    classes = []
    used: set[str] = set()
    for asset_id, (_, _, _, _, _, summon, _) in BOSS_DATA.items():
        class_name = f"Summon{pascal(summon)}"
        boss_class = pascal(asset_id)
        if class_name in used:
            class_name = f"{class_name}{boss_class}"
        used.add(class_name)
        if class_name in existing:
            continue
        copy_asset(summon, "item_icon", class_name, CONTENT / "Items" / "BossSummons" / "Generated")
        classes.append(f"""
public class {class_name} : ModItem
{{
    public override void SetDefaults()
    {{
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }}

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.{boss_class}>());

    public override bool? UseItem(Player player)
    {{
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.{boss_class}>());
        return true;
    }}
}}
""")
    write(CONTENT / "Items" / "BossSummons" / "Generated" / "GeneratedBossSummons.cs", SUMMON_HEADER + "\n".join(classes))


SUMMON_HEADER = """using Terraria;\nusing Terraria.ID;\nusing Terraria.ModLoader;\n\nnamespace XianXia.Content.Items.BossSummons.Generated;\n"""


def main() -> None:
    for folder in [
        CONTENT / "Items" / "Generated",
        CONTENT / "Items" / "BossSummons" / "Generated",
        CONTENT / "Projectiles" / "Generated",
        CONTENT / "Tiles" / "Generated",
        CONTENT / "NPCs" / "Enemies" / "Generated",
        CONTENT / "NPCs" / "Bosses" / "Generated",
    ]:
        if folder.exists():
            shutil.rmtree(folder)
    existing = existing_class_names()
    generate_materials(existing)
    existing = existing_class_names()
    generate_projectiles(existing)
    generate_tiles(existing)
    generate_biomes()
    existing = existing_class_names()
    generate_enemies(existing)
    generate_bosses(existing)
    existing = existing_class_names()
    generate_summons(existing)
    print("generated tModLoader content")


if __name__ == "__main__":
    main()
