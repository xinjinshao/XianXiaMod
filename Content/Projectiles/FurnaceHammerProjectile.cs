using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class FurnaceHammerProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Items/Weapons/ThunderPatternSwordCase";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 1; Projectile.timeLeft = 90;
        Projectile.tileCollide = true; Projectile.ignoreWater = true;
    }
    public override void AI()
    {
        Projectile.velocity.Y = Math.Min(12f, Projectile.velocity.Y + 0.25f);
        Projectile.rotation += Projectile.velocity.X < 0 ? -0.18f : 0.18f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.3f, 0.12f, 0.02f);
    }
    public override void OnKill(int timeLeft)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers) return;
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) return;
        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Microsoft.Xna.Framework.Vector2.Zero,
            ModContent.ProjectileType<FurnaceImpactBurst>(), Math.Max(1, Projectile.damage / 2), Projectile.knockBack / 2f, Projectile.owner);
    }
}

public class FurnaceImpactBurst : ModProjectile
{
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
    // Damage lasts two ticks; the remaining frames preserve a visible impact.
    public override bool? CanDamage() => Projectile.timeLeft > 8 ? null : false;
    public override bool? CanHitNPC(NPC target) =>
        Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1) ? null : false;
    public override void AI()
    {
        Projectile.velocity = Microsoft.Xna.Framework.Vector2.Zero;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.3f, 0.12f, 0.02f);
    }
}
