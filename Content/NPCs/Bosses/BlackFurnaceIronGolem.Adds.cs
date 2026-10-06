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
            int id = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(),
                (int)NPC.Center.X + Main.rand.Next(-60, 61),
                (int)NPC.Center.Y + Main.rand.Next(-40, 41), type, ai0: NPC.whoAmI);
            if (id < 0 || id >= Main.maxNPCs) break;
            NPC created = Main.npc[id];
            if (!created.active || created.type != type) break;
            created.target = NPC.target;
            created.netUpdate = true;
        }
    }
}
