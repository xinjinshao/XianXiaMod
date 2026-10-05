"""Ensure every summon uses the shared transaction without changing its target."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def verify() -> None:
    paths = list((ROOT / "Content/Items/BossSummons").glob("*.cs"))
    if len(paths) != 12:
        raise SystemExit(f"Expected twelve boss summon items, found {len(paths)}")
    targets = set()
    for path in paths:
        text = path.read_text(encoding="utf-8")
        if not re.search(r"class \w+ : global::XianXia.Common.Items.CultivationBossSummonItem", text):
            raise SystemExit(f"Summon bypasses shared transaction: {path.name}")
        if "SpawnOnPlayer" in text or "override bool? UseItem" in text:
            raise SystemExit(f"Summon retains a direct spawn/use hook: {path.name}")
        boss_type = re.search(r"override int BossType => ModContent.NPCType<([^>]+)>", text)
        can_use = re.search(r"CanUseBossSummon\(\s*ModContent.NPCType<([^>]+)>", text)
        if not boss_type or not can_use or boss_type[1] != can_use[1]:
            raise SystemExit(f"Summoning target and eligibility target differ: {path.name}")
        targets.add(boss_type[1])
    if len(targets) != 12:
        raise SystemExit("Summon items must cover twelve distinct bosses")
    print("Boss summon transaction coverage verified: 12 items / 12 targets.")


if __name__ == "__main__":
    verify()
