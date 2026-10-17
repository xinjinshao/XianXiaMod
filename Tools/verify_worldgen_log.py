"""Validate a real isolated generation run; not a map accessibility audit."""
import argparse
import json
from pathlib import Path
import re

parser = argparse.ArgumentParser()
parser.add_argument("run", type=Path)
parser.add_argument("--require-sect-halls", action="store_true", help="Require persisted open sect halls in the final read-only world audit")
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
hall_reports = []
if args.require_sect_halls:
    audits = re.findall(r"SavedWorldAudit: (\{[^\r\n]+\})", log)
    assert audits, "Run xianxia-world-audit before saving"
    hall_reports = json.loads(audits[-1]).get("SectRuinHalls", [])
    assert 1 <= len(hall_reports) <= 2, "Expected one or two safely placed sect halls"
    for hall in hall_reports:
        assert (hall["Width"], hall["Height"]) == (41, 19), "Unexpected hall geometry"
        assert hall["Bricks"] >= 180 and hall["BlockedInterior"] == 0, "Hall shell or interior invalid"
        assert hall["EntrancesOpen"] is True and hall["Dry"] is True, "Hall entrance or liquid obstruction"
        assert hall["SwordTablets"] >= 2, "Hall lacks its two exploration sword tablets"
report = {
    "scope": "generation-stage object placement, server startup, and saved files",
    "sect_halls": hall_reports,
    "objects": [{"tile_type": int(t), "placed": int(p), "target": int(n), "regions": int(r)} for t, p, n, r in records],
    "limitations": "Does not inspect final persisted tile counts, accessibility, biome thresholds, or gameplay."
}
(args.run / "worldgen-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print("Real generation log verified: five object targets met, server started, WLD/TWLD saved.")
