from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw


FINAL = Path("Assets/Final")


def save(path: Path, image: Image.Image) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


def draw_spiritual_energy_bar_frame() -> Image.Image:
    img = Image.new("RGBA", (164, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    gold_dark = (92, 58, 25, 255)
    gold = (196, 150, 63, 255)
    gold_light = (255, 225, 133, 255)
    jade = (65, 232, 222, 255)
    dark = (13, 22, 25, 230)

    d.rectangle((8, 3, 155, 12), outline=gold_dark, fill=dark)
    d.line((9, 4, 154, 4), fill=gold_light)
    d.line((9, 12, 154, 12), fill=gold)
    d.rectangle((0, 5, 10, 10), outline=gold, fill=(35, 38, 34, 255))
    d.rectangle((153, 5, 163, 10), outline=gold, fill=(35, 38, 34, 255))
    for cx in (82,):
        d.polygon([(cx, 1), (cx + 4, 5), (cx, 9), (cx - 4, 5)], fill=gold_light)
        d.polygon([(cx, 3), (cx + 2, 5), (cx, 7), (cx - 2, 5)], fill=jade)
    d.point((6, 13), fill=jade)
    d.point((157, 13), fill=jade)
    return img


def draw_spiritual_energy_bar_fill() -> Image.Image:
    img = Image.new("RGBA", (160, 12), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    for x in range(2, 158):
        t = x / 157
        color = (
            int(16 + 54 * t),
            int(174 + 64 * t),
            int(178 + 52 * t),
            245,
        )
        d.line((x, 3, x, 8), fill=color)
    d.line((2, 2, 157, 2), fill=(190, 255, 245, 220))
    d.line((2, 9, 157, 9), fill=(18, 92, 112, 220))
    return img


def draw_tribulation_warning_line() -> Image.Image:
    img = Image.new("RGBA", (16, 4), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.line((0, 1, 5, 1), fill=(255, 226, 124, 230))
    d.line((10, 1, 15, 1), fill=(255, 226, 124, 230))
    d.line((5, 0, 8, 3), fill=(255, 94, 48, 255))
    d.line((8, 3, 10, 0), fill=(255, 181, 72, 255))
    d.point((2, 2), fill=(255, 110, 70, 180))
    d.point((13, 2), fill=(255, 110, 70, 180))
    return img


def main() -> None:
    save(
        FINAL / "spiritual_energy_bar_frame" / "spiritual_energy_bar_frame__ui__v01.png",
        draw_spiritual_energy_bar_frame(),
    )
    save(
        FINAL / "spiritual_energy_bar_fill" / "spiritual_energy_bar_fill__ui__v01.png",
        draw_spiritual_energy_bar_fill(),
    )
    save(
        FINAL / "tribulation_warning_line" / "tribulation_warning_line__ui__v01.png",
        draw_tribulation_warning_line(),
    )
    print({"repaired": 3})


if __name__ == "__main__":
    main()
