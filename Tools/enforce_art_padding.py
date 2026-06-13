from __future__ import annotations

from pathlib import Path

from PIL import Image

from verify_art_quality import OBJECT_TILES, ROOT, TILEABLE_PREFIXES, png_paths, rel, required_margin


def enforce_padding(path: Path) -> bool:
    item = rel(path)
    image = Image.open(path).convert("RGBA")
    margin = required_margin(path, image)
    if margin == 0 or item in TILEABLE_PREFIXES:
        return False
    alpha = image.getchannel("A")
    bbox = alpha.getbbox()
    if bbox is None:
        return False
    width, height = image.size
    left, top, right, bottom = bbox
    if left >= margin and top >= margin and width - right >= margin and height - bottom >= margin:
        return False

    cropped = image.crop(bbox)
    target_width = max(1, width - margin * 2)
    target_height = max(1, height - margin * 2)
    scale = min(target_width / cropped.width, target_height / cropped.height)
    resized = cropped.resize(
        (max(1, int(cropped.width * scale)), max(1, int(cropped.height * scale))),
        Image.Resampling.NEAREST,
    )
    output = Image.new("RGBA", image.size, (0, 0, 0, 0))
    output.alpha_composite(resized, ((width - resized.width) // 2, (height - resized.height) // 2))
    output.save(path)
    return True


def main() -> None:
    changed = []
    for path in png_paths():
        if enforce_padding(path):
            changed.append(rel(path))
    print({"padded": len(changed)})
    for item in changed:
        print(item)


if __name__ == "__main__":
    main()
