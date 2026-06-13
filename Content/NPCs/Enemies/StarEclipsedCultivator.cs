using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class StarEclipsedCultivator : ModNPC

{

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.StarEclipsedCultivator.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 360;

        NPC.damage = 50;

        NPC.defense = 20;

        NPC.value = 180f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Fighter;

        AIType = NPCID.CaveBat;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {

        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.StarAbyssRiftBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()

    {

        Player target = Main.player[NPC.target];

        if (!target.active || target.dead)

        {

            NPC.TargetClosest(false);

            target = Main.player[NPC.target];

        }



        if (!target.active || target.dead)

        {

            return;

        }



        float distance = Vector2.Distance(target.Center, NPC.Center);

        if (distance < 240f)

        {

            NPC.velocity += (NPC.Center - target.Center).SafeNormalize(Vector2.Zero) * 0.12f;

        }



        if (NPC.life < NPC.lifeMax * 0.4f && NPC.localAI[1]++ > 180f)

        {

            NPC.localAI[1] = 0f;

            NPC.velocity += (NPC.Center - target.Center).SafeNormalize(Vector2.Zero) * 6f;

        }



        NPC.localAI[0]++;

        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] >= 135f)

        {

            NPC.localAI[0] = 0f;

            Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 7.5f;

            Projectile.NewProjectile(

                NPC.GetSource_FromAI(),

                NPC.Center,

                velocity,

                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),

                Math.Max(1, NPC.damage / 3),

                1f);

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.StarEclipseCrystal>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenHeavenJade>(), 4, 1, 2));

    }

}
