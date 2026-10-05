"""Check the six material types, textures and dynamic localized inscription UI."""
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def verify() -> None:
    equipment = (ROOT / "Content/Items/HandGenerated/P3Equipment.cs").read_text(encoding="utf-8")
    expected = {
        "GreenwoodInscriptionNeedle": "Greenwood", "FurnaceInscriptionNeedle": "Furnace",
        "ThunderInscriptionNeedle": "Thunder", "StarAbyssInscriptionNeedle": "StarAbyss",
        "BrokenHeavenInscriptionNeedle": "BrokenHeaven", "InscriptionRemovalStone": "None",
    }
    for name, kind in expected.items():
        match = re.search(rf"public class {name}\s*:\s*global::XianXia.Common.Items.InscriptionToolItem\s*\{{(.*?)\n\}}", equipment, re.S)
        if not match or f"InscriptionKind.{kind};" not in match.group(1):
            raise SystemExit(f"{name}: inscription material routing missing or incorrect")
        if not (ROOT / f"Content/Items/HandGenerated/{name}.png").exists():
            raise SystemExit(f"{name}: client texture missing")
    ui = (ROOT / "Common/UI/InscriptionUIState.cs").read_text(encoding="utf-8")
    required = set(re.findall(r'\bText\("([A-Za-z]+)"', ui)) | set(expected.values())
    for kind in list(expected.values())[:-1]:
        required.update((kind + "Weapon", kind + "Accessory"))
    required.update(("Applied", "Stacking", "ToolHelp", "NeedleCost", "RemovalCost"))
    transactions = (ROOT / "Common/Systems/InscriptionTransactions.cs").read_text(encoding="utf-8")
    required.update(re.findall(r'"Mods\.XianXia\.Inscriptions\.([A-Za-z]+)"', transactions))
    for language in ("zh-Hans", "en-US"):
        text = (ROOT / f"Localization/inscriptions/{language}.hjson").read_text(encoding="utf-8-sig")
        available = set(re.findall(r"^\s*([A-Za-z]+):", text, re.M))
        if missing := required - available:
            raise SystemExit(f"{language}: missing inscription localization {sorted(missing)}")
    print("Inscription routing, textures and dynamic localization verified: 5 needles / 1 removal stone.")


if __name__ == "__main__":
    verify()
