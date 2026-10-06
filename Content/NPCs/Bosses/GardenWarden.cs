using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.Localization;

using Terraria.ModLoader;

using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class GardenWarden : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.BossFrameCount;
    }


    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.GardenWarden.Text"));

    }



    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = BossStatRules.ScaleLife(NPC.lifeMax, balance, bossAdjustment);
    }

    public override void SetDefaults()

    {

        NPC.width = 96;

        NPC.height = 96;

        var stats = BossStatRules.Get(nameof(GardenWarden));
        NPC.lifeMax = stats.Life;
        NPC.damage = stats.Damage;

        NPC.defense = 10;

        NPC.knockBackResist = 0f;

        NPC.value = Item.buyPrice(gold: 1);

        NPC.boss = true;

        NPC.noGravity = true;

        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit4;

        NPC.DeathSound = SoundID.NPCDeath14;

        NPC.aiStyle = -1;

        Music = MusicID.Boss2;

    }



    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 8);
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("garden_warden");



    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<global::XianXia.Content.Items.TreasureBags.GardenWardenBag>()));
        npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<global::XianXia.Content.Items.MasterRewards.GardenWardenMonument>()));
        var normal = new LeadingConditionRule(new Conditions.NotExpert());
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 1, 16, 28));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 1, 8, 16));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        normal.OnSuccess(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.GardenWardenMask>(), 7, 1, 1));
        npcLoot.Add(normal);
    }

}
