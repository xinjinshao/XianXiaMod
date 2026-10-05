using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Bosses;

public class ShatteredJadeWyrmMinion : ModNPC
{
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

    public override void AI()
    {
        int parent = (int)NPC.ai[0];
        if (parent < 0 || parent >= Main.maxNPCs || !Main.npc[parent].active
            || Main.npc[parent].type != ModContent.NPCType<SpiritVeinWyrm>())
        {
            NPC.damage = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient) SegmentedWormAI.Deactivate(NPC);
            return;
        }
        EnsureSegments();

        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        if (!target.active || target.dead)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) SegmentedWormAI.Deactivate(NPC);
            return;
        }

        NPC.localAI[0]++;
        NPC.ai[3]++;
        if (NPC.ai[3] >= 900f)
        {
            NPC.damage = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient) SegmentedWormAI.Deactivate(NPC);
            return;
        }

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

public class ShatteredJadeWyrmMinionBody : ModNPC
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

    public override void AI() => SegmentedWormAI.FollowPreviousSegment(NPC, ShatteredJadeWyrmMinion.SegmentSpacing, 0.02f, 0.13f, 0.1f, ModContent.NPCType<ShatteredJadeWyrmMinion>());
}

public class ShatteredJadeWyrmMinionTail : ModNPC
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

    public override void AI() => SegmentedWormAI.FollowPreviousSegment(NPC, ShatteredJadeWyrmMinion.SegmentSpacing, 0.02f, 0.1f, 0.08f, ModContent.NPCType<ShatteredJadeWyrmMinion>());
}
