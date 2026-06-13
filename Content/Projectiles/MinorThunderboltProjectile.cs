using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class MinorThunderboltProjectile : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 16;

        Projectile.height = 64;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Generic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



    }



    public override void AI()

    {

        if (Projectile.velocity.LengthSquared() > 0.01f)

            Projectile.rotation = Projectile.velocity.ToRotation();

        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);

    }



}
