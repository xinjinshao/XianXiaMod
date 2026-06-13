using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class WoodgrainSwordProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.05f, 0.25f, 0.2f);

        if (Projectile.timeLeft < 45)
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 toOwner = owner.Center - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner.SafeNormalize(Vector2.Zero) * 12f, 0.08f);
            if (toOwner.Length() < 24f)
            {
                Projectile.Kill();
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.timeLeft = System.Math.Min(Projectile.timeLeft, 35);
        Projectile.tileCollide = false;
        return false;
    }
}
