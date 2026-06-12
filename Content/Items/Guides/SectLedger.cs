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
        string reputation = Guidance("SectReputation").Format(DownedBossSystem.SectReputation);

        if (cultivation.cultivationStage < CultivationStage.QiAwakening)
        {
            return WithCommission($"{reputation} {GuidanceValue("AwakenQi")}");
        }

        if (!DownedBossSystem.DownedSpiritVeinWyrm)
        {
            return WithCommission($"{reputation} {GuidanceValue("SpiritVeinWyrm")}");
        }

        if (cultivation.cultivationStage < CultivationStage.Foundation)
        {
            return WithCommission($"{reputation} {GuidanceValue("Foundation")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("garden_warden"))
        {
            return WithCommission($"{reputation} {GuidanceValue("GardenWarden")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("black_furnace_iron_golem"))
        {
            return WithCommission($"{reputation} {GuidanceValue("BlackFurnace")}");
        }

        if (cultivation.cultivationStage < CultivationStage.GoldenCore)
        {
            return WithCommission($"{reputation} {GuidanceValue("GoldenCore")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("formless_sword_soul"))
        {
            return WithCommission($"{reputation} {GuidanceValue("SwordSoul")}");
        }

        if (cultivation.cultivationStage < CultivationStage.NascentSoul)
        {
            return WithCommission($"{reputation} {GuidanceValue("NascentSoul")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("heaven_tablet_guardian"))
        {
            return WithCommission($"{reputation} {GuidanceValue("HeavenTablet")}");
        }

        if (cultivation.cultivationStage < CultivationStage.DaoSevering)
        {
            return WithCommission($"{reputation} {Guidance("DaoSevering").Format(DownedBossSystem.SectReputation)}");
        }

        return WithCommission($"{reputation} {Guidance("Endgame").Format(DownedBossSystem.SectReputation)}");
    }

    private static string WithCommission(string guidance)
    {
        return $"{guidance} {GetCommissionGuidance()}";
    }

    private static string GetCommissionGuidance()
    {
        if (CanClaimCommission("herb_sect_apprentice_garden", "garden_warden"))
        {
            return GuidanceValue("CommissionHerbReady");
        }

        if (CanClaimCommission("wandering_artificer_furnace", "black_furnace_iron_golem"))
        {
            return GuidanceValue("CommissionFurnaceReady");
        }

        if (CanClaimCommission("tribulation_observer_thunder", "thunder_marsh_jiao"))
        {
            return GuidanceValue("CommissionThunderReady");
        }

        if (CanClaimCommission("archive_scroll_spirit_trial", "formless_sword_soul"))
        {
            return GuidanceValue("CommissionArchiveReady");
        }

        if (CanClaimCommission("fallen_heaven_messenger_tablet", "heaven_tablet_guardian"))
        {
            return GuidanceValue("CommissionHeavenReady");
        }

        return GuidanceValue("CommissionNoneReady");
    }

    private static bool CanClaimCommission(string commissionId, string requiredBoss)
    {
        return DownedBossSystem.DownedBosses.Contains(requiredBoss)
            && !DownedBossSystem.ClaimedCommissions.Contains(commissionId);
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
