from __future__ import annotations

import argparse
import json
from dataclasses import dataclass
from pathlib import Path

from PIL import Image


@dataclass(frozen=True)
class Asset:
    slug: str
    type: str
    content: list[str]
    size: tuple[int, int]


def parse_size(value: str) -> tuple[int, int]:
    width, height = value.lower().split("x", 1)
    return int(width), int(height)


def load_assets(path: Path) -> list[Asset]:
    data = json.loads(path.read_text(encoding="utf-8"))
    assets = []
    for item in data["assets"]:
        content = item["content"]
        if isinstance(content, str):
            content = [content]
        assets.append(Asset(item["slug"], item["type"], content, parse_size(item["size"])))
    return assets


def chroma_to_alpha(image: Image.Image) -> Image.Image:
    image = image.convert("RGBA")
    pixels = image.load()
    width, height = image.size
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if r > 170 and b > 170 and g < 125 and abs(r - b) < 100:
                pixels[x, y] = (r, g, b, 0)
    return image


def alpha_bbox(image: Image.Image, threshold: int = 8) -> tuple[int, int, int, int] | None:
    alpha = image.getchannel("A")
    mask = alpha.point(lambda value: 255 if value > threshold else 0)
    return mask.getbbox()


def has_alpha_on_guard(cell: Image.Image, guard: int, threshold: int = 8) -> bool:
    alpha = cell.getchannel("A")
    width, height = alpha.size
    if guard <= 0:
        return False
    bands = [
        (0, 0, width, min(guard, height)),
        (0, max(0, height - guard), width, height),
        (0, 0, min(guard, width), height),
        (max(0, width - guard), 0, width, height),
    ]
    for box in bands:
        if alpha.crop(box).point(lambda value: 255 if value > threshold else 0).getbbox():
            return True
    return False


def occupied_indices(image: Image.Image, axis: str, threshold: int = 8) -> list[int]:
    alpha = image.getchannel("A")
    width, height = alpha.size
    occupied = []
    if axis == "y":
        for y in range(height):
            if alpha.crop((0, y, width, y + 1)).point(lambda value: 255 if value > threshold else 0).getbbox():
                occupied.append(y)
    else:
        for x in range(width):
            if alpha.crop((x, 0, x + 1, height)).point(lambda value: 255 if value > threshold else 0).getbbox():
                occupied.append(x)
    return occupied


def bands(indices: list[int], min_gap: int) -> list[tuple[int, int]]:
    if not indices:
        return []
    result = []
    start = previous = indices[0]
    for index in indices[1:]:
        if index - previous >= min_gap:
            result.append((start, previous + 1))
            start = index
        previous = index
    result.append((start, previous + 1))
    return result


def projection_cells(image: Image.Image, rows: int, cols: int, min_gap: int) -> list[tuple[int, int, int, int]]:
    row_bands = bands(occupied_indices(image, "y"), min_gap)
    if len(row_bands) != rows:
        raise SystemExit(f"Expected {rows} occupied row bands, found {len(row_bands)}: {row_bands}")
    cells: list[tuple[int, int, int, int]] = []
    for top, bottom in row_bands:
        row_image = image.crop((0, top, image.width, bottom))
        col_bands = bands(occupied_indices(row_image, "x"), min_gap)
        if len(col_bands) != cols:
            raise SystemExit(f"Expected {cols} occupied column bands in row {(top, bottom)}, found {len(col_bands)}: {col_bands}")
        for left, right in col_bands:
            cells.append((left, top, right, bottom))
    return cells


def validate_gaps(cells: list[tuple[int, int, int, int]], rows: int, cols: int, minimum_gap: int) -> list[str]:
    failures = []
    for row in range(rows):
        row_cells = cells[row * cols : (row + 1) * cols]
        for index in range(cols - 1):
            gap = row_cells[index + 1][0] - row_cells[index][2]
            if gap < minimum_gap:
                failures.append(f"row {row} gap {index}->{index + 1} is {gap}px, expected >= {minimum_gap}px")
    for col in range(cols):
        col_cells = [cells[row * cols + col] for row in range(rows)]
        for index in range(rows - 1):
            gap = col_cells[index + 1][1] - col_cells[index][3]
            if gap < minimum_gap:
                failures.append(f"column {col} gap {index}->{index + 1} is {gap}px, expected >= {minimum_gap}px")
    return failures


def fit_to_canvas(sprite: Image.Image, size: tuple[int, int], margin: int) -> Image.Image:
    bbox = alpha_bbox(sprite)
    if bbox:
        sprite = sprite.crop(bbox)
    width, height = size
    scale = min((width - margin * 2) / sprite.width, (height - margin * 2) / sprite.height)
    resized = sprite.resize(
        (max(1, int(sprite.width * scale)), max(1, int(sprite.height * scale))),
        Image.Resampling.NEAREST,
    )
    output = Image.new("RGBA", size, (0, 0, 0, 0))
    output.alpha_composite(resized, ((width - resized.width) // 2, (height - resized.height) // 2))
    return output


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract final sprites from a wide-gutter atlas.")
    parser.add_argument("--atlas", required=True, type=Path)
    parser.add_argument("--manifest", required=True, type=Path)
    parser.add_argument("--rows", required=True, type=int)
    parser.add_argument("--cols", required=True, type=int)
    parser.add_argument("--root", default=Path("."), type=Path)
    parser.add_argument("--generated-dir", required=True, type=Path)
    parser.add_argument("--cleaned-dir", required=True, type=Path)
    parser.add_argument("--final-dir", default=Path("Assets/Final"), type=Path)
    parser.add_argument("--guard", default=45, type=int, help="Alpha in this many pixels near a cell edge fails extraction.")
    parser.add_argument("--min-gutter", default=100, type=int, help="Required empty pixels between detected sprite bounds.")
    parser.add_argument("--segment", choices=["grid", "projection"], default="projection")
    parser.add_argument("--margin", default=3, type=int, help="Transparent margin for final output.")
    args = parser.parse_args()

    root = args.root.resolve()
    assets = load_assets(args.manifest)
    expected = args.rows * args.cols
    if len(assets) != expected:
        raise SystemExit(f"manifest asset count {len(assets)} does not match grid {expected}")

    atlas = Image.open(args.atlas)
    source_atlas = args.generated_dir / args.atlas.name
    source_atlas.parent.mkdir(parents=True, exist_ok=True)
    source_atlas.write_bytes(args.atlas.read_bytes())

    image = chroma_to_alpha(atlas)
    width, height = image.size
    if args.segment == "projection":
        detected_cells = projection_cells(image, args.rows, args.cols, max(12, args.min_gutter // 2))
        gap_failures = validate_gaps(detected_cells, args.rows, args.cols, args.min_gutter)
        if gap_failures:
            print("Safe atlas extraction failed:")
            for failure in gap_failures:
                print(f" - {failure}")
            raise SystemExit(1)
    else:
        cell_width = width / args.cols
        cell_height = height / args.rows
        detected_cells = []
        for index in range(expected):
            col = index % args.cols
            row = index // args.cols
            detected_cells.append(
                (
                    round(col * cell_width),
                    round(row * cell_height),
                    round((col + 1) * cell_width),
                    round((row + 1) * cell_height),
                )
            )

    failures: list[str] = []
    extracted: list[tuple[Asset, Image.Image]] = []
    for index, asset in enumerate(assets):
        left, top, right, bottom = detected_cells[index]
        if args.segment == "grid":
            cell = image.crop((left, top, right, bottom))
        else:
            pad = min(args.guard, args.min_gutter // 2)
            cell = image.crop((max(0, left - pad), max(0, top - pad), min(width, right + pad), min(height, bottom + pad)))
        if args.segment == "grid" and has_alpha_on_guard(cell, args.guard):
            failures.append(f"{asset.slug}: alpha inside {args.guard}px cell guard; atlas gutter is unsafe")
            continue
        bbox = alpha_bbox(cell)
        if bbox is None:
            failures.append(f"{asset.slug}: empty cell")
            continue
        extracted.append((asset, cell.crop(bbox)))

    if failures:
        print("Safe atlas extraction failed:")
        for failure in failures:
            print(f" - {failure}")
        raise SystemExit(1)

    args.cleaned_dir.mkdir(parents=True, exist_ok=True)
    args.final_dir.mkdir(parents=True, exist_ok=True)
    for asset, sprite in extracted:
        cleaned = args.cleaned_dir / f"{asset.slug}__{asset.type}__candidate.png"
        sprite.save(cleaned)
        output = fit_to_canvas(sprite, asset.size, args.margin)
        asset_final_dir = args.final_dir / asset.slug
        asset_final_dir.mkdir(parents=True, exist_ok=True)
        final = asset_final_dir / f"{asset.slug}__{asset.type}.png"
        output.save(final)
        for content in asset.content:
            output.save(root / content)
        print({"slug": asset.slug, "size": asset.size, "content": asset.content})


if __name__ == "__main__":
    main()
