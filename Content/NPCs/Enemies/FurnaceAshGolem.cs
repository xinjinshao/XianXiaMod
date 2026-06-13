using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class FurnaceAshGolem : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.FurnaceAshGolem.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 180;

        NPC.damage = 28;

        NPC.defense = 14;

        NPC.value = 90f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Fighter;

        AIType = NPCID.CaveBat;



        NPC.knockBackResist = 0.2f;

    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {

        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.SunkenFurnaceVeinBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()

    {

        NPC.defense = NPC.velocity.LengthSquared() < 0.1f ? 22 : 14;

    }



    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)

    {

        target.AddBuff(BuffID.OnFire3, 180);

    }



    public override void HitEffect(NPC.HitInfo hit)

    {

        for (int i = 0; i < 6; i++)

        {

            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, hit.HitDirection * 1.2f, -1.4f);

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.FurnaceSlagIron>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.FurnaceCharcoal>(), 4, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
