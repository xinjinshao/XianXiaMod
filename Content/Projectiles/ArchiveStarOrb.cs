using System;
using Terraria;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class ArchiveStarOrb : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/StarEclipseSplitBolt";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 4;
        Projectile.timeLeft = 150;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }
    public override bool? CanDamage() => Projectile.ai[0] >= 18f ? null : false;
    public override void AI()
    {
        Projectile.ai[0] = Math.Min(18f, Projectile.ai[0] + 1f);
        Projectile.rotation = Projectile.velocity.ToRotation() + (18f - Projectile.ai[0]) * 0.025f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.18f, 0.12f, 0.32f);
    }
}
