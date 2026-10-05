using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public class AmbientLightningSystem : ModSystem
{
    private static readonly List<(Vector2 Center, ulong Expires)> RecentStrikes = new();
    public override void ClearWorld() => RecentStrikes.Clear();

    public static bool TryStartStrike(Vector2 center)
    {
        RecentStrikes.RemoveAll(strike => strike.Expires <= Main.GameUpdateCount);
        foreach (var strike in RecentStrikes)
            if (Vector2.DistanceSquared(center, strike.Center) <= 600f * 600f) return false;
        RecentStrikes.Add((center, Main.GameUpdateCount + 420));
        return true;
    }
}
