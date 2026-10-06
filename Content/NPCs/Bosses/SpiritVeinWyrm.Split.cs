using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class SpiritVeinWyrm
{
    public const int SplitRetryInterval = 60;
    private int plannedChildren;
    private int createdChildren;
    private int splitRetry;

    internal void SpawnSplitMinions(bool phaseTwo)
    {
        if (!phaseTwo || Main.netMode == NetmodeID.MultiplayerClient || !BossTargeting.HasLivingTarget(NPC)) return;
        if (plannedChildren > 0 && createdChildren >= plannedChildren) return;
        if (splitRetry > 0 && --splitRetry > 0) return;
        if (plannedChildren == 0) plannedChildren = Main.rand.Next(2, 4);
        int free = 0;
        for (int slot = 0; slot < Main.maxNPCs; slot++) if (!Main.npc[slot].active) free++;
        // Budget one head plus its four bodies and tail for each outstanding child.
        // This is a preflight, not a global reservation; child chain creation still handles races.
        if (free < (plannedChildren - createdChildren) * 6) { splitRetry = SplitRetryInterval; return; }
        int childType = ModContent.NPCType<ShatteredJadeWyrmMinion>();
        while (createdChildren < plannedChildren) {
            int id = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(),
                (int)NPC.Center.X + Main.rand.Next(-80, 81),
                (int)NPC.Center.Y + Main.rand.Next(-40, 41), childType, ai0: NPC.whoAmI);
            if (id < 0 || id >= Main.maxNPCs || !Main.npc[id].active || Main.npc[id].type != childType) {
                splitRetry = SplitRetryInterval;
                return;
            }
            Main.npc[id].target = NPC.target;
            Main.npc[id].velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            Main.npc[id].netUpdate = true;
            createdChildren++;
        }
    }
}
