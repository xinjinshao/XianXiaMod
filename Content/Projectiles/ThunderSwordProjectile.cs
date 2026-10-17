using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class ThunderSwordProjectile : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 56;

        Projectile.height = 24;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Melee;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



        Projectile.penetrate = 3;

        Projectile.timeLeft = 110;

    }



    public override void AI()

    {

        if (Projectile.velocity.LengthSquared() > 0.01f)

            Projectile.rotation = Projectile.velocity.ToRotation();

        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);

    }





    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)

    {

        // Friendly projectile NPC hits follow vanilla owner authority, including clients on dedicated servers.
        // Killing hits may already have deactivated the target, so do not require target.active here.
        if (Main.netMode == NetmodeID.Server || Projectile.owner != Main.myPlayer || Main.player == null || Projectile.owner < 0
            || Projectile.owner >= Main.maxPlayers || Projectile.owner >= Main.player.Length
            || Main.player[Projectile.owner] == null || !Main.player[Projectile.owner].active || Main.player[Projectile.owner].dead
            || target == null || !Finite(target.Center) || !Finite(Projectile.Center) || !Finite(Projectile.velocity) || !HasFreeSlot()) return;
        if (Main.rand.NextBool(3))

        {

            Projectile.NewProjectile(

                Projectile.GetSource_OnHit(target),

                target.Center + new Vector2(Main.rand.NextFloat(-32f, 32f), -240f),

                Vector2.UnitY * 12f,

                ModContent.ProjectileType<MinorThunderboltProjectile>(),

                Math.Max(1, Projectile.damage / 2),

                0.5f,

                Projectile.owner);

        }

    }



    private static bool HasFreeSlot()
    {
        if (Main.projectile == null) return false;
        for (int i = 0; i < Math.Min(Main.maxProjectiles,Main.projectile.Length); i++)
            if (Main.projectile[i] != null && !Main.projectile[i].active) return true;
        return false;
    }
    private static bool Finite(Vector2 point) => float.IsFinite(point.X) && float.IsFinite(point.Y) && float.IsFinite(point.LengthSquared());

}
