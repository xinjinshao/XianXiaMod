from __future__ import annotations

import struct
import sys
from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"


def png_size(path: Path) -> tuple[int, int]:
    with path.open("rb") as f:
        signature = f.read(8)
        if signature != PNG_SIGNATURE:
            raise ValueError("invalid PNG signature")

        length_bytes = f.read(4)
        chunk_type = f.read(4)
        if len(length_bytes) != 4 or chunk_type != b"IHDR":
            raise ValueError("missing IHDR chunk")

        length = struct.unpack(">I", length_bytes)[0]
        if length < 8:
            raise ValueError("invalid IHDR length")

        width, height = struct.unpack(">II", f.read(8))
        return width, height


def main() -> int:
    invalid: list[str] = []
    checked = 0

    for root_name in ("Assets", "Content"):
        for path in (ROOT / root_name).rglob("*.png"):
            checked += 1
            try:
                width, height = png_size(path)
                if width <= 0 or height <= 0:
                    raise ValueError(f"invalid dimensions {width}x{height}")
            except Exception as exc:
                invalid.append(f"{path.relative_to(ROOT)}: {exc}")

    mod_texture_roots = [
        ("Content/Items", "ModItem"),
        ("Content/Tiles", "ModTile"),
        ("Content/Projectiles", "ModProjectile"),
        ("Content/Buffs", "ModBuff"),
    ]
    for folder, base_class in mod_texture_roots:
        for source in (ROOT / folder).rglob("*.cs"):
            text = source.read_text(encoding="utf-8")
            if base_class not in text:
                continue

            matches = re.findall(rf"public\s+class\s+(\w+)\s*:\s*{base_class}", text)
            if not matches:
                invalid.append(f"{source.relative_to(ROOT)}: could not find {base_class} class name")
                continue

            for class_name in matches:
                texture_override = re.search(r'override\s+string\s+Texture\s*=>\s*"XianXia/([^"]+)"', text)
                if texture_override:
                    texture = ROOT / f"{texture_override.group(1)}.png"
                else:
                    texture = source.parent / f"{class_name}.png"
                if not texture.exists():
                    invalid.append(f"{source.relative_to(ROOT)}: missing client texture {texture.relative_to(ROOT)}")

    if invalid:
        print("Invalid PNG assets:")
        for line in invalid:
            print(f"  {line}")
        return 1

    print(f"PNG assets verified: {checked} files.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
