using System.IO;
using Terraria.DataStructures;
using XianXia.Common.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class BossSpiritBoltProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBolt";

    public const int Lifetime = 240;
    public const int CancellationFade = 6;
    private readonly HostileSourceBinding sourceBinding = new();
    private bool invalidAge, invalidGeometry;
    public override void OnSpawn(IEntitySource source) => sourceBinding.Capture(source);
    public override void SendExtraAI(BinaryWriter writer) => sourceBinding.Write(writer, Projectile, Lifetime);
    public override void ReceiveExtraAI(BinaryReader reader) { if (!sourceBinding.Read(reader, Projectile, Lifetime)) invalidAge = true; }
    public override bool? CanDamage() => !invalidAge && !invalidGeometry && Projectile.timeLeft > 0 && Projectile.timeLeft <= Lifetime
        && float.IsFinite(Projectile.Center.X) && float.IsFinite(Projectile.Center.Y)
        && float.IsFinite(Projectile.velocity.X) && float.IsFinite(Projectile.velocity.Y) && sourceBinding.IsValid();
    public override bool CanHitPlayer(Player target) => CanDamage() == true && target.active && !target.dead
        && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1);

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = Lifetime;
        Projectile.netImportant = true;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        if (!float.IsFinite(Projectile.Center.X) || !float.IsFinite(Projectile.Center.Y)
            || !float.IsFinite(Projectile.velocity.X) || !float.IsFinite(Projectile.velocity.Y)) {
            invalidGeometry = true;
            sourceBinding.CancelOnAuthority(Projectile, CancellationFade);
        }
        sourceBinding.CancelIfInvalid(Projectile, CancellationFade, Lifetime);
        if (invalidAge || invalidGeometry || !sourceBinding.IsValid()) {
            Projectile.velocity = Vector2.Zero;
            if (sourceBinding.IsCancelled)
                Projectile.alpha = (int)MathHelper.Lerp(255f, 180f, System.Math.Clamp(Projectile.timeLeft / (float)CancellationFade, 0f, 1f));
            return;
        }
        if (Projectile.velocity.LengthSquared() > 0.01f)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        if (Main.dedServ) return;
        Lighting.AddLight(Projectile.Center, 0.1f, 0.16f, 0.22f);
        if (Main.rand.NextBool(5))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemSapphire);
            dust.noGravity = true;
            dust.velocity *= 0.2f;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (!CanHitPlayer(target)) return;
        target.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 60);
    }
}
