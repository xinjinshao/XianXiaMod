using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public abstract class MechanismBolt : ModProjectile
{
    protected abstract int Hits { get; }
    public override string Texture => "XianXia/Content/Projectiles/SpiritBoltProjectile";
    public override void SetDefaults()
    {
        Projectile.width = 16; Projectile.height = 8;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = Hits; Projectile.timeLeft = 150;
        Projectile.tileCollide = true; Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 20;
    }
    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.1f, 0.18f, 0.24f);
    }
}
public class SectMechanismBolt : MechanismBolt
{
    protected override int Hits => 3;
}
public class HeavenLawBolt : MechanismBolt
{
    protected override int Hits => 2;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Ichor, 120);
}
public class StarCalamityMechanismBolt : MechanismBolt
{
    protected override int Hits => 4;
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.ai[0] >= 2) return true;
        Projectile.ai[0]++;
        if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X;
        if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
        Projectile.netUpdate = true;
        return false;
    }
}
