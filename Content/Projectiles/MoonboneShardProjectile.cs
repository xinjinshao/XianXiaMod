using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class MoonboneShardProjectile : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 28;

        Projectile.height = 20;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Generic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



        Projectile.penetrate = 2;

        Projectile.timeLeft = 80;

    }



    public override void AI()

    {

        if (Projectile.velocity.LengthSquared() > 0.01f)

            Projectile.rotation = Projectile.velocity.ToRotation();

        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);

    }





    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)

    {

        Projectile.velocity *= 0.2f;

        Projectile.timeLeft = Math.Min(Projectile.timeLeft, 24);

    }



}
