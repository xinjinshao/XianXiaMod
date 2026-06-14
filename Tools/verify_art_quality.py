from __future__ import annotations

from pathlib import Path
from collections import deque

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]

TILEABLE_PREFIXES = {
    "Content/Tiles/BlackFurnaceWall.png",
    "Content/Tiles/FallenHeavenJadeTile.png",
    "Content/Tiles/FurnaceSlagTile.png",
    "Content/Tiles/GreenwoodSoilTile.png",
    "Content/Tiles/MoonboneTile.png",
    "Content/Tiles/SectRuinBrickTile.png",
    "Content/Tiles/SpiritMossTile.png",
    "Content/Tiles/SpiritOreTile.png",
    "Content/Tiles/StarAbyssCrystalTile.png",
    "Content/Tiles/ThunderCloudTile.png",
}

OBJECT_TILES = {
    "Content/Tiles/ArchiveLightPillarTile.png",
    "Content/Tiles/BrokenHeavenTabletTile.png",
    "Content/Tiles/RiftMembraneTile.png",
    "Content/Tiles/SingingThunderStoneTile.png",
    "Content/Tiles/SpiritHerbTile.png",
    "Content/Tiles/SwordTabletTile.png",
}

PROJECTILE_MIN_COLORS = {
    "Content/Projectiles/TribulationWarningLineProjectile.png": 4,
    "Content/Projectiles/SpiritBolt.png": 8,
    "Content/Projectiles/SpiritBoltProjectile.png": 8,
}


def rel(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def required_margin(path: Path, image: Image.Image) -> int:
    item = rel(path)
    width, height = image.size
    if item in TILEABLE_PREFIXES:
        return 0
    if "Content/Projectiles/" in item:
        if min(width, height) <= 4:
            return 1
        return 2
    if "Content/Tiles/Stations/" in item or item in OBJECT_TILES:
        return 2
    if width <= 24 or height <= 24:
        return 2
    return 3


def png_paths() -> list[Path]:
    roots = [ROOT / "Content" / "Buffs", ROOT / "Content" / "Items", ROOT / "Content" / "Projectiles", ROOT / "Content" / "Tiles" / "Stations"]
    paths: list[Path] = []
    for root in roots:
        if root.exists():
            paths.extend(sorted(root.rglob("*.png")))
    for item in OBJECT_TILES:
        path = ROOT / item
        if path.exists():
            paths.append(path)
    return sorted(set(paths))


def nontransparent_colors(image: Image.Image) -> int:
    rgba = image.convert("RGBA")
    colors = set()
    for r, g, b, a in rgba.getdata():
        if a:
            colors.add((r, g, b))
    return len(colors)


def alpha_components(image: Image.Image, threshold: int = 24) -> list[dict[str, object]]:
    alpha = image.convert("RGBA").getchannel("A")
    width, height = alpha.size
    pixels = alpha.load()
    seen: set[tuple[int, int]] = set()
    components: list[dict[str, object]] = []
    for y in range(height):
        for x in range(width):
            if pixels[x, y] <= threshold or (x, y) in seen:
                continue
            queue: deque[tuple[int, int]] = deque([(x, y)])
            seen.add((x, y))
            points: list[tuple[int, int]] = []
            while queue:
                current_x, current_y = queue.popleft()
                points.append((current_x, current_y))
                for next_x in range(current_x - 1, current_x + 2):
                    for next_y in range(current_y - 1, current_y + 2):
                        if next_x < 0 or next_y < 0 or next_x >= width or next_y >= height:
                            continue
                        if (next_x, next_y) in seen or pixels[next_x, next_y] <= threshold:
                            continue
                        seen.add((next_x, next_y))
                        queue.append((next_x, next_y))
            xs = [point[0] for point in points]
            ys = [point[1] for point in points]
            components.append({"pixels": len(points), "bbox": (min(xs), min(ys), max(xs) + 1, max(ys) + 1)})
    return sorted(components, key=lambda component: int(component["pixels"]), reverse=True)


def suspect_stray_components(image: Image.Image) -> list[dict[str, object]]:
    components = alpha_components(image)
    if len(components) <= 1:
        return []
    main_pixels = int(components[0]["pixels"])
    width, height = image.size
    suspects: list[dict[str, object]] = []
    for component in components[1:]:
        left, top, right, bottom = component["bbox"]  # type: ignore[misc]
        box_width = right - left
        box_height = bottom - top
        pixels = int(component["pixels"])
        near_top_or_bottom = top <= 3 or height - bottom <= 3
        tiny_relative_to_main = pixels <= max(12, main_pixels * 0.08)
        sliver_like = box_width <= 12 and box_height <= 4
        if near_top_or_bottom and tiny_relative_to_main and sliver_like:
            suspects.append(component)
    return suspects


def assert_safe_edges(path: Path, image: Image.Image, failures: list[str]) -> None:
    item = rel(path)
    margin = required_margin(path, image)
    if margin == 0:
        return
    alpha = image.convert("RGBA").getchannel("A")
    bbox = alpha.getbbox()
    if bbox is None:
        failures.append(f"{item}: empty alpha")
        return
    width, height = image.size
    left, top, right, bottom = bbox
    if left < margin or top < margin or width - right < margin or height - bottom < margin:
        failures.append(f"{item}: needs {margin}px visual margin, bbox={bbox}, size={image.size}")


def assert_color_depth(path: Path, image: Image.Image, failures: list[str]) -> None:
    item = rel(path)
    if item in TILEABLE_PREFIXES:
        return
    area = image.width * image.height
    minimum = PROJECTILE_MIN_COLORS.get(item)
    if minimum is None:
        if "Content/Projectiles/" in item and area <= 128:
            minimum = 8
        elif "Content/Projectiles/" in item:
            minimum = 16
        else:
            minimum = 24
    count = nontransparent_colors(image)
    if count < minimum:
        failures.append(f"{item}: too few visible colors ({count} < {minimum}); likely placeholder-like")


def assert_no_stray_components(path: Path, image: Image.Image, failures: list[str]) -> None:
    item = rel(path)
    if item in TILEABLE_PREFIXES:
        return
    suspects = suspect_stray_components(image)
    if suspects:
        failures.append(f"{item}: suspected cross-cell fragments: {suspects}")


def main() -> None:
    failures: list[str] = []
    checked = 0
    for path in png_paths():
        image = Image.open(path)
        checked += 1
        assert_safe_edges(path, image, failures)
        assert_color_depth(path, image, failures)
        assert_no_stray_components(path, image, failures)
    if failures:
        print("Art quality verification failed:")
        for failure in failures:
            print(f" - {failure}")
        raise SystemExit(1)
    print({"checked": checked, "status": "ok"})


if __name__ == "__main__":
    main()
