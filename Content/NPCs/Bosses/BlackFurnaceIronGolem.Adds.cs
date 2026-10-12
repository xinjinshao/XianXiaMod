using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class BlackFurnaceIronGolem
{
    public const int MaximumNearbyShards = 6;
    public const float ShardArenaRadius = 1600f;

    internal void SpawnShardAdds()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !BossTargeting.HasLivingTarget(NPC)) return;
        int type = ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.IronShardSpirit>();
        for (int attempt = 0; attempt < 2; attempt++) {
            int alive = 0;
            foreach (NPC other in Main.ActiveNPCs) {
                if (other.type == type && other.life > 0
                    && Vector2.DistanceSquared(NPC.Center, other.Center) <= ShardArenaRadius * ShardArenaRadius) alive++;
            }
            // Natural shards in the same arena also contribute to the combat population.
            if (alive >= MaximumNearbyShards) break;
            if (!TryShardSpawnPosition(out int x, out int y)) break;
            int id = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(), x, y, type, ai0: NPC.whoAmI);
            if (id < 0 || id >= Main.maxNPCs || id == NPC.whoAmI) break;
            NPC created = Main.npc[id];
            if (created == null || !created.active || created.life <= 0 || created.type != type) break;
            created.target = NPC.target;
            created.netUpdate = true;
        }
    }
    internal bool TryShardSpawnPosition(out int x, out int y)
    {
        x = y = 0;
        if (!float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return false;
        for (int attempt = 0; attempt < 12; attempt++) {
            float candidateX = NPC.Center.X + Main.rand.Next(-60, 61);
            float candidateY = NPC.Center.Y + Main.rand.Next(-40, 41);
            if (candidateX < 40f || candidateX > Main.maxTilesX * 16f - 40f
                || candidateY < 64f || candidateY > Main.maxTilesY * 16f - 16f) continue;
            int centerX = (int)candidateX, bottomY = (int)candidateY;
            // NewNPC takes center X / bottom Y; protect the entire body and a 16px world margin.
            Vector2 topLeft = new(centerX - 24f, bottomY - 48f);
            if (Collision.SolidCollision(topLeft, 48, 48, acceptTopSurfaces: true)
                || Collision.LavaCollision(topLeft, 48, 48)) continue;
            x = centerX; y = bottomY; return true;
        }
        return false;
    }

}
