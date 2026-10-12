using Microsoft.Xna.Framework;
using XianXia.Common.Projectiles;
namespace XianXia.Content.Projectiles;
public class InspectorDecreeBeamProjectile : HostileJudgmentBeamProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBolt";
    protected override int BeamWidth => 64;
    protected override int PhysicalWidth => 32;
    protected override int PhysicalHeight => 32;
    protected override Color BeamColor => Color.OrangeRed;
}
