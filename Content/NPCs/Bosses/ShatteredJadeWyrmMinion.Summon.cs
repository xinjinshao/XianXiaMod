using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class ShatteredJadeWyrmMinion
{
    public const int MaximumSummonLifetime = 900;
    private short parentSlot = -1;
    private long parentSession;

    public override void OnSpawn(IEntitySource source)
    {
        if (source is not EntitySource_Parent { Entity: NPC parent }
            || parent.ModNPC is not SpiritVeinWyrm wyrm) return;
        parentSlot = (short)parent.whoAmI;
        parentSession = wyrm.SummonSession;
    }

    private bool HasSummonSource()
    {
        if (!NPC.active || NPC.life <= 0 || parentSession <= 0
            || parentSlot < 0 || parentSlot >= Main.maxNPCs
            || !float.IsFinite(NPC.ai[3]) || NPC.ai[3] < 0 || NPC.ai[3] >= MaximumSummonLifetime
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)
            || !float.IsFinite(NPC.velocity.X) || !float.IsFinite(NPC.velocity.Y)) return false;
        NPC parent = Main.npc[parentSlot];
        return parent?.ModNPC is SpiritVeinWyrm wyrm
            && wyrm.SummonSession == parentSession && BossTargeting.HasLivingTarget(parent)
            && Vector2.DistanceSquared(NPC.Center, parent.Center)
                <= BossTargeting.MaximumDistance * BossTargeting.MaximumDistance;
    }

    public override bool PreAI()
    {
        if (HasSummonSource()) {
            NPC parent = Main.npc[parentSlot];
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                // The head's ordinary target/age data is also authoritative.
                if (NPC.target == parent.target && BossTargeting.HasLivingTarget(NPC)) return true;
                NPC.velocity = Vector2.Zero;
                return false;
            }
            if (NPC.target != parent.target) {
                NPC.target = parent.target;
                NPC.netUpdate = true;
            }
            NPC.ai[3]++;
            if (NPC.ai[3] < MaximumSummonLifetime) {
                if (NPC.ai[3] % 60 == 0) NPC.netUpdate = true;
                return true;
            }
        }
        NPC.velocity = Vector2.Zero;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.active) {
            NPC.damage = 0;
            SegmentedWormAI.Deactivate(NPC);
        }
        return false;
    }

    internal bool HasSummonTarget() => HasSummonSource()
        && NPC.target == Main.npc[parentSlot].target && BossTargeting.HasLivingTarget(NPC);

    internal static bool HasLinkedSummonTarget(NPC segment)
    {
        float rawHead = segment.ai[1];
        if (!segment.active || segment.life <= 0 || !float.IsFinite(rawHead)
            || rawHead < 0 || rawHead >= Main.maxNPCs || rawHead != (int)rawHead
            || segment.realLife != (int)rawHead) return false;
        return SegmentedWormAI.HasValidLinks(segment, out NPC head)
            && head.ModNPC is ShatteredJadeWyrmMinion minion
            && minion.HasSummonTarget();
    }

    internal static void FollowSummonSegment(NPC segment, float green, float blue)
    {
        if (!HasLinkedSummonTarget(segment)) {
            segment.damage = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient && segment.active)
                SegmentedWormAI.Deactivate(segment);
            return;
        }
        SegmentedWormAI.FollowPreviousSegment(segment, SegmentSpacing, 0.02f, green, blue,
            Terraria.ModLoader.ModContent.NPCType<ShatteredJadeWyrmMinion>());
    }

    public override bool CanHitNPC(NPC target) => HasSummonTarget();

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(parentSlot);
        writer.Write(parentSession);
        base.SendExtraAI(writer);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        short slot = reader.ReadInt16();
        long session = reader.ReadInt64();
        base.ReceiveExtraAI(reader);
        parentSlot = slot >= 0 && slot < Main.maxNPCs ? slot : (short)-1;
        parentSession = session > 0 ? session : 0;
    }
}
