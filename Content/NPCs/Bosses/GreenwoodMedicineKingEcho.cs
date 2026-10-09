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

public partial class GreenwoodMedicineKingEcho : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.BossFrameCount;
    }


    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.GreenwoodMedicineKingEcho.Text"));

    }



    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = BossStatRules.ScaleLife(NPC.lifeMax, balance, bossAdjustment);
    }

    public override void SetDefaults()

    {

        NPC.width = 96;

        NPC.height = 96;

        var stats = BossStatRules.Get(nameof(GreenwoodMedicineKingEcho));
        NPC.lifeMax = stats.Life;
        NPC.damage = stats.Damage;

        NPC.defense = 34;

        NPC.knockBackResist = 0f;

        NPC.value = Item.buyPrice(gold: 1);

        NPC.boss = true;

        NPC.noGravity = true;

        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit4;

        NPC.DeathSound = SoundID.NPCDeath14;

        NPC.aiStyle = -1;

        Music = MusicID.Boss2;

    }



    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 8);
    }

    public override void AI()

    {

        if (!BossTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            NPC.velocity = new Vector2(0f, -2f);
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                bool changed = NPC.ai[0] != 0f || NPC.ai[1] != 0f || NPC.ai[2] != 0f || NPC.ai[3] != 0f || NPC.timeLeft > 30;
                NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0f;
                summonTarget = -1; summonPlayer = null;
                if (changed) NPC.netUpdate = true;
                NPC.EncourageDespawn(30);
            }
            return;
        }

        Vector2 desired = target.Center - NPC.Center;

        float p2 = 0.7f;

        float p3 = 0.4f;

        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);

        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);

        if (phaseTwo && NPC.localAI[0] < 1f)

        {

            NPC.localAI[0] = 1f;

            if (!Main.dedServ)

                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));

        }

        if (finalPhase && NPC.localAI[0] < 2f)

        {

            NPC.localAI[0] = 2f;

            if (!Main.dedServ)

                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));

        }

        if (UpdateVineSummon(target)) return;
        if (UpdateMedicineRitual(target, phaseTwo)) return;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (phaseTwo && NPC.ai[2] >= patternInterval - 1 && BeginVineSummon(target)) return;

        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;

        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);

        NPC.rotation = NPC.velocity.ToRotation();

        if (!Main.dedServ) Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);



        NPC.ai[0]++;

        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;

        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)

        {

            NPC.ai[0] = 0f;

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



        NPC.ai[2]++;

        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)

        {

            NPC.ai[2] = 0f;



            int fDmg = Math.Max(18, NPC.damage / 4);

            Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 16f, Vector2.Zero,

                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), fDmg, 1.2f, Main.myPlayer);

            if (finalPhase) {

                Vector2 up = new Vector2(0, -1);

                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + up * 80f, Vector2.Zero,

                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), fDmg, 1.2f, Main.myPlayer);

            }



        }



    }

    public override void OnKill() => DownedBossSystem.MarkDowned("greenwood_medicine_king_echo");



    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<global::XianXia.Content.Items.TreasureBags.GreenwoodMedicineKingEchoBag>()));
        npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<global::XianXia.Content.Items.MasterRewards.GreenwoodMedicineKingEchoMonument>()));
        var normal = new LeadingConditionRule(new Conditions.NotExpert());
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.MedicineKingWoodHeart>(), 1));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 1, 16, 28));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpringReturnPill>(), 1, 8, 16));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.MedicineKingCauldronDecoration>(), 10, 1, 1));
        npcLoot.Add(normal);
    }

}
