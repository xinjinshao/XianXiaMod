using Terraria.DataStructures;
using XianXia.Common.Projectiles;

namespace XianXia.Content.Projectiles;

public partial class BossArrayFieldProjectile
{
    private readonly HostileSourceBinding sourceBinding = new();
    public override void OnSpawn(IEntitySource source) => sourceBinding.Capture(source);
    private bool SourceAllowsDamage() => sourceBinding.IsValid();
    private void CancelInvalidSource() => sourceBinding.CancelIfInvalid(Projectile, FadeTicks, Lifetime);
}
