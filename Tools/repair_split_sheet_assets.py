from __future__ import annotations

import csv
from pathlib import Path

from PIL import Image

import postprocess_art_assets as art


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "Assets/Specs/art_asset_manifest.csv"
GENERATED = ROOT / "Assets/Generated/BatchSheets"
FINAL = ROOT / "Assets/Final"

TARGETS = {
    ("moonbone_immortal", "body"),
    ("moonbone_immortal", "boss_head"),
    ("old_heaven_dao_core", "body"),
    ("old_heaven_dao_core", "boss_head"),
    ("formless_sword_wheel_proj", "projectile"),
    ("greenwood_array_field", "projectile"),
    ("thunder_talisman_array", "projectile"),
    ("decree_judgement_beam", "projectile"),
    ("star_eclipse_split_bolt", "projectile"),
}


def rows() -> list[art.AssetRow]:
    parsed: list[art.AssetRow] = []
    with MANIFEST.open(encoding="utf-8-sig", newline="") as f:
        for item in csv.DictReader(f):
            parsed.append(
                art.AssetRow(
                    sheet=item["sheet"],
                    asset_id=item["asset_id"],
                    output_type=item["output_type"],
                    col=int(item["col"]),
                    row=int(item["row"]),
                    cols=int(item["cols"]),
                    rows=int(item["rows"]),
                    width=int(item["width"]),
                    height=int(item["height"]),
                )
            )
    return parsed


def main() -> None:
    all_rows = rows()
    rows_by_sheet: dict[str, list[art.AssetRow]] = {}
    for row in all_rows:
        rows_by_sheet.setdefault(row.sheet, []).append(row)

    repaired: list[str] = []
    sheet_cache: dict[str, Image.Image] = {}
    component_cache: dict[str, list[tuple[int, int, int, int]]] = {}
    try:
        for row in all_rows:
            if (row.asset_id, row.output_type) not in TARGETS:
                continue

            if row.sheet not in sheet_cache:
                with Image.open(GENERATED / row.sheet) as raw:
                    sheet_cache[row.sheet] = art.remove_chroma_key(raw)
                component_cache[row.sheet] = art.component_boxes(sheet_cache[row.sheet])

            sheet = sheet_cache[row.sheet]
            crop_box = art.smart_cell_box(sheet, row, rows_by_sheet[row.sheet], component_cache[row.sheet])
            crop = sheet.crop(crop_box)
            final = art.fit_canvas(crop, (row.width, row.height), reserved_padding=8)
            out = FINAL / row.asset_id / f"{row.asset_id}__{row.output_type}__v01.png"
            out.parent.mkdir(parents=True, exist_ok=True)
            final.save(out)
            repaired.append(str(out.relative_to(ROOT)))
    finally:
        for sheet in sheet_cache.values():
            sheet.close()

    print({"repaired": len(repaired), "files": repaired})


if __name__ == "__main__":
    main()
