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
    private Player LivingOwner
    {
        get
        {
            if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers) return null;
            Player owner = Main.player[Projectile.owner];
            return owner.active && !owner.dead && Vector2.DistanceSquared(owner.Center, Projectile.Center) <= 1600f * 1600f ? owner : null;
        }
    }
    public override bool? CanDamage() => LivingOwner == null ? false : null;
    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
        Player owner = LivingOwner;
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
