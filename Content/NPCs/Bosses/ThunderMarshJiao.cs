using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

[AutoloadBossHead]
public class ThunderMarshJiao : ModNPC
{
    internal const float SegmentSpacing = 48f;
    private const int BodySegments = 13;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ThunderMarshJiao.Text"));
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = BossStatRules.ScaleLife(NPC.lifeMax, balance, bossAdjustment);
    }

    public override void SetDefaults()
    {
        NPC.width = 78;
        NPC.height = 66;
        var stats = BossStatRules.Get(nameof(ThunderMarshJiao));
        NPC.lifeMax = stats.Life;
        NPC.damage = stats.Damage;
        NPC.defense = 26;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) =>
        global::XianXia.Common.Systems.BossTargeting.HasLivingTarget(NPC) && target.active && !target.dead;

    public override void AI()
    {
        if (!BossTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            NPC.velocity = new Vector2(0f, -2f);
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                bool changed = NPC.ai[0] != 0f || NPC.ai[1] != 0f || NPC.ai[2] != 0f || NPC.localAI[1] != 0f || NPC.timeLeft > 30;
                NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = 0f;
                NPC.localAI[1] = 0f;
                if (changed) NPC.netUpdate = true;
                NPC.EncourageDespawn(30);
            }
            return;
        }

        EnsureSegments();

        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * 0.7f);
        bool brokenHorn = NPC.life < (int)(NPC.lifeMax * 0.35f);
        AnnouncePhases(phaseTwo, brokenHorn);

        NPC.ai[0]++;
        if (NPC.ai[1] > 0f)
        {
            NPC.ai[1]--;
        }
        else if (NPC.ai[0] >= (brokenHorn ? 150f : phaseTwo ? 190f : 240f))
        {
            NPC.ai[0] = 0f;
            NPC.ai[1] = brokenHorn ? 64f : 52f;
            NPC.netUpdate = true;
        }

        bool diving = NPC.ai[1] > 0f;
        float speed = brokenHorn ? 10.4f : phaseTwo ? 8f : 6.2f;
        Vector2 desiredVelocity;
        if (diving)
        {
            Vector2 diveAim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            desiredVelocity = diveAim * (brokenHorn ? 15.5f : phaseTwo ? 13f : 11f);
        }
        else
        {
            float t = NPC.ai[0] * 0.035f;
            Vector2 hoverPoint = target.Center + new Vector2((float)Math.Sin(t) * 340f, -300f + (float)Math.Sin(t * 1.7f) * 90f);
            desiredVelocity = (hoverPoint - NPC.Center).SafeNormalize(Vector2.UnitY) * speed;
        }

        NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, diving ? 0.12f : 0.055f);
        NPC.rotation = NPC.velocity.ToRotation();

        FireLightningPatterns(target, phaseTwo, brokenHorn);
        if (!Main.dedServ) Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("thunder_marsh_jiao");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<global::XianXia.Content.Items.TreasureBags.ThunderMarshJiaoBag>()));
        npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<global::XianXia.Content.Items.MasterRewards.ThunderMarshJiaoMonument>()));
        var normal = new LeadingConditionRule(new Conditions.NotExpert());
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 1, 16, 28));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 1, 8, 16));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ThunderMarshJiaoWing>(), 12, 1, 1));
        npcLoot.Add(normal);
    }

    private void EnsureSegments()
    {
        if (NPC.localAI[3] == 1f || Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        if (SegmentedWormAI.TrySpawnChain(NPC, ModContent.NPCType<ThunderMarshJiaoBody>(),
            ModContent.NPCType<ThunderMarshJiaoTail>(), BodySegments))
            NPC.localAI[3] = 1f;
    }

    private void AnnouncePhases(bool phaseTwo, bool brokenHorn)
    {
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (!Main.dedServ)
            {
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
            }
        }

        if (brokenHorn && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (!Main.dedServ)
            {
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
            }
        }
    }

    private void FireLightningPatterns(Player target, bool phaseTwo, bool brokenHorn)
    {
        NPC.ai[2]++;
        int shotInterval = brokenHorn ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= shotInterval)
        {
            NPC.ai[2] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.localAI[1]++;
        int patternInterval = brokenHorn ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode == NetmodeID.MultiplayerClient || NPC.localAI[1] < patternInterval)
        {
            return;
        }

        NPC.localAI[1] = 0f;
        int warningDamage = Math.Max(18, NPC.damage / 3);
        int lanes = brokenHorn ? 5 : phaseTwo ? 3 : 1;
        for (int i = 0; i < lanes; i++)
        {
            float offset = (i - (lanes - 1) / 2f) * 112f;
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                target.Center + new Vector2(offset, 0f),
                Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(),
                warningDamage,
                1.2f,
                Main.myPlayer);
        }

        if (brokenHorn)
        {
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(),
                Math.Max(18, NPC.damage / 4),
                1.2f,
                Main.myPlayer);
        }
    }
}

public class ThunderMarshJiaoBody : ModNPC
{
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 1;

    public override void SetDefaults()
    {
        NPC.width = 76;
        NPC.height = 62;
        NPC.damage = 48;
        NPC.defense = 24;
        NPC.lifeMax = 18000;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
    }

    public override bool CheckActive() => false;

    public override void AI() => SegmentedWormAI.FollowPreviousSegment(NPC, ThunderMarshJiao.SegmentSpacing, 0.1f, 0.08f, 0.2f, ModContent.NPCType<ThunderMarshJiao>());
}

public class ThunderMarshJiaoTail : ModNPC
{
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 1;

    public override void SetDefaults()
    {
        NPC.width = 72;
        NPC.height = 52;
        NPC.damage = 46;
        NPC.defense = 22;
        NPC.lifeMax = 18000;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
    }

    public override bool CheckActive() => false;

    public override void AI()
    {
        SegmentedWormAI.FollowPreviousSegment(NPC, ThunderMarshJiao.SegmentSpacing, 0.1f, 0.08f, 0.2f, ModContent.NPCType<ThunderMarshJiao>());
        int headIndex = (int)NPC.ai[1];
        if (headIndex < 0 || headIndex >= Main.maxNPCs)
        {
            return;
        }

        NPC head = Main.npc[headIndex];
        if (!BossTargeting.HasLivingTarget(head)) {
            NPC.damage = 0;
            NPC.localAI[0] = 0f;
            return;
        }
        bool brokenHorn = head.life < (int)(head.lifeMax * 0.35f);
        NPC.localAI[0]++;
        if (!brokenHorn || Main.netMode == NetmodeID.MultiplayerClient || NPC.localAI[0] < 120f)
        {
            return;
        }

        NPC.localAI[0] = 0f;
        Player target = Main.player[head.target];
        Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 8f;
        Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            NPC.Center,
            aim,
            ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
            Math.Max(18, head.damage / 3),
            1.3f,
            Main.myPlayer);
    }
}
