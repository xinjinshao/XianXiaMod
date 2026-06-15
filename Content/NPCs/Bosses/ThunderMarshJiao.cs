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

    public override void SetDefaults()
    {
        NPC.width = 78;
        NPC.height = 66;
        int baseLife = 18000;
        int baseDamage = 58;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
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

    public override void AI()
    {
        EnsureSegments();

        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }

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
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("thunder_marsh_jiao");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ThunderMarshJiaoWing>(), 12, 1, 1));
    }

    private void EnsureSegments()
    {
        if (NPC.localAI[3] == 1f || Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        NPC.localAI[3] = 1f;
        int previous = NPC.whoAmI;
        for (int i = 0; i < BodySegments; i++)
        {
            previous = SegmentedWormAI.SpawnSegment(NPC, previous, ModContent.NPCType<ThunderMarshJiaoBody>(), i + 1);
        }

        SegmentedWormAI.SpawnSegment(NPC, previous, ModContent.NPCType<ThunderMarshJiaoTail>(), BodySegments + 1);
    }

    private void AnnouncePhases(bool phaseTwo, bool brokenHorn)
    {
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
            }
        }

        if (brokenHorn && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
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

    public override void AI() => SegmentedWormAI.FollowPreviousSegment(NPC, ThunderMarshJiao.SegmentSpacing, 0.1f, 0.08f, 0.2f);
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
        SegmentedWormAI.FollowPreviousSegment(NPC, ThunderMarshJiao.SegmentSpacing, 0.1f, 0.08f, 0.2f);
        int headIndex = (int)NPC.ai[1];
        if (headIndex < 0 || headIndex >= Main.maxNPCs)
        {
            return;
        }

        NPC head = Main.npc[headIndex];
        bool brokenHorn = head.active && head.life < (int)(head.lifeMax * 0.35f);
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
