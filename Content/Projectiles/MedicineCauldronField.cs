using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Projectiles;

namespace XianXia.Content.Projectiles;

public class MedicineCauldronField : ModProjectile
{
    private ArtifactProjectileOwnerBinding ownerBinding;
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) => ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
    public override string Texture => "XianXia/Content/Projectiles/GreenwoodArrayField";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 96;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1; Projectile.timeLeft = 300;
        Projectile.tileCollide = false; Projectile.ignoreWater = true;
        Projectile.netImportant = true;
    }
    private bool invalidState;
    public override bool? CanDamage() => false;
    public override void AI()
    {
        Player owner = global::XianXia.Common.Projectiles.FriendlyFieldOwner.Find(Projectile, 300,ownerBinding);
        bool validClock = float.IsFinite(Projectile.ai[0]) && Projectile.ai[0] >= 0f
            && Projectile.ai[0] < 60f && Projectile.ai[0] == System.MathF.Truncate(Projectile.ai[0]);
        invalidState |= !global::XianXia.Common.Projectiles.FriendlyFieldOwner.HasValidState(Projectile, 300) || !validClock;
        Projectile.velocity = Vector2.Zero;
        if (owner == null || invalidState)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) { invalidState = true; Projectile.friendly = false; if (Projectile.active) Projectile.Kill(); }
            return;
        }
        Projectile.rotation += 0.02f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.05f, 0.24f, 0.12f);
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        // A missed wave is discarded; acquiring a target cannot release stored attacks.
        if (++Projectile.ai[0] < 60) return;
        Projectile.ai[0] = 0;
        NPC target = null;
        float nearest = 600f * 600f;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC candidate = Main.npc[i];
            if (candidate == null || !float.IsFinite(candidate.Center.X) || !float.IsFinite(candidate.Center.Y)) continue;
            float distance = Vector2.DistanceSquared(Projectile.Center, candidate.Center);
            if (distance >= nearest || !candidate.CanBeChasedBy(this)
                || !Collision.CanHitLine(Projectile.Center, 1, 1, candidate.Center, 1, 1)) continue;
            target = candidate; nearest = distance;
        }
        if (target == null) return;
        Vector2 velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
        for (int i = 0; i < 2; i++)
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity.RotatedBy(i == 0 ? -0.08 : 0.08),
                ModContent.ProjectileType<MedicineSpiritBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
    }
}

public class MedicineSpiritBolt : ModProjectile
{
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled;
    public override string Texture => "XianXia/Content/Projectiles/SpiritBoltProjectile";
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) { ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile); Projectile.DamageType = DamageClass.Magic; }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1; Projectile.timeLeft = 90;
        Projectile.tileCollide = true; Projectile.ignoreWater = true;
    }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private bool ValidGeometry => Projectile.width == 16 && Projectile.height == 16 && Projectile.timeLeft > 0 && Projectile.timeLeft <= 90
        && Finite(Projectile.Center) && Finite(Projectile.velocity) && float.IsFinite(Projectile.rotation);
    private Player LivingOwner {
        get {
            if (!ValidGeometry || Main.player == null || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers || Projectile.owner >= Main.player.Length) return null;
            Player owner = Main.player[Projectile.owner];
            return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Finite(owner.velocity) && ownerBinding.Matches(Projectile,owner) ? owner : null;
        }
    }
    public override bool? CanDamage() => cancelled || LivingOwner == null ? false : null;
    public override bool? CanHitNPC(NPC target) => CanDamage() == false || target == null || !target.active || target.life <= 0 || !Finite(target.Center) || !Finite(target.velocity)
        || !Collision.CanHitLine(Projectile.Center,1,1,target.Center,1,1) ? false : null;
    public override void SendExtraAI(System.IO.BinaryWriter writer) => writer.Write(cancelled);
    public override void ReceiveExtraAI(System.IO.BinaryReader reader) { bool stop = reader.ReadBoolean(); cancelled |= stop; Projectile.DamageType = DamageClass.Magic; }
    private void Cancel()
    {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = true; Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero;
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        Projectile.width = Projectile.height = 16; Projectile.timeLeft = System.Math.Min(6,System.Math.Max(0,Projectile.timeLeft));
    }
    public override void AI()
    {
        if (cancelled || !ValidGeometry) { Cancel(); return; }
        if (LivingOwner == null) { if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return; }
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center,0.05f,0.24f,0.12f);
    }
}
