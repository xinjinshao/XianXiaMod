using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class ThunderPatternHawk : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ThunderPatternHawk.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 300;

        NPC.damage = 48;

        NPC.defense = 18;

        NPC.value = 150f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Bat;

        AIType = NPCID.CaveBat;

        NPC.noGravity = true;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(ThunderPatternHawk), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;


        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.ThunderMarshCloudsBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()
    {
        if (!Main.dedServ && NPC.velocity.LengthSquared() > 80f)
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, -NPC.velocity.X * 0.1f, -NPC.velocity.Y * 0.1f);
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            if (NPC.localAI[1] > 0f)
            {
                NPC.velocity *= 0.3f;
                NPC.netUpdate = true;
            }
            NPC.localAI[0] = NPC.localAI[1] = 0f;
            return;
        }
        bool diving = NPC.localAI[1] > 0f;
        if (++NPC.localAI[0] < (diving ? 30f : 140f)) return;
        NPC.localAI[0] = 0f;
        if (diving)
        {
            NPC.localAI[1] = 0f;
            NPC.velocity *= 0.3f;
        }
        else
        {
            NPC.localAI[1] = 1f;
            NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 15f;
        }
        NPC.netUpdate = true;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ThunderPatternFeather>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 5, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
