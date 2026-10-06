using System.IO;
using Terraria.DataStructures;
using XianXia.Common.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class TribulationWarningLineProjectile : ModProjectile
{
    public const int Lifetime = 36;
    public const int CancellationFade = 6;
    private readonly HostileSourceBinding sourceBinding = new();
    private bool strikeReleased;
    public override void OnSpawn(IEntitySource source) => sourceBinding.Capture(source);
    public override bool? CanDamage() => false;
    public override bool CanHitPlayer(Player target) => false;
    public override void SendExtraAI(BinaryWriter writer) => sourceBinding.Write(writer, Projectile, Lifetime);
    public override void ReceiveExtraAI(BinaryReader reader) { if (!sourceBinding.Read(reader, Projectile, Lifetime)) strikeReleased = true; }

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 16;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.netImportant = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = false;
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
        sourceBinding.CancelIfInvalid(Projectile, CancellationFade, Lifetime);
        Projectile.alpha = sourceBinding.IsCancelled
            ? (int)MathHelper.Lerp(255f, 180f, System.Math.Clamp(Projectile.timeLeft / (float)CancellationFade, 0f, 1f))
            : (int)MathHelper.Lerp(40f, 180f, Projectile.timeLeft / (float)Lifetime);
        if (Main.dedServ) return;
        Lighting.AddLight(Projectile.Center, 0.12f, 0.22f, 0.35f);

        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
            dust.noGravity = true;
            dust.velocity *= 0.15f;
        }
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || timeLeft != 0 || strikeReleased || !sourceBinding.IsValid())
        {
            return;
        }

        strikeReleased = true;
        Vector2 position = Projectile.Center + new Vector2(0f, -540f);
        Vector2 velocity = new(0f, 11f);
        Projectile.NewProjectile(
            sourceBinding.StrikeSource(Projectile),
            position,
            velocity,
            ModContent.ProjectileType<TribulationLightningProjectile>(),
            Projectile.damage,
            1.5f,
            Projectile.owner);
    }
}
