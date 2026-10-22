using System;
using System.IO;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Systems;

// Runtime identities are never saved. The authority allocates; clients only accept server identities.
public class NpcEncounterIdentity : GlobalNPC
{
    private static long nextIdentity;
    private long identity;
    private NPC identityOwner;
    public override bool InstancePerEntity => true;
    public override void SetDefaults(NPC npc) { identityOwner = npc; identity = 0; }

    internal long GetIdentity(NPC npc, bool requestSync = true)
    {
        if (!IsCurrent(npc)) return 0;
        if (!ReferenceEquals(identityOwner,npc)) { identityOwner = npc; identity = 0; }
        if (identity == 0 && Main.netMode != NetmodeID.MultiplayerClient) {
            long allocated = Interlocked.Increment(ref nextIdentity);
            identity = allocated > 0 ? allocated : 0;
            if (identity > 0 && requestSync && Main.netMode == NetmodeID.Server) npc.netUpdate = true;
        }
        return identity;
    }
    private static bool IsCurrent(NPC npc) => npc != null && Main.npc != null
        && npc.whoAmI >= 0 && npc.whoAmI < Main.maxNPCs && npc.whoAmI < Main.npc.Length
        && ReferenceEquals(Main.npc[npc.whoAmI],npc);

    public override void SendExtraAI(NPC npc,BitWriter bitWriter,BinaryWriter writer) => writer.Write(GetIdentity(npc,requestSync:false));
    public override void ReceiveExtraAI(NPC npc,BitReader bitReader,BinaryReader reader)
    {
        long incoming = reader.ReadInt64(); // Read the full field before changing either identity or history.
        if (Main.netMode != NetmodeID.MultiplayerClient || incoming <= 0 || !IsCurrent(npc)) return;
        long previous = NpcEncounterIdentitySystem.ClientLastSeen[npc.whoAmI];
        long current = ReferenceEquals(identityOwner,npc) ? identity : 0;
        if (incoming < previous || (incoming == previous && incoming != current)) return;
        identityOwner = npc; identity = incoming;
        NpcEncounterIdentitySystem.ClientLastSeen[npc.whoAmI] = incoming;
    }
}

public class NpcEncounterIdentitySystem : ModSystem
{
    internal static readonly long[] ClientLastSeen = new long[Main.maxNPCs];
    public override void ClearWorld() => Array.Clear(ClientLastSeen);
    public override void OnWorldLoad() => Array.Clear(ClientLastSeen);
    public override void OnWorldUnload() => Array.Clear(ClientLastSeen);
}
