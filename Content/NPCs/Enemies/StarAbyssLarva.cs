using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class StarAbyssLarva : ModNPC

{

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.StarAbyssLarva.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 260;

        NPC.damage = 46;

        NPC.defense = 18;

        NPC.value = 130f;

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



        NPC.localAI[0]++;

        if (NPC.localAI[1] > 0f)

        {

            NPC.localAI[1]--;

            if (target.active && !target.dead && Vector2.Distance(target.Center, NPC.Center) < 40f)

            {

                target.velocity *= 0.6f;

            }

        }

        else if (target.active && !target.dead && NPC.localAI[0] >= 90f && Vector2.Distance(target.Center, NPC.Center) < 260f)

        {

            NPC.localAI[0] = 0f;

            Vector2 leap = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 8f;

            leap.Y -= 4f;

            NPC.velocity = leap;

            NPC.localAI[1] = 90f;

            NPC.netUpdate = true;

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.AbyssDust>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.DarkBlueSpiritFluid>(), 5, 1, 2));

    }

}
