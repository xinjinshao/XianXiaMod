using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class GreenwoodArrayField : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 96;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1; Projectile.timeLeft = 300;
        Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.netImportant = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 30;
    }
    private bool invalidState;
    private Player LivingOwner => global::XianXia.Common.Projectiles.FriendlyFieldOwner.Find(Projectile, 300);
    internal bool CanPulse(Player player) => Projectile.active && !invalidState
        && ReferenceEquals(LivingOwner, player) && player != null
        && Vector2.DistanceSquared(player.Center, Projectile.Center) <= 160f * 160f;
    public override bool? CanDamage() => invalidState || LivingOwner == null ? false : null;
    public override void AI()
    {
        invalidState |= !global::XianXia.Common.Projectiles.FriendlyFieldOwner.HasValidState(Projectile, 300);
        Player owner = invalidState ? null : LivingOwner;
        Projectile.velocity = Vector2.Zero;
        if (owner == null)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) Projectile.Kill();
            return;
        }
        Projectile.rotation += 0.02f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.05f, 0.24f, 0.12f);
        if (Main.netMode == NetmodeID.MultiplayerClient || !owner.Hitbox.Intersects(Projectile.Hitbox)
            || Main.GameUpdateCount % 60 != 0
            || !owner.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryArrayRecovery(Main.GameUpdateCount)) return;
        global::XianXia.Common.Systems.AuthoritativeHealing.Apply(owner, 1);
        owner.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().RestoreSpiritualEnergy(1);
    }
}
