"""Verify all current energy weapons retain their cost and shared firing path."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def verify() -> None:
    checked = 0
    arrays = 0
    for path in (ROOT / "Content/Items/Weapons").glob("*.cs"):
        text = path.read_text(encoding="utf-8")
        if "CanConsumeSpiritualEnergy" not in text and "CanDeployArray" not in text:
            continue
        if ": global::XianXia.Common.Items.CultivationWeaponItem" not in text:
            raise SystemExit(f"Weapon bypasses authority firing path: {path.name}")
        if "override bool Shoot" in text or "TryConsumeSpiritualEnergy" in text or "TryDeployArray" in text:
            raise SystemExit(f"Weapon retains its own firing/spending path: {path.name}")
        cost = re.search(r"GetSpiritCost\(Player player\) => ([^;]+);", text)
        check = re.search(r"\.Can(?:ConsumeSpiritualEnergy|DeployArray)\((?:Item.shoot, )?(.+?)\);", text)
        if not cost or not check or (re.sub(r"\s", "", check[1]) != "GetSpiritCost(player)" and re.sub(r"\s", "", cost[1]) != re.sub(r"\s", "", check[1])):
            raise SystemExit(f"Eligibility and actual firing costs differ: {path.name}")
        array = "DeploysArray => true" in text
        if array != ("CanDeployArray" in text):
            raise SystemExit(f"Array eligibility and deployment identity differ: {path.name}")
        arrays += array
        checked += 1
    if checked != 12 or arrays != 2:
        raise SystemExit(f"Expected 12 energy weapons including 2 arrays, found {checked}/{arrays}")
    print("Authority firing/cost coverage verified: 12 weapons / 2 arrays.")


if __name__ == "__main__":
    verify()
