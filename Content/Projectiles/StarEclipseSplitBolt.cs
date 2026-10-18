using System;
using System.IO;
using XianXia.Common.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class StarEclipseSplitBolt : ModProjectile
{
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled;
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) => ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(cancelled);
    public override void ReceiveExtraAI(BinaryReader reader) { bool stop = reader.ReadBoolean(); cancelled |= stop; }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private bool ValidGeometry => ValidState(false);
    private bool ValidState(bool expired) => Projectile.width == 32 && Projectile.height == 32 && Projectile.timeLeft >= (expired ? 0 : 1) && Projectile.timeLeft <= 160
        && Finite(Projectile.Center) && Finite(Projectile.velocity) && float.IsFinite(Projectile.rotation);
    private Player LivingOwner => FindOwner(false);
    private Player FindOwner(bool expired) {
        if (!ValidState(expired) || Main.player == null || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers || Projectile.owner >= Main.player.Length) return null;
        Player owner = Main.player[Projectile.owner];
        return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Finite(owner.velocity) && ownerBinding.Matches(Projectile,owner) ? owner : null;
    }
    public override bool? CanDamage() => cancelled || LivingOwner == null ? false : null;
    public override bool? CanHitNPC(NPC target) => CanDamage() == false || target == null || !target.active || target.life <= 0 || !Finite(target.Center) || !Finite(target.velocity)
        || !Collision.CanHitLine(Projectile.Center,1,1,target.Center,1,1) ? false : null;
    private void Cancel() {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = true; Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero;
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        Projectile.width = 32; Projectile.height = 32; Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft));
    }
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 160;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        if (cancelled || !ValidGeometry) { Cancel(); return; }
        Player owner = LivingOwner;
        if (owner == null) { if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return; }
        if (Projectile.velocity.LengthSquared() > .01f) Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center,.06f,.18f,.2f);
    }
    public override void OnHitNPC(NPC target,NPC.HitInfo hit,int damageDone) {
        // Vanilla friendly hits are owned by the firing client, including killing hits.
        if (CanDamage() == false || Main.netMode == NetmodeID.Server || Projectile.owner != Main.myPlayer || Projectile.ai[0] != 0f
            || target == null || !Finite(target.Center) || !Finite(target.velocity)) return;
        for (int side = -1; side <= 1; side += 2) {
            bool available = false;
            if (Main.projectile != null) for (int slot = 0; slot < Math.Min(Main.maxProjectiles,Main.projectile.Length); slot++)
                if (Main.projectile[slot] != null && !Main.projectile[slot].active) { available = true; break; }
            if (!available) break;
            int index = Projectile.NewProjectile(Projectile.GetSource_OnHit(target),Projectile.Center,
                Projectile.velocity.RotatedBy(MathHelper.ToRadians(18f * side)) * .85f,
                ModContent.ProjectileType<SpiritBolt>(),Math.Max(1,Projectile.damage / 2),1f,Projectile.owner,1f);
            if (index < 0 || index >= Main.maxProjectiles) break;
        }
    }
}
