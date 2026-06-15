using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.NPCs.Bosses;

[AutoloadBossHead]
public class SpiritVeinWyrm : ModNPC
{
    private const int BodySegments = 7;
    internal const float SegmentSpacing = 34f;
    private bool spawnedChildren;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.SpiritVeinWyrm"));
    }

    public override void SetDefaults()
    {
        NPC.width = 52;
        NPC.height = 52;
        int baseLife = 1200;
        int baseDamage = 22;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 6;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(silver: 80);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss1;
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
                NPC.velocity.Y -= 0.15f;
                NPC.EncourageDespawn(30);
                return;
            }
        }

        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
        AnnouncePhases(phaseTwo, finalPhase);
        SpawnSplitMinions(phaseTwo);

        NPC.ai[0]++;
        if (NPC.ai[1] > 0f)
        {
            NPC.ai[1]--;
        }
        else if (NPC.ai[0] >= (finalPhase ? 180f : phaseTwo ? 210f : 240f))
        {
            NPC.ai[0] = 0f;
            NPC.ai[1] = finalPhase ? 54f : 42f;
            NPC.netUpdate = true;
        }

        Vector2 toTarget = target.Center - NPC.Center;
        bool dashing = NPC.ai[1] > 0f;
        float baseSpeed = finalPhase ? 6.8f : phaseTwo ? 5.9f : 4.8f;
        if (dashing)
        {
            baseSpeed *= 1.5f;
        }

        Vector2 aim = toTarget.SafeNormalize(Vector2.UnitY);
        if (!dashing)
        {
            Vector2 wave = aim.RotatedBy(MathHelper.PiOver2) * (float)Math.Sin(NPC.ai[0] * 0.06f) * 120f;
            aim = (toTarget + wave).SafeNormalize(Vector2.UnitY);
        }

        NPC.velocity = Vector2.Lerp(NPC.velocity, aim * baseSpeed, dashing ? 0.11f : 0.045f);
        NPC.rotation = NPC.velocity.ToRotation();

        FireSpiritBolts(target, phaseTwo, finalPhase);
        Lighting.AddLight(NPC.Center, 0.05f, 0.28f, 0.2f);
    }

    public override void OnKill()
    {
        DownedBossSystem.MarkDowned("spirit_vein_wyrm");
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 1, 12, 18));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 1, 20, 35));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SpiritVeinWyrmTrophy>(), 10, 1, 1));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.LowGradeSpiritCore>(), 1, 1, 1));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SpiritVeinScale>(), 1, 12, 18));
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
            previous = SegmentedWormAI.SpawnSegment(NPC, previous, ModContent.NPCType<SpiritVeinWyrmBody>(), i + 1);
        }

        SegmentedWormAI.SpawnSegment(NPC, previous, ModContent.NPCType<SpiritVeinWyrmTail>(), BodySegments + 1);
    }

    private void AnnouncePhases(bool phaseTwo, bool finalPhase)
    {
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritVeinTremor"));
            }
        }

        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.ShatteredJadeRampage"));
            }
        }
    }

    private void SpawnSplitMinions(bool phaseTwo)
    {
        if (spawnedChildren || !phaseTwo || Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        spawnedChildren = true;
        int count = Main.rand.Next(2, 4);
        for (int i = 0; i < count; i++)
        {
            int id = NPC.NewNPC(
                NPC.GetSource_FromAI(),
                (int)NPC.Center.X + Main.rand.Next(-80, 81),
                (int)NPC.Center.Y + Main.rand.Next(-40, 41),
                ModContent.NPCType<ShatteredJadeWyrmMinion>(),
                ai0: NPC.whoAmI);
            Main.npc[id].velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
        }
    }

    private void FireSpiritBolts(Player target, bool phaseTwo, bool finalPhase)
    {
        NPC.ai[2]++;
        int shotInterval = finalPhase ? 90 : phaseTwo ? 130 : 180;
        if (Main.netMode == NetmodeID.MultiplayerClient || NPC.ai[2] < shotInterval)
        {
            return;
        }

        NPC.ai[2] = 0f;
        Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
        int damage = Math.Max(12, NPC.damage / 4);
        int spread = finalPhase ? 2 : phaseTwo ? 1 : 0;
        for (int i = -spread; i <= spread; i++)
        {
            Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(10f * i)) * (finalPhase ? 8.5f : 7f);
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                velocity,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                damage,
                1.2f,
                Main.myPlayer);
        }
    }
}

public class SpiritVeinWyrmBody : ModNPC
{
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 1;

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.damage = 18;
        NPC.defense = 6;
        NPC.lifeMax = 1200;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
    }

    public override bool CheckActive() => false;

    public override void AI() => SegmentedWormAI.FollowPreviousSegment(NPC, SpiritVeinWyrm.SegmentSpacing, 0.03f, 0.22f, 0.16f);
}

public class SpiritVeinWyrmTail : ModNPC
{
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 1;

    public override void SetDefaults()
    {
        NPC.width = 44;
        NPC.height = 36;
        NPC.damage = 16;
        NPC.defense = 5;
        NPC.lifeMax = 1200;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
    }

    public override bool CheckActive() => false;

    public override void AI() => SegmentedWormAI.FollowPreviousSegment(NPC, SpiritVeinWyrm.SegmentSpacing, 0.02f, 0.16f, 0.12f);
}
