using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles.Generated;

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
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
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
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
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
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
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
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
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
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
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
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
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
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
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
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);
    }
}
