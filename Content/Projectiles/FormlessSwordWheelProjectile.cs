using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class FormlessSwordWheelProjectile : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 80;

        Projectile.height = 80;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Generic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



        Projectile.penetrate = 5;

        Projectile.timeLeft = 150;

        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;

        Projectile.localNPCHitCooldown = 12;

    }





    public override void AI()

    {

        Player owner = Main.player[Projectile.owner];

        Projectile.rotation += 0.28f;

        if (owner.active)

        {

            Vector2 drift = owner.velocity.SafeNormalize(Vector2.Zero) * 3f;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.velocity + drift, 0.04f);

        }

        Lighting.AddLight(Projectile.Center, 0.08f, 0.2f, 0.24f);

    }



}
