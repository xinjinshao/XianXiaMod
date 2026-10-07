using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class ThunderTalismanArray : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 96;
        Projectile.friendly = true; Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1; Projectile.timeLeft = 240;
        Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.netImportant = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 30;
    }
    private bool invalidState;
    private byte attemptedWaves;
    private Player LivingOwner => global::XianXia.Common.Projectiles.FriendlyFieldOwner.Find(Projectile, 240);
    public override bool? CanDamage() => invalidState || LivingOwner == null ? false : null;
    public override void AI()
    {
        invalidState |= !global::XianXia.Common.Projectiles.FriendlyFieldOwner.HasValidState(Projectile, 240);
        Player owner = invalidState ? null : LivingOwner;
        Projectile.velocity = Vector2.Zero;
        if (owner == null)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) Projectile.Kill();
            return;
        }
        Projectile.rotation += 0.035f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.12f, 0.08f, 0.25f);
        if (Main.netMode == NetmodeID.MultiplayerClient || Projectile.timeLeft % 45 != 0) return;
        int wave = Projectile.timeLeft / 45 - 1;
        byte bit = (byte)(1 << wave);
        if ((attemptedWaves & bit) != 0) return;
        // Every lifetime boundary gets one attempt; failed creation does not bank a retry.
        attemptedWaves |= bit;
        Projectile.NewProjectile(Projectile.GetSource_FromAI(),
            Projectile.Center + new Vector2(Main.rand.NextFloat(-48f, 48f), -220f), Vector2.UnitY * 13f,
            ModContent.ProjectileType<MinorThunderboltProjectile>(), Math.Max(1, Projectile.damage / 2), 0.5f, Projectile.owner);
    }
}
