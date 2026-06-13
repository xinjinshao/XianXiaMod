using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class IronShardSpirit : ModNPC

{

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.IronShardSpirit.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 70;

        NPC.damage = 22;

        NPC.defense = 6;

        NPC.value = 60f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Bat;

        AIType = NPCID.CaveBat;

        NPC.noGravity = true;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {

        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.SunkenFurnaceVeinBiome>() ? 0.18f : 0f;

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

        if (target.active && !target.dead && NPC.localAI[0] >= 75f)

        {

            NPC.localAI[0] = 0f;

            float swarmBonus = 1f;

            foreach (NPC other in Main.ActiveNPCs)

            {

                if (other.whoAmI != NPC.whoAmI && other.type == NPC.type && Vector2.Distance(NPC.Center, other.Center) < 200f)

                    swarmBonus += 0.25f;

            }

            Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);

            NPC.velocity = direction * (11f * swarmBonus);

            NPC.netUpdate = true;

        }



        NPC.rotation = NPC.velocity.X * 0.04f;

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.FurnaceSlagIron>(), 4, 1, 2));

    }

}
