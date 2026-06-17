from __future__ import annotations

import csv
import json
import re
from pathlib import Path
from typing import Iterable

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "Assets" / "Specs" / "art_asset_manifest.csv"
FINAL = ROOT / "Assets" / "Final"
REPORT = ROOT / "Docs" / "art_quality_redesign_report.json"

TILEABLE_OUTPUTS = {"tile", "wall"}
SOFT_ALPHA_ALLOWED = {"ui"}
PROJECTILE_SOFT_ALPHA_ALLOWED = {
    "minor_thunderbolt_proj",
    "decree_judgement_beam",
    "greenwood_array_field",
    "thunder_talisman_array",
}


def category(sheet: str) -> str:
    if sheet.startswith("bosses_"):
        return "boss"
    if sheet.startswith("enemies_"):
        return "enemy"
    if sheet.startswith("npcs_"):
        return "npc"
    if sheet.startswith("items_"):
        return "item"
    if sheet.startswith("equipment_"):
        return "equipment"
    if sheet.startswith("projectiles_"):
        return "projectile"
    if sheet.startswith("tiles_ui"):
        return "tile_ui"
    return "other"


def version(path: Path) -> int:
    match = re.search(r"v(\d+)", path.stem)
    return int(match.group(1)) if match else 0


def latest_asset(row: dict[str, str]) -> Path | None:
    base = FINAL / row["asset_id"]
    output = row["output_type"]
    candidates: list[Path] = []
    for pattern in (
        f"{row['asset_id']}__{output}__v*.png",
        f"{row['asset_id']}__{output}_v*.png",
        f"{row['asset_id']}__{output}.png",
    ):
        candidates.extend(base.glob(pattern))
    if not candidates:
        return None
    return sorted(set(candidates), key=version, reverse=True)[0]


def read_rows() -> list[dict[str, str]]:
    with MANIFEST.open(encoding="utf-8-sig", newline="") as handle:
        return list(csv.DictReader(handle))


def visible_pixels(image: Image.Image) -> list[tuple[int, int, int, int]]:
    return [pixel for pixel in image.convert("RGBA").getdata() if pixel[3] > 0]


def alpha_stats(image: Image.Image) -> tuple[float, float]:
    pixels = visible_pixels(image)
    if not pixels:
        return 0.0, 0.0
    partial = sum(1 for pixel in pixels if 0 < pixel[3] < 255)
    low = sum(1 for pixel in pixels if 0 < pixel[3] < 96)
    return round(partial / len(pixels) * 100, 2), round(low / len(pixels) * 100, 2)


def color_count(image: Image.Image) -> int:
    return len({pixel[:3] for pixel in visible_pixels(image)})


def fill_percent(image: Image.Image) -> float:
    pixels = visible_pixels(image)
    if not pixels:
        return 0.0
    return round(len(pixels) / (image.width * image.height) * 100, 2)


def quality_flags(row: dict[str, str], stats: dict[str, object]) -> list[str]:
    asset_id = row["asset_id"]
    output_type = row["output_type"]
    cat = category(row["sheet"])
    partial = float(stats["partial_alpha_pct"])
    low = float(stats["low_alpha_pct"])
    fill = float(stats["fill_pct"])
    colors = int(stats["colors"])

    if output_type in TILEABLE_OUTPUTS:
        return []

    flags: list[str] = []
    if output_type not in SOFT_ALPHA_ALLOWED and not (
        cat == "projectile" and asset_id in PROJECTILE_SOFT_ALPHA_ALLOWED
    ):
        if partial > 12:
            flags.append("soft_edge_alpha")
        if low > 4:
            flags.append("low_alpha_fringe")

    if cat == "projectile":
        if fill < 5 and asset_id not in {"spirit_bolt", "cloud_wisp_proj"}:
            flags.append("underfilled_projectile")
        if colors < 24 and asset_id not in {"spirit_bolt"}:
            flags.append("low_projectile_color_depth")
    elif output_type == "ui":
        if colors < 6 and fill > 0:
            flags.append("low_ui_color_depth")
        if fill <= 0:
            flags.append("empty_ui_asset")
    elif cat in {"item", "equipment", "npc", "enemy", "tile_ui"}:
        if colors < 24:
            flags.append("low_color_depth")
        if cat in {"item", "equipment"} and fill < 12:
            flags.append("underfilled_icon")

    return flags


def recommendation(flags: Iterable[str], cat: str) -> str:
    flags = set(flags)
    if not flags:
        return "keep"
    if "soft_edge_alpha" in flags or "low_alpha_fringe" in flags:
        return "regenerate_or_hard-edge_repaint"
    if "underfilled_projectile" in flags:
        return "redesign_projectile_silhouette"
    if cat in {"item", "equipment"}:
        return "regenerate_icon_with_stronger_silhouette"
    return "review_and_regenerate"


def build_report() -> dict[str, object]:
    summary: dict[str, dict[str, int]] = {}
    assets: list[dict[str, object]] = []

    for row in read_rows():
        cat = category(row["sheet"])
        summary.setdefault(cat, {"total": 0, "flagged": 0, "missing": 0})
        summary[cat]["total"] += 1

        path = latest_asset(row)
        if path is None:
            summary[cat]["missing"] += 1
            assets.append({**row, "category": cat, "flags": ["missing"], "recommendation": "regenerate"})
            continue

        with Image.open(path) as image:
            rgba = image.convert("RGBA")
            partial, low = alpha_stats(rgba)
            stats = {
                "category": cat,
                "asset_id": row["asset_id"],
                "output_type": row["output_type"],
                "size": f"{rgba.width}x{rgba.height}",
                "partial_alpha_pct": partial,
                "low_alpha_pct": low,
                "fill_pct": fill_percent(rgba),
                "colors": color_count(rgba),
                "path": path.relative_to(ROOT).as_posix(),
            }
        flags = quality_flags(row, stats)
        if flags:
            summary[cat]["flagged"] += 1
        assets.append({**stats, "flags": flags, "recommendation": recommendation(flags, cat)})

    return {"summary": summary, "assets": assets}


def main() -> None:
    report = build_report()
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(report["summary"], ensure_ascii=False, indent=2))
    print(f"wrote {REPORT.relative_to(ROOT).as_posix()}")


if __name__ == "__main__":
    main()
