using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using System.Collections.Generic;

namespace XianXia.Content.NPCs.Bosses;

internal static class SegmentedWormAI
{
    public static void FollowPreviousSegment(NPC npc, float spacing, float lightR, float lightG, float lightB, int expectedHeadType)
    {
        int previousIndex = (int)npc.ai[0];
        int headIndex = (int)npc.ai[1];
        if (!IsLinkedSegmentActive(previousIndex) || !IsLinkedSegmentActive(headIndex)
            || Main.npc[headIndex].type != expectedHeadType
            || previousIndex == npc.whoAmI
            || (previousIndex != headIndex && Main.npc[previousIndex].realLife != headIndex))
        {
            npc.damage = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient) Deactivate(npc);
            return;
        }

        NPC previous = Main.npc[previousIndex];
        NPC head = Main.npc[headIndex];
        npc.realLife = headIndex;
        npc.life = head.life;
        npc.lifeMax = head.lifeMax;
        npc.damage = global::XianXia.Common.Systems.BossTargeting.HasLivingTarget(head) ? head.damage : 0;
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

        if (!Main.dedServ) Lighting.AddLight(npc.Center, lightR, lightG, lightB);
    }

    public static int SpawnSegment(NPC head, int previousIndex, int segmentType, int order)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !IsLinkedSegmentActive(previousIndex)) return -1;
        int id = NPC.NewNPC(
            head.GetSource_FromAI(),
            (int)head.Center.X,
            (int)head.Center.Y,
            segmentType,
            ai0: previousIndex,
            ai1: head.whoAmI,
            ai2: order);

        if (id < 0 || id >= Main.maxNPCs || !Main.npc[id].active || Main.npc[id].type != segmentType) return -1;
        Main.npc[id].realLife = head.whoAmI;
        Main.npc[id].netUpdate = true;
        return id;
    }

    public static bool TrySpawnChain(NPC head, int bodyType, int tailType, int bodyCount)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !head.active || bodyCount < 0) return false;
        int free = 0;
        for (int i = 0; i < Main.maxNPCs; i++) if (!Main.npc[i].active) free++;
        if (free < bodyCount + 1) return false;
        var created = new List<int>();
        int previous = head.whoAmI;
        for (int order = 1; order <= bodyCount + 1; order++)
        {
            int id = SpawnSegment(head, previous, order <= bodyCount ? bodyType : tailType, order);
            if (id < 0)
            {
                foreach (int owned in created) Deactivate(Main.npc[owned]);
                return false;
            }
            created.Add(id);
            previous = id;
        }
        return true;
    }

    public static void Deactivate(NPC npc)
    {
        npc.active = false;
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
    }

    private static bool IsLinkedSegmentActive(int index)
    {
        return index >= 0 && index < Main.maxNPCs && Main.npc[index].active;
    }
}
