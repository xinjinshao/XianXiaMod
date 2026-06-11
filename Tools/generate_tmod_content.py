from __future__ import annotations

import csv
import json
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


ZH_NAMES = {
    "greenwood_root": "青木根",
    "furnace_slag_iron": "炉渣铁",
    "artifact_blank_shard": "器胚碎片",
    "tribulation_cloud_dew": "劫云露",
    "star_eclipse_crystal": "星蚀晶",
    "sect_trial_token": "宗门试炼令",
    "heaven_dao_fragment": "天道碎片",
    "moonbone": "月骨",
    "dao_severing_dust": "斩道尘",
    "spring_return_pill": "回春丹",
    "qi_condensing_pill": "凝气丹",
    "foundation_pill": "筑基丹",
    "tribulation_resisting_pill": "抗劫丹",
    "star_abyss_forbidden_talisman": "星渊禁符",
    "garden_broken_key": "守园残钥",
    "old_furnace_ember": "旧炉火种",
    "thunder_calling_jade": "引雷玉",
    "star_abyss_membrane": "星渊胎膜",
    "heaven_tablet_rubbing": "天碑拓片",
    "moonbone_ritual_talisman": "月骨祭符",
    "cloudpiercer_flying_sword": "破云飞剑",
    "thunder_pattern_sword_case": "雷纹剑匣",
    "formless_sword_wheel": "无相剑轮",
    "moonbone_dharma_sword": "月骨法剑",
    "cinnabar_talisman_flame_item": "朱砂符火",
    "greenwood_array_plate": "青木阵盘",
    "thunder_talisman_array_plate": "雷符阵盘",
    "broken_heaven_decree": "残天法令",
    "old_heaven_dao_scroll": "旧天道残卷",
    "star_eclipse_arbalest": "星蚀弩机",
    "qi_gathering_pendant": "聚气坠",
    "spiritwood_charm": "灵木护符",
    "furnace_heart_ring": "炉心戒",
    "lightning_ward_jade": "避雷玉佩",
    "star_abyss_eye": "星渊眼",
    "nascent_soul_jade_box": "元婴玉匣",
    "broken_heaven_crown_seal": "残天冠印",
    "dao_severing_ring": "斩道环",
    "herb_garden_vine_spirit": "药园藤灵",
    "miasma_flower_moth": "瘴花蛾",
    "furnace_ash_golem": "炉灰石傀",
    "iron_shard_spirit": "铁屑精",
    "tribulation_cloudling": "劫云灵",
    "thunder_pattern_hawk": "雷纹隼",
    "star_eclipsed_cultivator": "星蚀修士",
    "star_abyss_larva": "星渊幼体",
    "obsessed_sword_cultivator": "执剑疯修",
    "scripture_archive_echo": "经阁回声",
    "celestial_puppet": "天庭傀儡",
    "heaven_tablet_guard": "天碑守卫",
    "moonbone_cultivator": "月骨修士",
    "archived_immortal_soul": "封卷仙魂",
    "garden_warden": "药园守园人",
    "black_furnace_iron_golem": "玄炉铁傀",
    "tribulation_cloud_avatar": "劫云化身",
    "thunder_marsh_jiao": "雷泽蛟",
    "abyssal_star_womb": "星渊胎主",
    "formless_sword_soul": "无相剑魂",
    "greenwood_medicine_king_echo": "青木药王残影",
    "heaven_tablet_guardian": "天碑守尽",
    "broken_heaven_inspector": "残天监察使",
    "moonbone_immortal": "月骨仙君",
    "old_heaven_dao_core": "旧天道核心",
    "herb_sect_apprentice": "药宗学徒",
    "wandering_artificer": "游方炼器师",
    "tribulation_observer": "观劫客",
    "archive_scroll_spirit": "经阁卷灵",
    "fallen_heaven_messenger": "坠天使者",
}

EN_NAMES = {
    "greenwood_root": "Greenwood Root",
    "furnace_slag_iron": "Furnace Slag Iron",
    "artifact_blank_shard": "Artifact Blank Shard",
    "tribulation_cloud_dew": "Tribulation Cloud Dew",
    "star_eclipse_crystal": "Star Eclipse Crystal",
    "sect_trial_token": "Sect Trial Token",
    "heaven_dao_fragment": "Heaven Dao Fragment",
    "moonbone": "Moonbone",
    "dao_severing_dust": "Dao Severing Dust",
    "spring_return_pill": "Spring Return Pill",
    "qi_condensing_pill": "Qi Condensing Pill",
    "foundation_pill": "Foundation Pill",
    "tribulation_resisting_pill": "Tribulation Resisting Pill",
    "star_abyss_forbidden_talisman": "Star Abyss Forbidden Talisman",
    "garden_broken_key": "Garden Broken Key",
    "old_furnace_ember": "Old Furnace Ember",
    "thunder_calling_jade": "Thunder Calling Jade",
    "star_abyss_membrane": "Star Abyss Membrane",
    "heaven_tablet_rubbing": "Heaven Tablet Rubbing",
    "moonbone_ritual_talisman": "Moonbone Ritual Talisman",
    "cloudpiercer_flying_sword": "Cloudpiercer Flying Sword",
    "thunder_pattern_sword_case": "Thunder Pattern Sword Case",
    "formless_sword_wheel": "Formless Sword Wheel",
    "moonbone_dharma_sword": "Moonbone Dharma Sword",
    "cinnabar_talisman_flame_item": "Cinnabar Talisman Flame",
    "greenwood_array_plate": "Greenwood Array Plate",
    "thunder_talisman_array_plate": "Thunder Talisman Array Plate",
    "broken_heaven_decree": "Broken Heaven Decree",
    "old_heaven_dao_scroll": "Old Heaven Dao Scroll",
    "star_eclipse_arbalest": "Star Eclipse Arbalest",
    "qi_gathering_pendant": "Qi Gathering Pendant",
    "spiritwood_charm": "Spiritwood Charm",
    "furnace_heart_ring": "Furnace Heart Ring",
    "lightning_ward_jade": "Lightning Ward Jade",
    "star_abyss_eye": "Star Abyss Eye",
    "nascent_soul_jade_box": "Nascent Soul Jade Box",
    "broken_heaven_crown_seal": "Broken Heaven Crown Seal",
    "dao_severing_ring": "Dao Severing Ring",
    "herb_garden_vine_spirit": "Herb Garden Vine Spirit",
    "miasma_flower_moth": "Miasma Flower Moth",
    "furnace_ash_golem": "Furnace Ash Golem",
    "iron_shard_spirit": "Iron Shard Spirit",
    "tribulation_cloudling": "Tribulation Cloudling",
    "thunder_pattern_hawk": "Thunder Pattern Hawk",
    "star_eclipsed_cultivator": "Star Eclipsed Cultivator",
    "star_abyss_larva": "Star Abyss Larva",
    "obsessed_sword_cultivator": "Obsessed Sword Cultivator",
    "scripture_archive_echo": "Scripture Archive Echo",
    "celestial_puppet": "Celestial Puppet",
    "heaven_tablet_guard": "Heaven Tablet Guard",
    "moonbone_cultivator": "Moonbone Cultivator",
    "archived_immortal_soul": "Archived Immortal Soul",
    "garden_warden": "Garden Warden",
    "black_furnace_iron_golem": "Black Furnace Iron Golem",
    "tribulation_cloud_avatar": "Tribulation Cloud Avatar",
    "thunder_marsh_jiao": "Thunder Marsh Jiao",
    "abyssal_star_womb": "Abyssal Star Womb",
    "formless_sword_soul": "Formless Sword Soul",
    "greenwood_medicine_king_echo": "Greenwood Medicine King Echo",
    "heaven_tablet_guardian": "Heaven Tablet Guardian",
    "broken_heaven_inspector": "Broken Heaven Inspector",
    "moonbone_immortal": "Moonbone Immortal",
    "old_heaven_dao_core": "Old Heaven Dao Core",
    "herb_sect_apprentice": "Herb Sect Apprentice",
    "wandering_artificer": "Wandering Artificer",
    "tribulation_observer": "Tribulation Observer",
    "archive_scroll_spirit": "Archive Scroll Spirit",
    "fallen_heaven_messenger": "Fallen Heaven Messenger",
}

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

BOSS_STAGE_REQUIREMENTS = {
    "garden_warden": "QiAwakening",
    "black_furnace_iron_golem": "QiAwakening",
    "tribulation_cloud_avatar": "QiCondensation",
    "thunder_marsh_jiao": "Foundation",
    "abyssal_star_womb": "Foundation",
    "formless_sword_soul": "GoldenCore",
    "greenwood_medicine_king_echo": "GoldenCore",
    "heaven_tablet_guardian": "NascentSoul",
    "broken_heaven_inspector": "NascentSoul",
    "moonbone_immortal": "Tribulation",
    "old_heaven_dao_core": "DaoSevering",
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

TOWN_NPCS = {
    "herb_sect_apprentice": "HerbSectApprentice",
    "wandering_artificer": "WanderingArtificer",
    "tribulation_observer": "TribulationObserver",
    "archive_scroll_spirit": "ArchiveScrollSpirit",
    "fallen_heaven_messenger": "FallenHeavenMessenger",
}


def generate_materials(existing: set[str]) -> None:
    classes = []
    consumables = {
        "spring_return_pill",
        "qi_condensing_pill",
        "foundation_pill",
        "tribulation_resisting_pill",
        "star_abyss_forbidden_talisman",
        "star_eclipse_crystal",
        "old_heaven_dao_scroll",
        "heaven_dao_fragment",
        "moonbone",
        "dao_severing_dust",
    }
    accessories = {
        "qi_gathering_pendant",
        "spiritwood_charm",
        "furnace_heart_ring",
        "lightning_ward_jade",
        "star_abyss_eye",
        "nascent_soul_jade_box",
        "broken_heaven_crown_seal",
        "dao_severing_ring",
    }
    weapons = {
        "cloudpiercer_flying_sword": ("CloudpiercerSwordProjectile", 42, 12, "ItemUseStyleID.Swing"),
        "thunder_pattern_sword_case": ("ThunderSwordProjectile", 56, 18, "ItemUseStyleID.HoldUp"),
        "formless_sword_wheel": ("FormlessSwordWheelProjectile", 88, 28, "ItemUseStyleID.Swing"),
        "moonbone_dharma_sword": ("MoonboneShardProjectile", 145, 36, "ItemUseStyleID.Swing"),
        "cinnabar_talisman_flame_item": ("CinnabarTalismanFlame", 38, 14, "ItemUseStyleID.HoldUp"),
        "greenwood_array_plate": ("GreenwoodArrayField", 30, 20, "ItemUseStyleID.HoldUp"),
        "thunder_talisman_array_plate": ("ThunderTalismanArray", 62, 22, "ItemUseStyleID.HoldUp"),
        "broken_heaven_decree": ("DecreeJudgementBeam", 128, 40, "ItemUseStyleID.HoldUp"),
        "star_eclipse_arbalest": ("StarEclipseSplitBolt", 74, 20, "ItemUseStyleID.Shoot"),
    }
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
        if asset_id in consumables:
            stack = 30
        if asset_id in accessories or asset_id in weapons:
            stack = 1
        use_setup = ""
        use_item = ""
        accessory = ""
        recipe = ""
        if asset_id in consumables:
            use_setup = """
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;"""
        if asset_id == "spring_return_pill":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpringReturnBuff>(), 60 * 60);
        return true;
    }
"""
        elif asset_id == "qi_condensing_pill":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.QiCondensation);
        return true;
    }
"""
        elif asset_id == "foundation_pill":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.Foundation);
        return true;
    }
"""
        elif asset_id == "tribulation_resisting_pill":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), 60 * 90);
        return true;
    }
"""
        elif asset_id == "star_eclipse_crystal":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.GoldenCore);
        return true;
    }
"""
        elif asset_id == "old_heaven_dao_scroll":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.NascentSoul);
        return true;
    }
"""
        elif asset_id == "heaven_dao_fragment":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.SpiritSevering);
        return true;
    }
"""
        elif asset_id == "moonbone":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.Tribulation);
        return true;
    }
"""
        elif asset_id == "dao_severing_dust":
            use_item = """
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.DaoSevering);
        return true;
    }
"""
        elif asset_id == "star_abyss_forbidden_talisman":
            use_item = """
    public override bool? UseItem(Player player)
    {
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
        cultivation.RestoreSpiritualEnergy(80);
        cultivation.spiritPressure = Math.Clamp(cultivation.spiritPressure + 25, 0, 100);
        return true;
    }
"""
        if asset_id in accessories:
            use_setup += """
        Item.accessory = true;"""
            effects = {
                "qi_gathering_pendant": "player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.QiGatheringBuff>(), 2);",
                "spiritwood_charm": "player.lifeRegen += 2;",
                "furnace_heart_ring": "player.statDefense += 3;",
                "lightning_ward_jade": "player.endurance += 0.04f;",
                "star_abyss_eye": "player.GetDamage(DamageClass.Generic) += 0.06f;",
                "nascent_soul_jade_box": "player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().maxSpiritualEnergy += 30;",
                "broken_heaven_crown_seal": "player.GetDamage(DamageClass.Generic) += 0.1f; player.statDefense -= 4;",
                "dao_severing_ring": "player.GetDamage(DamageClass.Generic) += 0.14f;",
            }
            accessory = f"""
    public override void UpdateAccessory(Player player, bool hideVisual)
    {{
        {effects[asset_id]}
    }}
"""
        if asset_id in weapons:
            projectile, damage, energy, use_style = weapons[asset_id]
            use_setup += f"""
        Item.damage = {damage};
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = {use_style};
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.{projectile}>();
        Item.shootSpeed = 11f;"""
            use_item = f"""
    public override bool CanUseItem(Player player)
    {{
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy({energy});
    }}
"""
            ingredient = {
                "cloudpiercer_flying_sword": "GreenwoodRoot",
                "thunder_pattern_sword_case": "TribulationCloudDew",
                "formless_sword_wheel": "SectTrialToken",
                "moonbone_dharma_sword": "Moonbone",
                "cinnabar_talisman_flame_item": "FurnaceSlagIron",
                "greenwood_array_plate": "GreenwoodRoot",
                "thunder_talisman_array_plate": "TribulationCloudDew",
                "broken_heaven_decree": "HeavenDaoFragment",
                "star_eclipse_arbalest": "StarEclipseCrystal",
                "old_heaven_dao_scroll": "HeavenDaoFragment",
            }[asset_id]
            recipe = f"""
    public override void AddRecipes()
    {{
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.{ingredient}>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }}
"""
        if asset_id in accessories:
            ingredient = {
                "qi_gathering_pendant": "GreenwoodRoot",
                "spiritwood_charm": "GreenwoodRoot",
                "furnace_heart_ring": "FurnaceSlagIron",
                "lightning_ward_jade": "TribulationCloudDew",
                "star_abyss_eye": "StarEclipseCrystal",
                "nascent_soul_jade_box": "SectTrialToken",
                "broken_heaven_crown_seal": "HeavenDaoFragment",
                "dao_severing_ring": "DaoSeveringDust",
            }[asset_id]
            recipe = f"""
    public override void AddRecipes()
    {{
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.{ingredient}>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }}
"""
        if asset_id == "spring_return_pill":
            recipe = """
    public override void AddRecipes()
    {
        CreateRecipe(3)
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(2)
            .AddIngredient(ItemID.BottledWater)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())
            .Register();
    }
"""
        elif asset_id == "qi_condensing_pill":
            recipe = """
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(3)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(5)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())
            .Register();
    }
"""
        elif asset_id == "foundation_pill":
            recipe = """
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(4)
            .AddIngredient<global::XianXia.Content.Items.Generated.FurnaceSlagIron>(4)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(10)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())
            .Register();
    }
"""
        elif asset_id == "tribulation_resisting_pill":
            recipe = """
    public override void AddRecipes()
    {
        CreateRecipe(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.TribulationCloudDew>(3)
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(2)
            .AddIngredient(ItemID.BottledWater)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())
            .Register();
    }
"""
        elif asset_id == "star_abyss_forbidden_talisman":
            recipe = """
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(6)
            .AddIngredient<global::XianXia.Content.Items.Generated.StarAbyssMembrane>(2)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
"""
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
{use_setup}
    }}
{use_item}{accessory}{recipe}
}}
""")
    write(CONTENT / "Items" / "Generated" / "GeneratedItems.cs", ITEMS_HEADER + "\n".join(classes))


ITEMS_HEADER = """using System;\nusing Terraria;\nusing Terraria.ID;\nusing Terraria.ModLoader;\n\nnamespace XianXia.Content.Items.Generated;\n"""


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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
        if (phaseTwo && NPC.localAI[0] < 1f)
        {{
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, "灵压暴涨");
        }}
        if (finalPhase && NPC.localAI[0] < 2f)
        {{
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, "道痕失控");
        }}
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {{
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {{
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }}
        }}

        if (finalPhase && NPC.ai[1]++ > 180f)
        {{
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }}
    }}

    public override void OnKill() => DownedBossSystem.MarkDowned("{asset_id}");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {{
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.{pascal(drop)}>(), 1, 12, 24));
    }}
}}
""")
    write(CONTENT / "NPCs" / "Bosses" / "Generated" / "GeneratedBosses.cs", BOSS_HEADER + "\n".join(classes))


BOSS_HEADER = """using System;\nusing Microsoft.Xna.Framework;\nusing Terraria;\nusing Terraria.GameContent.ItemDropRules;\nusing Terraria.ID;\nusing Terraria.ModLoader;\nusing XianXia.Common.Systems;\n\nnamespace XianXia.Content.NPCs.Bosses.Generated;\n"""


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
        required_stage = BOSS_STAGE_REQUIREMENTS[asset_id]
        spirit_stone_cost = 10 + list(BOSS_DATA).index(asset_id) * 3
        crafting_tile = "TileID.WorkBenches" if required_stage in {"QiAwakening", "QiCondensation"} else "TileID.DemonAltar"
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

    public override bool CanUseItem(Player player)
    {{
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.{boss_class}>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.{required_stage};
    }}

    public override bool? UseItem(Player player)
    {{
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.{boss_class}>());
        return true;
    }}

    public override void AddRecipes()
    {{
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.{pascal(summon)}>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>({spirit_stone_cost})
            .AddTile({crafting_tile})
            .Register();
    }}
}}
""")
    write(CONTENT / "Items" / "BossSummons" / "Generated" / "GeneratedBossSummons.cs", SUMMON_HEADER + "\n".join(classes))


SUMMON_HEADER = """using Terraria;\nusing Terraria.ID;\nusing Terraria.ModLoader;\n\nnamespace XianXia.Content.Items.BossSummons.Generated;\n"""


def hjson_block(entries: dict[str, dict[str, str]], indent: str = "\t\t\t") -> str:
    lines = []
    for key in sorted(entries):
        lines.append(f"{indent}{key}: {{")
        for child_key, value in entries[key].items():
            lines.append(f"{indent}\t{child_key}: {json.dumps(value, ensure_ascii=False)}")
        lines.append(f"{indent}}}")
    return "\n".join(lines)


def generate_localization() -> None:
    item_zh: dict[str, dict[str, str]] = {}
    item_en: dict[str, dict[str, str]] = {}
    for asset_id in DISPLAY:
        class_name = pascal(asset_id)
        if asset_id in BOSS_DATA:
            continue
        if next((r["output_type"] for r in manifest_rows() if r["asset_id"] == asset_id), None) != "item_icon":
            continue
        item_zh[class_name] = {
            "DisplayName": ZH_NAMES.get(asset_id, class_name),
            "Tooltip": "仙侠模组内容，可用于修行、炼制、战斗或突破。",
        }
        item_en[class_name] = {
            "DisplayName": EN_NAMES.get(asset_id, class_name),
            "Tooltip": "XianxiaMod content used for cultivation, crafting, combat, or breakthroughs.",
        }
    for boss_id, (_, _, _, _, _, summon, _) in BOSS_DATA.items():
        class_name = f"Summon{pascal(summon)}"
        if class_name in item_zh:
            class_name = f"{class_name}{pascal(boss_id)}"
        item_zh[class_name] = {
            "DisplayName": f"{ZH_NAMES.get(summon, class_name)}",
            "Tooltip": f"召唤 {ZH_NAMES.get(boss_id, pascal(boss_id))}。",
        }
        item_en[class_name] = {
            "DisplayName": f"{EN_NAMES.get(summon, class_name)}",
            "Tooltip": f"Summons {EN_NAMES.get(boss_id, pascal(boss_id))}.",
        }

    npc_zh = {pascal(asset_id): {"DisplayName": ZH_NAMES.get(asset_id, pascal(asset_id))} for asset_id in ENEMY_DATA | BOSS_DATA}
    npc_en = {pascal(asset_id): {"DisplayName": EN_NAMES.get(asset_id, pascal(asset_id))} for asset_id in ENEMY_DATA | BOSS_DATA}
    for asset_id, class_name in TOWN_NPCS.items():
        npc_zh[class_name] = {"DisplayName": ZH_NAMES[asset_id]}
        npc_en[class_name] = {"DisplayName": EN_NAMES[asset_id]}

    tile_names = {
        "GreenwoodSoilTile": ("青木土", "Greenwood Soil"),
        "SpiritHerbTile": ("灵草", "Spirit Herb"),
        "FurnaceSlagTile": ("炉渣石", "Furnace Slag"),
        "BlackFurnaceWall": ("玄炉墙", "Black Furnace Wall"),
        "ThunderCloudTile": ("雷云块", "Thunder Cloud"),
        "StarAbyssCrystalTile": ("星渊晶岩", "Star Abyss Crystal"),
        "SectRuinBrickTile": ("宗门石砖", "Sect Ruin Brick"),
        "FallenHeavenJadeTile": ("坠天玉砖", "Fallen Heaven Jade"),
        "MoonboneTile": ("月骨岩", "Moonbone Rock"),
    }
    tile_zh = {key: {"MapEntry": value[0]} for key, value in tile_names.items()}
    tile_en = {key: {"MapEntry": value[1]} for key, value in tile_names.items()}

    biome_names = {
        "GreenwoodHerbGardenBiome": ("青木药园", "Greenwood Herb Garden"),
        "SunkenFurnaceVeinBiome": ("沉炉矿脉", "Sunken Furnace Vein"),
        "ThunderMarshCloudsBiome": ("雷泽云层", "Thunder Marsh Clouds"),
        "StarAbyssRiftBiome": ("星渊裂隙", "Star Abyss Rift"),
        "TenThousandSectsRuinsBiome": ("万宗遗址", "Ten Thousand Sects Ruins"),
        "FallenHeavenPalaceBiome": ("坠天宫阙", "Fallen Heaven Palace"),
        "MoonboneAbyssBiome": ("月骨深渊", "Moonbone Abyss"),
    }
    biome_zh = {key: {"DisplayName": value[0]} for key, value in biome_names.items()}
    biome_en = {key: {"DisplayName": value[1]} for key, value in biome_names.items()}

    buff_zh = {
        "QiGatheringBuff": {"DisplayName": "聚气", "Description": "灵气恢复提升，灵气消耗小幅降低。"},
        "SpringReturnBuff": {"DisplayName": "回春", "Description": "生命恢复提升，并缓慢回补灵气。"},
        "TribulationPressureBuff": {"DisplayName": "劫压临身", "Description": "天劫锁定了你，造成与承受伤害都会变得更激烈。"},
        "TribulationResistanceBuff": {"DisplayName": "抗劫", "Description": "降低受到的伤害，并平复灵压。"},
        "SpiritualPressureDisorderBuff": {"DisplayName": "灵压紊乱", "Description": "防御和移动速度降低。"},
    }
    buff_en = {
        "QiGatheringBuff": {"DisplayName": "Qi Gathering", "Description": "Increases spiritual energy recovery and slightly lowers spiritual energy costs."},
        "SpringReturnBuff": {"DisplayName": "Spring Return", "Description": "Improves life regeneration and slowly restores spiritual energy."},
        "TribulationPressureBuff": {"DisplayName": "Tribulation Pressure", "Description": "The tribulation has marked you, making damage dealt and received more volatile."},
        "TribulationResistanceBuff": {"DisplayName": "Tribulation Resistance", "Description": "Reduces incoming damage and calms spirit pressure."},
        "SpiritualPressureDisorderBuff": {"DisplayName": "Spiritual Pressure Disorder", "Description": "Reduces defense and movement speed."},
    }

    template = """Mods: {{
\tXianXia: {{
\t\tItems: {{
{items}
\t\t}}
\t\tNPCs: {{
{npcs}
\t\t}}
\t\tTiles: {{
{tiles}
\t\t}}
\t\tBiomes: {{
{biomes}
\t\t}}
\t\tBuffs: {{
{buffs}
\t\t}}
\t}}
}}
"""
    write(ROOT / "Localization" / "generated.zh-Hans.hjson", template.format(
        items=hjson_block(item_zh),
        npcs=hjson_block(npc_zh),
        tiles=hjson_block(tile_zh),
        biomes=hjson_block(biome_zh),
        buffs=hjson_block(buff_zh),
    ))
    write(ROOT / "Localization" / "generated.en-US.hjson", template.format(
        items=hjson_block(item_en),
        npcs=hjson_block(npc_en),
        tiles=hjson_block(tile_en),
        biomes=hjson_block(biome_en),
        buffs=hjson_block(buff_en),
    ))


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
    generate_localization()
    print("generated tModLoader content")


if __name__ == "__main__":
    main()
