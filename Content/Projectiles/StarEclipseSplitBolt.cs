using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class StarEclipseSplitBolt : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 32;

        Projectile.height = 32;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Generic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



        Projectile.penetrate = 2;

        Projectile.timeLeft = 160;

    }



    public override void AI()

    {

        if (Projectile.velocity.LengthSquared() > 0.01f)

            Projectile.rotation = Projectile.velocity.ToRotation();

        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);

    }





    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)

    {

        if (Projectile.owner == Main.myPlayer && Projectile.ai[0] == 0f)

        {

            for (int i = -1; i <= 1; i += 2)

            {

                Projectile.NewProjectile(

                    Projectile.GetSource_OnHit(target),

                    Projectile.Center,

                    Projectile.velocity.RotatedBy(MathHelper.ToRadians(18f * i)) * 0.85f,

                    ModContent.ProjectileType<SpiritBolt>(),

                    Math.Max(1, Projectile.damage / 2),

                    1f,

                    Projectile.owner,

                    1f);

            }

        }

    }



}
