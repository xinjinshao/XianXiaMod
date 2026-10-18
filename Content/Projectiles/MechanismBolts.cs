using System;
using System.IO;
using XianXia.Common.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public abstract class MechanismBolt : ModProjectile
{
    protected abstract int Hits { get; }
    protected virtual bool ValidFlightState => true;
    public override string Texture => "XianXia/Content/Projectiles/SpiritBoltProjectile";
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled;
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) => ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(cancelled);
    public override void ReceiveExtraAI(BinaryReader reader) { bool stop = reader.ReadBoolean(); cancelled |= stop; }
    protected static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private bool ValidGeometry => ValidState(false);
    private bool ValidState(bool expired) => Projectile.width == 16 && Projectile.height == 8 && Projectile.timeLeft >= (expired ? 0 : 1) && Projectile.timeLeft <= 150
        && ValidFlightState && Finite(Projectile.Center) && Finite(Projectile.velocity) && float.IsFinite(Projectile.rotation);
    private Player LivingOwner => FindOwner(false);
    private Player FindOwner(bool expired) {
        if (!ValidState(expired) || Main.player == null || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers || Projectile.owner >= Main.player.Length) return null;
        Player owner = Main.player[Projectile.owner];
        return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Finite(owner.velocity) && ownerBinding.Matches(Projectile,owner) ? owner : null;
    }
    public override bool? CanDamage() => cancelled || LivingOwner == null ? false : null;
    public override bool? CanHitNPC(NPC target) => CanDamage() == false || target == null || !target.active || target.life <= 0 || !Finite(target.Center) || !Finite(target.velocity)
        || !Collision.CanHitLine(Projectile.Center,1,1,target.Center,1,1) ? false : null;
    protected void Cancel() {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = true; Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero;
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        Projectile.width = 16; Projectile.height = 8; Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft));
    }
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 8;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = Hits;
        Projectile.timeLeft = 150;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 20;
    }

    public override void AI()
    {
        if (cancelled || !ValidGeometry) { Cancel(); return; }
        Player owner = LivingOwner;
        if (owner == null) { if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return; }
        if (Projectile.velocity.LengthSquared() > .01f) Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center,.1f,.18f,.24f);
    }
}

public class SectMechanismBolt : MechanismBolt {
    protected override int Hits => 3;
}
public class HeavenLawBolt : MechanismBolt {
    protected override int Hits => 2;
    public override void OnHitNPC(NPC target,NPC.HitInfo hit,int damageDone) {
        if (CanHitNPC(target) == false) return;
        target.AddBuff(BuffID.Ichor,120);
    }
}
public class StarCalamityMechanismBolt : MechanismBolt {
    protected override int Hits => 4;
    protected override bool ValidFlightState => float.IsFinite(Projectile.ai[0]) && Projectile.ai[0] >= 0 && Projectile.ai[0] <= 2 && Projectile.ai[0] == MathF.Truncate(Projectile.ai[0]);
    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (CanDamage() == false || !Finite(oldVelocity)) { Cancel(); return false; }
        if (Projectile.ai[0] >= 2) return true;
        Projectile.ai[0]++;
        if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X;
        if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
        Projectile.netUpdate = true;
        return false;
    }
}
