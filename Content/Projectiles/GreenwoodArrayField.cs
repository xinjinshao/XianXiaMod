using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class GreenwoodArrayField : ModProjectile

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

        Projectile.timeLeft = 300;

        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;

        Projectile.localNPCHitCooldown = 30;

    }





    public override void AI()

    {

        Projectile.velocity = Vector2.Zero;

        Projectile.rotation += 0.02f;

        Player owner = Main.player[Projectile.owner];

        if (owner.active && owner.Hitbox.Intersects(Projectile.Hitbox) && Main.GameUpdateCount % 60 == 0)

        {

            owner.statLife = Math.Min(owner.statLifeMax2, owner.statLife + 1);

            owner.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().RestoreSpiritualEnergy(1);

        }

        Lighting.AddLight(Projectile.Center, 0.05f, 0.24f, 0.12f);

    }



}
