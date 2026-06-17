from __future__ import annotations

import argparse
import json
import math
import re
import shutil
from collections import deque
from pathlib import Path

from PIL import Image, ImageChops, ImageEnhance, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
KEY = (255, 0, 255)


def rgba(path: Path) -> Image.Image:
    return Image.open(path).convert("RGBA")


def alpha_bbox(image: Image.Image) -> tuple[int, int, int, int] | None:
    return image.convert("RGBA").getchannel("A").getbbox()


def remove_key(image: Image.Image, tolerance: int = 22) -> Image.Image:
    src = image.convert("RGBA")
    out = Image.new("RGBA", src.size, (0, 0, 0, 0))
    pixels = []
    for r, g, b, a in src.getdata():
        if abs(r - KEY[0]) <= tolerance and abs(g - KEY[1]) <= tolerance and abs(b - KEY[2]) <= tolerance:
            pixels.append((0, 0, 0, 0))
        else:
            pixels.append((r, g, b, 255 if a else 0))
    out.putdata(pixels)
    return out


def components(image: Image.Image) -> list[tuple[int, tuple[int, int, int, int]]]:
    alpha = image.getchannel("A")
    width, height = alpha.size
    pix = alpha.load()
    seen: set[tuple[int, int]] = set()
    found: list[tuple[int, tuple[int, int, int, int]]] = []
    for y in range(height):
        for x in range(width):
            if pix[x, y] == 0 or (x, y) in seen:
                continue
            q = deque([(x, y)])
            seen.add((x, y))
            pts: list[tuple[int, int]] = []
            while q:
                cx, cy = q.popleft()
                pts.append((cx, cy))
                for nx in range(cx - 1, cx + 2):
                    for ny in range(cy - 1, cy + 2):
                        if nx < 0 or ny < 0 or nx >= width or ny >= height:
                            continue
                        if (nx, ny) in seen or pix[nx, ny] == 0:
                            continue
                        seen.add((nx, ny))
                        q.append((nx, ny))
            xs = [p[0] for p in pts]
            ys = [p[1] for p in pts]
            found.append((len(pts), (min(xs), min(ys), max(xs) + 1, max(ys) + 1)))
    return sorted(found, key=lambda item: (item[1][1], item[1][0]))


def extract_ordered(atlas: Path, min_pixels: int = 128) -> list[Image.Image]:
    keyed = remove_key(rgba(atlas))
    sprites: list[Image.Image] = []
    for count, box in components(keyed):
        if count < min_pixels:
            continue
        sprites.append(keyed.crop(box))
    return sprites


def extract_grid(atlas: Path, cols: int, rows: int) -> list[Image.Image]:
    image = remove_key(rgba(atlas))
    cell_w = image.width / cols
    cell_h = image.height / rows
    sprites: list[Image.Image] = []
    for row in range(rows):
        for col in range(cols):
            left = int(round(col * cell_w))
            top = int(round(row * cell_h))
            right = int(round((col + 1) * cell_w))
            bottom = int(round((row + 1) * cell_h))
            cell = image.crop((left, top, right, bottom))
            bbox = alpha_bbox(cell)
            sprites.append(cell.crop(bbox) if bbox else cell)
    return sprites


def fit_canvas(sprite: Image.Image, size: tuple[int, int], margin: int = 2) -> Image.Image:
    sprite = sprite.convert("RGBA")
    bbox = alpha_bbox(sprite)
    out = Image.new("RGBA", size, (0, 0, 0, 0))
    if bbox is None:
        return out
    sprite = sprite.crop(bbox)
    width, height = size
    scale = min((width - margin * 2) / sprite.width, (height - margin * 2) / sprite.height, 1.0)
    new_size = (max(1, int(sprite.width * scale)), max(1, int(sprite.height * scale)))
    sprite = sprite.resize(new_size, Image.Resampling.NEAREST)
    out.alpha_composite(sprite, ((width - new_size[0]) // 2, (height - new_size[1]) // 2))
    return remove_tiny_components(harden_alpha(out))


def harden_alpha(image: Image.Image) -> Image.Image:
    out = image.convert("RGBA")
    data = []
    for r, g, b, a in out.getdata():
        if a < 96:
            data.append((0, 0, 0, 0))
        else:
            data.append((r, g, b, 255))
    out.putdata(data)
    return out


def remove_tiny_components(image: Image.Image, max_pixels: int = 3) -> Image.Image:
    out = image.convert("RGBA")
    pix = out.load()
    for comp in components(out):
        count, _ = comp
        if count > max_pixels:
            continue
        _, box = comp
        left, top, right, bottom = box
        for y in range(top, bottom):
            for x in range(left, right):
                if pix[x, y][3]:
                    pix[x, y] = (0, 0, 0, 0)
    return out


def pascal(asset_id: str) -> str:
    return "".join(part.capitalize() for part in asset_id.split("_"))


def projectile_class(asset_id: str) -> str:
    if asset_id.endswith("_proj"):
        asset_id = asset_id[:-5] + "_projectile"
    return pascal(asset_id)


def save_vertical(frames: list[Image.Image], path: Path) -> None:
    width, height = frames[0].size
    sheet = Image.new("RGBA", (width, height * len(frames)), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        sheet.alpha_composite(frame, (0, i * height))
    path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(path)


def shift_sprite(image: Image.Image, dx: int, dy: int, squash: float = 1.0, brighten: float = 1.0) -> Image.Image:
    bbox = alpha_bbox(image)
    out = Image.new("RGBA", image.size, (0, 0, 0, 0))
    if bbox is None:
        return out
    sprite = image.crop(bbox)
    if squash != 1.0:
        sprite = sprite.resize((sprite.width, max(1, int(sprite.height * squash))), Image.Resampling.NEAREST)
    if brighten != 1.0:
        alpha = sprite.getchannel("A")
        sprite = ImageEnhance.Brightness(sprite).enhance(brighten)
        sprite.putalpha(alpha)
    x = (image.width - sprite.width) // 2 + dx
    y = (image.height - sprite.height) // 2 + dy
    out.alpha_composite(sprite, (x, y))
    return harden_alpha(out)


def enemy_frames(base: Image.Image, name: str, count: int = 6) -> list[Image.Image]:
    frames = []
    for i in range(count):
        t = i / count
        wave = math.sin(t * math.tau)
        if any(key in name for key in ("bat", "moth", "hawk")):
            frames.append(shift_sprite(base, int(round(wave * 2)), int(round(math.cos(t * math.tau) * 3)), 1.0 + 0.05 * wave))
        elif any(key in name for key in ("slime", "larva", "worm")):
            frames.append(shift_sprite(base, 0, int(round(-2 * max(0, wave))), 1.05 if i in {1, 2} else 0.96 if i in {4, 5} else 1.0))
        else:
            frames.append(shift_sprite(base, int(round(wave * 1)), int(round(math.cos(t * math.tau) * 1)), 1.0))
    return frames


def town_frames(base: Image.Image, count: int = 4) -> list[Image.Image]:
    return [
        shift_sprite(base, 0, 0),
        shift_sprite(base, 1, -1, 1.0),
        shift_sprite(base, 0, 0, 0.98),
        shift_sprite(base, -1, 0, 1.0),
    ]


def projectile_frames(base: Image.Image, count: int = 4) -> list[Image.Image]:
    frames = []
    for i in range(count):
        frames.append(shift_sprite(base, i % 2, 0, 1.0, 1.0 + i * 0.04))
    return frames


def sync_content(asset_id: str, output_type: str, source: Path) -> None:
    targets: list[Path] = []
    cls = pascal(asset_id)
    if output_type in {"item_icon", "station_icon", "buff_icon"}:
        targets.extend((ROOT / "Content" / "Items").rglob(f"{cls}.png"))
        targets.extend((ROOT / "Content" / "Items").rglob(f"{cls}Item.png"))
        targets.extend((ROOT / "Content" / "Buffs").glob(f"{cls}.png"))
    elif output_type == "object":
        targets.extend((ROOT / "Content" / "Tiles").rglob(f"{cls}Tile.png"))
        targets.extend((ROOT / "Content" / "Tiles").rglob(f"{cls}.png"))
    elif output_type == "tile":
        targets.extend((ROOT / "Content" / "Tiles").rglob(f"{cls}Tile.png"))
    elif output_type == "wall":
        targets.extend((ROOT / "Content" / "Tiles").rglob(f"{cls}.png"))
    elif output_type == "ui":
        targets.extend((ROOT / "Common" / "UI").glob(f"{cls}.png"))
        if asset_id == "tribulation_warning_line":
            targets.append(ROOT / "Content" / "Projectiles" / "TribulationWarningLineProjectile.png")
    elif output_type == "projectile":
        pcls = projectile_class(asset_id)
        path = ROOT / "Content" / "Projectiles" / f"{pcls}.png"
        if path.exists():
            targets.append(path)
    for target in sorted(set(targets)):
        shutil.copyfile(source, target)


def write_asset(asset_id: str, output_type: str, image: Image.Image, version: str = "v09") -> Path:
    out_dir = ROOT / "Assets" / "Final" / asset_id
    out_dir.mkdir(parents=True, exist_ok=True)
    path = out_dir / f"{asset_id}__{output_type}__{version}.png"
    image.save(path)
    return path


def process_manifest(manifest: Path, atlas: Path, version: str = "v09") -> None:
    spec = json.loads(manifest.read_text(encoding="utf-8"))
    grid = spec.get("grid")
    sprites = extract_grid(atlas, int(grid["cols"]), int(grid["rows"])) if grid else extract_ordered(atlas)
    assets = spec["assets"]
    if len(sprites) < len(assets):
        raise SystemExit(f"atlas has {len(sprites)} sprites, manifest expects {len(assets)}")
    for sprite, item in zip(sprites, assets):
        asset_id = item["asset_id"]
        output_type = item["output_type"]
        size = tuple(item["size"])
        image = fit_canvas(sprite, size, int(item.get("margin", 2)))
        final = write_asset(asset_id, output_type, image, version)
        sync_content(asset_id, output_type, final)
        category = item.get("category", "")
        if category == "enemy":
            frames = enemy_frames(image, asset_id)
            save_vertical(frames, ROOT / "Content" / "NPCs" / "Enemies" / f"{pascal(asset_id)}.png")
            save_vertical(frames, ROOT / "Assets" / "Final" / asset_id / f"{asset_id}__animation_sheet__{version}.png")
        elif category == "town_body":
            frames = town_frames(image)
            save_vertical(frames, ROOT / "Content" / "NPCs" / "Town" / f"{pascal(asset_id)}.png")
            save_vertical(frames, ROOT / "Assets" / "Final" / asset_id / f"{asset_id}__animation_sheet__{version}.png")
        elif category == "town_head":
            target = ROOT / "Content" / "NPCs" / "Town" / f"{pascal(asset_id)}_Head.png"
            if target.exists():
                shutil.copyfile(final, target)
        elif category == "projectile":
            frames = projectile_frames(image)
            save_vertical(frames, ROOT / "Assets" / "Final" / asset_id / f"{asset_id}__motion_sheet__{version}.png")
        elif category in {"equipment", "item", "station"}:
            frames = projectile_frames(image)
            save_vertical(frames, ROOT / "Assets" / "Final" / asset_id / f"{asset_id}__use_sheet__{version}.png")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", required=True, type=Path)
    parser.add_argument("--atlas", required=True, type=Path)
    parser.add_argument("--version", default="v09")
    args = parser.parse_args()
    process_manifest(args.manifest, args.atlas, args.version)


if __name__ == "__main__":
    main()
