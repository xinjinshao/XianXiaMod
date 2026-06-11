from __future__ import annotations

import csv
import json
from collections import deque
from pathlib import Path

from PIL import Image


def final_paths(manifest: Path, final_dir: Path) -> list[Path]:
    paths: list[Path] = []
    with manifest.open(encoding="utf-8-sig", newline="") as f:
        for row in csv.DictReader(f):
            path = final_dir / row["asset_id"] / f"{row['asset_id']}__{row['output_type']}__v01.png"
            if path.exists():
                paths.append(path)
    return paths


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

    total_area = sum(part["area"] for part in parts)
    major_threshold = max(16, int(total_area * 0.04))
    major_parts = [part for part in parts if part["area"] >= major_threshold]
    if not major_parts:
        return 0
    major_bottom = max(part["max_y"] for part in major_parts)
    tiny_threshold = max(16, int(total_area * 0.06))

    remove: list[dict[str, int]] = []
    for part in parts:
        separated_below = part["min_y"] > major_bottom + 1
        low_tiny = part["max_y"] >= rgba.height - 6 and part["area"] <= tiny_threshold
        text_like = part["height"] <= 10 and part["width"] <= max(24, rgba.width // 3)
        if part["area"] <= tiny_threshold and text_like and (separated_below or low_tiny):
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


def main() -> None:
    manifest = Path("Assets/Specs/art_asset_manifest.csv")
    final_dir = Path("Assets/Final")
    changed = 0
    removed = 0
    for path in final_paths(manifest, final_dir):
        removed_pixels = clean_image(path)
        if removed_pixels:
            changed += 1
            removed += removed_pixels
    print(json.dumps({"changed_files": changed, "removed_pixels": removed}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
