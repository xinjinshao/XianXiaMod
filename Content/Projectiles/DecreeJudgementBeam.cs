using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class DecreeJudgementBeam : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 32;

        Projectile.height = 128;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Generic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



        Projectile.penetrate = 8;

        Projectile.timeLeft = 36;

        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;

        Projectile.localNPCHitCooldown = 8;

    }



    public override void AI()

    {

        if (Projectile.velocity.LengthSquared() > 0.01f)

            Projectile.rotation = Projectile.velocity.ToRotation();

        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);

    }





    public override bool? CanDamage()

    {

        return Projectile.timeLeft < 18;

    }



}
