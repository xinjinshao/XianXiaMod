"""Validate a real isolated generation run; not a map accessibility audit."""
import argparse
import json
from pathlib import Path
import re

parser = argparse.ArgumentParser()
parser.add_argument("run", type=Path)
parser.add_argument("--require-sect-halls", action="store_true", help="Require persisted open sect halls in the final read-only world audit")
parser.add_argument("--require-sect-supplies", action="store_true", help="Also require intact fresh hall supply chests and advertised loot")
parser.add_argument("--require-sect-paths", action="store_true", help="Require at least one persisted clear protected hall-to-cave connector")
parser.add_argument("--require-greenwood-gardens", action="store_true", help="Require fresh planted Greenwood clearings in the final scan")
args = parser.parse_args()
log = (args.run / "runtime.log").read_text(encoding="utf-8-sig")
records = re.findall(r"\[XianXia\]: Cultivation object (\d+): (\d+)/(\d+) placed in (\d+) matching regions", log)
assert len(records) == 5, "Expected five object placement reports"
assert len({row[0] for row in records}) == 5, "Duplicate object reports"
assert sorted(int(row[2]) for row in records) == [2, 2, 3, 3, 4], "Unexpected placement targets"
assert all(int(row[1]) == int(row[2]) and int(row[3]) > 0 for row in records), "Object shortage"
assert "Server started" in log, "Generated world did not start"
assert "Saving modded world data" in log, "Modded world was not saved"
assert "/ERROR]" not in log, "Runtime error in generation log"
assert "Mods.XianXia.WorldGeneration.CultivationDomains" not in log, "Untranslated world-generation key"
worlds = args.run / "save" / "Worlds"
for suffix in ("wld", "twld"):
    assert (worlds / f"GenerationProbe.{suffix}").stat().st_size > 0, f"Missing saved {suffix}"
audits = re.findall(r"SavedWorldAudit: (\{[^\r\n]+\})", log)
if args.require_sect_halls or args.require_sect_supplies or args.require_sect_paths or args.require_greenwood_gardens:
    assert audits, "Run xianxia-world-audit before saving"
hall_reports = []
if args.require_sect_halls or args.require_sect_supplies or args.require_sect_paths:
    hall_reports = json.loads(audits[-1]).get("SectRuinHalls", [])
    assert 1 <= len(hall_reports) <= 2, "Expected one or two safely placed sect halls"
    for hall in hall_reports:
        assert (hall["Width"], hall["Height"]) == (41, 19), "Unexpected hall geometry"
        assert hall["Bricks"] >= 180 and hall["BlockedInterior"] == 0, "Hall shell or interior invalid"
        assert hall["EntrancesOpen"] is True and hall["Dry"] is True, "Hall entrance or liquid obstruction"
        assert hall["SwordTablets"] >= 2, "Hall lacks its two exploration sword tablets"
        if args.require_sect_supplies:
            chests = hall.get("SupplyChests", [])
            assert len(chests) == 1 and chests[0]["Intact"] is True, "Hall supply chest missing or damaged"
            items = chests[0]["Items"]
            assert len(items) == 4, "Unexpected hall supply loot"
            loot = {item["Name"]: item["Stack"] for item in items}
            assert 6 <= loot.get("LowGradeSpiritStone", 0) <= 10 and 2 <= loot.get("ArtifactBlankShard", 0) <= 4, "Invalid hall material supplies"
            assert 1 <= loot.get("RecallPotion", 0) <= 2 and loot.get("Torch") == 10, "Invalid hall exploration supplies"
path_reports = []
if args.require_sect_paths:
    path_reports = json.loads(audits[-1]).get("SectRuinPaths", [])
    assert 1 <= len(path_reports) <= len(hall_reports), "Expected one connector per connected hall"
    for path in path_reports:
        assert 5 <= path["Width"] <= 48 and path["Height"] == 7, "Invalid connector bounds"
        assert all(path.get(flag) is True for flag in ("InteriorClear", "ShellIntact", "Dry", "AttachedHall", "OpenCavityExit")), "Connector obstructed, detached or sealed off from its cave"
garden_reports = []
if args.require_greenwood_gardens:
    garden_reports = json.loads(audits[-1]).get("GreenwoodGardens", [])
    assert 1 <= len(garden_reports) <= 3, "Expected one to three safe Greenwood clearings"
    for garden in garden_reports:
        assert (garden["Width"], garden["Height"]) == (31, 13) and garden["SoilTiles"] == 124, "Invalid herb bed geometry"
        assert all(garden.get(flag) is True for flag in ("BedIntact", "WalkwayClear", "Dry")), "Garden obstructed or flooded"
        assert garden["Crops"] == 6 and len(garden["CropFrames"]) == 6, "Garden crops incomplete"
        assert all(frame in (0, 18, 36) for frame in garden["CropFrames"]), "Invalid herb growth frames"
report = {
    "scope": "generation-stage object placement, server startup, and saved files; optional final read-only sect hall/chest/path and Greenwood scans",
    "sect_halls": hall_reports,
    "sect_paths": path_reports,
    "greenwood_gardens": garden_reports,
    "objects": [{"tile_type": int(t), "placed": int(p), "target": int(n), "regions": int(r)} for t, p, n, r in records],
    "limitations": "Only requested scans inspect final geometry, plants and chest data. No actual player traversal, global cave connectivity, full biome matrix, client rendering or gameplay acceptance."
}
(args.run / "worldgen-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print("Real generation log verified: five object targets met, server started, WLD/TWLD saved.")
