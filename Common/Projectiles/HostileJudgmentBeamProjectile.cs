using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using XianXia.Common.Projectiles;

namespace XianXia.Common.Projectiles;

public abstract class HostileJudgmentBeamProjectile : ModProjectile
{
    protected virtual int BeamWidth => 32;
    protected virtual int BeamHeight => 480;
    protected virtual int PhysicalWidth => BeamWidth;
    protected virtual int PhysicalHeight => BeamHeight;
    protected virtual Color BeamColor => Color.Cyan;
    public const int Lifetime = 30, CancellationFade = 6;
    private readonly HostileSourceBinding sourceBinding = new();
    private bool invalidState;
    private bool ValidGeometry() => float.IsFinite(Projectile.Center.X) && float.IsFinite(Projectile.Center.Y)
        && Projectile.velocity == Vector2.Zero && Projectile.width == PhysicalWidth && Projectile.height == PhysicalHeight;
    public override void SetDefaults()
    {
        Projectile.width = PhysicalWidth; Projectile.height = PhysicalHeight;
        Projectile.hostile = true; Projectile.friendly = false; Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime; Projectile.tileCollide = false; Projectile.ignoreWater = true;
        Projectile.netImportant = true;
    }
    public override void OnSpawn(IEntitySource source) => sourceBinding.Capture(source);
    public override void SendExtraAI(BinaryWriter writer) => sourceBinding.Write(writer, Projectile, Lifetime);
    public override void ReceiveExtraAI(BinaryReader reader) { if (!sourceBinding.Read(reader, Projectile, Lifetime)) invalidState = true; }
    public override bool? CanDamage() => !invalidState && ValidGeometry() && Projectile.timeLeft > 0
        && Projectile.timeLeft <= Lifetime && sourceBinding.IsValid();
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (PhysicalWidth == BeamWidth && PhysicalHeight == BeamHeight) return null;
        if (CanDamage() != true || targetHitbox.Width <= 0 || targetHitbox.Height <= 0) return false;
        double left = Projectile.Center.X - BeamWidth / 2d, top = Projectile.Center.Y - BeamHeight / 2d;
        return (double)targetHitbox.X + targetHitbox.Width > left && targetHitbox.X < left + BeamWidth
            && (double)targetHitbox.Y + targetHitbox.Height > top && targetHitbox.Y < top + BeamHeight;
    }
    public override bool CanHitPlayer(Player target) => target != null && CanDamage() == true && target.active && !target.dead
        && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && float.IsFinite(target.velocity.X) && float.IsFinite(target.velocity.Y)
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
        var rectangle = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X) - BeamWidth / 2,
            (int)(Projectile.Center.Y - Main.screenPosition.Y) - BeamHeight / 2, BeamWidth, BeamHeight);
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, BeamColor * opacity);
        return false;
    }
}
