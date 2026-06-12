from __future__ import annotations

import csv
from pathlib import Path

from PIL import Image


MANIFEST = Path("Assets/Specs/art_asset_manifest.csv")
FINAL = Path("Assets/Final")

SKIP_OUTPUT_TYPES = {"tile", "wall", "ui"}


def target_margin(width: int, height: int, output_type: str) -> int:
    longest = max(width, height)
    if output_type == "boss_head":
        desired = 5
    elif longest >= 128:
        desired = 12
    elif longest >= 64:
        desired = 6
    else:
        desired = 5

    # Tiny projectiles may be too short to physically hold a 5px transparent
    # border. Clamp to the largest margin that still leaves at least one
    # visible pixel in each axis, so the script is stable across repeated runs.
    return max(0, min(desired, (width - 1) // 2, (height - 1) // 2))


def alpha_bbox(image: Image.Image) -> tuple[int, int, int, int] | None:
    return image.convert("RGBA").getchannel("A").getbbox()


def margins(image: Image.Image, bbox: tuple[int, int, int, int]) -> tuple[int, int, int, int]:
    left, top, right, bottom = bbox
    return left, top, image.width - right, image.height - bottom


def fit_with_margin(image: Image.Image, margin: int) -> Image.Image:
    rgba = image.convert("RGBA")
    bbox = alpha_bbox(rgba)
    if bbox is None:
        return rgba

    sprite = rgba.crop(bbox)
    usable_w = max(1, rgba.width - margin * 2)
    usable_h = max(1, rgba.height - margin * 2)
    scale = min(1.0, usable_w / sprite.width, usable_h / sprite.height)
    if scale < 1.0:
        sprite = sprite.resize(
            (max(1, int(sprite.width * scale)), max(1, int(sprite.height * scale))),
            Image.Resampling.NEAREST,
        )

    canvas = Image.new("RGBA", rgba.size, (0, 0, 0, 0))
    canvas.alpha_composite(sprite, ((rgba.width - sprite.width) // 2, (rgba.height - sprite.height) // 2))
    return canvas


def iter_manifest_rows() -> list[dict[str, str]]:
    with MANIFEST.open(encoding="utf-8-sig", newline="") as f:
        return list(csv.DictReader(f))


def main() -> None:
    repaired: list[str] = []
    for row in iter_manifest_rows():
        output_type = row["output_type"]
        if output_type in SKIP_OUTPUT_TYPES:
            continue

        asset_id = row["asset_id"]
        path = FINAL / asset_id / f"{asset_id}__{output_type}__v01.png"
        if not path.exists():
            continue

        with Image.open(path) as raw:
            image = raw.convert("RGBA")
        bbox = alpha_bbox(image)
        if bbox is None:
            continue

        required = target_margin(image.width, image.height, output_type)
        if min(margins(image, bbox)) >= required:
            continue

        fixed = fit_with_margin(image, required)
        fixed.save(path)
        repaired.append(str(path))

    print({"repaired": len(repaired), "files": repaired})


if __name__ == "__main__":
    main()
