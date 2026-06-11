from __future__ import annotations

import argparse
import csv
import json
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

from PIL import Image, ImageChops


MAGENTA = (255, 0, 255)


@dataclass(frozen=True)
class AssetRow:
    sheet: str
    asset_id: str
    output_type: str
    col: int
    row: int
    cols: int
    rows: int
    width: int
    height: int

    @property
    def output_name(self) -> str:
        return f"{self.asset_id}__{self.output_type}__v01.png"


def load_manifest(path: Path) -> list[AssetRow]:
    rows: list[AssetRow] = []
    with path.open("r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        for item in reader:
            rows.append(
                AssetRow(
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
    return rows


def remove_chroma_key(image: Image.Image, threshold: int = 36) -> Image.Image:
    rgba = image.convert("RGBA")
    pixels = rgba.load()
    width, height = rgba.size
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            distance_match = abs(r - MAGENTA[0]) + abs(g - MAGENTA[1]) + abs(b - MAGENTA[2]) <= threshold
            saturated_magenta = r > 150 and b > 150 and g < 120 and abs(r - b) < 100
            fringe_magenta = r > 120 and b > 120 and g < min(r, b) * 0.45 and abs(r - b) < 130
            if distance_match or saturated_magenta or fringe_magenta:
                pixels[x, y] = (r, g, b, 0)
    return rgba


def trim_alpha(image: Image.Image, padding: int = 2) -> Image.Image:
    rgba = image.convert("RGBA")
    alpha = rgba.getchannel("A")
    bbox = alpha.getbbox()
    if bbox is None:
        return Image.new("RGBA", (1, 1), (0, 0, 0, 0))
    left, top, right, bottom = bbox
    left = max(0, left - padding)
    top = max(0, top - padding)
    right = min(rgba.width, right + padding)
    bottom = min(rgba.height, bottom + padding)
    return rgba.crop((left, top, right, bottom))


def fit_canvas(image: Image.Image, size: tuple[int, int], reserved_padding: int = 0) -> Image.Image:
    target_w, target_h = size
    trimmed = trim_alpha(image)
    usable_w = max(1, target_w - reserved_padding * 2)
    usable_h = max(1, target_h - reserved_padding * 2)
    if trimmed.width > usable_w or trimmed.height > usable_h:
        scale = min(usable_w / trimmed.width, usable_h / trimmed.height)
        new_size = (max(1, int(trimmed.width * scale)), max(1, int(trimmed.height * scale)))
        trimmed = trimmed.resize(new_size, Image.Resampling.NEAREST)
    canvas = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
    x = (target_w - trimmed.width) // 2
    y = (target_h - trimmed.height) // 2
    canvas.alpha_composite(trimmed, (x, y))
    return canvas


def stretch_canvas(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    target_w, target_h = size
    trimmed = trim_alpha(image, padding=0)
    if trimmed.size != size:
        trimmed = trimmed.resize(size, Image.Resampling.NEAREST)
    canvas = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
    canvas.alpha_composite(trimmed, (0, 0))
    return canvas


def cell_box(sheet: Image.Image, row: AssetRow) -> tuple[int, int, int, int]:
    custom = custom_cell_box(sheet, row)
    if custom is not None:
        return custom
    cell_w = sheet.width / row.cols
    cell_h = sheet.height / row.rows
    return (
        round(row.col * cell_w),
        round(row.row * cell_h),
        round((row.col + 1) * cell_w),
        round((row.row + 1) * cell_h),
    )


def custom_cell_box(sheet: Image.Image, row: AssetRow) -> tuple[int, int, int, int] | None:
    width, height = sheet.size
    # Image generation follows visible rows more than exact mathematical grids.
    # These bands avoid clipping tall sprites that cross default row boundaries.
    if row.sheet.startswith("bosses_") and row.cols == 4 and row.rows == 3:
        x0 = round(row.col * width / 4)
        x1 = round((row.col + 1) * width / 4)
        bands = {
            0: (0, round(height * 0.43)),
            1: (round(height * 0.36), round(height * 0.72)),
            2: (round(height * 0.66), height),
        }
        y0, y1 = bands[row.row]
        return (x0, y0, x1, y1)
    if row.sheet == "npcs_sheet_chromakey_v01.png" and row.cols == 5 and row.rows == 2:
        x0 = round(row.col * width / 5)
        x1 = round((row.col + 1) * width / 5)
        bands = {
            0: (0, round(height * 0.66)),
            1: (round(height * 0.54), height),
        }
        y0, y1 = bands[row.row]
        return (x0, y0, x1, y1)
    if row.sheet == "tiles_ui_sheet_chromakey_v01.png" and row.cols == 6 and row.rows == 6 and row.row <= 3:
        x0 = round(row.col * width / 6)
        x1 = round((row.col + 1) * width / 6)
        bands = {
            0: (round(height * 0.03), round(height * 0.22)),
            1: (round(height * 0.22), round(height * 0.42)),
            2: (round(height * 0.40), round(height * 0.62)),
            3: (round(height * 0.58), round(height * 0.76)),
        }
        y0, y1 = bands[row.row]
        return (x0, y0, x1, y1)
    return None


def process(manifest: Path, generated_dir: Path, final_dir: Path) -> dict[str, int]:
    rows = load_manifest(manifest)
    final_dir.mkdir(parents=True, exist_ok=True)
    stats = {"processed": 0, "missing_sheets": 0}
    sheet_cache: dict[str, Image.Image] = {}
    for row in rows:
        sheet_path = generated_dir / row.sheet
        if not sheet_path.exists():
            stats["missing_sheets"] += 1
            continue
        if row.sheet not in sheet_cache:
            with Image.open(sheet_path) as raw:
                sheet_cache[row.sheet] = remove_chroma_key(raw)
        rgba = sheet_cache[row.sheet]
        crop = rgba.crop(cell_box(rgba, row))
        if row.asset_id in {"spiritual_energy_bar_frame", "spiritual_energy_bar_fill", "tribulation_warning_line"}:
            final = stretch_canvas(crop, (row.width, row.height))
        else:
            reserved_padding = 0 if row.output_type in {"tile", "wall"} else 2
            final = fit_canvas(crop, (row.width, row.height), reserved_padding=reserved_padding)
        out_dir = final_dir / row.asset_id
        out_dir.mkdir(parents=True, exist_ok=True)
        final.save(out_dir / row.output_name)
        stats["processed"] += 1
    for sheet in sheet_cache.values():
        sheet.close()
    return stats


def make_contact_sheet(final_dir: Path, out_path: Path, thumb_size: int = 96) -> None:
    files = sorted(final_dir.glob("*/*.png"))
    if not files:
        return
    cols = 8
    rows = (len(files) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * thumb_size, rows * thumb_size), (24, 24, 24, 255))
    for i, path in enumerate(files):
        with Image.open(path) as img:
            thumb = fit_canvas(img, (thumb_size, thumb_size), reserved_padding=4)
            x = (i % cols) * thumb_size
            y = (i // cols) * thumb_size
            sheet.alpha_composite(thumb, (x, y))
    out_path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(out_path)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", default="Assets/Specs/art_asset_manifest.csv")
    parser.add_argument("--generated-dir", default="Assets/Generated/BatchSheets")
    parser.add_argument("--final-dir", default="Assets/Final")
    parser.add_argument("--contact-sheet", default="Assets/Final/contact_sheet_v01.png")
    args = parser.parse_args()

    stats = process(Path(args.manifest), Path(args.generated_dir), Path(args.final_dir))
    make_contact_sheet(Path(args.final_dir), Path(args.contact_sheet))
    print(json.dumps(stats, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
