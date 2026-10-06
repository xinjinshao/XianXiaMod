using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.Projectiles;

public partial class BossArrayFieldProjectile
{
    private short sourceSlot = -1;
    private int sourceType;
    private NPC sourceNpc;
    private ModNPC sourceModNpc;
    private bool sourceCancelled;

    public override void OnSpawn(IEntitySource source)
    {
        if (source is not EntitySource_Parent { Entity: NPC parent }) return;
        sourceNpc = parent;
        sourceModNpc = parent.ModNPC;
        sourceSlot = parent.whoAmI >= 0 && parent.whoAmI < Main.maxNPCs ? (short)parent.whoAmI : (short)-2;
        sourceType = parent.type;
    }

    private bool SourceAllowsDamage()
    {
        if (sourceCancelled) return false;
        if (sourceSlot == -1) return sourceType == 0; // Player tribulations have their own lifecycle.
        if (sourceSlot < 0 || sourceSlot >= Main.maxNPCs) return false;
        NPC parent = Main.npc[sourceSlot];
        if (parent == null || parent.type != sourceType || !BossTargeting.HasLivingTarget(parent)) return false;
        // Authority distinguishes a new ModNPC even when the engine reuses the NPC object and slot.
        return Main.netMode == NetmodeID.MultiplayerClient
            || (ReferenceEquals(parent, sourceNpc) && ReferenceEquals(parent.ModNPC, sourceModNpc));
    }

    private void CancelInvalidBossSource()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || sourceCancelled || (sourceSlot == -1 && sourceType == 0) || SourceAllowsDamage()) return;
        sourceCancelled = true;
        Projectile.timeLeft = System.Math.Min(Projectile.timeLeft, FadeTicks);
        Projectile.netUpdate = true;
    }
}
