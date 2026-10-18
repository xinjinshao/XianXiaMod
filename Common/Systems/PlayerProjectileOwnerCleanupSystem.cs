using Terraria;
using Terraria.ModLoader;
namespace XianXia.Common.Systems;
// Projectile.Update reads owner state before GlobalProjectile.PreAI; reject corrupt slots before that loop.
public class PlayerProjectileOwnerCleanupSystem : ModSystem {
    public override void PreUpdateProjectiles() {
        foreach (var projectile in Main.ActiveProjectiles)
            if (projectile.ModProjectile?.Mod is global::XianXia.XianXia)
                projectile.GetGlobalProjectile<ServerPlayerProjectileSync>().ValidateOwnerBeforeUpdate(projectile);
    }
}
