using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class CloudpiercerSwordProjectile : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 56;

        Projectile.height = 24;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Generic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



        Projectile.penetrate = 2;

        Projectile.timeLeft = 105;

    }



    public override void AI()

    {

        if (Projectile.velocity.LengthSquared() > 0.01f)

            Projectile.rotation = Projectile.velocity.ToRotation();

        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);

    }





    public override void OnKill(int timeLeft)

    {

        if (Projectile.owner == Main.myPlayer)

        {

            Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.55f) * 6f;

            Projectile.NewProjectile(

                Projectile.GetSource_FromThis(),

                Projectile.Center,

                velocity,

                ModContent.ProjectileType<CloudWispProjectile>(),

                Math.Max(1, Projectile.damage / 3),

                1f,

                Projectile.owner);

        }

    }



    public override bool OnTileCollide(Vector2 oldVelocity)

    {

        Projectile.tileCollide = false;

        Projectile.timeLeft = Math.Min(Projectile.timeLeft, 24);

        return false;

    }



}
