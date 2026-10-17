using Terraria;
using Terraria.ID;
using XianXia.Common.Players;
namespace XianXia.Common.Projectiles;

internal readonly struct ArtifactProjectileOwnerBinding
{
    private readonly Player ownerAtSpawn;
    private readonly long sessionAtSpawn;
    private readonly int slotAtSpawn;
    private ArtifactProjectileOwnerBinding(Player owner,long session,int slot) { ownerAtSpawn = owner; sessionAtSpawn = session; slotAtSpawn = slot; }
    internal static ArtifactProjectileOwnerBinding Capture(Projectile projectile)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || Main.player == null || projectile.owner < 0
            || projectile.owner >= Main.maxPlayers || projectile.owner >= Main.player.Length) return default;
        Player owner = Main.player[projectile.owner];
        return owner != null && owner.active && !owner.dead && owner.TryGetModPlayer<XianXiaPlayer>(out var state)
            ? new ArtifactProjectileOwnerBinding(owner,state.ArtifactOwnerSession,projectile.owner) : default;
    }
    internal bool Matches(Projectile projectile,Player owner) => Main.netMode == NetmodeID.MultiplayerClient
        || owner != null && projectile.owner == slotAtSpawn && ReferenceEquals(owner,ownerAtSpawn) && sessionAtSpawn != 0
            && owner.TryGetModPlayer<XianXiaPlayer>(out var state) && state.ArtifactOwnerSession == sessionAtSpawn;
}
