using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Projectiles;

namespace XianXia.Content.Projectiles;

public class ThunderCaseStrikeProjectile : MinorThunderboltProjectile
{
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled;
    public override string Texture => "XianXia/Content/Projectiles/MinorThunderboltProjectile";
    public override void SetDefaults() { base.SetDefaults(); Projectile.DamageType = DamageClass.Melee; Projectile.netImportant = true; }
    public override void OnSpawn(IEntitySource source) {
        ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
        Projectile.DamageType = DamageClass.Melee;
    }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private Player Owner() => Main.player != null && Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Projectile.owner < Main.player.Length ? Main.player[Projectile.owner] : null;
    private bool ValidGeometry() => Projectile.width == 24 && Projectile.height == 80 && Projectile.timeLeft > 0 && Projectile.timeLeft <= 180
        && Finite(Projectile.Center) && Finite(Projectile.velocity) && float.IsFinite(Projectile.rotation);
    private bool Valid() {
        Player owner = Owner();
        return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Finite(owner.velocity) && ValidGeometry()
            && ownerBinding.Matches(Projectile,owner);
    }
    public override bool? CanDamage() => cancelled || !Valid() ? false : null;
    public override void AI()
    {
        if (!ValidGeometry() || Main.netMode != NetmodeID.MultiplayerClient && !Valid()) Cancel();
        if (cancelled) { Cancel(); return; }
        if (!Valid()) return;
        base.AI();
    }
    private void Cancel()
    {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = true;
        Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero;
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        Projectile.width = 24; Projectile.height = 80;
        Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft));
    }
    public override void SendExtraAI(BinaryWriter writer) { writer.Write(true); writer.Write(cancelled); }
    public override void ReceiveExtraAI(BinaryReader reader) { _ = reader.ReadBoolean(); bool stop = reader.ReadBoolean(); Projectile.DamageType = DamageClass.Melee; cancelled |= stop; }
}
