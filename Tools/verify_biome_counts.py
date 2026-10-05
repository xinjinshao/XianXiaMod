"""Compare server biome rules with their existing client scene count sources."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
sources = {p.stem: p for folder in (ROOT / "Common", ROOT / "Content") for p in folder.rglob("*.cs")}
tile_types = re.compile(r"TileType<(?:[\w:.]+\.)?(\w+)>")
checked = 0
for path in (ROOT / "Content/Biomes").glob("*Biome.cs"):
    text = path.read_text(encoding="utf-8")
    client = re.search(r"return ModContent.GetInstance<(\w+)>\(\)\.(\w+) >= (\d+);", text)
    server = re.search(r"if \(Main.netMode == Terraria.ID.NetmodeID.Server\)\s*return (.+?);", text, re.S)
    if not client or not server:
        raise SystemExit(f"Missing paired client/server count rule: {path.name}")
    system, field, threshold = client.groups()
    assignment = re.search(rf"\b{re.escape(field)}\s*=\s*([^;]+);", sources[system].read_text(encoding="utf-8"))
    if not assignment:
        raise SystemExit(f"Missing client scene assignment: {system}.{field}")
    if sorted(tile_types.findall(assignment[1])) != sorted(tile_types.findall(server[1])):
        raise SystemExit(f"Client/server counted tile types differ: {path.name}")
    if not re.search(rf">=\s*{threshold}\s*$", server[1]):
        raise SystemExit(f"Client/server activation threshold differs: {path.name}")
    checked += 1
if checked != 8:
    raise SystemExit(f"Expected eight paired biome rules, found {checked}")
print(f"Client/server biome count parity verified: {checked} biomes.")
