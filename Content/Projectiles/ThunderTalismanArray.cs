using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class ThunderTalismanArray : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 96;

        Projectile.height = 96;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Generic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



        Projectile.penetrate = -1;

        Projectile.timeLeft = 240;

        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;

        Projectile.localNPCHitCooldown = 30;

    }





    public override void AI()

    {

        Projectile.velocity = Vector2.Zero;

        Projectile.rotation += 0.035f;

        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft % 45 == 0)

        {

            Projectile.NewProjectile(

                Projectile.GetSource_FromAI(),

                Projectile.Center + new Vector2(Main.rand.NextFloat(-48f, 48f), -220f),

                Vector2.UnitY * 13f,

                ModContent.ProjectileType<MinorThunderboltProjectile>(),

                Math.Max(1, Projectile.damage / 2),

                0.5f,

                Projectile.owner);

        }

        Lighting.AddLight(Projectile.Center, 0.12f, 0.08f, 0.25f);

    }



}
