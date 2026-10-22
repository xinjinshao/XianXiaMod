"""Verify explicit console tile audits; generation counts alone are insufficient."""
import argparse
import json
from pathlib import Path

EXPECTED = {"SwordTabletTile": (3, 4), "SingingThunderStoneTile": (2, 3),
            "RiftMembraneTile": (2, 3), "BrokenHeavenTabletTile": (4, 2),
            "ArchiveLightPillarTile": (6, 2)}

def verify_workshops(report):
    rooms = report.get("FurnaceWorkshops")
    assert isinstance(rooms, list) and 0 < len(rooms) <= 3, "Missing or excessive furnace workshops"
    positions = set()
    for room in rooms:
        assert (room["Width"], room["Height"]) == (35, 17), f"Workshop dimensions: {room}"
        assert room["SlagTiles"] == 238 and room["ShellIntact"] is True, f"Workshop shell: {room}"
        assert room["FurnaceCells"] == 6 and room["AnvilCells"] == 2, f"Workshop stations: {room}"
        assert room.get("StationsIntact") is True, f"Workshop station frames: {room}"
        assert room["BlockedInterior"] == 0 and room["Dry"] is True, f"Workshop interior: {room}"
        point = (room["X"], room["Y"])
        assert point not in positions, "Duplicate workshop bounds"
        assert 12 <= point[0] <= report["Width"] - 47 and 12 <= point[1] <= report["Height"] - 29
        positions.add(point)


def verify_workshop_paths(report):
    paths = report.get("FurnaceWorkshopPaths")
    assert isinstance(paths, list) and 0 < len(paths) <= 3, "No saved workshop access paths"
    for path in paths:
        assert path["Height"] == 6 and 5 <= path["Width"] <= 40
        assert all(path.get(flag) is True for flag in ("InteriorClear", "ShellIntact", "Dry", "OpenCavityExit")), path


def verify_sect_paths(report):
    paths = report.get("SectRuinPaths")
    assert isinstance(paths, list) and 0 < len(paths) <= 2, "No saved sect hall access paths"
    for path in paths:
        assert path["Height"] == 7 and 5 <= path["Width"] <= 48
        assert all(path.get(flag) is True for flag in ("InteriorClear", "ShellIntact", "Dry", "AttachedHall", "OpenCavityExit")), path


def verify_gardens(report):
    gardens = report.get("GreenwoodGardens")
    assert isinstance(gardens, list) and 0 < len(gardens) <= 3, "No saved Greenwood gardens"
    for garden in gardens:
        assert (garden["Width"], garden["Height"], garden["SoilTiles"]) == (31, 13, 124)
        assert garden["Crops"] == 6 and sorted(garden["CropFrames"]) == [0, 0, 18, 18, 36, 36]
        assert all(garden.get(flag) is True for flag in ("BedIntact", "WalkwayClear", "Dry", "ShelterIntact")), garden


def verify_thunder_shrines(report):
    shrines = report.get("ThunderCloudShrines")
    assert isinstance(shrines, list) and 0 < len(shrines) <= 3, "No saved thunder cloud shrines"
    for shrine in shrines:
        assert (shrine["Width"], shrine["Height"], shrine["CloudTiles"]) == (51, 13, 153)
        assert all(shrine.get(flag) is True for flag in ("PlatformIntact", "AirspaceClear", "Dry", "StoneIntact")), shrine


def verify_heaven_courts(report):
    courts = report.get("FallenHeavenCourts")
    assert isinstance(courts, list) and 0 < len(courts) <= 2, "No saved fallen heaven courts"
    for court in courts:
        assert (court["Width"], court["Height"], court["JadeTiles"]) == (49, 23, 247)
        assert all(court.get(flag) is True for flag in ("ShellIntact", "InteriorClear", "EntrancesOpen", "Dry", "TabletIntact", "PillarIntact")), court


def verify(run, require_workshops=False, require_paths=False, require_sect_paths=False, require_gardens=False, require_shrines=False, require_courts=False):
    root = Path(run)
    log = (root / "runtime.log").read_text(encoding="utf-8-sig")
    reports = [json.loads(line.split("SavedWorldAudit: ", 1)[1])
               for line in log.splitlines() if "SavedWorldAudit: " in line]
    assert reports, "No actual console tile audit recorded"
    report = reports[-1]
    assert "Server started" in log and "[ERROR]" not in log, "Server startup/error check failed"
    entries = report["Objects"]
    assert len(entries) == len(EXPECTED) and {e["Name"] for e in entries} == set(EXPECTED)
    for entry in entries:
        height, count = EXPECTED[entry["Name"]]
        assert entry["Height"] == height
        assert entry["Origins"] == entry["Complete"] == count, f"Missing/incomplete: {entry}"
        assert entry["Cells"] == count * height * 2, f"Orphan tiles: {entry}"
    if require_workshops:
        verify_workshops(report)
    if require_paths:
        verify_workshop_paths(report)
    if require_sect_paths:
        verify_sect_paths(report)
    if require_gardens:
        verify_gardens(report)
    if require_shrines:
        verify_thunder_shrines(report)
    if require_courts:
        verify_heaven_courts(report)
    for extension in ("wld", "twld"):
        assert (root / "save/Worlds" / f"GenerationProbe.{extension}").stat().st_size > 0
    report["Scope"] = "Final loaded tile counts and frames; accessibility/biome activation unverified"
    (root / "save-audit-report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(f"Saved tile audit passed: {report['Width']}x{report['Height']}; {root}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("runs", nargs="+")
    parser.add_argument("--require-workshops", action="store_true", help="Require saved furnace room shells and both vanilla station footprints")
    parser.add_argument("--require-workshop-paths", action="store_true")
    parser.add_argument("--require-sect-paths", action="store_true", help="Require hall connectors with an open dry cave beyond the finished tunnel")
    parser.add_argument("--require-greenwood-gardens", action="store_true", help="Require planted gardens and intact vanilla alchemy shelters")
    parser.add_argument("--require-thunder-shrines", action="store_true", help="Require flat thunder cloud platforms and complete singing stones")
    parser.add_argument("--require-heaven-courts", action="store_true", help="Require open jade courts with complete tablets and archive pillars")
    args = parser.parse_args()
    for run in args.runs:
        verify(run, args.require_workshops, args.require_workshop_paths, args.require_sect_paths, args.require_greenwood_gardens, args.require_thunder_shrines, args.require_heaven_courts)
