using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class WoodgrainSwordProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 16;
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


public class CloudpiercerSwordProjectile : ModProjectile
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


public class CloudWispProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 16;
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


public class FormlessSwordWheelProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
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


public class MoonboneShardProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 16;
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


public class CinnabarTalismanFlame : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
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


    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.OnFire3, 60 * 3);
    }

}


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


public class SpiritBolt : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 8;
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


public class StarEclipseSplitBolt : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
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
