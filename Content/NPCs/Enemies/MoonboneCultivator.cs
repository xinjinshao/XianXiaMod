using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class MoonboneCultivator : ModNPC

{

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.MoonboneCultivator.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 4200;

        NPC.damage = 160;

        NPC.defense = 72;

        NPC.value = 2100f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Fighter;

        AIType = NPCID.CaveBat;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {

        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.MoonboneAbyssBiome>() ? 0.18f : 0f;

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

        if (target.active && !target.dead && NPC.localAI[0] >= 70f)

        {

            NPC.localAI[0] = 0f;

            if (Main.netMode != NetmodeID.MultiplayerClient)

            {

                Vector2 predicted = target.Center + target.velocity * 18f;

                Vector2 velocity = (predicted - NPC.Center).SafeNormalize(Vector2.UnitY) * 9f;

                Projectile.NewProjectile(

                    NPC.GetSource_FromAI(),

                    NPC.Center,

                    velocity,

                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),

                    Math.Max(1, NPC.damage / 2),

                    1f);

            }

            NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 12f;

            NPC.netUpdate = true;

        }



        Lighting.AddLight(NPC.Center, 0.08f, 0.18f, 0.24f);

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.Moonbone>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ColdMoonDust>(), 3, 1, 2));

    }

}
