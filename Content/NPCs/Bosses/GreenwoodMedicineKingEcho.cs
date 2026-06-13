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

public class GreenwoodMedicineKingEcho : ModNPC

{

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.GreenwoodMedicineKingEcho.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 96;

        NPC.height = 96;

        int baseLife = 52000;

        int baseDamage = 66;

        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }

        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }

        NPC.lifeMax = baseLife;

        NPC.damage = baseDamage;

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



    public override void AI()

    {

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

        Vector2 desired = target.Center - NPC.Center;

        float p2 = 0.7f;

        float p3 = 0.4f;

        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);

        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);

        if (phaseTwo && NPC.localAI[0] < 1f)

        {

            NPC.localAI[0] = 1f;

            if (Main.netMode != NetmodeID.Server)

                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));

        }

        if (finalPhase && NPC.localAI[0] < 2f)

        {

            NPC.localAI[0] = 2f;

            if (Main.netMode != NetmodeID.Server)

                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));

        }

        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;

        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);

        NPC.rotation = NPC.velocity.ToRotation();

        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);



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

        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;

        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)

        {

            NPC.ai[2] = 0f;



            int fDmg = Math.Max(18, NPC.damage / 4);

            if (phaseTwo && NPC.ai[3]++ == 0) {

                for (int f = 0; f < 3; f++)

                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)target.Center.X + Main.rand.Next(-120, 121), (int)target.Center.Y - 60,

                        ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.HerbGardenVineSpirit>(), ai0: NPC.whoAmI);

            }

            Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 16f, Vector2.Zero,

                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), fDmg, 1.2f, Main.myPlayer);

            if (finalPhase) {

                Vector2 up = new Vector2(0, -1);

                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + up * 80f, Vector2.Zero,

                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), fDmg, 1.2f, Main.myPlayer);

            }



        }



        if (finalPhase && NPC.ai[1]++ > 180f)

        {

            NPC.ai[1] = 0f;

            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;

        }

    }



    public override void OnKill() => DownedBossSystem.MarkDowned("greenwood_medicine_king_echo");



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 1, 16, 28));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpringReturnPill>(), 1, 8, 16));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.MedicineKingCauldronDecoration>(), 10, 1, 1));

    }

}





[AutoloadBossHead]
