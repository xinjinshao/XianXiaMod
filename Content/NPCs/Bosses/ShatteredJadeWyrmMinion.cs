using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Bosses;

public partial class ShatteredJadeWyrmMinion : global::XianXia.Common.NPCs.LinkedWormNPC
{
    internal override bool IsWormHead => true;
    internal const float SegmentSpacing = 18f;
    private const int BodySegments = 4;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ShatteredJadeWyrmMinion"));
    }

    public override void SetDefaults()
    {
        NPC.width = 26;
        NPC.height = 26;
        NPC.damage = 14;
        NPC.defense = 2;
        NPC.lifeMax = 70;
        NPC.knockBackResist = 0.2f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = -1;
    }

    public override bool CheckActive() => false;

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) =>
        HasSummonTarget() && target.active && !target.dead;

    public override void AI()
    {
        if (!HasSummonSource()) return;
        if (!global::XianXia.Common.Systems.BossTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            NPC.damage = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient) SegmentedWormAI.Deactivate(NPC);
            return;
        }
        EnsureSegments();

        NPC.localAI[0]++;
        Vector2 toTarget = target.Center - NPC.Center;
        Vector2 wave = toTarget.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * (float)System.Math.Sin(NPC.localAI[0] * 0.09f) * 32f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, (toTarget + wave).SafeNormalize(Vector2.UnitY) * 4.5f, 0.06f);
        NPC.rotation = NPC.velocity.ToRotation();
        if (!Main.dedServ) Lighting.AddLight(NPC.Center, 0.03f, 0.18f, 0.14f);
    }

    private void EnsureSegments()
    {
        if (NPC.localAI[3] == 1f || Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        if (SegmentedWormAI.TrySpawnChain(NPC, ModContent.NPCType<ShatteredJadeWyrmMinionBody>(),
            ModContent.NPCType<ShatteredJadeWyrmMinionTail>(), BodySegments))
            NPC.localAI[3] = 1f;
    }
}

public class ShatteredJadeWyrmMinionBody : global::XianXia.Common.NPCs.LinkedWormNPC
{
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 1;

    public override void SetDefaults()
    {
        NPC.width = 24;
        NPC.height = 24;
        NPC.damage = 12;
        NPC.defense = 2;
        NPC.lifeMax = 70;
        NPC.knockBackResist = 0.2f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = -1;
    }

    public override bool CheckActive() => false;

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => ShatteredJadeWyrmMinion.HasLinkedSummonTarget(NPC) && target.active && !target.dead;
    public override bool CanHitNPC(NPC target) => ShatteredJadeWyrmMinion.HasLinkedSummonTarget(NPC);
    public override void AI() => ShatteredJadeWyrmMinion.FollowSummonSegment(NPC, 0.13f, 0.1f);
}

public class ShatteredJadeWyrmMinionTail : global::XianXia.Common.NPCs.LinkedWormNPC
{
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 1;

    public override void SetDefaults()
    {
        NPC.width = 22;
        NPC.height = 18;
        NPC.damage = 10;
        NPC.defense = 1;
        NPC.lifeMax = 70;
        NPC.knockBackResist = 0.2f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = -1;
    }

    public override bool CheckActive() => false;

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => ShatteredJadeWyrmMinion.HasLinkedSummonTarget(NPC) && target.active && !target.dead;
    public override bool CanHitNPC(NPC target) => ShatteredJadeWyrmMinion.HasLinkedSummonTarget(NPC);
    public override void AI() => ShatteredJadeWyrmMinion.FollowSummonSegment(NPC, 0.1f, 0.08f);
}
