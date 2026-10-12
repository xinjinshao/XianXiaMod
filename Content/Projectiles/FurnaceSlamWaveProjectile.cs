using Microsoft.Xna.Framework;
using XianXia.Common.Projectiles;
namespace XianXia.Content.Projectiles;

public class FurnaceSlamWaveProjectile : HostileJudgmentBeamProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBolt";
    public const int WaveWidth = 280, WaveHeight = 32, AnchorSize = 32;
    protected override int BeamWidth => WaveWidth;
    protected override int BeamHeight => WaveHeight;
    protected override int PhysicalWidth => AnchorSize;
    protected override int PhysicalHeight => AnchorSize;
    protected override Color BeamColor => Color.OrangeRed;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CanDamage() != true || targetHitbox.Width <= 0 || targetHitbox.Height <= 0) return false;
        // The small engine anchor survives world edges; damage still covers the entire warned rectangle.
        double left = Projectile.Center.X - WaveWidth / 2d, top = Projectile.Center.Y - WaveHeight / 2d;
        return (double)targetHitbox.X + targetHitbox.Width > left && targetHitbox.X < left + WaveWidth
            && (double)targetHitbox.Y + targetHitbox.Height > top && targetHitbox.Y < top + WaveHeight;
    }
}
