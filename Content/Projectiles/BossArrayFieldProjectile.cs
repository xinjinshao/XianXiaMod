using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class BossArrayFieldProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/ThunderTalismanArray";

    public const int Lifetime = 120;
    public const int WarningTicks = 45;

    public override void SetDefaults()
    {
        Projectile.width = 96;
        Projectile.height = 96;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Projectile.velocity *= 0f;
        Projectile.rotation += 0.035f;
        int age = Lifetime - Projectile.timeLeft;
        Projectile.alpha = age < WarningTicks
            ? (int)MathHelper.Lerp(200f, 40f, age / (float)WarningTicks)
            : (int)MathHelper.Lerp(40f, 190f, (age - WarningTicks) / (float)(Lifetime - WarningTicks));
        if (Main.dedServ) return;

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
        return Projectile.timeLeft > 0 && Projectile.timeLeft <= Lifetime - WarningTicks;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 60 * 2);
    }
}
