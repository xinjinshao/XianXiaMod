from __future__ import annotations

from collections import deque
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
TARGET_ROOTS = [
    ROOT / "Content" / "Items",
    ROOT / "Content" / "Buffs",
    ROOT / "Content" / "Projectiles",
    ROOT / "Content" / "NPCs" / "Enemies",
    ROOT / "Content" / "NPCs" / "Town",
    ROOT / "Common" / "UI",
]


def components(image: Image.Image) -> list[list[tuple[int, int]]]:
    alpha = image.getchannel("A")
    width, height = image.size
    pix = alpha.load()
    seen: set[tuple[int, int]] = set()
    found: list[list[tuple[int, int]]] = []
    for y in range(height):
        for x in range(width):
            if pix[x, y] == 0 or (x, y) in seen:
                continue
            queue: deque[tuple[int, int]] = deque([(x, y)])
            seen.add((x, y))
            points: list[tuple[int, int]] = []
            while queue:
                cx, cy = queue.popleft()
                points.append((cx, cy))
                for nx in range(cx - 1, cx + 2):
                    for ny in range(cy - 1, cy + 2):
                        if nx < 0 or ny < 0 or nx >= width or ny >= height:
                            continue
                        if (nx, ny) in seen or pix[nx, ny] == 0:
                            continue
                        seen.add((nx, ny))
                        queue.append((nx, ny))
            found.append(points)
    return sorted(found, key=len, reverse=True)


def harden(path: Path) -> bool:
    image = Image.open(path).convert("RGBA")
    original = list(image.getdata())
    hardened = []
    for r, g, b, a in original:
        if a < 96:
            hardened.append((0, 0, 0, 0))
        else:
            hardened.append((r, g, b, 255))
    image.putdata(hardened)

    pix = image.load()
    for comp in components(image)[1:]:
        if len(comp) <= 3:
            for x, y in comp:
                pix[x, y] = (0, 0, 0, 0)

    if list(image.getdata()) == original:
        return False
    image.save(path)
    return True


def main() -> None:
    changed: list[str] = []
    for root in TARGET_ROOTS:
        if not root.exists():
            continue
        for path in sorted(root.rglob("*.png")):
            if harden(path):
                changed.append(path.relative_to(ROOT).as_posix())
    print({"changed": len(changed), "paths": changed[:40]})


if __name__ == "__main__":
    main()
