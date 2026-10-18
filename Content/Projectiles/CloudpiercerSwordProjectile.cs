using System;
using System.IO;
using XianXia.Common.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class CloudpiercerSwordProjectile : ModProjectile
{
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled, released;
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) => ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
    public override void SendExtraAI(BinaryWriter writer) => writer.Write((byte)((cancelled ? 1 : 0) | (released ? 2 : 0)));
    public override void ReceiveExtraAI(BinaryReader reader) { byte flags = reader.ReadByte(); cancelled |= flags > 3 || (flags & 1) != 0; released |= cancelled || (flags & 2) != 0; }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private bool ValidGeometry => ValidState(false);
    private bool ValidState(bool expired) => Projectile.width == 56 && Projectile.height == 24 && Projectile.timeLeft >= (expired ? 0 : 1) && Projectile.timeLeft <= 105
        && Finite(Projectile.Center) && Finite(Projectile.velocity) && float.IsFinite(Projectile.rotation);
    private Player LivingOwner => FindOwner(false);
    private Player FindOwner(bool expired) {
        if (!ValidState(expired) || Main.player == null || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers || Projectile.owner >= Main.player.Length) return null;
        Player owner = Main.player[Projectile.owner];
        return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Finite(owner.velocity) && ownerBinding.Matches(Projectile,owner) ? owner : null;
    }
    public override bool? CanDamage() => cancelled || released || LivingOwner == null ? false : null;
    public override bool? CanHitNPC(NPC target) => CanDamage() == false || target == null || !target.active || target.life <= 0 || !Finite(target.Center) || !Finite(target.velocity)
        || !Collision.CanHitLine(Projectile.Center,1,1,target.Center,1,1) ? false : null;
    private void Cancel() {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = released = true; Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero;
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        Projectile.width = 56; Projectile.height = 24; Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft));
    }
    public override void SetDefaults()
    {
        Projectile.width = 56;
        Projectile.height = 24;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 105;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        if (cancelled || released || !ValidGeometry) { Cancel(); return; }
        Player owner = LivingOwner;
        if (owner == null) { if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return; }
        if (Projectile.velocity.LengthSquared() > .01f) Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center,.06f,.18f,.2f);
    }
    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (CanDamage() == false) { Cancel(); return false; }
        Projectile.tileCollide = false; Projectile.timeLeft = Math.Min(Projectile.timeLeft,24); return false;
    }
    public override void OnKill(int timeLeft) {
        if (Main.netMode == NetmodeID.MultiplayerClient || released || cancelled) return;
        released = true;
        if (FindOwner(true) == null || Main.projectile == null) return;
        bool available = false;
        for (int i = 0; i < Math.Min(Main.maxProjectiles,Main.projectile.Length); i++)
            if (Main.projectile[i] != null && !Main.projectile[i].active) { available = true; break; }
        if (!available) return;
        Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(.55f) * 6f;
        Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,velocity,ModContent.ProjectileType<CloudWispProjectile>(),Math.Max(1,Projectile.damage / 3),1f,Projectile.owner);
    }
}
