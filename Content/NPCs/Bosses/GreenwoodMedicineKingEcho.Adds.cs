using Microsoft.Xna.Framework;
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
            if (!TryVineSpawnPosition(out int x, out int y)) break;
            int index = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(), x, y, type, ai0: NPC.whoAmI);
            if (index < 0 || index >= Main.maxNPCs || index == NPC.whoAmI) break;
            NPC created = Main.npc[index];
            if (created == null || !created.active || created.life <= 0 || created.type != type) break;
            created.target = NPC.target;
            created.netUpdate = true;
            vineSummonsCreated++;
        }
        // A later pattern retries only the missing successful creations. Killed adds never replenish the quota.
    }
    internal bool TryVineSpawnPosition(out int x, out int y)
    {
        x = y = 0;
        if (!float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return false;
        float bottom = NPC.Center.Y - 60f;
        if (bottom < 64f || bottom > Main.maxTilesY * 16f - 16f) return false;
        int candidateY = (int)bottom;
        for (int attempt = 0; attempt < 12; attempt++) {
            float candidate = NPC.Center.X + Main.rand.Next(-120, 121);
            if (candidate < 40f || candidate > Main.maxTilesX * 16f - 40f) continue;
            int candidateX = (int)candidate;
            // NewNPC takes horizontal center and bottom: validate the full 48x48 body.
            Vector2 topLeft = new(candidateX - 24f, candidateY - 48f);
            if (Collision.SolidCollision(topLeft, 48, 48, acceptTopSurfaces: true) || Collision.LavaCollision(topLeft, 48, 48)) continue;
            x = candidateX; y = candidateY; return true;
        }
        return false; // Stay inside the telegraphed area; a later pattern can retry elsewhere.
    }

}
