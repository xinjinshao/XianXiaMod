using System.IO;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.NPCs.Bosses;

namespace XianXia.Common.NPCs;

// Only worm heads and segments carry this state; unrelated NPC packets stay unchanged.
public abstract class LinkedWormNPC : ModNPC
{
    private static long nextInstance;
    private long instance;
    internal long HeadInstance { get; private set; }
    internal long PreviousInstance { get; private set; }
    internal virtual bool IsWormHead => false;
    internal virtual bool ContactEnabled => true;
    internal long Instance {
        get {
            if (instance == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                instance = Interlocked.Increment(ref nextInstance);
            return instance;
        }
    }
    internal void Bind(LinkedWormNPC head, LinkedWormNPC previous)
    {
        HeadInstance = head.Instance;
        PreviousInstance = previous.Instance;
    }
    private bool CanContact(out NPC head)
    {
        head = NPC;
        if (IsWormHead) return ContactEnabled && BossTargeting.HasLivingTarget(NPC);
        return SegmentedWormAI.HasValidLinks(NPC, out head) && BossTargeting.HasLivingTarget(head)
            && head.ModNPC is LinkedWormNPC linked && linked.ContactEnabled;
    }
    private static bool Finite(Microsoft.Xna.Framework.Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => target != null && target.active && !target.dead
        && Finite(target.Center) && Finite(target.velocity) && Finite(NPC.Center) && Finite(NPC.velocity)
        && CanContact(out NPC head)
        && Microsoft.Xna.Framework.Vector2.DistanceSquared(head.Center, target.Center) <= BossTargeting.MaximumDistance * BossTargeting.MaximumDistance
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
    public override bool CanHitNPC(NPC target) => target != null && target.active && target.life > 0
        && Finite(target.Center) && Finite(target.velocity) && Finite(NPC.Center) && Finite(NPC.velocity)
        && CanContact(out NPC head)
        && Microsoft.Xna.Framework.Vector2.DistanceSquared(head.Center, target.Center) <= BossTargeting.MaximumDistance * BossTargeting.MaximumDistance
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
    private bool CanReceiveHit() => IsWormHead
        || (SegmentedWormAI.HasValidLinks(NPC, out NPC head) && BossTargeting.HasLivingTarget(head));
    public override bool? CanBeHitByItem(Player player, Item item) => CanReceiveHit() ? null : false;
    public override bool? CanBeHitByProjectile(Projectile projectile) => CanReceiveHit() ? null : false;
    // Segments share head health; only the head can own death rewards/events.
    public override bool PreKill() => IsWormHead;

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Instance);
        writer.Write(HeadInstance);
        writer.Write(PreviousInstance);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        long own = reader.ReadInt64(), head = reader.ReadInt64(), previous = reader.ReadInt64();
        instance = own > 0 ? own : 0;
        HeadInstance = head > 0 ? head : 0;
        PreviousInstance = previous > 0 ? previous : 0;
    }
}
