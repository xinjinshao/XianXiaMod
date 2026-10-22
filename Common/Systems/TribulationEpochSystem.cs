using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public class TribulationEpochSystem : ModSystem
{
    internal static readonly long[] ClientLastSeen = new long[Main.maxPlayers];
    public override void ClearWorld() => Array.Clear(ClientLastSeen);
    public override void OnWorldLoad() => Array.Clear(ClientLastSeen);
    public override void OnWorldUnload() => Array.Clear(ClientLastSeen);
    public override void PostUpdatePlayers()
    {
        if (Main.netMode != NetmodeID.Server || Main.player == null) return;
        for (int i = 0; i < Math.Min(Main.maxPlayers,Main.player.Length); i++)
            if (Main.player[i] is { active: true } actor && actor.TryGetModPlayer<XianXiaPlayer>(out var state))
                state.BroadcastTribulationEpochIfChanged();
    }
}

// Vanilla player synchronization calls every ModPlayer, including for late joiners.
public class TribulationEpochSyncPlayer : ModPlayer
{
    public override void SyncPlayer(int toWho,int fromWho,bool newPlayer)
    {
        if (Main.netMode == NetmodeID.Server && Player.TryGetModPlayer<XianXiaPlayer>(out var state))
            state.SendTribulationEpoch(toWho,fromWho);
    }
}
