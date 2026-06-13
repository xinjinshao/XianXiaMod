using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class TribulationCloudling : ModNPC

{

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.TribulationCloudling.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 240;

        NPC.damage = 42;

        NPC.defense = 16;

        NPC.value = 120f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Bat;

        AIType = NPCID.CaveBat;

        NPC.noGravity = true;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {

        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.ThunderMarshCloudsBiome>() ? 0.18f : 0f;

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

        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead && NPC.localAI[0] >= 150f)

        {

            NPC.localAI[0] = 0f;

            Vector2 predicted = target.Center + target.velocity * 30f;

            NPC.Center = predicted + new Vector2(Main.rand.NextFloat(-120f, 120f), Main.rand.NextFloat(-160f, -80f));

            Projectile.NewProjectile(

                NPC.GetSource_FromAI(),

                predicted + new Vector2(0f, -340f),

                Vector2.UnitY * 8f,

                ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(),

                Math.Max(1, NPC.damage / 2),

                0f);

            NPC.netUpdate = true;

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SingingThunderStoneItem>(), 5, 1, 2));

    }

}
