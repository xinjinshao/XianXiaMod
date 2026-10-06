using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public partial class BossArrayFieldProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/ThunderTalismanArray";

    public const int Lifetime = 120;
    public const int WarningTicks = 45;
    public const int FadeTicks = 15;

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
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
        CancelInvalidSource();
        if (Main.netMode != NetmodeID.MultiplayerClient
            && (Projectile.timeLeft == Lifetime - WarningTicks || Projectile.timeLeft == FadeTicks)) Projectile.netUpdate = true;
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
        return SourceAllowsDamage() && Projectile.timeLeft > FadeTicks && Projectile.timeLeft <= Lifetime - WarningTicks;
    }

    public override bool CanHitPlayer(Player target) => CanDamage() == true
        && target.active && !target.dead
        && Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1);

    public override void SendExtraAI(BinaryWriter writer) => sourceBinding.Write(writer, Projectile, Lifetime);
    public override void ReceiveExtraAI(BinaryReader reader) => sourceBinding.Read(reader, Projectile, Lifetime);

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        // Preserve the native player-hit/Buff path, with the same active window and visibility check.
        if (!CanHitPlayer(target)) return;
        target.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 60 * 2);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.dedServ || Projectile.timeLeft <= 0 || Projectile.timeLeft > Lifetime) return false;
        bool active = CanDamage() == true;
        Color color = Color.MediumPurple * (Projectile.timeLeft <= FadeTicks
            ? Projectile.timeLeft / (float)FadeTicks * 0.4f : active ? 0.85f : 0.6f);
        Rectangle box = new((int)(Projectile.position.X - Main.screenPosition.X),
            (int)(Projectile.position.Y - Main.screenPosition.Y), Projectile.width, Projectile.height);
        var pixel = TextureAssets.MagicPixel.Value;
        if (active) Main.spriteBatch.Draw(pixel, box, color * 0.25f);
        Main.spriteBatch.Draw(pixel, new Rectangle(box.X, box.Y, box.Width, 2), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(box.X, box.Y + box.Height - 2, box.Width, 2), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(box.X, box.Y, 2, box.Height), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(box.X + box.Width - 2, box.Y, 2, box.Height), color);
        return false;
    }
}
