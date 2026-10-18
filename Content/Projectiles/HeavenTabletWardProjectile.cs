using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Projectiles;
namespace XianXia.Content.Projectiles;
public class HeavenTabletWardProjectile : ModProjectile {
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled;
    public override string Texture => "XianXia/Content/Items/HandGenerated/HeavenTabletSeal";
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) => ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(cancelled);
    public override void ReceiveExtraAI(BinaryReader reader) { bool stop = reader.ReadBoolean(); cancelled |= stop; }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private bool ValidGeometry => Projectile.width == 40 && Projectile.height == 40 && Projectile.timeLeft > 0 && Projectile.timeLeft <= 120
        && Finite(Projectile.Center) && Finite(Projectile.velocity) && float.IsFinite(Projectile.rotation)
        && float.IsFinite(Projectile.ai[0]) && Projectile.ai[0] >= 0 && Projectile.ai[0] <= 30 && Projectile.ai[0] == MathF.Truncate(Projectile.ai[0]);
    private Player LivingOwner {
        get {
            if (!Projectile.active || !ValidGeometry || Main.player == null || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers || Projectile.owner >= Main.player.Length) return null;
            Player owner = Main.player[Projectile.owner];
            return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Finite(owner.velocity)
                && Vector2.DistanceSquared(owner.Center,Projectile.Center) <= 1600f * 1600f && ownerBinding.Matches(Projectile,owner) ? owner : null;
        }
    }
    internal bool CanProvideWard(Player player) => !cancelled && ReferenceEquals(LivingOwner,player) && player != null
        && Vector2.DistanceSquared(player.Center,Projectile.Center) <= 160f * 160f && Collision.CanHitLine(player.Center,1,1,Projectile.Center,1,1);
    public override bool? CanDamage() => cancelled || LivingOwner == null ? false : null;
    public override bool? CanHitNPC(NPC target) {
        Player owner = LivingOwner;
        return cancelled || owner == null || target == null || !target.active || target.life <= 0 || !Finite(target.Center) || !Finite(target.velocity)
            || !Collision.CanHitLine(Projectile.Center,1,1,target.Center,1,1) || !Collision.CanHitLine(owner.Center,1,1,target.Center,1,1) ? false : null;
    }
    public override void SetDefaults() {
        Projectile.width = Projectile.height = 40; Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee; Projectile.penetrate = 3; Projectile.timeLeft = 120;
        Projectile.tileCollide = true; Projectile.ignoreWater = true; Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 20;
    }
    private void Cancel() {
        cancelled = true; Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero; Projectile.ai[0] = 0;
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        Projectile.width = Projectile.height = 40; Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft));
        if (Main.netMode != NetmodeID.MultiplayerClient && Projectile.active) {
            // Vanilla Kill indexes the owner table; corrupted/missing slots cannot use that path.
            if (Main.player != null && Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Projectile.owner < Main.player.Length && Main.player[Projectile.owner] != null) Projectile.Kill();
            else Projectile.active = false;
        }
    }
    public override void AI() {
        if (cancelled || !ValidGeometry) { Cancel(); return; }
        Player owner = LivingOwner;
        if (owner == null) { if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return; }
        float distance = Vector2.DistanceSquared(owner.Center,Projectile.Center);
        Projectile.ai[0] = Math.Min(30f,Projectile.ai[0] + 1f);
        if (Projectile.ai[0] >= 30) {
            Projectile.tileCollide = false;
            if (distance <= 24f * 24f) { if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return; }
            Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12;
        }
        Projectile.rotation += .05f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center,.24f,.18f,.04f);
    }
    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (CanDamage() == false || !Finite(oldVelocity)) { Cancel(); return false; }
        Projectile.ai[0] = 30; Projectile.tileCollide = false;
        if (Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        return false;
    }
}
