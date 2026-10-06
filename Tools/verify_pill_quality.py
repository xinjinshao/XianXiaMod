"""Protect the quality-aware crafting boundary against tML's cursor merge path."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def main() -> None:
    # Main.CraftItem checks the recipe template before OnCreated, then invokes
    # StackItems directly. A randomly graded output must start on an empty cursor.
    for name in ("QiCondensingPill", "FoundationPill", "SpringReturnPill", "TribulationResistingPill", "QiRecoveryPill", "FurnaceGuardPill", "WindStepPill", "ThunderBurstPill"):
        text = (ROOT / f"Content/Items/Materials/{name}.cs").read_text(encoding="utf-8")
        if text.count(".Register();") != text.count("Main.mouseItem.IsAir"):
            raise SystemExit(f"{name}: each recipe needs the empty-cursor quality boundary")
        if "PillQualitySystem.Scale(Item," not in text:
            raise SystemExit(f"{name}: missing quality-dependent benefit")
    system = (ROOT / "Common/Systems/PillQualitySystem.cs").read_text(encoding="utf-8")
    bonus = system.split("public static void ApplyUseBonus", 1)[1]
    if "Main.rand" in bonus:
        raise SystemExit("Stored quality must not be rerolled during consumption")
    print("Pill quality crafting boundary verified: 8 recipes; no use-time reroll.")


if __name__ == "__main__":
    main()
