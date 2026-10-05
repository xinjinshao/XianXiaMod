from pathlib import Path
import re
root=Path(__file__).resolve().parents[1]
names=[p.stem.removesuffix("Bag") for p in (root/"Content/Items/TreasureBags").glob("*Bag.cs")]
assert len(names)==12
for name in names:
 s=(root/f"Content/NPCs/Bosses/{name}.cs").read_text(encoding="utf-8")
 assert f"BossStatRules.Get(nameof({name}))" in s,name
 assert "ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)" in s,name
 assert s.count("BossStatRules.ScaleLife(NPC.lifeMax, balance, bossAdjustment)")==1,name
 assert "baseLife *" not in s and "baseDamage *" not in s,name
print("12 boss baseline/engine scaling hooks verified; no duplicate custom difficulty multipliers.")
