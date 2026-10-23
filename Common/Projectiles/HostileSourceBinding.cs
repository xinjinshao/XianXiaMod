using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Common.Players;

namespace XianXia.Common.Projectiles;

internal sealed class HostileSourceBinding
{
    private short sourceSlot = -1;
    private int sourceType;
    private NPC sourceNpc;
    private ModNPC sourceModNpc;
    private long sourceNpcIdentity;
    private bool sourceCancelled, invalidLifetime, hasSourceMetadata;
    private short sourcePlayerSlot = -1;
    private long sourcePlayerSession;
    private Player sourcePlayer;

    public bool IsCancelled => sourceCancelled || invalidLifetime;

    public void Capture(IEntitySource source)
    {
        if (source is EntitySource_Parent { Entity: Player player }) {
            hasSourceMetadata = true;
            sourcePlayer = player;
            sourcePlayerSlot = player.whoAmI >= 0 && player.whoAmI < Main.maxPlayers ? (short)player.whoAmI : (short)-2;
            sourcePlayerSession = player.GetModPlayer<XianXiaPlayer>().TribulationSession;
            return;
        }
        if (source is not EntitySource_Parent { Entity: NPC parent }) return;
        hasSourceMetadata = true;
        sourceNpc = parent;
        sourceModNpc = parent.ModNPC;
        sourceSlot = parent.whoAmI >= 0 && parent.whoAmI < Main.maxNPCs ? (short)parent.whoAmI : (short)-2;
        sourceType = parent.type;
        sourceNpcIdentity = GetNpcIdentity(parent);
    }

    private static long GetNpcIdentity(NPC npc) => ModContent.GetInstance<NpcEncounterIdentity>() != null
        && npc.TryGetGlobalNPC<NpcEncounterIdentity>(out var identity) ? identity.GetIdentity(npc) : 0;

    public bool IsValid()
    {
        if (IsCancelled || (Main.netMode == NetmodeID.MultiplayerClient && !hasSourceMetadata)) return false;
        if (sourcePlayerSlot != -1) {
            if (sourcePlayerSlot < 0 || sourcePlayerSlot >= Main.maxPlayers || sourcePlayerSession <= 0) return false;
            Player player = Main.player[sourcePlayerSlot];
            if (player == null || !player.active || player.dead
                || !float.IsFinite(player.Center.X) || !float.IsFinite(player.Center.Y)
                || !float.IsFinite(player.velocity.X) || !float.IsFinite(player.velocity.Y)) return false;
            XianXiaPlayer cultivation = player.GetModPlayer<XianXiaPlayer>();
            return cultivation.tribulationTimer > 0 && cultivation.TribulationSession == sourcePlayerSession
                && (ReferenceEquals(player,sourcePlayer) || (Main.netMode == NetmodeID.MultiplayerClient && sourcePlayer == null));
        }
        if (sourceSlot == -1) return sourceType == 0;
        if (sourceSlot < 0 || sourceSlot >= Main.maxNPCs) return false;
        NPC parent = Main.npc[sourceSlot];
        if (parent == null || parent.type != sourceType || sourceNpcIdentity <= 0 || !BossTargeting.HasLivingTarget(parent)
            || GetNpcIdentity(parent) != sourceNpcIdentity) return false;
        if (ModContent.GetInstance<HeartDemonStats>() != null && parent.TryGetGlobalNPC<HeartDemonStats>(out var heart)
            && !heart.IsSourceValid(parent)) return false;
        // Network-spawned client shots have no local source reference; the server identity is decisive.
        bool sameReferences = ReferenceEquals(parent,sourceNpc) && ReferenceEquals(parent.ModNPC,sourceModNpc);
        return sameReferences || (Main.netMode == NetmodeID.MultiplayerClient && sourceNpc == null);
    }

    public void CancelIfInvalid(Projectile projectile, int fadeTicks, int lifetime)
    {
        if (projectile.timeLeft < 0 || projectile.timeLeft > lifetime) {
            invalidLifetime = true;
            CancelOnAuthority(projectile, fadeTicks);
        }
        if (IsCancelled) { projectile.timeLeft = System.Math.Min(projectile.timeLeft, fadeTicks); return; }
        if (Main.netMode == NetmodeID.MultiplayerClient || (sourceSlot == -1 && sourceType == 0 && sourcePlayerSlot == -1) || IsValid()) return;
        CancelOnAuthority(projectile, fadeTicks);
    }

    public void CancelOnAuthority(Projectile projectile, int fadeTicks)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        bool changed = !sourceCancelled;
        sourceCancelled = true;
        projectile.timeLeft = System.Math.Min(projectile.timeLeft, fadeTicks);
        if (changed) projectile.netUpdate = true;
    }

    public IEntitySource StrikeSource(Projectile fallback) => sourcePlayer != null ? sourcePlayer.GetSource_FromThis()
        : sourceNpc != null ? sourceNpc.GetSource_FromAI() : fallback.GetSource_FromThis();

    public void Write(BinaryWriter writer, Projectile projectile, int lifetime)
    {
        int remaining = projectile.timeLeft;
        invalidLifetime |= remaining < 0 || remaining > lifetime;
        writer.Write((short)(remaining >= 0 && remaining <= lifetime ? remaining : 0));
        writer.Write(IsCancelled || remaining < 0 || remaining > lifetime);
        writer.Write(sourceSlot);
        writer.Write(sourceType);
        writer.Write(sourcePlayerSlot);
        writer.Write(sourcePlayerSession);
        writer.Write(sourceNpcIdentity);
    }

    public bool Read(BinaryReader reader, Projectile projectile, int lifetime)
    {
        int remaining = reader.ReadInt16();
        bool cancelled = reader.ReadBoolean();
        short slot = reader.ReadInt16();
        int type = reader.ReadInt32();
        short playerSlot = reader.ReadInt16();
        long session = reader.ReadInt64();
        long npcIdentity = reader.ReadInt64();
        projectile.timeLeft = remaining >= 0 && remaining <= lifetime ? remaining : 0;
        sourceCancelled |= cancelled;
        invalidLifetime |= remaining < 0 || remaining > lifetime;
        sourceSlot = slot;
        sourceType = type;
        sourcePlayerSlot = playerSlot;
        sourcePlayerSession = session > 0 ? session : 0;
        sourceNpcIdentity = npcIdentity > 0 ? npcIdentity : 0;
        hasSourceMetadata = true;
        return remaining >= 0 && remaining <= lifetime;
    }
}
