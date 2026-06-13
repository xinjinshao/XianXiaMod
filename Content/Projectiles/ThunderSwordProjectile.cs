using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class ThunderSwordProjectile : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 48;

        Projectile.height = 16;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Generic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



        Projectile.penetrate = 3;

        Projectile.timeLeft = 110;

    }



    public override void AI()

    {

        if (Projectile.velocity.LengthSquared() > 0.01f)

            Projectile.rotation = Projectile.velocity.ToRotation();

        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);

    }





    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)

    {

        if (Projectile.owner == Main.myPlayer && Main.rand.NextBool(3))

        {

            Projectile.NewProjectile(

                Projectile.GetSource_OnHit(target),

                target.Center + new Vector2(Main.rand.NextFloat(-32f, 32f), -240f),

                Vector2.UnitY * 12f,

                ModContent.ProjectileType<MinorThunderboltProjectile>(),

                Math.Max(1, Projectile.damage / 2),

                0.5f,

                Projectile.owner);

        }

    }



}
