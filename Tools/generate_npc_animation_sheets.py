from __future__ import annotations

import math
import re
from pathlib import Path

from PIL import Image, ImageEnhance, ImageFilter


ROOT = Path(".")

FRAME_COUNTS = {
    "Enemies": 6,
    "Bosses": 6,
    "Town": 4,
}


def snake(name: str) -> str:
    name = re.sub(r"(.)([A-Z][a-z]+)", r"\1_\2", name)
    name = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", name)
    return name.lower()


def asset_id_for(path: Path) -> str:
    stem = path.stem
    aliases = {
        "SpiritVeinWyrm": "spirit_vein_wyrm",
        "ShatteredJadeWyrmMinion": "shattered_jade_worm",
    }
    return aliases.get(stem, snake(stem))


def source_for(path: Path) -> Path:
    asset_id = asset_id_for(path)
    final = ROOT / "Assets" / "Final" / asset_id
    for suffix in ("base", "body"):
        candidate = final / f"{asset_id}__{suffix}__v01.png"
        if candidate.exists():
            return candidate
    return path


def alpha_bbox(image: Image.Image) -> tuple[int, int, int, int] | None:
    return image.convert("RGBA").getchannel("A").getbbox()


def center_sprite(base: Image.Image, dx: int, dy: int, squash: float, glow: bool, pulse: float) -> Image.Image:
    rgba = base.convert("RGBA")
    bbox = alpha_bbox(rgba)
    canvas = Image.new("RGBA", rgba.size, (0, 0, 0, 0))
    if bbox is None:
        return canvas

    sprite = rgba.crop(bbox)
    if squash != 1.0:
        new_h = max(1, int(sprite.height * squash))
        sprite = sprite.resize((sprite.width, new_h), Image.Resampling.NEAREST)

    x = (rgba.width - sprite.width) // 2 + dx
    y = (rgba.height - sprite.height) // 2 + dy

    if glow:
        glow_layer = Image.new("RGBA", rgba.size, (0, 0, 0, 0))
        glow_sprite = ImageEnhance.Brightness(sprite).enhance(1.12 + pulse * 0.2)
        glow_alpha = glow_sprite.getchannel("A").point(lambda a: int(a * (0.25 + pulse * 0.18)))
        glow_sprite.putalpha(glow_alpha)
        glow_layer.alpha_composite(glow_sprite, (x, y))
        glow_layer = glow_layer.filter(ImageFilter.GaussianBlur(radius=0.45))
        canvas.alpha_composite(glow_layer)

    canvas.alpha_composite(sprite, (x, y))
    return canvas


def make_frames(base: Image.Image, count: int, family: str, name: str) -> list[Image.Image]:
    frames: list[Image.Image] = []
    for i in range(count):
        t = i / count
        wave = math.sin(t * math.tau)
        pulse = (math.sin(t * math.tau) + 1) / 2

        if family == "Town":
            dx = int(round(math.sin(t * math.tau) * 1))
            dy = -1 if i == 1 else 0
            squash = 0.98 if i == 2 else 1.0
            glow = "Scroll" in name or "Messenger" in name
        elif "Worm" in name or "Wyrm" in name or "Jiao" in name:
            dx = int(round(math.sin(t * math.tau) * 3))
            dy = int(round(math.cos(t * math.tau) * 1))
            squash = 1.0
            glow = True
        elif "Bat" in name or "Moth" in name or "Hawk" in name:
            dx = int(round(math.sin(t * math.tau) * 2))
            dy = int(round(math.cos(t * math.tau) * 3))
            squash = 0.94 + 0.08 * pulse
            glow = False
        elif "Slime" in name or "Larva" in name:
            dx = 0
            dy = int(round(-2 * max(0, wave)))
            squash = 1.08 if i in {1, 2} else 0.94 if i in {4, 5} else 1.0
            glow = True
        elif family == "Bosses":
            dx = int(round(math.sin(t * math.tau) * 2))
            dy = int(round(math.cos(t * math.tau) * 2))
            squash = 0.99 + 0.02 * pulse
            glow = True
        else:
            dx = int(round(math.sin(t * math.tau) * 1))
            dy = int(round(math.cos(t * math.tau) * 1))
            squash = 0.97 + 0.04 * pulse
            glow = "Soul" in name or "Cloud" in name or "Spirit" in name

        frames.append(center_sprite(base, dx, dy, squash, glow, pulse))
    return frames


def save_sheet(frames: list[Image.Image], out_path: Path) -> None:
    width, height = frames[0].size
    sheet = Image.new("RGBA", (width, height * len(frames)), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        sheet.alpha_composite(frame, (0, i * height))
    out_path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(out_path)


def main() -> None:
    generated: list[str] = []
    for family, count in FRAME_COUNTS.items():
        for path in sorted((ROOT / "Content" / "NPCs" / family).glob("*.png")):
            if "_Head" in path.stem:
                continue

            source = source_for(path)
            with Image.open(source) as raw:
                base = raw.convert("RGBA")

            frames = make_frames(base, count, family, path.stem)
            save_sheet(frames, path)

            asset_id = asset_id_for(path)
            final_path = ROOT / "Assets" / "Final" / asset_id / f"{asset_id}__animation_sheet__v01.png"
            save_sheet(frames, final_path)
            generated.append(path.as_posix())

    print({"animated_sheets": len(generated), "files": generated})


if __name__ == "__main__":
    main()
