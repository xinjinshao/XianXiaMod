using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

// Native automatic sync is owner-driven; these projectiles are created by the server for a player.
public class ServerPlayerProjectileSync : GlobalProjectile
{
    public override bool InstancePerEntity => true;
    private bool playerOwned, retired;
    private static bool IsOwn(Projectile projectile) => projectile.ModProjectile?.Mod is global::XianXia.XianXia;
    private static bool ValidOwnerSlot(Projectile projectile) => Main.player != null && projectile.owner >= 0 && projectile.owner < Main.maxPlayers
        && projectile.owner < Main.player.Length && Main.player[projectile.owner] != null;
    public override bool? CanDamage(Projectile projectile) => retired ? false : null;
    public override bool PreAI(Projectile projectile) => ValidateOwnerBeforeUpdate(projectile);
    internal bool ValidateOwnerBeforeUpdate(Projectile projectile) {
        if (!playerOwned || !IsOwn(projectile) || !retired && ValidOwnerSlot(projectile)) return true;
        retired = true; projectile.active = false; projectile.friendly = projectile.hostile = false; projectile.hide = true;
        projectile.tileCollide = false; projectile.velocity = Vector2.Zero; projectile.damage = 0; projectile.DamageType = DamageClass.Default;
        // Retired before the engine Update loop, so native visuals/Kill never index a corrupt owner.
        projectile.timeLeft = 2; projectile.netUpdate = false;
        return false;
    }
    private static bool NeedsServerRelay(Projectile projectile) => Main.netMode == NetmodeID.Server
        && projectile.ModProjectile?.Mod is global::XianXia.XianXia && projectile.owner >= 0 && projectile.owner < Main.maxPlayers
        && projectile.owner != Main.myPlayer;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        playerOwned = IsOwn(projectile) && (projectile.friendly || projectile.minion);
        retired = false;
        if (!NeedsServerRelay(projectile) || !projectile.active) return;
        Publish(projectile);
    }
    public override void PostAI(Projectile projectile)
    {
        if (NeedsServerRelay(projectile) && projectile.active && projectile.netUpdate) Publish(projectile);
    }
    private static void Publish(Projectile projectile)
    {
        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
        projectile.netUpdate = false;
    }
    public override void OnKill(Projectile projectile, int timeLeft)
    {
        if (NeedsServerRelay(projectile))
            NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, projectile.identity, projectile.owner);
    }
}
