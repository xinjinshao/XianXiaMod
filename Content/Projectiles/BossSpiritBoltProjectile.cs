using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class BossSpiritBoltProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/Generated/SpiritBolt";

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 240;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.01f)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        Lighting.AddLight(Projectile.Center, 0.1f, 0.16f, 0.22f);
        if (Main.rand.NextBool(5))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemSapphire);
            dust.noGravity = true;
            dust.velocity *= 0.2f;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 60);
    }
}
