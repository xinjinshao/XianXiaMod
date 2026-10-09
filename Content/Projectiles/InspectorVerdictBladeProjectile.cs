using Microsoft.Xna.Framework;
using XianXia.Common.Projectiles;
namespace XianXia.Content.Projectiles;

// A stationary close-range slash: warning and collision share the same 160 x 48 body.
public class InspectorVerdictBladeProjectile : HostileJudgmentBeamProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBolt";
    protected override int BeamWidth => 160;
    protected override int BeamHeight => 48;
    protected override Color BeamColor => Color.OrangeRed;
}
