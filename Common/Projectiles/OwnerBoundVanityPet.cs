using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace XianXia.Common.Projectiles;
public abstract class OwnerBoundVanityPet : ModProjectile {
    protected abstract int PetBuff { get; }
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled;
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) => ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(cancelled);
    public override void ReceiveExtraAI(BinaryReader reader) { bool stop = reader.ReadBoolean(); cancelled |= stop; }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private bool ValidSlot => Main.player != null && Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Projectile.owner < Main.player.Length;
    private bool ValidGeometry => Projectile.width == 24 && Projectile.height == 24 && Projectile.timeLeft > 0 && Projectile.timeLeft <= 180
        && Finite(Projectile.Center) && Finite(Projectile.velocity) && float.IsFinite(Projectile.rotation);
    public override void SetStaticDefaults() => Main.projPet[Type] = true;
    public override void SetDefaults() {
        Projectile.width = Projectile.height = 24; Projectile.friendly = true; Projectile.hostile = false; Projectile.penetrate = -1;
        Projectile.timeLeft = 180; Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.netImportant = true;
    }
    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;
    private void Cancel() {
        cancelled = true; Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero;
        Projectile.width = Projectile.height = 24; Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft));
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        if (!ValidSlot || Main.player[Projectile.owner] == null) { Projectile.active = false; return; }
        if (Main.netMode != NetmodeID.MultiplayerClient && Projectile.active) Projectile.Kill();
    }
    public override void AI() {
        if (cancelled || !ValidGeometry || !ValidSlot) { Cancel(); return; }
        Player owner = Main.player[Projectile.owner];
        if (owner == null || !owner.active || owner.dead || !Finite(owner.Center) || !Finite(owner.velocity) || !ownerBinding.Matches(Projectile,owner)) {
            if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return;
        }
        if (owner.HasBuff(PetBuff)) Projectile.timeLeft = 2;
        else { if (Main.myPlayer == Projectile.owner) Cancel(); return; }
        Vector2 idle = owner.Center + new Vector2(-48 * owner.direction,-56), delta = idle - Projectile.Center;
        if (!Finite(idle) || !Finite(delta)) { Cancel(); return; }
        if (delta.LengthSquared() > 1600f * 1600f && Main.myPlayer == Projectile.owner) {
            Projectile.Center = idle; Projectile.velocity = Vector2.Zero; Projectile.netUpdate = true;
        } else {
            Vector2 desired = delta.LengthSquared() > 16 * 16 ? delta.SafeNormalize(Vector2.Zero) * 10 : Vector2.Zero;
            Projectile.velocity = (Projectile.velocity * 11 + desired) / 12;
        }
        Projectile.spriteDirection = owner.direction;
    }
}
