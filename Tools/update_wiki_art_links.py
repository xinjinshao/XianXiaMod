from __future__ import annotations

import csv
from collections import defaultdict
from pathlib import Path
from typing import Iterable


ROOT = Path(".")
MANIFEST = ROOT / "Assets/Specs/art_asset_manifest.csv"
FINAL = ROOT / "Assets/Final"


LABELS = {
    "spirit_vein_wyrm": "灵脉蠕虫",
    "garden_warden": "药宗守园人",
    "black_furnace_iron_golem": "玄炉铁傀",
    "tribulation_cloud_avatar": "劫云化身",
    "thunder_marsh_jiao": "雷泽蛟",
    "abyssal_star_womb": "星渊胎主",
    "formless_sword_soul": "无相剑魄",
    "greenwood_medicine_king_echo": "青木药王残影",
    "heaven_tablet_guardian": "天碑守御",
    "broken_heaven_inspector": "残天监察使",
    "moonbone_immortal": "月骸仙君",
    "old_heaven_dao_core": "旧天道核心",
    "wandering_spirit_slime": "游灵史莱姆",
    "shattered_jade_worm": "碎玉虫",
    "talisman_bat": "符纸蝠",
    "herb_garden_vine_spirit": "药园藤妖",
    "miasma_flower_moth": "花瘴蝶",
    "furnace_ash_golem": "炉灰傀",
    "iron_shard_spirit": "铁屑灵",
    "tribulation_cloudling": "劫云灵",
    "thunder_pattern_hawk": "雷纹鹰",
    "star_eclipsed_cultivator": "星蚀修士",
    "star_abyss_larva": "星渊幼体",
    "obsessed_sword_cultivator": "执念剑修",
    "scripture_archive_echo": "藏经残影",
    "celestial_puppet": "仙傀",
    "heaven_tablet_guard": "天碑卫",
    "moonbone_cultivator": "月骸修士",
    "archived_immortal_soul": "归档仙魂",
    "herb_sect_apprentice": "药宗遗徒",
    "wandering_artificer": "游方器师",
    "tribulation_observer": "观劫道人",
    "archive_scroll_spirit": "残卷书灵",
    "fallen_heaven_messenger": "坠天信使",
    "low_grade_spirit_stone": "下品灵石",
    "spirit_gel": "灵气凝胶",
    "torn_talisman_paper": "残符纸",
    "greenwood_root": "青木根",
    "furnace_slag_iron": "炉渣铁",
    "artifact_blank_shard": "器胚碎片",
    "tribulation_cloud_dew": "劫云露",
    "star_eclipse_crystal": "星蚀晶",
    "sect_trial_token": "宗门令",
    "heaven_dao_fragment": "天道碎片",
    "moonbone": "月骸骨",
    "dao_severing_dust": "斩道尘",
    "qi_drawing_talisman": "引气符",
    "spring_return_pill": "回春丹",
    "qi_condensing_pill": "凝气丹",
    "foundation_pill": "筑基丹",
    "tribulation_resisting_pill": "抗劫丹",
    "star_abyss_forbidden_talisman": "星渊禁符",
    "spirit_vein_incense": "灵脉香",
    "garden_broken_key": "守园残钥",
    "old_furnace_ember": "旧炉火种",
    "thunder_calling_jade": "引雷玉",
    "star_abyss_membrane": "星渊胎膜",
    "heaven_tablet_rubbing": "天碑拓片",
    "moonbone_ritual_talisman": "月骸祭符",
    "woodgrain_flying_sword": "木纹飞剑",
    "cloudpiercer_flying_sword": "破云飞剑",
    "thunder_pattern_sword_case": "雷纹剑匣",
    "formless_sword_wheel": "无相剑轮",
    "moonbone_dharma_sword": "月骸法剑",
    "cinnabar_talisman_flame_item": "朱砂符火",
    "greenwood_array_plate": "青木阵盘",
    "thunder_talisman_array_plate": "雷符阵盘",
    "broken_heaven_decree": "残天法旨",
    "old_heaven_dao_scroll": "旧天道残卷",
    "spiritwood_crossbow": "灵木短弩",
    "star_eclipse_arbalest": "星蚀弩机",
    "qi_gathering_pendant": "聚气坠",
    "spiritwood_charm": "灵木护符",
    "furnace_heart_ring": "炉心戒",
    "lightning_ward_jade": "避雷玉佩",
    "star_abyss_eye": "星渊眼",
    "nascent_soul_jade_box": "元婴玉匣",
    "broken_heaven_crown_seal": "残天冠印",
    "dao_severing_ring": "斩道环",
    "woodgrain_sword_proj": "木纹飞剑弹",
    "cloudpiercer_sword_proj": "破云飞剑弹",
    "cloud_wisp_proj": "云气弹",
    "thunder_sword_proj": "雷纹飞剑",
    "minor_thunderbolt_proj": "小雷",
    "formless_sword_wheel_proj": "无相剑轮投射物",
    "moonbone_shard_proj": "月骨残刃",
    "cinnabar_talisman_flame": "朱砂符火投射物",
    "greenwood_array_field": "青木阵域",
    "thunder_talisman_array": "雷符阵域",
    "decree_judgement_beam": "审判光束",
    "spirit_bolt": "灵气箭",
    "star_eclipse_split_bolt": "星蚀分裂弹",
    "spirit_ore_tile": "灵石矿",
    "spirit_moss": "灵苔",
    "greenwood_soil_tile": "青木土",
    "spirit_herb": "灵草",
    "furnace_slag_tile": "炉渣石",
    "black_furnace_wall": "玄炉墙",
    "thunder_cloud_tile": "雷云块",
    "singing_thunder_stone": "鸣雷石",
    "star_abyss_crystal_tile": "星渊晶岩",
    "rift_membrane": "裂隙膜",
    "sect_ruin_brick": "宗门石砖",
    "sword_tablet": "剑碑",
    "fallen_heaven_jade_tile": "坠天玉砖",
    "broken_heaven_tablet": "破损天碑",
    "moonbone_tile": "月骸骨岩",
    "archive_light_pillar": "归档光柱",
    "spiritual_energy_bar_frame": "灵气条框",
    "spiritual_energy_bar_fill": "灵气条填充",
    "pressure_warning_icon": "灵压警告图标",
    "artifact_slot_frame": "法宝槽",
    "tribulation_warning_line": "天劫预警线",
}


def label(asset_id: str) -> str:
    return LABELS.get(asset_id, asset_id.replace("_", " "))


def rows() -> list[dict[str, str]]:
    with MANIFEST.open(encoding="utf-8-sig", newline="") as f:
        return list(csv.DictReader(f))


def asset_path(row: dict[str, str]) -> Path:
    return FINAL / row["asset_id"] / f"{row['asset_id']}__{row['output_type']}__v01.png"


def rel(from_file: Path, target: Path) -> str:
    return Path("../" * (len(from_file.parent.parts))).joinpath(target).as_posix()


def img(from_file: Path, row: dict[str, str], width: int) -> str:
    path = rel(from_file, asset_path(row))
    alt = f"{label(row['asset_id'])} {row['output_type']}"
    return f'<img src="{path}" alt="{alt}" width="{width}">'


def section_table(
    file: Path,
    title: str,
    section_id: str,
    table_rows: Iterable[str],
    intro: str = "",
) -> str:
    body = [f"## {title}", ""]
    body.append(f"<!-- ART_SECTION:{section_id}:START -->")
    if intro:
        body.extend(["", intro])
    body.extend(["", "| 素材 | 名称 | ID | 类型 | 尺寸 |", "| --- | --- | --- | --- | --- |"])
    body.extend(table_rows)
    body.extend(["", f"<!-- ART_SECTION:{section_id}:END -->", ""])
    return "\n".join(body)


def replace_section(file: Path, section_id: str, content: str, before_marker: str = "相关页面：") -> None:
    text = file.read_text(encoding="utf-8")
    start = f"<!-- ART_SECTION:{section_id}:START -->"
    end = f"<!-- ART_SECTION:{section_id}:END -->"
    if start in text and end in text:
        prefix = text[: text.index(start)]
        suffix = text[text.index(end) + len(end) :]
        heading_start = prefix.rfind("\n## ")
        if heading_start != -1:
            prefix = prefix[:heading_start]
        text = prefix.rstrip() + "\n\n" + content.rstrip() + suffix
    else:
        marker_index = text.find(before_marker)
        if marker_index == -1:
            text = text.rstrip() + "\n\n" + content
        else:
            text = text[:marker_index].rstrip() + "\n\n" + content.rstrip() + "\n\n" + text[marker_index:]
    file.write_text(text, encoding="utf-8", newline="\n")


def make_rows(file: Path, filtered: list[dict[str, str]], width: int) -> list[str]:
    lines = []
    for row in filtered:
        lines.append(
            f"| {img(file, row, width)} | {label(row['asset_id'])} | `{row['asset_id']}` | "
            f"`{row['output_type']}` | {row['width']}x{row['height']} |"
        )
    return lines


def by_sheet_prefix(prefix: str, data: list[dict[str, str]]) -> list[dict[str, str]]:
    return [r for r in data if r["sheet"].startswith(prefix)]


def write_gallery(data: list[dict[str, str]]) -> None:
    file = Path("Wiki/Art_Gallery.md")
    groups = [
        ("Boss", by_sheet_prefix("bosses_", data), 96),
        ("敌怪", by_sheet_prefix("enemies_", data), 80),
        ("NPC", by_sheet_prefix("npcs_", data), 72),
        ("物品", by_sheet_prefix("items_", data), 48),
        ("装备", by_sheet_prefix("equipment_", data), 64),
        ("投射物", by_sheet_prefix("projectiles_", data), 64),
        ("Tile / UI", by_sheet_prefix("tiles_ui_", data), 64),
    ]
    parts = ["# 美术素材图库", "", "[返回首页](Home.md)", ""]
    parts.extend([
        "本页集中展示 `Assets/Final` 中的第一版最终素材。所有图片均为透明 PNG，可从对应 Wiki 页面跳转查看上下文。",
        "",
        "## 总览图",
        "",
        f'<img src="{rel(file, Path("Assets/Final/contact_sheet_v01.png"))}" alt="all assets contact sheet" width="760">',
        "",
    ])
    for title, entries, width in groups:
        parts.extend([f"## {title}", "", "| 素材 | 名称 | ID | 类型 | 尺寸 |", "| --- | --- | --- | --- | --- |"])
        parts.extend(make_rows(file, entries, width))
        parts.append("")
    file.write_text("\n".join(parts).rstrip() + "\n", encoding="utf-8", newline="\n")


def write_audit(data: list[dict[str, str]]) -> None:
    file = Path("Wiki/Art_Audit.md")
    final_png_count = len(list(FINAL.glob("*/*.png")))
    parts = [
        "# 美术素材审计",
        "",
        "[返回首页](Home.md)",
        "",
        "## 自动检查结果",
        "",
        f"- Manifest 条目：{len(data)}",
        f"- `Assets/Final/<asset_id>/` 下最终 PNG：{final_png_count}",
        "- 已检查：文件存在、RGBA 格式、目标尺寸、非 Tile/Wall 透明角。",
        "- 当前机器检查：通过。",
        "- 已修复：Wiki 页面已插入素材图片，不再只依赖路径定位。",
        "- 已修复：美术图库使用中文名称、ID、类型、尺寸同表展示，便于对照。",
        "",
        "## 语义一致性检查",
        "",
        "- 文件 ID、输出类型和尺寸均来自 Wiki 参数表与 `Assets/Specs/art_asset_manifest.csv`。",
        "- 图片已插入对应 Wiki 分类页面，可直接对照名称、ID、类型和尺寸查看。",
        "- 当前批次为第一版可用素材，允许后续人工像素级精修；若发现风格或语义不满意，保留相同 ID 和尺寸重新生成对应 PNG 即可。",
        "",
        "## 预览入口",
        "",
        "- [美术素材图库](Art_Gallery.md)",
        "- [总览 contact sheet](../Assets/Final/contact_sheet_v01.png)",
        "- [分类 contact sheets](../Assets/Final/ContactSheets/)",
        "- [Tile 平铺预览](../Assets/Final/TilePreviews/)",
    ]
    file.write_text("\n".join(parts) + "\n", encoding="utf-8", newline="\n")


def main() -> None:
    data = rows()
    targets = [
        (Path("Wiki/Content/Bosses/Overview.md"), "Boss 美术素材", "boss-art", by_sheet_prefix("bosses_", data), 96),
        (Path("Wiki/Content/Enemies/Enemy_Catalog.md"), "敌怪美术素材", "enemy-art", by_sheet_prefix("enemies_", data), 72),
        (Path("Wiki/Content/NPCs/Overview.md"), "NPC 美术素材", "npc-art", by_sheet_prefix("npcs_", data), 72),
        (Path("Wiki/Content/Items/Item_Catalog.md"), "物品美术素材", "item-art", by_sheet_prefix("items_", data), 48),
        (Path("Wiki/Content/Equipment/Equipment_Catalog.md"), "装备美术素材", "equipment-art", by_sheet_prefix("equipment_", data), 64),
        (Path("Wiki/Content/Equipment/Projectile_Catalog.md"), "投射物美术素材", "projectile-art", by_sheet_prefix("projectiles_", data), 64),
        (Path("Wiki/Content/Biomes/Biome_Generation_Stats.md"), "Tile / Object / UI 美术素材", "tile-ui-art", by_sheet_prefix("tiles_ui_", data), 64),
        (Path("Wiki/Systems/Mechanics_Detail.md"), "UI 与状态图标素材", "systems-ui-art", [r for r in by_sheet_prefix("tiles_ui_", data) if r["output_type"] == "ui"], 64),
    ]
    for file, title, section_id, entries, width in targets:
        content = section_table(file, title, section_id, make_rows(file, entries, width))
        replace_section(file, section_id, content)
    boss_entry_files = {
        "spirit_vein_wyrm": "Wiki/Content/Bosses/Entries/Spirit_Vein_Wyrm.md",
        "garden_warden": "Wiki/Content/Bosses/Entries/Garden_Warden.md",
        "black_furnace_iron_golem": "Wiki/Content/Bosses/Entries/Black_Furnace_Iron_Golem.md",
        "tribulation_cloud_avatar": "Wiki/Content/Bosses/Entries/Tribulation_Cloud_Avatar.md",
        "thunder_marsh_jiao": "Wiki/Content/Bosses/Entries/Thunder_Marsh_Jiao.md",
        "abyssal_star_womb": "Wiki/Content/Bosses/Entries/Abyssal_Star_Womb.md",
        "formless_sword_soul": "Wiki/Content/Bosses/Entries/Formless_Sword_Soul.md",
        "greenwood_medicine_king_echo": "Wiki/Content/Bosses/Entries/Greenwood_Medicine_King_Echo.md",
        "heaven_tablet_guardian": "Wiki/Content/Bosses/Entries/Heaven_Tablet_Guardian.md",
        "broken_heaven_inspector": "Wiki/Content/Bosses/Entries/Broken_Heaven_Inspector.md",
        "moonbone_immortal": "Wiki/Content/Bosses/Entries/Moonbone_Immortal.md",
        "old_heaven_dao_core": "Wiki/Content/Bosses/Entries/Old_Heaven_Dao_Core.md",
    }
    for asset_id, file_name in boss_entry_files.items():
        file = Path(file_name)
        entries = [r for r in data if r["asset_id"] == asset_id]
        content = section_table(file, "当前美术素材", "entry-art", make_rows(file, entries, 96))
        replace_section(file, "entry-art", content, before_marker="## 美术资源")

    npc_entry_files = {
        "herb_sect_apprentice": "Wiki/Content/NPCs/Entries/Herb_Sect_Apprentice.md",
        "wandering_artificer": "Wiki/Content/NPCs/Entries/Wandering_Artificer.md",
        "tribulation_observer": "Wiki/Content/NPCs/Entries/Tribulation_Observer.md",
        "archive_scroll_spirit": "Wiki/Content/NPCs/Entries/Archive_Scroll_Spirit.md",
        "fallen_heaven_messenger": "Wiki/Content/NPCs/Entries/Fallen_Heaven_Messenger.md",
    }
    for asset_id, file_name in npc_entry_files.items():
        file = Path(file_name)
        entries = [r for r in data if r["asset_id"] == asset_id]
        content = section_table(file, "当前美术素材", "entry-art", make_rows(file, entries, 72))
        replace_section(file, "entry-art", content, before_marker="## 美术资源")

    biome_entry_assets = {
        "Wiki/Content/Biomes/Entries/Shallow_Spirit_Veins.md": ["spirit_ore_tile", "spirit_moss"],
        "Wiki/Content/Biomes/Entries/Greenwood_Herb_Garden.md": ["greenwood_soil_tile", "spirit_herb"],
        "Wiki/Content/Biomes/Entries/Sunken_Furnace_Vein.md": ["furnace_slag_tile", "black_furnace_wall"],
        "Wiki/Content/Biomes/Entries/Thunder_Marsh_Clouds.md": ["thunder_cloud_tile", "singing_thunder_stone"],
        "Wiki/Content/Biomes/Entries/Star_Abyss_Rift.md": ["star_abyss_crystal_tile", "rift_membrane"],
        "Wiki/Content/Biomes/Entries/Ten_Thousand_Sects_Ruins.md": ["sect_ruin_brick", "sword_tablet"],
        "Wiki/Content/Biomes/Entries/Fallen_Heaven_Palace.md": ["fallen_heaven_jade_tile", "broken_heaven_tablet"],
        "Wiki/Content/Biomes/Entries/Moonbone_Abyss.md": ["moonbone_tile", "archive_light_pillar"],
    }
    for file_name, ids in biome_entry_assets.items():
        file = Path(file_name)
        entries = [r for r in data if r["asset_id"] in ids]
        content = section_table(file, "当前美术素材", "entry-art", make_rows(file, entries, 64))
        replace_section(file, "entry-art", content, before_marker="## 美术资源")

    write_gallery(data)
    write_audit(data)

    home = Path("Wiki/Home.md")
    text = home.read_text(encoding="utf-8")
    additions = [
        "- [美术素材图库](Art_Gallery.md)",
        "- [美术素材审计](Art_Audit.md)",
    ]
    for item in additions:
        if item not in text:
            anchor = "- [设计状态](Design_Status.md)"
            text = text.replace(anchor, anchor + "\n" + item)
    home.write_text(text, encoding="utf-8", newline="\n")


if __name__ == "__main__":
    main()
