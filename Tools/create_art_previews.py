from __future__ import annotations

import csv
from collections import defaultdict
from pathlib import Path

from PIL import Image, ImageDraw


def fit_thumb(image: Image.Image, size: int = 96) -> Image.Image:
    rgba = image.convert("RGBA")
    alpha = rgba.getchannel("A")
    bbox = alpha.getbbox()
    if bbox:
        rgba = rgba.crop(bbox)
    scale = min((size - 8) / rgba.width, (size - 8) / rgba.height)
    resized = rgba.resize((max(1, int(rgba.width * scale)), max(1, int(rgba.height * scale))), Image.Resampling.NEAREST)
    canvas = Image.new("RGBA", (size, size), (30, 30, 30, 255))
    canvas.alpha_composite(resized, ((size - resized.width) // 2, (size - resized.height) // 2))
    return canvas


def make_category_sheets(manifest: Path, final_dir: Path, out_dir: Path) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)
    groups: dict[str, list[Path]] = defaultdict(list)
    with manifest.open(encoding="utf-8-sig", newline="") as f:
        for row in csv.DictReader(f):
            sheet_name = row["sheet"].split("_sheet_")[0]
            path = final_dir / row["asset_id"] / f"{row['asset_id']}__{row['output_type']}__v01.png"
            if path.exists():
                groups[sheet_name].append(path)
    for group, files in groups.items():
        cols = 8
        thumb = 96
        rows = (len(files) + cols - 1) // cols
        gap = 8
        sheet = Image.new("RGBA", (cols * thumb + (cols - 1) * gap, rows * thumb + (rows - 1) * gap), (12, 12, 12, 255))
        for i, path in enumerate(files):
            tile = fit_thumb(Image.open(path), thumb)
            sheet.alpha_composite(tile, ((i % cols) * (thumb + gap), (i // cols) * (thumb + gap)))
        sheet.save(out_dir / f"{group}_contact_sheet_v01.png")


def iter_manifest_final_paths(manifest: Path, final_dir: Path) -> list[Path]:
    files: list[Path] = []
    with manifest.open(encoding="utf-8-sig", newline="") as f:
        for row in csv.DictReader(f):
            path = final_dir / row["asset_id"] / f"{row['asset_id']}__{row['output_type']}__v01.png"
            if path.exists():
                files.append(path)
    return files


def make_all_contact_sheet(manifest: Path, final_dir: Path, out_path: Path) -> None:
    files = iter_manifest_final_paths(manifest, final_dir)
    if not files:
        return
    cols = 8
    thumb = 96
    rows = (len(files) + cols - 1) // cols
    gap = 8
    sheet = Image.new("RGBA", (cols * thumb + (cols - 1) * gap, rows * thumb + (rows - 1) * gap), (12, 12, 12, 255))
    for i, path in enumerate(files):
        with Image.open(path) as img:
            tile = fit_thumb(img, thumb)
        sheet.alpha_composite(tile, ((i % cols) * (thumb + gap), (i // cols) * (thumb + gap)))
    out_path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(out_path)


def make_tile_previews(manifest: Path, final_dir: Path, out_dir: Path) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)
    with manifest.open(encoding="utf-8-sig", newline="") as f:
        for row in csv.DictReader(f):
            if row["output_type"] not in {"tile", "wall"}:
                continue
            path = final_dir / row["asset_id"] / f"{row['asset_id']}__{row['output_type']}__v01.png"
            if not path.exists():
                continue
            with Image.open(path) as img:
                tile = img.convert("RGBA")
                preview = Image.new("RGBA", (tile.width * 4, tile.height * 4), (0, 0, 0, 0))
                for y in range(4):
                    for x in range(4):
                        preview.alpha_composite(tile, (x * tile.width, y * tile.height))
                preview.save(out_dir / f"{row['asset_id']}__{row['output_type']}_tile_preview__v01.png")


def main() -> None:
    manifest = Path("Assets/Specs/art_asset_manifest.csv")
    final_dir = Path("Assets/Final")
    make_all_contact_sheet(manifest, final_dir, Path("Assets/Final/contact_sheet_v01.png"))
    make_category_sheets(manifest, final_dir, Path("Assets/Final/ContactSheets"))
    make_tile_previews(manifest, final_dir, Path("Assets/Final/TilePreviews"))


if __name__ == "__main__":
    main()
