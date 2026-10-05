"""Verify explicit console tile audits; generation counts alone are insufficient."""
import json
import sys
from pathlib import Path

EXPECTED = {"SwordTabletTile": (3, 4), "SingingThunderStoneTile": (2, 3),
            "RiftMembraneTile": (2, 3), "BrokenHeavenTabletTile": (4, 2),
            "ArchiveLightPillarTile": (6, 2)}

def verify(run):
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
    for extension in ("wld", "twld"):
        assert (root / "save/Worlds" / f"GenerationProbe.{extension}").stat().st_size > 0
    report["Scope"] = "Final loaded tile counts and frames; accessibility/biome activation unverified"
    (root / "save-audit-report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(f"Saved tile audit passed: {report['Width']}x{report['Height']}; {root}")

if __name__ == "__main__":
    for run in sys.argv[1:]:
        verify(run)
