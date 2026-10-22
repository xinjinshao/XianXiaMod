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


def verify_star_bridges(report):
    bridges = report.get("StarAbyssBridges")
    assert isinstance(bridges, list) and 0 < len(bridges) <= 3, "No saved star abyss bridges"
    for bridge in bridges:
        assert (bridge["Width"], bridge["Height"], bridge["CrystalTiles"]) == (45, 27, 252)
        assert all(bridge.get(flag) is True for flag in ("GeometryIntact", "InteriorClear", "Dry", "MembraneIntact")), bridge


def verify_moonbone_terraces(report):
    terraces = report.get("MoonboneTerraces")
    assert isinstance(terraces, list) and 0 < len(terraces) <= 2, "No saved moonbone terraces"
    for terrace in terraces:
        assert (terrace["Width"], terrace["Height"], terrace["BoneTiles"]) == (47, 23, 316)
        assert all(terrace.get(flag) is True for flag in ("GeometryIntact", "InteriorClear", "Dry")), terrace


def verify_shallow_chambers(report):
    chambers = report.get("ShallowSpiritChambers")
    assert isinstance(chambers, list) and 0 < len(chambers) <= 3, "No saved shallow spirit chambers"
    for chamber in chambers:
        assert (chamber["Width"], chamber["Height"]) == (29, 15)
        assert 1 <= chamber["OreTiles"] <= 64 and chamber["OreTiles"] + chamber["MossTiles"] == 119
        assert chamber["ExposedOreTiles"] == min(chamber["OreTiles"], 6)
        assert all(chamber.get(flag) is True for flag in ("GeometryIntact", "InteriorClear", "Dry", "OreSeamIntact")), chamber


def verify_sect_halls(report):
    for hall in report["SectRuinHalls"]:
        assert hall["Bricks"] == 278 and hall["BlockedInterior"] == 0 and hall["SwordTablets"] >= 2, hall
        assert hall["EntrancesOpen"] is True and hall["Dry"] is True, hall
        chests = hall.get("SupplyChests")
        assert isinstance(chests, list) and len(chests) <= 1
        for chest in chests:
            assert chest["Intact"] is True and (chest["x"], chest["y"]) == (hall["X"] + 19, hall["Y"] + 14), chest


STRUCTURES = {
    "SectRuinHalls": (2, 41, 41, 19, verify_sect_halls),
    "SectRuinPaths": (2, 5, 48, 7, verify_sect_paths),
    "GreenwoodGardens": (3, 31, 31, 13, verify_gardens),
    "FurnaceWorkshops": (3, 35, 35, 17, verify_workshops),
    "FurnaceWorkshopPaths": (3, 5, 40, 6, verify_workshop_paths),
    "ThunderCloudShrines": (3, 51, 51, 13, verify_thunder_shrines),
    "FallenHeavenCourts": (2, 49, 49, 23, verify_heaven_courts),
    "StarAbyssBridges": (3, 45, 45, 27, verify_star_bridges),
    "MoonboneTerraces": (2, 47, 47, 23, verify_moonbone_terraces),
    "ShallowSpiritChambers": (3, 29, 29, 15, verify_shallow_chambers),
}


def verify_all_structures(report):
    """Check every recorded fresh structure, including cross-system rectangle conflicts.

    A safe placement may be skipped; empty lists are reported rather than treated as coverage.
    This cannot discover structures missing from the saved registry or prove player traversal.
    """
    rectangles = []
    counts = {}
    for name, (limit, min_width, max_width, height, validate) in STRUCTURES.items():
        entries = report.get(name)
        assert isinstance(entries, list) and len(entries) <= limit, f"Missing or excessive registry: {name}"
        counts[name] = len(entries)
        for entry in entries:
            x, y, w, h = (entry[key] for key in ("X", "Y", "Width", "Height"))
            assert all(type(value) is int for value in (x, y, w, h)), f"Noninteger bounds: {name}"
            assert min_width <= w <= max_width and h == height, f"Invalid dimensions: {name}: {entry}"
            assert 12 <= x <= report["Width"] - w - 12 and 12 <= y <= report["Height"] - h - 12, f"Out of world: {name}: {entry}"
            for other_name, ox, oy, ow, oh in rectangles:
                assert not (x < ox + ow and x + w > ox and y < oy + oh and y + h > oy), f"Overlapping structures: {name} and {other_name}"
            rectangles.append((name, x, y, w, h))
        if entries:
            validate(report)
    return counts


def verify(run, *, required=(), all_structures=False):
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
    for name in required:
        STRUCTURES[name][-1](report)
    if all_structures:
        report["VerifiedStructureCounts"] = verify_all_structures(report)
    for extension in ("wld", "twld"):
        assert (root / "save/Worlds" / f"GenerationProbe.{extension}").stat().st_size > 0
    report["Scope"] = ("Final loaded counts/frames and all registered fresh structure geometry/bounds; unregistered structures, global accessibility and full biome activation unverified"
                       if all_structures else "Final loaded tile counts and frames; accessibility/biome activation unverified")
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
    parser.add_argument("--require-star-bridges", action="store_true", help="Require broken crystal crossings, dry lower landings and complete rift membranes")
    parser.add_argument("--require-moonbone-terraces", action="store_true", help="Require dry lunar terraces with intact steps, raised center and broken ribs")
    parser.add_argument("--require-shallow-chambers", action="store_true", help="Require dry starter chambers with intact exposed spirit-ore seams")
    parser.add_argument("--verify-structures", action="store_true", help="Validate every recorded fresh structure, world bounds and cross-system overlaps; report skipped placements as zero")
    args = parser.parse_args()
    required = [name for option, name in (
        ("require_workshops", "FurnaceWorkshops"),
        ("require_workshop_paths", "FurnaceWorkshopPaths"),
        ("require_sect_paths", "SectRuinPaths"),
        ("require_greenwood_gardens", "GreenwoodGardens"),
        ("require_thunder_shrines", "ThunderCloudShrines"),
        ("require_heaven_courts", "FallenHeavenCourts"),
        ("require_star_bridges", "StarAbyssBridges"),
        ("require_moonbone_terraces", "MoonboneTerraces"),
        ("require_shallow_chambers", "ShallowSpiritChambers"),
    ) if getattr(args, option)]
    for run in args.runs:
        verify(run, required=required, all_structures=args.verify_structures)
