using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class ThunderCaseStrikeProjectile : MinorThunderboltProjectile
{
    private Player ownerAtSpawn;
    private bool cancelled;
    public override string Texture => "XianXia/Content/Projectiles/MinorThunderboltProjectile";
    public override void SetDefaults() { base.SetDefaults(); Projectile.DamageType = DamageClass.Melee; Projectile.netImportant = true; }
    public override void OnSpawn(IEntitySource source) { ownerAtSpawn = Owner(); Projectile.DamageType = DamageClass.Melee; }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    private Player Owner() => Main.player != null && Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Projectile.owner < Main.player.Length ? Main.player[Projectile.owner] : null;
    private bool Valid() {
        Player owner = Owner();
        return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Projectile.width == 24 && Projectile.height == 80
            && Projectile.timeLeft > 0 && Projectile.timeLeft <= 180 && Finite(Projectile.Center) && Finite(Projectile.velocity)
            && (Main.netMode == NetmodeID.MultiplayerClient || ReferenceEquals(owner,ownerAtSpawn));
    }
    public override bool? CanDamage() => cancelled || !Valid() ? false : null;
    public override void AI()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient && !cancelled && !Valid()) { cancelled = true; Projectile.netUpdate = true; }
        if (cancelled) { Projectile.friendly = false; Projectile.velocity = Vector2.Zero; Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft)); return; }
        base.AI();
    }
    public override void SendExtraAI(BinaryWriter writer) { writer.Write(true); writer.Write(cancelled); }
    public override void ReceiveExtraAI(BinaryReader reader) { _ = reader.ReadBoolean(); bool stop = reader.ReadBoolean(); Projectile.DamageType = DamageClass.Melee; cancelled |= stop; }
}
