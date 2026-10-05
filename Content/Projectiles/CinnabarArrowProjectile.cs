using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class CinnabarArrowProjectile : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FireArrow}";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 10;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.arrow = true; Projectile.penetrate = 2; Projectile.timeLeft = 180;
        Projectile.tileCollide = true; Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = -1;
    }
    public override void AI()
    {
        Projectile.ai[0] = Math.Min(20f, Projectile.ai[0] + 1f);
        if (Projectile.ai[0] >= 20f) Projectile.velocity.Y = Math.Min(12f, Projectile.velocity.Y + 0.1f);
        Projectile.rotation = Projectile.velocity.ToRotation() + MathF.PI / 2f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.3f, 0.12f, 0.02f);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.OnFire3, 120);
}
