using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class FormlessSwordWheelProjectile : ModProjectile
{
    private bool cancelled;
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
    private bool ValidGeometry => Projectile.width == 80 && Projectile.height == 80 && Projectile.timeLeft > 0 && Projectile.timeLeft <= 150
        && Finite(Projectile.Center) && Finite(Projectile.velocity) && float.IsFinite(Projectile.rotation);
    private Player LivingOwner
    {
        get {
            if (!ValidGeometry || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers
                || Projectile.owner >= Main.player.Length) return null;
            Player owner = Main.player[Projectile.owner];
            return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Finite(owner.velocity) ? owner : null;
        }
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 80;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 5; Projectile.timeLeft = 150;
        Projectile.tileCollide = false; Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 12;
    }
    public override bool? CanDamage() => cancelled || LivingOwner == null ? false : null;
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(cancelled);
    public override void ReceiveExtraAI(BinaryReader reader) => cancelled |= reader.ReadBoolean();
    private void Cancel()
    {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = true;
        Projectile.friendly = false; Projectile.velocity = Vector2.Zero;
        Projectile.hide = true;
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        Projectile.width = Projectile.height = 80;
        Projectile.timeLeft = Math.Min(6, Math.Max(0, Projectile.timeLeft));
    }
    public override void AI()
    {
        if (cancelled || !ValidGeometry) { Cancel(); return; }
        Player owner = LivingOwner;
        if (owner == null) {
            if (Main.netMode != NetmodeID.MultiplayerClient) Cancel();
            return;
        }
        Projectile.rotation += 0.28f;
        Vector2 drift = owner.velocity.SafeNormalize(Vector2.Zero) * 3f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.velocity + drift, 0.04f);
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.08f, 0.2f, 0.24f);
    }
}
