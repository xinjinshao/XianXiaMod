using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using XianXia.Common.Systems;
using XianXia.Content.NPCs.Bosses;

namespace XianXia.Content.NPCs.Enemies;

public partial class CelestialPuppet
{
    public const int MaximumSummonLifetime = 900;
    private bool puppetSummon, rejectedBinding;
    private short parentSlot = -1;
    private long parentSession;
    private short remaining;

    public override void OnSpawn(IEntitySource source)
    {
        if (source is not EntitySource_Parent { Entity: NPC parent }
            || parent.ModNPC is not BrokenHeavenInspector inspector) return;
        puppetSummon = true;
        parentSlot = parent.whoAmI >= 0 && parent.whoAmI < Main.maxNPCs ? (short)parent.whoAmI : (short)-1;
        parentSession = inspector.SummonSession;
        remaining = MaximumSummonLifetime;
    }

    private bool HasSummonSource(bool dying = false)
    {
        if (rejectedBinding || !NPC.active || (!dying && NPC.life <= 0) || remaining <= 0 || parentSession <= 0
            || parentSlot < 0 || parentSlot >= Main.maxNPCs
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)
            || !float.IsFinite(NPC.velocity.X) || !float.IsFinite(NPC.velocity.Y)) return false;
        NPC parent = Main.npc[parentSlot];
        return parent?.ModNPC is BrokenHeavenInspector inspector
            && inspector.SummonSession == parentSession && BossTargeting.HasLivingTarget(parent)
            && Vector2.DistanceSquared(NPC.Center, parent.Center)
                <= BossTargeting.MaximumDistance * BossTargeting.MaximumDistance;
    }

    private bool HasSynchronizedSummonTarget() => HasSummonSource() && NPC.target == Main.npc[parentSlot].target;

    private bool SynchronizeSummonTarget()
    {
        if (!puppetSummon) return true;
        if (!HasSummonSource()) return false;
        int target = Main.npc[parentSlot].target;
        if (NPC.target == target) return true;
        if (Main.netMode == NetmodeID.MultiplayerClient) return false;
        NPC.target = target;
        NPC.netUpdate = true;
        return true;
    }

    public override bool PreAI()
    {
        if (!puppetSummon) return true;
        if (SynchronizeSummonTarget()) {
            // Clients wait harmlessly for an out-of-order parent packet; only the authority ages summons.
            if (Main.netMode == NetmodeID.MultiplayerClient) return true;
            remaining--;
            if (remaining > 0) {
                if (remaining % 60 == 0) NPC.netUpdate = true;
                return true;
            }
        }
        NPC.velocity = Vector2.Zero;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.active) {
            NPC.damage = 0;
            NPC.active = false; // Despawn without death rewards or kill progression.
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
        }
        return false;
    }

    public override bool? CanBeHitByItem(Player player, Item item)
        => !puppetSummon || HasSynchronizedSummonTarget() ? null : false;
    public override bool? CanBeHitByProjectile(Projectile projectile)
        => !puppetSummon || HasSynchronizedSummonTarget() ? null : false;

    public override bool CheckDead()
    {
        if (!puppetSummon) return true;
        if (Main.netMode == NetmodeID.MultiplayerClient) { NPC.life = 1; return false; }
        if (HasSummonSource(dying: true) && NPC.target == Main.npc[parentSlot].target) return true;
        NPC.damage = 0;
        if (NPC.active) {
            NPC.active = false;
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
        }
        return false; // An invalid encounter cannot produce ordinary death rewards between AI updates.
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        => !puppetSummon || (target.active && !target.dead && HasSynchronizedSummonTarget());

    public override bool CanHitNPC(NPC target) => !puppetSummon || HasSynchronizedSummonTarget();

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(puppetSummon);
        writer.Write(parentSlot);
        writer.Write(parentSession);
        writer.Write(remaining);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        // Read the complete packet before changing live state. Invalid bound packets stay harmless.
        bool bound = reader.ReadBoolean();
        short slot = reader.ReadInt16();
        long session = reader.ReadInt64();
        short lifetime = reader.ReadInt16();
        if (puppetSummon && (!bound || (parentSlot >= 0 && slot != parentSlot)
            || (parentSession > 0 && session != parentSession))) {
            rejectedBinding = true;
            remaining = 0;
            return;
        }
        short receivedLifetime = lifetime >= 0 && lifetime <= MaximumSummonLifetime ? lifetime : (short)0;
        bool sameBinding = puppetSummon && bound && slot == parentSlot && session == parentSession;
        remaining = rejectedBinding ? (short)0
            : sameBinding ? System.Math.Min(remaining, receivedLifetime) : receivedLifetime;
        puppetSummon = bound;
        parentSlot = slot >= 0 && slot < Main.maxNPCs ? slot : (short)-1;
        parentSession = session > 0 ? session : 0;

    }
}
