using Terraria;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

// Early enemies use a hostile bolt without the boss bolt's pressure debuff.
public class EnemySpiritBoltProjectile : SpiritBoltProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBoltProjectile";

    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.DamageType = DamageClass.Generic;
    }

    public override void AI()
    {
        if (!Main.dedServ) base.AI();
    }
}
