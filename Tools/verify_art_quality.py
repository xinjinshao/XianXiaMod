from __future__ import annotations

from pathlib import Path

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


def assert_safe_edges(path: Path, image: Image.Image, failures: list[str]) -> None:
    item = rel(path)
    if item in TILEABLE_PREFIXES:
        return
    alpha = image.convert("RGBA").getchannel("A")
    bbox = alpha.getbbox()
    if bbox is None:
        failures.append(f"{item}: empty alpha")
        return
    width, height = image.size
    left, top, right, bottom = bbox
    if left <= 0 or top <= 0 or right >= width or bottom >= height:
        failures.append(f"{item}: non-tileable alpha touches edge, bbox={bbox}, size={image.size}")


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


def main() -> None:
    failures: list[str] = []
    checked = 0
    for path in png_paths():
        image = Image.open(path)
        checked += 1
        assert_safe_edges(path, image, failures)
        assert_color_depth(path, image, failures)
    if failures:
        print("Art quality verification failed:")
        for failure in failures:
            print(f" - {failure}")
        raise SystemExit(1)
    print({"checked": checked, "status": "ok"})


if __name__ == "__main__":
    main()
