using Terraria.ModLoader;
namespace XianXia.Content.Projectiles;

public class ThunderCaseStrikeProjectile : MinorThunderboltProjectile
{
    protected override bool FixedMelee => true;
    public override string Texture => "XianXia/Content/Projectiles/MinorThunderboltProjectile";
    public override void SetDefaults() { base.SetDefaults(); Projectile.netImportant = true; }
}
