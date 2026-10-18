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
    public override void OnHitNPC(NPC target,NPC.HitInfo hit,int damageDone) {
        if (!CanAct || target == null || !target.active || target.life <= 0 || !float.IsFinite(target.Center.X) || !float.IsFinite(target.Center.Y)) return;
        target.AddBuff(BuffID.OnFire3,120);
    }
}

public class StarAbyssSpirit : SmallArtifactSpirit
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/StarAbyssLarvaContract";
    protected override int SpiritBuff => ModContent.BuffType<StarAbyssSpiritBuff>();
    protected override float AttackRange => 800f;
    protected override bool ValidAttackState => Counter(Projectile.ai[0],59) && Projectile.ai[1] == 0;
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
        TryFireSpiritBolt(velocity,Projectile.damage);
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
}


public abstract class FanContractSpirit : SmallArtifactSpirit
{
    protected abstract int ShotCount { get; }
    protected abstract int ShotInterval { get; }
    protected override bool ValidAttackState => Counter(Projectile.ai[0],ShotInterval - 1) && Projectile.ai[1] == 0;
    protected override float AttackRange => 1000f;
    public override bool MinionContactDamage() => false;
    protected override Vector2 AimPosition(Player owner, NPC target) =>
        target.Center + (owner.Center - target.Center).SafeNormalize(Vector2.UnitY) * 280f;
    protected override void AttackTarget(Player owner, NPC target)
    {
        if (Main.myPlayer != Projectile.owner) return;
        if (target == null) { Projectile.ai[0] = 0; return; }
        if (++Projectile.ai[0] < ShotInterval) return;
        Projectile.ai[0] = 0;
        Vector2 velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 11f;
        int damage = (int)System.Math.Max(1L,(long)Projectile.damage * 3 / 4);
        for (int i = 0; i < ShotCount; i++)
        {
            double angle = (i - (ShotCount - 1) / 2f) * 0.18;
            if (!TryFireSpiritBolt(velocity.RotatedBy(angle),damage)) break;
        }
    }
}

public class NascentSoulSpirit : FanContractSpirit
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/NascentSoulCloneTalisman";
    protected override int SpiritBuff => ModContent.BuffType<NascentSoulSpiritBuff>();
    protected override int ShotCount => 2;
    protected override int ShotInterval => 75;
}

public class ArchivedSoulSpirit : FanContractSpirit
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/ArchivedImmortalSoulContract";
    protected override int SpiritBuff => ModContent.BuffType<ArchivedSoulSpiritBuff>();
    protected override int ShotCount => 3;
    protected override int ShotInterval => 90;
    public override void SetDefaults() { base.SetDefaults(); Projectile.minionSlots = 2; }
}

public class CelestialPuppetSpirit : SmallArtifactSpirit
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/CelestialPuppetToken";
    protected override int SpiritBuff => ModContent.BuffType<CelestialPuppetSpiritBuff>();
    protected override float AttackRange => 900f;
    protected override bool ValidAttackState => Counter(Projectile.ai[0],89) && Counter(Projectile.ai[1],18);
    protected override float CombatSpeed => Projectile.ai[1] > 0 ? 24f : 7f;
    public override bool MinionContactDamage() => Projectile.ai[1] > 0;
    public override void AI()
    {
        base.AI();
        if (Projectile.ai[1] <= 0) Projectile.friendly = false;
    }
    protected override void AttackTarget(Player owner, NPC target)
    {
        if (Main.myPlayer != Projectile.owner)
        {
            // Predict the end of an already-synchronized dash; only the owner starts one.
            if (Projectile.ai[1] > 0) Projectile.ai[1]--;
            return;
        }
        if (target == null)
        {
            if (Projectile.ai[1] > 0) Projectile.netUpdate = true;
            Projectile.ai[0] = Projectile.ai[1] = 0;
            return;
        }
        if (Projectile.ai[1] > 0)
        {
            if (--Projectile.ai[1] == 0) Projectile.netUpdate = true;
            return;
        }
        if (++Projectile.ai[0] < 90) return;
        Projectile.ai[0] = 0;
        Projectile.ai[1] = 18;
        Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 24f;
        Projectile.netUpdate = true;
    }
}
