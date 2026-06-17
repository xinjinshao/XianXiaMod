from __future__ import annotations

import re
import os
from dataclasses import dataclass
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
FINAL = ROOT / "Assets" / "Final"
HOME = ROOT / "Wiki" / "Home.md"
GALLERY = ROOT / "Wiki" / "Art_Gallery.md"


@dataclass(frozen=True)
class Asset:
    asset_id: str
    output_type: str
    path: Path
    width: int
    height: int


def version(path: Path) -> int:
    match = re.search(r"v(\d+)$", path.stem)
    return int(match.group(1)) if match else 0


def output_type(asset_id: str, path: Path) -> str:
    stem = path.stem
    prefix = f"{asset_id}__"
    if stem.startswith(prefix):
        rest = stem[len(prefix) :]
        rest = re.sub(r"__?v\d+$", "", rest)
        return rest
    return "unknown"


def rel(from_file: Path, target: Path) -> str:
    return Path(os.path.relpath(target, from_file.parent)).as_posix()


def display_name(asset_id: str) -> str:
    return " ".join(part.capitalize() for part in asset_id.split("_"))


def collect_assets() -> list[Asset]:
    latest: dict[tuple[str, str], Path] = {}
    for path in FINAL.glob("*/*.png"):
        if path.parent.name in {"ContactSheets", "TilePreviews"}:
            continue
        asset_id = path.parent.name
        typ = output_type(asset_id, path)
        key = (asset_id, typ)
        current = latest.get(key)
        if current is None or version(path) > version(current):
            latest[key] = path

    assets: list[Asset] = []
    for (asset_id, typ), path in sorted(latest.items()):
        with Image.open(path) as image:
            assets.append(Asset(asset_id, typ, path, image.width, image.height))
    return assets


def group_name(asset: Asset) -> str:
    typ = asset.output_type
    boss_ids = {
        "garden_warden",
        "black_furnace_iron_golem",
        "tribulation_cloud_avatar",
        "abyssal_star_womb",
        "formless_sword_soul",
        "greenwood_medicine_king_echo",
        "heaven_tablet_guardian",
        "broken_heaven_inspector",
        "moonbone_immortal",
        "old_heaven_dao_core",
        "spirit_vein_wyrm",
        "thunder_marsh_jiao",
        "shattered_jade_wyrm_minion",
    }
    is_boss = asset.asset_id in boss_ids
    if typ in {"boss_head", "head", "body", "tail"} and (
        "wyrm" in asset.asset_id
        or "jiao" in asset.asset_id
        or is_boss
    ):
        return "Boss"
    if typ == "animation_sheet" and is_boss:
        return "Boss Animation Sheets"
    if typ == "base":
        return "Enemies"
    if typ == "animation_sheet":
        if asset.asset_id in {
            "herb_sect_apprentice",
            "wandering_artificer",
            "tribulation_observer",
            "archive_scroll_spirit",
            "fallen_heaven_messenger",
        }:
            return "NPC Animation Sheets"
        return "Enemy Animation Sheets"
    if typ in {"head", "body"}:
        return "Town NPCs"
    if typ == "buff_icon":
        return "Buffs"
    if typ == "station_icon":
        return "Crafting Station Icons"
    if typ == "object":
        return "Objects And Station Tiles"
    if typ == "projectile":
        return "Projectiles"
    if typ == "motion_sheet":
        return "Projectile Motion Sheets"
    if typ == "use_sheet":
        return "Use/Inspect Animation Sheets"
    if typ == "item_icon":
        if asset.asset_id in {
            "woodgrain_flying_sword",
            "cloudpiercer_flying_sword",
            "thunder_pattern_sword_case",
            "formless_sword_wheel",
            "moonbone_dharma_sword",
            "cinnabar_talisman_flame_item",
            "greenwood_array_plate",
            "thunder_talisman_array_plate",
            "broken_heaven_decree",
            "old_heaven_dao_scroll",
            "spiritwood_crossbow",
            "star_eclipse_arbalest",
            "qi_gathering_pendant",
            "spiritwood_charm",
            "furnace_heart_ring",
            "lightning_ward_jade",
            "star_abyss_eye",
            "nascent_soul_jade_box",
            "broken_heaven_crown_seal",
            "dao_severing_ring",
        }:
            return "Equipment"
        return "Items"
    if typ in {"tile", "wall", "ui"}:
        return "Tiles / UI"
    return "Other"


def width_for(asset: Asset) -> int:
    if asset.output_type in {"animation_sheet", "motion_sheet", "use_sheet"}:
        return 72
    if asset.output_type in {"tile", "wall"}:
        return 48
    if asset.output_type in {"boss_head", "body", "head", "tail"} and group_name(asset) == "Boss":
        return 96
    if asset.output_type in {"item_icon", "station_icon", "buff_icon"}:
        return 56
    return 72


def row_for(file: Path, asset: Asset) -> str:
    src = rel(file, asset.path)
    alt = f"{display_name(asset.asset_id)} {asset.output_type}"
    return (
        f'| <img src="{src}" alt="{alt}" width="{width_for(asset)}"> '
        f"| {display_name(asset.asset_id)} | `{asset.asset_id}` | "
        f"`{asset.output_type}` | {asset.width}x{asset.height} |"
    )


def refresh_home() -> None:
    text = HOME.read_text(encoding="utf-8")

    def replace(match: re.Match[str]) -> str:
        src = match.group(1)
        m = re.search(r"\.\./Assets/Final/([^/]+)/([^\"/]+)\.png", src)
        if not m:
            return match.group(0)
        asset_id = m.group(1)
        file_stem = m.group(2)
        typ = output_type(asset_id, Path(file_stem + ".png"))
        if typ == "unknown":
            return match.group(0)
        candidates = list((FINAL / asset_id).glob(f"{asset_id}__{typ}__v*.png"))
        candidates += list((FINAL / asset_id).glob(f"{asset_id}__{typ}_v*.png"))
        if not candidates:
            return match.group(0)
        latest = sorted(candidates, key=version, reverse=True)[0]
        return match.group(0).replace(src, rel(HOME, latest))

    text = re.sub(r'src="([^"]+)"', replace, text)
    HOME.write_text(text, encoding="utf-8", newline="\n")


def write_gallery(assets: list[Asset]) -> None:
    order = [
        "Boss",
        "Boss Animation Sheets",
        "Enemies",
        "Enemy Animation Sheets",
        "Town NPCs",
        "NPC Animation Sheets",
        "Buffs",
        "Items",
        "Equipment",
        "Use/Inspect Animation Sheets",
        "Projectiles",
        "Projectile Motion Sheets",
        "Crafting Station Icons",
        "Objects And Station Tiles",
        "Tiles / UI",
        "Other",
    ]
    grouped: dict[str, list[Asset]] = {name: [] for name in order}
    for asset in assets:
        grouped.setdefault(group_name(asset), []).append(asset)

    parts = [
        "# 美术素材图库",
        "",
        "[返回首页](Home.md)",
        "",
        "本页按 `Assets/Final` 中每个素材输出类型的最新版本自动生成，包含静态图标、运行时贴图、多帧动画预览、投射物运动预览、Buff、制作台与 UI。",
        "",
        "## 总览图",
        "",
        '<img src="../Assets/Final/contact_sheet_v01.png" alt="all assets contact sheet" width="760">',
        "",
    ]
    for name in order:
        entries = grouped.get(name, [])
        if not entries:
            continue
        parts.extend(
            [
                f"## {name}",
                "",
                "| 素材 | 名称 | ID | 类型 | 尺寸 |",
                "| --- | --- | --- | --- | --- |",
            ]
        )
        parts.extend(row_for(GALLERY, asset) for asset in sorted(entries, key=lambda a: (a.asset_id, a.output_type)))
        parts.append("")
    GALLERY.write_text("\n".join(parts).rstrip() + "\n", encoding="utf-8", newline="\n")


def main() -> None:
    assets = collect_assets()
    refresh_home()
    write_gallery(assets)
    print({"home": str(HOME.relative_to(ROOT)), "gallery": str(GALLERY.relative_to(ROOT)), "assets": len(assets)})


if __name__ == "__main__":
    main()
