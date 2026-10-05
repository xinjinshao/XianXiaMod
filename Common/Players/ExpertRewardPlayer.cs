using System.Collections.Generic;
using Terraria.ModLoader;

namespace XianXia.Common.Players;

public class ExpertRewardPlayer : ModPlayer
{
    private readonly HashSet<int> applied = new();
    public override void ResetEffects() => applied.Clear();
    public bool TryActivate(int reward) => reward is >= 0 and < 12 && applied.Add(reward);
}
