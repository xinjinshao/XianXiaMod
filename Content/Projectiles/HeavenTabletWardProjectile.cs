using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class HeavenTabletWardProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/HeavenTabletSeal";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 40;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 3; Projectile.timeLeft = 120;
        Projectile.tileCollide = true; Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 20;
    }
    public override void AI()
    {
        if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
        {
            EndWard();
            return;
        }
        Player owner = Main.player[Projectile.owner];
        float distance = Vector2.DistanceSquared(owner.Center, Projectile.Center);
        if (!owner.active || owner.dead || distance > 1600f * 1600f)
        {
            EndWard();
            return;
        }
        Projectile.ai[0] = Math.Min(30f, Projectile.ai[0] + 1f);
        if (Projectile.ai[0] >= 30)
        {
            Projectile.tileCollide = false;
            if (distance <= 24f * 24f)
            {
                EndWard();
                return;
            }
            Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
        }
        Projectile.rotation += 0.05f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.24f, 0.18f, 0.04f);
    }
    private void EndWard()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        int identity = Projectile.identity, owner = Projectile.owner;
        Projectile.Kill();
        // The server terminates a player-owned projectile, so explicitly notify its peers.
        if (Main.netMode == NetmodeID.Server && owner >= 0 && owner < Main.maxPlayers)
            NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, identity, owner);
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.ai[0] = 30; Projectile.tileCollide = false;
        if (Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
        return false;
    }
    public override bool? CanHitNPC(NPC target)
    {
        if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers) return false;
        return Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1)
            && Collision.CanHitLine(Main.player[Projectile.owner].Center, 1, 1, target.Center, 1, 1) ? null : false;
    }
}
