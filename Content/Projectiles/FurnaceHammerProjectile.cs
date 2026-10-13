using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

internal static class FurnaceProjectileOwner
{
    internal static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.LengthSquared());
    internal static bool ValidState(Projectile projectile, int size, int lifetime, bool expired = false) => projectile.width == size && projectile.height == size
        && projectile.timeLeft >= (expired ? 0 : 1) && projectile.timeLeft <= lifetime
        && Finite(projectile.Center) && Finite(projectile.velocity) && float.IsFinite(projectile.rotation);
    internal static Player Find(Projectile projectile, int size, int lifetime, bool expired = false)
    {
        if (!ValidState(projectile, size, lifetime, expired) || projectile.owner < 0 || projectile.owner >= Main.maxPlayers || projectile.owner >= Main.player.Length) return null;
        Player owner = Main.player[projectile.owner];
        return owner != null && owner.active && !owner.dead && Finite(owner.Center) && Finite(owner.velocity) ? owner : null;
    }
    internal static bool ValidTarget(NPC target) => target != null && target.active && target.life > 0 && Finite(target.Center) && Finite(target.velocity);
    internal static void Stop(Projectile projectile, int size)
    {
        projectile.friendly = false; projectile.hide = true; projectile.velocity = Vector2.Zero;
        if (!Finite(projectile.Center)) projectile.Center = Vector2.Zero;
        if (!float.IsFinite(projectile.rotation)) projectile.rotation = 0;
        projectile.width = projectile.height = size;
        projectile.timeLeft = Math.Min(6, Math.Max(0, projectile.timeLeft));
    }
}

public class FurnaceHammerProjectile : ModProjectile
{
    private bool impactReleased, cancelled;
    public override string Texture => "XianXia/Content/Items/Weapons/ThunderPatternSwordCase";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 1; Projectile.timeLeft = 90;
        Projectile.tileCollide = true; Projectile.ignoreWater = true;
    }
    public override bool? CanDamage() => cancelled || impactReleased || FurnaceProjectileOwner.Find(Projectile, 32, 90) == null ? false : null;
    public override bool? CanHitNPC(NPC target) => CanDamage() == false || !FurnaceProjectileOwner.ValidTarget(target) ? false : null;
    public override void SendExtraAI(BinaryWriter writer) => writer.Write((byte)((cancelled ? 1 : 0) | (impactReleased ? 2 : 0)));
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        byte flags = reader.ReadByte();
        cancelled |= flags > 3 || (flags & 1) != 0;
        impactReleased |= cancelled || (flags & 2) != 0;
    }
    private void Cancel()
    {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = impactReleased = true;
        FurnaceProjectileOwner.Stop(Projectile, 32);
    }
    public override void AI()
    {
        if (cancelled || impactReleased || !FurnaceProjectileOwner.ValidState(Projectile, 32, 90)) { Cancel(); return; }
        if (FurnaceProjectileOwner.Find(Projectile, 32, 90) == null) {
            if (Main.netMode != NetmodeID.MultiplayerClient) Cancel();
            return;
        }
        Projectile.velocity.Y = Math.Min(12f, Projectile.velocity.Y + 0.25f);
        Projectile.rotation += Projectile.velocity.X < 0 ? -0.18f : 0.18f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.3f, 0.12f, 0.02f);
    }
    public override void OnKill(int timeLeft)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || impactReleased || cancelled) return;
        impactReleased = true;
        // Natural expiry has timeLeft=0 and must still release its one ordinary impact.
        if (FurnaceProjectileOwner.Find(Projectile, 32, 90, expired: true) == null) return;
        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
            ModContent.ProjectileType<FurnaceImpactBurst>(), Math.Max(1, Projectile.damage / 2), Projectile.knockBack / 2f, Projectile.owner);
    }
}

public class FurnaceImpactBurst : ModProjectile
{
    private bool cancelled;
    public override string Texture => "XianXia/Content/Projectiles/GreenwoodArrayField";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 96;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1; Projectile.timeLeft = 10;
        Projectile.tileCollide = false; Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = -1;
    }
    public override bool? CanDamage() => cancelled || FurnaceProjectileOwner.Find(Projectile, 96, 10) == null || Projectile.timeLeft <= 8 ? false : null;
    public override bool? CanHitNPC(NPC target) => cancelled || FurnaceProjectileOwner.Find(Projectile, 96, 10) == null || !FurnaceProjectileOwner.ValidTarget(target)
        || !Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1) ? false : null;
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(cancelled);
    public override void ReceiveExtraAI(BinaryReader reader) => cancelled |= reader.ReadBoolean();
    private void Cancel()
    {
        if (!cancelled && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        cancelled = true;
        FurnaceProjectileOwner.Stop(Projectile, 96);
    }
    public override void AI()
    {
        if (cancelled || !FurnaceProjectileOwner.ValidState(Projectile, 96, 10)) { Cancel(); return; }
        if (FurnaceProjectileOwner.Find(Projectile, 96, 10) == null) {
            if (Main.netMode != NetmodeID.MultiplayerClient) Cancel();
            return;
        }
        Projectile.velocity = Vector2.Zero;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.3f, 0.12f, 0.02f);
    }
}
