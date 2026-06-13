using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class MiasmaFlowerMoth : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.MiasmaFlowerMoth.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 90;

        NPC.damage = 20;

        NPC.defense = 4;

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

        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.GreenwoodHerbGardenBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()

    {

        NPC.velocity *= 0.985f;

        NPC.localAI[0]++;

        if (NPC.localAI[0] >= 45f)

        {

            NPC.localAI[0] = 0f;

            foreach (Player player in Main.ActivePlayers)

            {

                if (Vector2.Distance(player.Center, NPC.Center) <= 128f)

                {

                    player.AddBuff(BuffID.Poisoned, 90);

                }

            }



            for (int i = 0; i < 10; i++)

            {

                float angle = MathHelper.TwoPi * i / 10f;

                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 48f;

                Dust.NewDust(NPC.Center + offset, 4, 4, DustID.Poisoned, offset.X * 0.03f, offset.Y * 0.03f, 100, default, 0.7f);

            }

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.HerbDew>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.CinnabarPowder>(), 3, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
