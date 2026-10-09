using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using XianXia.Common.Projectiles;

namespace XianXia.Content.Projectiles;

public class TabletJudgmentBeamProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBolt";
    public const int Lifetime = 30, CancellationFade = 6;
    private readonly HostileSourceBinding sourceBinding = new();
    private bool invalidState;
    private bool ValidGeometry() => float.IsFinite(Projectile.Center.X) && float.IsFinite(Projectile.Center.Y)
        && Projectile.velocity == Vector2.Zero && Projectile.width == 32 && Projectile.height == 480;
    public override void SetDefaults()
    {
        Projectile.width = 32; Projectile.height = 480;
        Projectile.hostile = true; Projectile.friendly = false; Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime; Projectile.tileCollide = false; Projectile.ignoreWater = true;
        Projectile.netImportant = true;
    }
    public override void OnSpawn(IEntitySource source) => sourceBinding.Capture(source);
    public override void SendExtraAI(BinaryWriter writer) => sourceBinding.Write(writer, Projectile, Lifetime);
    public override void ReceiveExtraAI(BinaryReader reader) { if (!sourceBinding.Read(reader, Projectile, Lifetime)) invalidState = true; }
    public override bool? CanDamage() => !invalidState && ValidGeometry() && Projectile.timeLeft > 0
        && Projectile.timeLeft <= Lifetime && sourceBinding.IsValid();
    public override bool CanHitPlayer(Player target) => CanDamage() == true && target.active && !target.dead
        && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1);
    public override void AI()
    {
        if (!ValidGeometry()) { invalidState = true; sourceBinding.CancelOnAuthority(Projectile, CancellationFade); }
        sourceBinding.CancelIfInvalid(Projectile, CancellationFade, Lifetime);
        Projectile.velocity = Vector2.Zero;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.dedServ || !ValidGeometry() || Projectile.timeLeft <= 0) return false;
        float opacity = CanDamage() == true ? 0.85f : 0.15f;
        var rectangle = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X) - 16,
            (int)(Projectile.Center.Y - Main.screenPosition.Y) - 240, 32, 480);
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.Cyan * opacity);
        return false;
    }
}
