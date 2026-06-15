using Microsoft.Xna.Framework;
using Terraria;

namespace XianXia.Content.NPCs.Bosses;

internal static class SegmentedWormAI
{
    public static void FollowPreviousSegment(NPC npc, float spacing, float lightR, float lightG, float lightB)
    {
        int previousIndex = (int)npc.ai[0];
        int headIndex = (int)npc.ai[1];
        if (!IsLinkedSegmentActive(previousIndex) || !IsLinkedSegmentActive(headIndex))
        {
            npc.active = false;
            return;
        }

        NPC previous = Main.npc[previousIndex];
        NPC head = Main.npc[headIndex];
        npc.realLife = headIndex;
        npc.life = head.life;
        npc.lifeMax = head.lifeMax;
        npc.damage = head.damage;
        npc.defense = head.defense;
        npc.timeLeft = 300;

        Vector2 toPrevious = previous.Center - npc.Center;
        float distance = toPrevious.Length();
        if (distance > 1f)
        {
            Vector2 direction = toPrevious / distance;
            npc.Center = previous.Center - direction * spacing;
            npc.rotation = direction.ToRotation();
            npc.velocity = Vector2.Zero;
        }

        Lighting.AddLight(npc.Center, lightR, lightG, lightB);
    }

    public static int SpawnSegment(NPC head, int previousIndex, int segmentType, int order)
    {
        int id = NPC.NewNPC(
            head.GetSource_FromAI(),
            (int)head.Center.X,
            (int)head.Center.Y,
            segmentType,
            ai0: previousIndex,
            ai1: head.whoAmI,
            ai2: order);

        Main.npc[id].realLife = head.whoAmI;
        Main.npc[id].netUpdate = true;
        return id;
    }

    private static bool IsLinkedSegmentActive(int index)
    {
        return index >= 0 && index < Main.maxNPCs && Main.npc[index].active;
    }
}
