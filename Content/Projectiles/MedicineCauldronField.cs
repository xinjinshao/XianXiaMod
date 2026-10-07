using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class MedicineCauldronField : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/GreenwoodArrayField";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 96;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1; Projectile.timeLeft = 300;
        Projectile.tileCollide = false; Projectile.ignoreWater = true;
        Projectile.netImportant = true;
    }
    private bool invalidState;
    public override bool? CanDamage() => false;
    public override void AI()
    {
        Player owner = global::XianXia.Common.Projectiles.FriendlyFieldOwner.Find(Projectile, 300);
        bool validClock = float.IsFinite(Projectile.ai[0]) && Projectile.ai[0] >= 0f
            && Projectile.ai[0] < 60f && Projectile.ai[0] == System.MathF.Truncate(Projectile.ai[0]);
        invalidState |= !global::XianXia.Common.Projectiles.FriendlyFieldOwner.HasValidState(Projectile, 300) || !validClock;
        Projectile.velocity = Vector2.Zero;
        if (owner == null || invalidState)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) Projectile.Kill();
            return;
        }
        Projectile.rotation += 0.02f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.05f, 0.24f, 0.12f);
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        // A missed wave is discarded; acquiring a target cannot release stored attacks.
        if (++Projectile.ai[0] < 60) return;
        Projectile.ai[0] = 0;
        NPC target = null;
        float nearest = 600f * 600f;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC candidate = Main.npc[i];
            if (candidate == null || !float.IsFinite(candidate.Center.X) || !float.IsFinite(candidate.Center.Y)) continue;
            float distance = Vector2.DistanceSquared(Projectile.Center, candidate.Center);
            if (distance >= nearest || !candidate.CanBeChasedBy(this)
                || !Collision.CanHitLine(Projectile.Center, 1, 1, candidate.Center, 1, 1)) continue;
            target = candidate; nearest = distance;
        }
        if (target == null) return;
        Vector2 velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
        for (int i = 0; i < 2; i++)
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity.RotatedBy(i == 0 ? -0.08 : 0.08),
                ModContent.ProjectileType<MedicineSpiritBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
    }
}

public class MedicineSpiritBolt : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBoltProjectile";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1; Projectile.timeLeft = 90;
        Projectile.tileCollide = true; Projectile.ignoreWater = true;
    }
    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.05f, 0.24f, 0.12f);
    }
}
