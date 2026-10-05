"""Check native boss-bag routing and preservation of existing loot tables.

This source contract does not replace multiplayer kill/open-bag gameplay tests.
"""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
bags = sorted((ROOT / "Content/Items/TreasureBags").glob("*Bag.cs"))
assert len(bags) == 12, "All twelve bosses require a bag"
for bag in bags:
    name = bag.stem.removesuffix("Bag")
    boss = (ROOT / f"Content/NPCs/Bosses/{name}.cs").read_text(encoding="utf-8-sig")
    content = bag.read_text(encoding="utf-8-sig")
    assert f"TreasureBags.{name}Bag" in boss, name
    assert "ItemDropRule.BossBag(" in boss, name
    assert "new Conditions.NotExpert()" in boss, name
    assert f"MasterModeCommonDrop(ModContent.ItemType<global::XianXia.Content.Items.MasterRewards.{name}Monument>())" in boss, name
    monument = (ROOT / f"Content/Items/MasterRewards/{name}Monument.cs").read_text(encoding="utf-8")
    tile = (ROOT / f"Content/Tiles/MasterRewards/{name}MonumentTile.cs").read_text(encoding="utf-8")
    assert f"MasterRewards.{name}MonumentTile>()" in monument, name
    assert f"MasterRewards.{name}Monument>()" in tile, name
    normal = re.findall(r"normal\.OnSuccess\((ItemDropRule\.Common\([^\n]+)\);", boss)
    rewards = re.findall(r"itemLoot\.Add\((ItemDropRule\.Common\([^\n]+)\);", content)
    expert = [rule for rule in rewards if "Items.ExpertRewards." in rule]
    rewards = [rule for rule in rewards if "Items.ExpertRewards." not in rule]
    assert len(expert) == 1 and expert[0].endswith(">())"), f"Guaranteed expert reward missing: {name}"
    assert normal and normal == rewards, f"Reward mismatch: {name}"
    assert "npcLoot.Add(ItemDropRule.Common(" not in boss, f"Duplicate direct loot: {name}"
    assert f"Bosses.{name}>()" in content, name
    assert "CoinsBasedOnNPCValue(BossType)" in content, name
base = (ROOT / "Common/Items/CultivationBossBag.cs").read_text(encoding="utf-8")
for required in ("ItemID.Sets.BossBag[Type] = true", "Item.expert = true",
                 "Item.ResearchUnlockCount = 3", "CanRightClick() => true"):
    assert required in base, required
print("Boss bag routing and identical normal/bag loot verified: 12 bosses.")
