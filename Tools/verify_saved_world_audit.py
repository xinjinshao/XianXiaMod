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


def verify(run, require_workshops=False, require_paths=False):
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
    args = parser.parse_args()
    for run in args.runs:
        verify(run, args.require_workshops, args.require_workshop_paths)
