"""Validate a real isolated generation run; not a map accessibility audit."""
import argparse
import json
from pathlib import Path
import re

parser = argparse.ArgumentParser()
parser.add_argument("run", type=Path)
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
report = {
    "scope": "generation-stage object placement, server startup, and saved files",
    "objects": [{"tile_type": int(t), "placed": int(p), "target": int(n), "regions": int(r)} for t, p, n, r in records],
    "limitations": "Does not inspect final persisted tile counts, accessibility, biome thresholds, or gameplay."
}
(args.run / "worldgen-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print("Real generation log verified: five object targets met, server started, WLD/TWLD saved.")
