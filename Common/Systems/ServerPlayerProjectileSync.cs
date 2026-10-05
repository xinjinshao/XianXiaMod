using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

// Native automatic sync is owner-driven; these projectiles are created by the server for a player.
public class ServerPlayerProjectileSync : GlobalProjectile
{
    private static bool NeedsServerRelay(Projectile projectile) => Main.netMode == NetmodeID.Server
        && projectile.ModProjectile?.Mod is global::XianXia.XianXia && projectile.owner >= 0 && projectile.owner < Main.maxPlayers
        && projectile.owner != Main.myPlayer;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
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
