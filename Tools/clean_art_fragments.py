from __future__ import annotations

from pathlib import Path

from PIL import Image

from verify_art_quality import ROOT, alpha_components, png_paths, rel, suspect_stray_components


def erase_component(image: Image.Image, bbox: tuple[int, int, int, int]) -> None:
    pixels = image.load()
    left, top, right, bottom = bbox
    for y in range(top, bottom):
        for x in range(left, right):
            pixels[x, y] = (0, 0, 0, 0)


def clean_file(path: Path) -> int:
    image = Image.open(path).convert("RGBA")
    suspects = suspect_stray_components(image)
    if not suspects:
        return 0
    components = alpha_components(image)
    removed = 0
    for suspect in suspects:
        bbox = suspect["bbox"]
        if bbox == components[0]["bbox"]:
            continue
        erase_component(image, bbox)  # type: ignore[arg-type]
        removed += 1
    if removed:
        image.save(path)
    return removed


def main() -> None:
    cleaned: list[tuple[str, int]] = []
    for path in png_paths():
        removed = clean_file(path)
        if removed:
            cleaned.append((rel(path), removed))
    print({"cleaned_files": len(cleaned), "removed_components": sum(count for _, count in cleaned)})
    for item, count in cleaned:
        print(f"{item}: {count}")


if __name__ == "__main__":
    main()
