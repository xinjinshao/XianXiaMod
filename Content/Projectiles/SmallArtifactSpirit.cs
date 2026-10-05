using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Buffs;

namespace XianXia.Content.Projectiles;

public class SmallArtifactSpirit : ModProjectile
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/SmallArtifactPendant";
    public override void SetStaticDefaults()
    {
        Main.projPet[Type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 24; Projectile.minion = true; Projectile.minionSlots = 1;
        Projectile.DamageType = DamageClass.Summon; Projectile.penetrate = -1; Projectile.timeLeft = 180;
        Projectile.tileCollide = false; Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 30;
    }
    public override bool MinionContactDamage() => true;
    public override bool? CanCutTiles() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) { Projectile.Kill(); return; }
        if (owner.HasBuff(ModContent.BuffType<SmallArtifactSpiritBuff>())) Projectile.timeLeft = 2;
        else { Projectile.friendly = false; if (Main.myPlayer == Projectile.owner) Projectile.Kill(); return; }
        Vector2 idle = owner.Center + new Vector2(-40 * owner.direction,-48);
        if (Vector2.DistanceSquared(Projectile.Center,owner.Center) > 1600*1600 && Main.myPlayer == Projectile.owner) {
            Projectile.Center = idle; Projectile.velocity = Vector2.Zero; Projectile.netUpdate = true;
        }
        NPC target = null;
        bool Reachable(NPC npc) => npc.CanBeChasedBy(Projectile) && Vector2.DistanceSquared(npc.Center,owner.Center) <= 700*700
            && Collision.CanHitLine(Projectile.position,Projectile.width,Projectile.height,npc.position,npc.width,npc.height);
        if (owner.HasMinionAttackTargetNPC && owner.MinionAttackTargetNPC >= 0 && owner.MinionAttackTargetNPC < Main.maxNPCs
            && Reachable(Main.npc[owner.MinionAttackTargetNPC])) target = Main.npc[owner.MinionAttackTargetNPC];
        if (target == null) {
            float nearest = 700*700;
            foreach (NPC npc in Main.ActiveNPCs) {
                float distance = Vector2.DistanceSquared(npc.Center,Projectile.Center);
                if (distance < nearest && Reachable(npc)) { nearest = distance; target = npc; }
            }
        }
        Projectile.friendly = target != null;
        Vector2 delta = (target?.Center ?? idle) - Projectile.Center;
        float speed = target != null ? 10 : delta.LengthSquared() > 400*400 ? 14 : 6;
        Vector2 desired = delta.LengthSquared() > 16*16 ? delta.SafeNormalize(Vector2.Zero)*speed : Vector2.Zero;
        Projectile.velocity = (Projectile.velocity*11+desired)/12;
        Projectile.rotation = Projectile.velocity.X*0.04f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center,0.12f,0.2f,0.24f);
    }
}
