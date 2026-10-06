using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using System.Collections.Generic;
using XianXia.Common.NPCs;

namespace XianXia.Content.NPCs.Bosses;

internal static class SegmentedWormAI
{
    public static void FollowPreviousSegment(NPC npc, float spacing, float lightR, float lightG, float lightB, int expectedHeadType)
    {
        if (!HasValidLinks(npc, out NPC head) || head.type != expectedHeadType
            || !float.IsFinite(spacing) || spacing <= 0)
        {
            npc.damage = 0;
            npc.velocity = Vector2.Zero;
            if (Main.netMode != NetmodeID.MultiplayerClient) Deactivate(npc);
            return;
        }

        NPC previous = Main.npc[(int)npc.ai[0]];
        npc.realLife = head.whoAmI;
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
        if (Main.netMode == NetmodeID.MultiplayerClient || !IsLinkedSegmentActive(previousIndex)
            || !head.active || head.life <= 0 || head.ModNPC is not LinkedWormNPC headLink
            || Main.npc[previousIndex].ModNPC is not LinkedWormNPC previousLink) return -1;
        int id = NPC.NewNPC(
            head.GetSource_FromAI(),
            (int)head.Center.X,
            (int)head.Center.Y,
            segmentType,
            ai0: previousIndex,
            ai1: head.whoAmI,
            ai2: order);

        if (id < 0 || id >= Main.maxNPCs || !Main.npc[id].active || Main.npc[id].type != segmentType) return -1;
        if (Main.npc[id].ModNPC is not LinkedWormNPC createdLink) { Deactivate(Main.npc[id]); return -1; }
        createdLink.Bind(headLink, previousLink);
        Main.npc[id].realLife = head.whoAmI;
        Main.npc[id].netUpdate = true;
        return id;
    }

    public static bool TrySpawnChain(NPC head, int bodyType, int tailType, int bodyCount)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !head.active || head.life <= 0 || bodyCount < 0 || bodyCount >= Main.maxNPCs) return false;
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
        npc.damage = 0;
        npc.velocity = Vector2.Zero;
        if (!npc.active) return;
        npc.active = false;
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
    }

    public static bool HasValidLinks(NPC segment, out NPC head)
    {
        head = null;
        if (!segment.active || segment.life <= 0 || segment.ModNPC is not LinkedWormNPC link
            || !Index(segment.ai[0], out int previousIndex) || !Index(segment.ai[1], out int headIndex)
            || previousIndex == segment.whoAmI || headIndex == segment.whoAmI
            || segment.realLife != headIndex || !Finite(segment)) return false;
        NPC previous = Main.npc[previousIndex], candidate = Main.npc[headIndex];
        if (candidate == null || previous == null || !candidate.active || candidate.life <= 0
            || !previous.active || previous.life <= 0 || !Finite(candidate) || !Finite(previous)
            || !float.IsFinite(Vector2.DistanceSquared(segment.Center, previous.Center))
            || candidate.ModNPC is not LinkedWormNPC headLink || !headLink.IsWormHead
            || previous.ModNPC is not LinkedWormNPC previousLink
            || link.Instance <= 0 || link.HeadInstance <= 0 || link.PreviousInstance <= 0
            || headLink.Instance != link.HeadInstance || previousLink.Instance != link.PreviousInstance
            || (previousIndex != headIndex && (previous.realLife != headIndex
                || previousLink.HeadInstance != link.HeadInstance))) return false;
        head = candidate;
        return true;
    }
    private static bool Index(float raw, out int index)
    {
        index = -1;
        if (!float.IsFinite(raw) || raw < 0 || raw >= Main.maxNPCs || raw != (int)raw) return false;
        index = (int)raw;
        return true;
    }
    private static bool Finite(NPC npc) => float.IsFinite(npc.Center.X) && float.IsFinite(npc.Center.Y)
        && float.IsFinite(npc.velocity.X) && float.IsFinite(npc.velocity.Y);

    private static bool IsLinkedSegmentActive(int index)
    {
        return index >= 0 && index < Main.maxNPCs && Main.npc[index] is NPC npc && npc.active;
    }
}
