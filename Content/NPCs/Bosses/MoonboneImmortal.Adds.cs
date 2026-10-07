using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class MoonboneImmortal
{
    public const int MoonSummonQuota = 2;
    private int moonSummonsCreated;
    internal void SpawnMoonAdds()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !BossTargeting.HasLivingTarget(NPC)) return;
        int type = ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.ArchivedImmortalSoul>();
        while (moonSummonsCreated < MoonSummonQuota) {
            int index = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(),
                (int)NPC.Center.X + Main.rand.Next(-80, 81),
                (int)NPC.Center.Y + Main.rand.Next(-40, 41), type, ai0: NPC.whoAmI);
            if (index < 0 || index >= Main.maxNPCs || index == NPC.whoAmI) break;
            NPC created = Main.npc[index];
            if (created == null || !created.active || created.life <= 0 || created.type != type) break;
            created.target = NPC.target;
            created.netUpdate = true;
            moonSummonsCreated++;
        }
        // A later ring retries only the missing successful creations. Killed adds never replenish the quota.
    }
}
