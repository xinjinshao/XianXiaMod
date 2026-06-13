using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class TribulationLightningProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/MinorThunderboltProjectile";

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 64;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 120;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + 0.18f, 8f, 18f);
        Lighting.AddLight(Projectile.Center, 0.15f, 0.25f, 0.35f);

        if (Main.rand.NextBool(3))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
            dust.noGravity = true;
            dust.velocity *= 0.25f;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 60 * 3);
    }
}
