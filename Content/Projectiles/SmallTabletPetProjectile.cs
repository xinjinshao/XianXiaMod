using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace XianXia.Content.Projectiles;
public class SmallTabletPetProjectile : ModProjectile
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/SmallTabletPet";
    public override void SetStaticDefaults() => Main.projPet[Type] = true;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
    }
    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) { Projectile.Kill(); return; }
        if (owner.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SmallTabletPetBuff>())) Projectile.timeLeft = 2;
        else { if (Main.myPlayer == Projectile.owner) Projectile.Kill(); return; }
        Vector2 idle = owner.Center + new Vector2(-48 * owner.direction, -56);
        Vector2 delta = idle - Projectile.Center;
        if (delta.LengthSquared() > 1600 * 1600 && Main.myPlayer == Projectile.owner)
        {
            Projectile.Center = idle; Projectile.velocity = Vector2.Zero; Projectile.netUpdate = true;
        }
        else
        {
            Vector2 desired = delta.LengthSquared() > 16 * 16 ? delta.SafeNormalize(Vector2.Zero) * 10 : Vector2.Zero;
            Projectile.velocity = (Projectile.velocity * 11 + desired) / 12;
        }
        Projectile.spriteDirection = owner.direction;
    }
}
