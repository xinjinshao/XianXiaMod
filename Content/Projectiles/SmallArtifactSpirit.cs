using System;
using System.IO;
using XianXia.Common.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Buffs;

namespace XianXia.Content.Projectiles;

public class SmallArtifactSpirit : ModProjectile
{
    protected static bool Counter(float value,int maximum) => float.IsFinite(value) && value >= 0 && value <= maximum && value == MathF.Truncate(value);
    protected virtual bool ValidAttackState => float.IsFinite(Projectile.ai[0]) && float.IsFinite(Projectile.ai[1]);
    protected bool TryFireSpiritBolt(Vector2 velocity,int damage) {
        if (!CanAct || !Finite(velocity) || Main.projectile == null) return false;
        bool available = false;
        for (int slot = 0; slot < Math.Min(Main.maxProjectiles,Main.projectile.Length); slot++)
            if (Main.projectile[slot] != null && !Main.projectile[slot].active) { available = true; break; }
        if (!available) return false;
        int index = Projectile.NewProjectile(Projectile.GetSource_FromAI(),Projectile.Center,velocity,ModContent.ProjectileType<ContractSpiritBolt>(),Math.Max(1,damage),1f,Projectile.owner);
        return index >= 0 && index < Main.maxProjectiles;
    }
    private ArtifactProjectileOwnerBinding ownerBinding;
    private bool cancelled;
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) => ownerBinding = ArtifactProjectileOwnerBinding.Capture(Projectile);
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(cancelled);
    public override void ReceiveExtraAI(BinaryReader reader) { bool stop = reader.ReadBoolean(); cancelled |= stop; }
    internal bool CanAct => !cancelled && TryOwner(out var owner) && owner.HasBuff(SpiritBuff);
    public override bool? CanDamage() => CanAct && MinionContactDamage() ? null : false;
    protected virtual int SpiritBuff => ModContent.BuffType<SmallArtifactSpiritBuff>();
    protected virtual float AttackRange => 700f;
    protected virtual float CombatSpeed => 10f;
    protected virtual Vector2 AimPosition(Player owner, NPC target) => target.Center;
    protected virtual void AttackTarget(Player owner, NPC target) { }
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
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y)
        && float.IsFinite(value.LengthSquared());
    private bool TryOwner(out Player owner)
    {
        owner = null;
        if (Main.player == null || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers || Projectile.owner >= Main.player.Length) return false;
        owner = Main.player[Projectile.owner];
        return !cancelled && ValidAttackState && Projectile.width == 24 && Projectile.height == 24 && Projectile.timeLeft > 0 && Projectile.timeLeft <= 180
            && float.IsFinite(Projectile.rotation) && owner != null && owner.active && !owner.dead && Finite(owner.Center)
            && Finite(owner.velocity) && Finite(Projectile.Center) && Finite(Projectile.velocity) && ownerBinding.Matches(Projectile,owner);
    }
    private bool Reachable(Player owner, NPC npc) => npc != null && Finite(npc.Center) && Finite(npc.position)
        && npc.width > 0 && npc.height > 0 && npc.CanBeChasedBy(Projectile)
        && Vector2.DistanceSquared(npc.Center, owner.Center) <= AttackRange * AttackRange
        && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
    public override bool? CanHitNPC(NPC target) => TryOwner(out var owner) && owner.HasBuff(SpiritBuff)
        && MinionContactDamage() && Reachable(owner, target) ? null : false;
    private void Cancel() {
        cancelled = true; Projectile.friendly = false; Projectile.hide = true; Projectile.velocity = Vector2.Zero; Projectile.ai[0] = Projectile.ai[1] = 0;
        Projectile.width = Projectile.height = 24; Projectile.timeLeft = Math.Min(6,Math.Max(0,Projectile.timeLeft));
        if (!Finite(Projectile.Center)) Projectile.Center = Vector2.Zero;
        if (!float.IsFinite(Projectile.rotation)) Projectile.rotation = 0;
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (!Projectile.active) return;
        if (Main.player != null && Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Projectile.owner < Main.player.Length && Main.player[Projectile.owner] != null) Projectile.Kill();
        else Projectile.active = false;
    }
    public override void AI()
    {
        if (cancelled || !ValidAttackState || Projectile.width != 24 || Projectile.height != 24 || Projectile.timeLeft <= 0 || Projectile.timeLeft > 180
            || !Finite(Projectile.Center) || !Finite(Projectile.velocity) || !float.IsFinite(Projectile.rotation)) { Cancel(); return; }
        if (!TryOwner(out Player owner)) { Projectile.friendly = false; if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(); return; }
        if (owner.HasBuff(SpiritBuff)) Projectile.timeLeft = 2;
        else { Projectile.friendly = false; if (Main.myPlayer == Projectile.owner) Cancel(); return; }
        Vector2 idle = owner.Center + new Vector2(-40 * owner.direction,-48);
        if (Vector2.DistanceSquared(Projectile.Center,owner.Center) > 1600*1600 && Main.myPlayer == Projectile.owner) {
            Projectile.Center = idle; Projectile.velocity = Vector2.Zero; Projectile.netUpdate = true;
        }
        NPC target = null;
        if (owner.HasMinionAttackTargetNPC && owner.MinionAttackTargetNPC >= 0 && owner.MinionAttackTargetNPC < Main.maxNPCs
            && Main.npc != null && owner.MinionAttackTargetNPC < Main.npc.Length
            && Reachable(owner, Main.npc[owner.MinionAttackTargetNPC])) target = Main.npc[owner.MinionAttackTargetNPC];
        if (target == null && Main.npc != null) {
            float nearest = AttackRange*AttackRange;
            foreach (NPC npc in Main.npc) {
                if (npc == null) continue;
                float distance = Vector2.DistanceSquared(npc.Center,Projectile.Center);
                if (distance < nearest && Reachable(owner, npc)) { nearest = distance; target = npc; }
            }
        }
        Projectile.friendly = target != null && MinionContactDamage();
        Vector2 delta = (target != null ? AimPosition(owner, target) : idle) - Projectile.Center;
        if (!Finite(delta)) { Cancel(); return; }
        AttackTarget(owner, target);
        float speed = target != null ? CombatSpeed : delta.LengthSquared() > 400*400 ? 14 : 6;
        Vector2 desired = delta.LengthSquared() > 16*16 ? delta.SafeNormalize(Vector2.Zero)*speed : Vector2.Zero;
        Projectile.velocity = (Projectile.velocity*11+desired)/12;
        Projectile.rotation = Projectile.velocity.X*0.04f;
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center,0.12f,0.2f,0.24f);
    }
}
