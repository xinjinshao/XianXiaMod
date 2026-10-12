using Microsoft.Xna.Framework;
using XianXia.Common.Projectiles;
namespace XianXia.Content.Projectiles;

public class FurnaceSlamWaveProjectile : HostileJudgmentBeamProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBolt";
    protected override int BeamWidth => 280;
    protected override int BeamHeight => 32;
    protected override Color BeamColor => Color.OrangeRed;
}
