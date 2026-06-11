using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Items.Generated;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.Guides;

public class SectLedger : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item4;
        Item.value = Item.buyPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            Main.NewText(GetNextGuidance(player), 120, 245, 220);
        }

        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SectTrialToken>()
            .AddIngredient<LowGradeSpiritStone>(5)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

    private static string GetNextGuidance(Player player)
    {
        XianXiaPlayer cultivation = player.GetModPlayer<XianXiaPlayer>();

        if (cultivation.cultivationStage < CultivationStage.QiAwakening)
        {
            return GuidanceValue("AwakenQi");
        }

        if (!DownedBossSystem.DownedSpiritVeinWyrm)
        {
            return GuidanceValue("SpiritVeinWyrm");
        }

        if (cultivation.cultivationStage < CultivationStage.Foundation)
        {
            return GuidanceValue("Foundation");
        }

        if (!DownedBossSystem.DownedBosses.Contains("garden_warden"))
        {
            return GuidanceValue("GardenWarden");
        }

        if (!DownedBossSystem.DownedBosses.Contains("black_furnace_iron_golem"))
        {
            return GuidanceValue("BlackFurnace");
        }

        if (cultivation.cultivationStage < CultivationStage.GoldenCore)
        {
            return GuidanceValue("GoldenCore");
        }

        if (!DownedBossSystem.DownedBosses.Contains("formless_sword_soul"))
        {
            return GuidanceValue("SwordSoul");
        }

        if (cultivation.cultivationStage < CultivationStage.NascentSoul)
        {
            return GuidanceValue("NascentSoul");
        }

        if (!DownedBossSystem.DownedBosses.Contains("heaven_tablet_guardian"))
        {
            return GuidanceValue("HeavenTablet");
        }

        if (cultivation.cultivationStage < CultivationStage.DaoSevering)
        {
            return Guidance("DaoSevering").Format(DownedBossSystem.SectReputation);
        }

        return Guidance("Endgame").Format(DownedBossSystem.SectReputation);
    }

    private static LocalizedText Guidance(string key)
    {
        return ModContent.GetInstance<SectLedger>().GetLocalization($"Guidance.{key}");
    }

    private static string GuidanceValue(string key)
    {
        return Guidance(key).Value;
    }
}
