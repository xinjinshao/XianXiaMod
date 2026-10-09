using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class GreenwoodMedicineKingEcho
{
    public const int VineSummonQuota = 3;
    private int vineSummonsCreated;
    internal void SpawnVineAdds()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !BossTargeting.HasLivingTarget(NPC)) return;
        int type = ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.HerbGardenVineSpirit>();
        while (vineSummonsCreated < VineSummonQuota) {
            int index = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(),
                (int)NPC.Center.X + Main.rand.Next(-120, 121),
                (int)NPC.Center.Y - 60, type, ai0: NPC.whoAmI);
            if (index < 0 || index >= Main.maxNPCs || index == NPC.whoAmI) break;
            NPC created = Main.npc[index];
            if (created == null || !created.active || created.life <= 0 || created.type != type) break;
            created.target = NPC.target;
            created.netUpdate = true;
            vineSummonsCreated++;
        }
        // A later pattern retries only the missing successful creations. Killed adds never replenish the quota.
    }
}
