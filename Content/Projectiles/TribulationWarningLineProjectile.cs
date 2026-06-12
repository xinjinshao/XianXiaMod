using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class TribulationWarningLineProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 16;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 36;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = false;
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
        Projectile.alpha = (int)MathHelper.Lerp(40f, 180f, Projectile.timeLeft / 36f);
        Lighting.AddLight(Projectile.Center, 0.12f, 0.22f, 0.35f);

        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
            dust.noGravity = true;
            dust.velocity *= 0.15f;
        }
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        Vector2 position = Projectile.Center + new Vector2(0f, -540f);
        Vector2 velocity = new(0f, 11f);
        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            position,
            velocity,
            ModContent.ProjectileType<TribulationLightningProjectile>(),
            Projectile.damage,
            1.5f,
            Projectile.owner);
    }
}
