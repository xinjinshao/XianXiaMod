using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class BossArrayFieldProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/Generated/ThunderTalismanArray";

    public override void SetDefaults()
    {
        Projectile.width = 96;
        Projectile.height = 96;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 120;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Projectile.velocity *= 0f;
        Projectile.rotation += 0.035f;
        Projectile.alpha = Projectile.timeLeft > 90
            ? (int)MathHelper.Lerp(160f, 40f, (120f - Projectile.timeLeft) / 30f)
            : (int)MathHelper.Lerp(40f, 190f, 1f - Projectile.timeLeft / 90f);

        Lighting.AddLight(Projectile.Center, 0.12f, 0.05f, 0.24f);
        if (Main.rand.NextBool(4))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst);
            dust.noGravity = true;
            dust.velocity *= 0.15f;
        }
    }

    public override bool? CanDamage()
    {
        return Projectile.timeLeft < 96;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 60 * 2);
    }
}
