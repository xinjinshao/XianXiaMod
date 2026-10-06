using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Common.Players;

namespace XianXia.Content.Projectiles;

public partial class BossArrayFieldProjectile
{
    private short sourceSlot = -1;
    private int sourceType;
    private NPC sourceNpc;
    private ModNPC sourceModNpc;
    private bool sourceCancelled;
    private short sourcePlayerSlot = -1;
    private long sourcePlayerSession;
    private Player sourcePlayer;

    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_Parent { Entity: Player player }) {
            sourcePlayer = player;
            sourcePlayerSlot = player.whoAmI >= 0 && player.whoAmI < Main.maxPlayers ? (short)player.whoAmI : (short)-2;
            sourcePlayerSession = player.GetModPlayer<XianXiaPlayer>().TribulationSession;
            return;
        }
        if (source is not EntitySource_Parent { Entity: NPC parent }) return;
        sourceNpc = parent;
        sourceModNpc = parent.ModNPC;
        sourceSlot = parent.whoAmI >= 0 && parent.whoAmI < Main.maxNPCs ? (short)parent.whoAmI : (short)-2;
        sourceType = parent.type;
    }

    private bool SourceAllowsDamage()
    {
        if (sourceCancelled) return false;
        if (sourcePlayerSlot != -1) {
            if (sourcePlayerSlot < 0 || sourcePlayerSlot >= Main.maxPlayers || sourcePlayerSession <= 0) return false;
            Player player = Main.player[sourcePlayerSlot];
            if (player == null || !player.active || player.dead
                || !float.IsFinite(player.Center.X) || !float.IsFinite(player.Center.Y)
                || !float.IsFinite(player.velocity.X) || !float.IsFinite(player.velocity.Y)) return false;
            XianXiaPlayer cultivation = player.GetModPlayer<XianXiaPlayer>();
            return cultivation.tribulationTimer > 0 && (Main.netMode == NetmodeID.MultiplayerClient
                || (ReferenceEquals(player, sourcePlayer) && cultivation.TribulationSession == sourcePlayerSession));
        }
        if (sourceSlot == -1) return sourceType == 0;
        if (sourceSlot < 0 || sourceSlot >= Main.maxNPCs) return false;
        NPC parent = Main.npc[sourceSlot];
        if (parent == null || parent.type != sourceType || !BossTargeting.HasLivingTarget(parent)) return false;
        // Authority distinguishes a new ModNPC even when the engine reuses the NPC object and slot.
        return Main.netMode == NetmodeID.MultiplayerClient
            || (ReferenceEquals(parent, sourceNpc) && ReferenceEquals(parent.ModNPC, sourceModNpc));
    }

    private void CancelInvalidSource()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || sourceCancelled || (sourceSlot == -1 && sourceType == 0 && sourcePlayerSlot == -1) || SourceAllowsDamage()) return;
        sourceCancelled = true;
        Projectile.timeLeft = System.Math.Min(Projectile.timeLeft, FadeTicks);
        Projectile.netUpdate = true;
    }
}
