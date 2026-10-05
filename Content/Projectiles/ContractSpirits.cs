using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Buffs;

namespace XianXia.Content.Projectiles;

public class FurnaceAshSpirit : SmallArtifactSpirit
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/FurnaceAshSpiritContract";
    protected override int SpiritBuff => ModContent.BuffType<FurnaceAshSpiritBuff>();
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.OnFire3, 120);
}

public class StarAbyssSpirit : SmallArtifactSpirit
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/StarAbyssLarvaContract";
    protected override int SpiritBuff => ModContent.BuffType<StarAbyssSpiritBuff>();
    protected override float AttackRange => 800f;
    public override bool MinionContactDamage() => false;
    protected override Vector2 AimPosition(Player owner, NPC target) =>
        target.Center + (owner.Center - target.Center).SafeNormalize(Vector2.UnitY) * 240f;
    protected override void AttackTarget(Player owner, NPC target)
    {
        if (Main.myPlayer != Projectile.owner) return;
        if (target == null) { Projectile.ai[0] = 0; return; }
        if (++Projectile.ai[0] < 60) return;
        Projectile.ai[0] = 0;
        Vector2 velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 9f;
        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity,
            ModContent.ProjectileType<ContractSpiritBolt>(), Projectile.damage, 1f, Projectile.owner);
    }
}

public class ContractSpiritBolt : SpiritBoltProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBoltProjectile";
    public override void SetStaticDefaults() => ProjectileID.Sets.MinionShot[Type] = true;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.DamageType = DamageClass.Summon;
    }
    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ) base.AI();
    }
}
