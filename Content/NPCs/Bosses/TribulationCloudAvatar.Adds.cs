using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class TribulationCloudAvatar
{
    private bool cloudAddCreated;

    internal void SpawnCloudAdd()
    {
        if (cloudAddCreated || Main.netMode == NetmodeID.MultiplayerClient
            || !BossTargeting.HasLivingTarget(NPC)) return;
        int type = ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.TribulationCloudling>();
        int index = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(),
            (int)NPC.Center.X, (int)NPC.Center.Y, type, ai0: NPC.whoAmI);
        if (index < 0 || index >= Main.maxNPCs || index == NPC.whoAmI) return;
        NPC created = Main.npc[index];
        if (created == null || !created.active || created.life <= 0 || created.type != type) return;
        created.target = NPC.target;
        created.netUpdate = true;
        cloudAddCreated = true;
        // Retry a failed creation on the next pattern; a defeated add never replenishes.
    }
}
