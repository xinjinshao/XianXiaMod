using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Projectiles;
namespace XianXia.Content.Projectiles;

public class MinorThunderboltProjectile : ModProjectile
{
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled;
    protected virtual bool FixedMelee => false;
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
    {
        ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
        if (FixedMelee) Projectile.DamageType = DamageClass.Melee;
        else if (source is Terraria.DataStructures.EntitySource_Parent parent && parent.Entity is Projectile origin)
            Projectile.DamageType = origin.DamageType == DamageClass.Melee ? DamageClass.Melee : DamageClass.Magic;
    }
    public override void SendExtraAI(BinaryWriter writer) { writer.Write(FixedMelee || Projectile.DamageType == DamageClass.Melee); writer.Write(cancelled); }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        bool melee = reader.ReadBoolean(), stop = reader.ReadBoolean();
        Projectile.DamageType = FixedMelee || melee ? DamageClass.Melee : DamageClass.Magic; cancelled |= stop;
    }
    public override void SetDefaults()
    {
        Projectile.width = 24; Projectile.height = 80;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = FixedMelee ? DamageClass.Melee : DamageClass.Magic;
        Projectile.penetrate = 1; Projectile.timeLeft = 180;
        Projectile.tileCollide = true; Projectile.ignoreWater = true;
    }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private bool ValidGeometry => Projectile.width == 24 && Projectile.height == 80 && Projectile.timeLeft > 0 && Projectile.timeLeft <= 180
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
    private void Cancel()
    {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = true; Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero;
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        Projectile.width = 24; Projectile.height = 80; Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft));
    }
    public override void AI()
    {
        if (cancelled || !ValidGeometry) { Cancel(); return; }
        if (LivingOwner == null) { if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return; }
        if (Projectile.velocity.LengthSquared() > .01f) Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center,.06f,.18f,.2f);
    }
}
