using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class ScriptureArchiveEcho : ModNPC

{

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ScriptureArchiveEcho.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 720;

        NPC.damage = 66;

        NPC.defense = 28;

        NPC.value = 360f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Bat;

        AIType = NPCID.CaveBat;

        NPC.noGravity = true;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {

        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.TenThousandSectsRuinsBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()

    {

        Player target = Main.player[NPC.target];

        if (!target.active || target.dead)

        {

            NPC.TargetClosest(false);

            target = Main.player[NPC.target];

        }



        NPC.localAI[0]++;

        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead && NPC.localAI[0] >= 105f)

        {

            NPC.localAI[0] = 0f;

            NPC.localAI[1]++;

            if (NPC.localAI[1] % 3 == 0)

            {

                NPC.localAI[1] = 0f;

                NPC.defense = 72;

                for (int j = 0; j < 12; j++)

                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCoin, 0f, -2f, 100, default, 0.6f);

            }

            else

            {

                NPC.defense = NPC.life < NPC.lifeMax / 2 ? 36 : 28;

            }

            for (int i = -1; i <= 1; i++)

            {

                Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(12f * i)) * 6.5f;

                Projectile.NewProjectile(

                    NPC.GetSource_FromAI(),

                    NPC.Center,

                    velocity,

                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),

                    Math.Max(1, NPC.damage / 4),

                    0.5f);

            }

        }



        if (NPC.defense == 72 && NPC.localAI[0] > 30f)

        {

            NPC.defense = NPC.life < NPC.lifeMax / 2 ? 36 : 28;

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.TornScrollPage>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SectTrialToken>(), 4, 1, 2));

    }

}
