from __future__ import annotations

import re
from collections import defaultdict
from pathlib import Path

from PIL import Image


ROOT = Path(".")
CONTENT = ROOT / "Content"
WIKI = ROOT / "Wiki"
OUT = WIKI / "Art_Gallery.md"
CONTACT_SHEET = ROOT / "Assets" / "Final" / "contact_sheet_v01.png"


BUFF_DESCRIPTIONS = {
    "AlchemyInsightBuff": "丹炉温养状态图标：丹炉附近的草木温养感，表现灵气恢复提升。",
    "ArchiveLockBuff": "归档锁状态图标：金色环锁与归档线，表现行动受限。",
    "ArtifactResonanceBuff": "器胚共鸣状态图标：法器共振与器胚灵光，表现灵气消耗降低。",
    "QiGatheringBuff": "聚气状态图标：青玉灵气旋涡，表现灵气恢复与消耗优化。",
    "SpiritualPressureDisorderBuff": "灵压紊乱状态图标：青紫裂纹与气旋失衡，表现防御和移动下降。",
    "SpringReturnBuff": "回春状态图标：春意丹药与青绿生机，表现生命恢复和灵气回补。",
    "SpringReturnRegenBuff": "回春再生状态图标：玉丹叶纹，表现生命与灵气恢复提升。",
    "StarAbyssCorrosionBuff": "星渊侵蚀状态图标：暗蓝星眼与紫色裂纹，表现防御下降和灵压增长。",
    "TribulationPressureBuff": "劫压临身状态图标：雷云威压与紫蓝电纹，表现天劫锁定。",
    "TribulationResistanceBuff": "抗劫状态图标：避雷玉符与稳定雷纹，表现伤害减免和灵压平复。",
}


STATION_DESCRIPTIONS = {
    "EarthClayFurnace": "48x48 小陶炉，暗红炉口，青色药烟硬边；用于早期炼丹。",
    "SimpleTalismanTable": "48x32 矮木案、纸张、朱砂碟，无可读文字；用于绘制基础符箓。",
    "AlchemyCauldron": "64x64 青铜丹炉，青木根缠绕，药绿火焰；用于炼制丹药。",
    "ArtifactForge": "64x48 黑铁台、迷你炉口、悬浮铭刻线；用于铸造飞剑、阵盘和法器。",
    "StarPatternCauldron": "64x64 暗蓝丹鼎，星晶嵌边，紫黑火焰；用于星渊高阶丹药。",
    "ThunderPatternForge": "64x48 云铁锻台，紫蓝雷纹，小电弧；用于雷系装备。",
    "SectTrialAltar": "64x48 白石台、插剑、玉牌槽；用于宗门职业装备。",
    "HeavenFireFurnace": "64x64 白玉炉，金色天火，破损法旨环绕；用于天道系装备。",
    "DaoSeveringAltar": "80x48 黑白断环石台，中间细小裂隙；用于终局路线装备。",
}


MATERIAL_DESCRIPTIONS = {
    "ShatteredJadeShell": "碎玉虫掉落物：玉壳断裂、深绿外轮廓，作为浅层灵脉材料。",
    "TornTalismanPaper": "符纸蝠掉落物：旧符纸碎片与朱砂痕，无可读文字。",
    "CinnabarPowder": "朱砂粉：符箓与丹药材料，使用朱砂红粉末轮廓。",
    "HerbDew": "药露：药园生机凝露，青绿水滴与草木高光。",
    "FurnaceCharcoal": "炉炭：沉炉矿脉余烬材料，黑灰炭块带暗红火点。",
    "ThunderPatternFeather": "雷纹羽：雷纹鹰羽毛，蓝紫雷纹沿羽轴延伸。",
    "SingingThunderStoneItem": "鸣雷石物品形态：小雷石与电弧，呼应雷泽云层生态物件。",
    "AbyssDust": "渊尘：星渊裂隙粉尘，暗蓝紫色星点颗粒。",
    "DarkBlueSpiritFluid": "暗蓝灵液：星渊流体材料，深蓝瓶滴与冷白高光。",
    "BrokenSwordIntent": "断剑残意：剑气碎片，银白裂刃与冷色尾光。",
    "TornScrollPage": "残卷页：旧宗门书页，无可读文字，墨色边缘。",
    "BrokenHeavenJade": "残天玉：坠天宫阙玉片，白玉裂缝与残金纹。",
    "BrokenDecreeItem": "破损法旨：残天法旨碎片，旧金边与墨灰封印。",
    "ColdMoonDust": "冷月尘：月骨天渊粉尘，月白骨粉与冷蓝微光。",
    "ArchiveRemnantLight": "归档残光：归档仙魂残留的环形光点。",
    "LowGradeSpiritCore": "低阶灵核：灵脉早期核心，青玉内光。",
    "SpiritVeinScale": "灵脉鳞：灵脉蠕虫鳞片，青玉鳞纹。",
}


def nice_name(stem: str) -> str:
    text = re.sub(r"([a-z0-9])([A-Z])", r"\1 \2", stem)
    text = text.replace("_", " ")
    return text


def rel(path: Path) -> str:
    return path.relative_to(WIKI).as_posix() if path.is_relative_to(WIKI) else ("../" + path.as_posix())


def classify(path: Path) -> tuple[str, str, int]:
    parts = path.parts
    stem = path.stem
    if "Buffs" in parts:
        return "Buff", "buff icon", 48
    if "NPCs" in parts and "Bosses" in parts:
        return "Boss", "animation sheet" if "_Head" not in stem else "boss head", 96
    if "NPCs" in parts and "Enemies" in parts:
        return "Enemy", "animation sheet", 80
    if "NPCs" in parts and "Town" in parts:
        return "Town NPC", "animation sheet" if "_Head" not in stem else "town head", 72
    if "Projectiles" in parts:
        return "Projectile", "projectile", 64
    if "Tiles" in parts and "Stations" in parts:
        return "Crafting Station Tile", "tile", 72
    if "Tiles" in parts:
        return "Tile / Object", "tile/object", 64
    if "Items" in parts and "Stations" in parts:
        return "Crafting Station Item", "item icon", 48
    if "Items" in parts and "Weapons" in parts:
        return "Equipment", "item icon", 64
    if "Items" in parts and "Accessories" in parts:
        return "Accessory", "item icon", 56
    if "Items" in parts and "BossSummons" in parts:
        return "Boss Summon", "item icon", 48
    if "Items" in parts and "Guides" in parts:
        return "Guide Item", "item icon", 48
    if "Items" in parts and "HandGenerated" in parts:
        return "Hand Generated Item", "item icon", 48
    if "Items" in parts and "Materials" in parts:
        return "Material", "item icon", 48
    if "Items" in parts and "Consumables" in parts:
        return "Consumable", "item icon", 48
    return "Other", "png", 48


def frames_for(category: str, kind: str, height: int) -> str:
    if kind != "animation sheet":
        return "-"
    if category == "Boss":
        return f"6 ({height // 6}px/frame)"
    if category == "Enemy":
        return f"6 ({height // 6}px/frame)"
    if category == "Town NPC":
        return f"4 ({height // 4}px/frame)"
    return "-"


def wiki_descriptions() -> dict[str, str]:
    descriptions: dict[str, str] = {}
    for path in WIKI.rglob("*.md"):
        try:
            text = path.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            continue
        current_id = None
        for line in text.splitlines():
            id_match = re.search(r"(?:ID|id)[：:]\s*`?([a-z0-9_]+)`?", line)
            if id_match:
                current_id = id_match.group(1)
            art_match = re.search(r"(?:美术|主体|图标|Prompt)[：:]\s*(.+)", line)
            if art_match and current_id and current_id not in descriptions:
                descriptions[current_id] = art_match.group(1).strip("。 ")
    return descriptions


def description_for(path: Path, category: str, kind: str) -> str:
    stem = path.stem
    normalized = stem.replace("Tile", "").replace("Projectile", "")
    snake = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", normalized).lower()
    wiki = wiki_descriptions()

    if stem in BUFF_DESCRIPTIONS:
        return BUFF_DESCRIPTIONS[stem]
    if normalized in STATION_DESCRIPTIONS:
        return STATION_DESCRIPTIONS[normalized]
    if stem in MATERIAL_DESCRIPTIONS:
        return MATERIAL_DESCRIPTIONS[stem]
    if snake in wiki:
        return wiki[snake]
    if category in {"Boss", "Enemy", "Town NPC"} and kind == "animation sheet":
        return "多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。"
    if category == "Buff":
        return "32x32 单核心符号状态图标，遵循无复杂背景、无文字、强轮廓的 Buff 规范。"
    if "Station" in category:
        return STATION_DESCRIPTIONS.get(normalized, "制作站资源：同步物品图标与 Tile 贴图，保持 Terraria 像素风、透明背景和清晰轮廓。")
    return f"{category} 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。"


def image_entries() -> list[dict[str, str]]:
    entries = []
    for path in sorted(CONTENT.rglob("*.png")):
        category, kind, preview_width = classify(path)
        with Image.open(path) as image:
            width, height = image.size
        entries.append(
            {
                "category": category,
                "kind": kind,
                "stem": path.stem,
                "name": nice_name(path.stem),
                "path": path.as_posix(),
                "size": f"{width}x{height}",
                "frames": frames_for(category, kind, height),
                "preview_width": str(preview_width),
                "description": description_for(path, category, kind),
            }
        )
    return entries


def write_gallery(entries: list[dict[str, str]]) -> None:
    grouped: dict[str, list[dict[str, str]]] = defaultdict(list)
    for entry in entries:
        grouped[entry["category"]].append(entry)

    order = [
        "Buff",
        "Boss",
        "Enemy",
        "Town NPC",
        "Crafting Station Tile",
        "Crafting Station Item",
        "Tile / Object",
        "Projectile",
        "Equipment",
        "Accessory",
        "Boss Summon",
        "Consumable",
        "Guide Item",
        "Material",
        "Hand Generated Item",
        "Other",
    ]

    parts = [
        "# 美术素材图库",
        "",
        "[返回首页](Home.md)",
        "",
        "本页由 `Tools/refresh_art_gallery.py` 从 `Content/**/*.png` 自动生成，覆盖当前 mod 实际加载的美术资源。描述优先引用 Wiki 中的美术/Prompt 文本；缺少专门条目的资源使用项目美术约束生成说明。",
        "",
        "## 总览图",
        "",
        '<img src="../Assets/Final/contact_sheet_v01.png" alt="all assets contact sheet" width="960">',
        "",
        "## 多帧规则",
        "",
        "- 敌怪与 Boss 主体贴图使用竖向 spritesheet；代码通过 `Main.npcFrameCount[Type]` 与 `FindFrame(int frameHeight)` 切帧。",
        "- 敌怪/Boss 每个主体 6 帧，城镇 NPC 每个主体 4 帧；头像、Buff、物品、Tile 与弹幕保持单帧。",
        "- 多帧由同一母版派生，统一轮廓、配色、透明边距和左上光源，避免帧间变成不同设计。",
        "",
    ]

    for category in order:
        rows = grouped.get(category)
        if not rows:
            continue
        parts.extend(
            [
                f"## {category}",
                "",
                "| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |",
                "| --- | --- | --- | --- | --- | --- | --- |",
            ]
        )
        for row in rows:
            img = f'<img src="../{row["path"]}" alt="{row["name"]}" width="{row["preview_width"]}">'
            parts.append(
                f"| {img} | {row['name']} | `{row['kind']}` | {row['size']} | {row['frames']} | {row['description']} | `{row['path']}` |"
            )
        parts.append("")

    OUT.write_text("\n".join(parts).rstrip() + "\n", encoding="utf-8", newline="\n")


def fit_thumb(path: Path, size: int = 96) -> Image.Image:
    with Image.open(path) as raw:
        image = raw.convert("RGBA")
    alpha = image.getchannel("A")
    bbox = alpha.getbbox()
    if bbox:
        image = image.crop(bbox)
    preview_padding = 18
    scale = min((size - preview_padding * 2) / image.width, (size - preview_padding * 2) / image.height)
    resized = image.resize(
        (max(1, int(image.width * scale)), max(1, int(image.height * scale))),
        Image.Resampling.NEAREST,
    )
    tile = Image.new("RGBA", (size, size), (18, 18, 18, 255))
    tile.alpha_composite(resized, ((size - resized.width) // 2, (size - resized.height) // 2))
    return tile


def write_contact_sheet(entries: list[dict[str, str]]) -> None:
    cols = 10
    thumb = 96
    gap = 8
    rows = (len(entries) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * thumb + (cols - 1) * gap, rows * thumb + (rows - 1) * gap), (12, 12, 12, 255))
    for index, entry in enumerate(entries):
        tile = fit_thumb(ROOT / entry["path"], thumb)
        sheet.alpha_composite(tile, ((index % cols) * (thumb + gap), (index // cols) * (thumb + gap)))
    CONTACT_SHEET.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(CONTACT_SHEET)


def main() -> None:
    entries = image_entries()
    write_contact_sheet(entries)
    write_gallery(entries)
    print({"gallery_entries": len(entries), "output": OUT.as_posix()})


if __name__ == "__main__":
    main()
