from __future__ import annotations

import csv
import json
from collections import deque
from pathlib import Path

from PIL import Image


def final_rows(manifest: Path, final_dir: Path) -> list[dict[str, str | Path]]:
    rows: list[dict[str, str | Path]] = []
    with manifest.open(encoding="utf-8-sig", newline="") as f:
        for row in csv.DictReader(f):
            path = final_dir / row["asset_id"] / f"{row['asset_id']}__{row['output_type']}__v01.png"
            if path.exists():
                rows.append({**row, "path": path})
    return rows


def final_paths(manifest: Path, final_dir: Path) -> list[Path]:
    return [row["path"] for row in final_rows(manifest, final_dir) if isinstance(row["path"], Path)]


def components(alpha: Image.Image) -> list[dict[str, int]]:
    width, height = alpha.size
    pix = alpha.load()
    seen = bytearray(width * height)
    found: list[dict[str, int]] = []
    for y in range(height):
        for x in range(width):
            idx = y * width + x
            if seen[idx] or pix[x, y] == 0:
                continue
            q: deque[tuple[int, int]] = deque([(x, y)])
            seen[idx] = 1
            area = 0
            min_x = max_x = x
            min_y = max_y = y
            while q:
                cx, cy = q.popleft()
                area += 1
                min_x = min(min_x, cx)
                max_x = max(max_x, cx)
                min_y = min(min_y, cy)
                max_y = max(max_y, cy)
                for ny in range(cy - 1, cy + 2):
                    for nx in range(cx - 1, cx + 2):
                        if nx == cx and ny == cy:
                            continue
                        if nx < 0 or ny < 0 or nx >= width or ny >= height:
                            continue
                        nidx = ny * width + nx
                        if not seen[nidx] and pix[nx, ny] != 0:
                            seen[nidx] = 1
                            q.append((nx, ny))
            found.append(
                {
                    "area": area,
                    "min_x": min_x,
                    "max_x": max_x,
                    "min_y": min_y,
                    "max_y": max_y,
                    "width": max_x - min_x + 1,
                    "height": max_y - min_y + 1,
                }
            )
    return found


def clean_image(path: Path) -> int:
    with Image.open(path) as src:
        rgba = src.convert("RGBA")
    alpha = rgba.getchannel("A")
    parts = components(alpha)
    if len(parts) < 2:
        return 0

    anchor = max(parts, key=lambda part: part["area"])
    anchor_threshold = max(16, int(anchor["area"] * 0.2))
    anchor_parts = [part for part in parts if part["area"] >= anchor_threshold]
    if not anchor_parts:
        return 0
    anchor_box = {
        "min_x": min(part["min_x"] for part in anchor_parts),
        "max_x": max(part["max_x"] for part in anchor_parts),
        "min_y": min(part["min_y"] for part in anchor_parts),
        "max_y": max(part["max_y"] for part in anchor_parts),
    }
    fragment_threshold = max(16, int(anchor["area"] * 0.35))

    remove: list[dict[str, int]] = []
    for part in parts:
        if part in anchor_parts:
            continue
        separated_outside = (
            part["max_y"] < anchor_box["min_y"] - 1
            or part["min_y"] > anchor_box["max_y"] + 1
            or part["max_x"] < anchor_box["min_x"] - 1
            or part["min_x"] > anchor_box["max_x"] + 1
        )
        low_edge = part["max_y"] >= rgba.height - 6
        high_edge = part["min_y"] <= 6
        thin_fragment = part["height"] <= 10 or part["width"] <= 10
        compact_fragment = part["area"] <= fragment_threshold
        if compact_fragment and thin_fragment and (separated_outside or low_edge or high_edge):
            remove.append(part)

    if not remove:
        return 0

    pix = rgba.load()
    alpha_pix = alpha.load()
    removed_pixels = 0
    for part in remove:
        for y in range(part["min_y"], part["max_y"] + 1):
            for x in range(part["min_x"], part["max_x"] + 1):
                if alpha_pix[x, y] != 0:
                    pix[x, y] = (0, 0, 0, 0)
                    removed_pixels += 1
    rgba.save(path)
    return removed_pixels


def normalize_tile_or_wall(path: Path, width: int, height: int) -> bool:
    if height != 16:
        return False
    with Image.open(path) as src:
        rgba = src.convert("RGBA")
    bbox = rgba.getchannel("A").getbbox()
    if bbox is None:
        return False
    cropped = rgba.crop(bbox)
    if cropped.size == (width, height) and bbox == (0, 0, width, height):
        return False
    normalized = cropped.resize((width, height), Image.Resampling.NEAREST)
    normalized.save(path)
    return True


def main() -> None:
    manifest = Path("Assets/Specs/art_asset_manifest.csv")
    final_dir = Path("Assets/Final")
    skip_clean = {"spiritual_energy_bar_frame", "spiritual_energy_bar_fill", "tribulation_warning_line"}
    changed = 0
    removed = 0
    normalized = 0
    for row in final_rows(manifest, final_dir):
        path = row["path"]
        if not isinstance(path, Path):
            continue
        if row["output_type"] in {"tile", "wall"} and normalize_tile_or_wall(path, int(row["width"]), int(row["height"])):
            normalized += 1
        if row["asset_id"] in skip_clean:
            continue
        removed_pixels = clean_image(path)
        if removed_pixels:
            changed += 1
            removed += removed_pixels
    print(
        json.dumps(
            {"changed_files": changed, "removed_pixels": removed, "normalized_tile_wall": normalized},
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
