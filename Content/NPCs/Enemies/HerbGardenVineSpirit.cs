using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class HerbGardenVineSpirit : ModNPC

{

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.HerbGardenVineSpirit.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 140;

        NPC.damage = 24;

        NPC.defense = 8;

        NPC.value = 70f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Bat;

        AIType = NPCID.CaveBat;

        NPC.noGravity = true;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {

        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.GreenwoodHerbGardenBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()

    {

        Player target = Main.player[NPC.target];

        float distance = Vector2.Distance(NPC.Center, target.Center);

        if (target.active && !target.dead && distance < 160f)

        {

            NPC.velocity *= 0.92f;

        }



        NPC.localAI[0]++;

        if (NPC.localAI[0] >= 90f)

        {

            NPC.localAI[0] = 0f;

            if (NPC.life < NPC.lifeMax)

            {

                NPC.life += Math.Min(4, NPC.lifeMax - NPC.life);

            }



            for (int i = 0; i < 6; i++)

            {

                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Grass, 0f, -0.6f);

            }

        }



        NPC.localAI[1]++;

        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead

            && NPC.localAI[1] >= 130f && distance > 160f && distance < 480f)

        {

            NPC.localAI[1] = 0f;

            Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 6f;

            Projectile.NewProjectile(

                NPC.GetSource_FromAI(),

                NPC.Center,

                velocity,

                ModContent.ProjectileType<global::XianXia.Content.Projectiles.SpiritBoltProjectile>(),

                Math.Max(1, NPC.damage / 3),

                0.8f);

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.HerbDew>(), 3, 1, 2));

    }

}
