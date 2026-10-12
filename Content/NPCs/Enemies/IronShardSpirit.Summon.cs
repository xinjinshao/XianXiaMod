using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using XianXia.Common.Systems;
using XianXia.Content.NPCs.Bosses;

namespace XianXia.Content.NPCs.Enemies;

public partial class IronShardSpirit
{
    public const int MaximumSummonLifetime = 900;
    public const int SummonWarningTicks = 45;
    private bool summonWarningFrame;
    private bool HasFinishedSummonWarning() => !furnaceSummon
        || (!summonWarningFrame && NPC.ai[3] == 0f);
    private bool furnaceSummon;
    private short parentSlot = -1;
    private long parentSession;
    private short remaining;

    public override void OnSpawn(IEntitySource source)
    {
        if (source is not EntitySource_Parent { Entity: NPC parent }
            || parent.ModNPC is not BlackFurnaceIronGolem golem) return;
        furnaceSummon = true;
        parentSlot = (short)parent.whoAmI;
        parentSession = golem.SummonSession;
        remaining = MaximumSummonLifetime;
        NPC.ai[3] = -SummonWarningTicks;
    }

    private bool HasSummonSource()
    {
        if (!float.IsFinite(NPC.ai[3]) || NPC.ai[3] != System.MathF.Truncate(NPC.ai[3])
            || NPC.ai[3] < -SummonWarningTicks || NPC.ai[3] > 0f
            || !NPC.active || NPC.life <= 0 || remaining <= 0 || parentSession <= 0
            || parentSlot < 0 || parentSlot >= Main.maxNPCs
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)
            || !float.IsFinite(NPC.velocity.X) || !float.IsFinite(NPC.velocity.Y)) return false;
        NPC parent = Main.npc[parentSlot];
        return parent?.ModNPC is BlackFurnaceIronGolem golem
            && golem.SummonSession == parentSession && BossTargeting.HasLivingTarget(parent)
            && Vector2.DistanceSquared(NPC.Center, parent.Center)
                <= BossTargeting.MaximumDistance * BossTargeting.MaximumDistance;
    }

    private bool HasSynchronizedSummonTarget() => HasSummonSource() && NPC.target == Main.npc[parentSlot].target;

    private bool SynchronizeSummonTarget()
    {
        if (!furnaceSummon) return true;
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
        summonWarningFrame = false;
        if (!furnaceSummon) return true;
        if (SynchronizeSummonTarget()) {
            if (Main.netMode != NetmodeID.MultiplayerClient) remaining--;
            if (remaining > 0) {
                if (NPC.ai[3] < 0f) {
                    summonWarningFrame = true;
                    NPC.velocity = Vector2.Zero;
                    if (Main.netMode != NetmodeID.MultiplayerClient) {
                        NPC.ai[3]++;
                        if (NPC.ai[3] % 15f == 0f) NPC.netUpdate = true;
                    }
                    return false;
                }
                if (Main.netMode != NetmodeID.MultiplayerClient && remaining % 60 == 0) NPC.netUpdate = true;
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

    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        => !furnaceSummon || (target.active && !target.dead && HasFinishedSummonWarning() && HasSynchronizedSummonTarget());

    public override bool CanHitNPC(NPC target) => !furnaceSummon || (HasFinishedSummonWarning() && HasSynchronizedSummonTarget());

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(furnaceSummon);
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
        furnaceSummon = bound;
        parentSlot = slot >= 0 && slot < Main.maxNPCs ? slot : (short)-1;
        parentSession = session > 0 ? session : 0;
        remaining = (short)System.Math.Clamp((int)lifetime, 0, MaximumSummonLifetime);
    }
}
