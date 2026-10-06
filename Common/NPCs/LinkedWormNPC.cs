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
    private bool CanContact() => IsWormHead ? BossTargeting.HasLivingTarget(NPC)
        : SegmentedWormAI.HasValidLinks(NPC, out NPC head) && BossTargeting.HasLivingTarget(head);
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => target.active && !target.dead && CanContact();
    public override bool CanHitNPC(NPC target) => CanContact();
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
