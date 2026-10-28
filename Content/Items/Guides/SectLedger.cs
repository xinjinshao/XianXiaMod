using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Items.Accessories;
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
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player)
    {
        if (!Main.dedServ && Main.myPlayer == player.whoAmI && player.active && !player.dead)
        {
            if (player.altFunctionUse == 2) {
                Main.NewText(CultivationFishingRules.GetGuidance(Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord), 120, 245, 220);
                return true;
            }
            Main.NewText(CultivationStatusText.Summary(player.GetModPlayer<XianXiaPlayer>()), 120, 245, 220);
            Main.NewText(GetPreparation(player), 120, 245, 220);
            Main.NewText(GetNextGuidance(player), 120, 245, 220);
            Main.NewText(SpiritTideSystem.GetGuidance(), 120, 245, 220);
        }

        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroupID.Wood, 10)
            .AddIngredient(ItemID.FallenStar)
            .AddTile(TileID.WorkBenches)
            .Register();
        CreateRecipe()
            .AddIngredient<SectTrialToken>()
            .AddIngredient<LowGradeSpiritStone>(5)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

    internal static string GetPreparation(Player player)
    {
        var cultivation = player.GetModPlayer<XianXiaPlayer>();
        if (cultivation.cultivationStage >= CultivationStage.DaoSevering)
            return Language.GetTextValue("Mods.XianXia.BreakthroughGuide.FinalRealm");
        var target = (CultivationStage)((int)cultivation.cultivationStage + 1);
        int item = target switch
        {
            CultivationStage.QiAwakening => ModContent.ItemType<global::XianXia.Content.Items.Consumables.QiDrawingTalisman>(),
            CultivationStage.QiCondensation => ModContent.ItemType<QiCondensingPill>(),
            CultivationStage.Foundation => ModContent.ItemType<FoundationPill>(),
            CultivationStage.GoldenCore => ModContent.ItemType<StarEclipseCrystal>(),
            CultivationStage.NascentSoul => ModContent.ItemType<OldHeavenDaoScroll>(),
            CultivationStage.SpiritSevering => ModContent.ItemType<HeavenDaoFragment>(),
            CultivationStage.Tribulation => ModContent.ItemType<Moonbone>(),
            CultivationStage.DaoSevering => ModContent.ItemType<DaoSeveringDust>(),
            _ => 0
        };
        if (item == 0) return Language.GetTextValue("Mods.XianXia.Progression.InvalidBreakthroughItem");
        string failure = cultivation.GetBreakthroughFailure(target);
        string boss = CultivationRules.GetRequiredBoss(target);
        string availability = failure.Length == 0
            ? Language.GetTextValue("Mods.XianXia.BreakthroughGuide.Ready")
            : Language.GetTextValue("Mods.XianXia.Progression." + failure,
                failure == "BreakthroughRequiresTrial" ? Language.GetTextValue("Mods.XianXia.Progression.TrialNames." + boss) : "");
        return Language.GetTextValue("Mods.XianXia.BreakthroughGuide.Next",
            CultivationStatusText.StageName(target), Lang.GetItemNameValue(item))
            + "\n" + availability + "\n" + CultivationStatusText.BreakthroughSource(target);
    }

    private static string GetNextGuidance(Player player)
    {
        XianXiaPlayer cultivation = player.GetModPlayer<XianXiaPlayer>();
        string reputation = Guidance("SectReputation").Format(DownedBossSystem.SectReputation);

        if (cultivation.tribulationTimer > 0)
            return WithCommission(player, $"{reputation} {GuidanceValue("ActiveTribulation")}");
        if (cultivation.CanRetryTribulation())
            return WithCommission(player, $"{reputation} {GuidanceValue("RetryTribulation")}");
        string worldFailure = CultivationRules.GetWorldFailure(
            (CultivationStage)((int)cultivation.cultivationStage + 1), Main.hardMode,
            NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord);
        if (cultivation.cultivationStage >= CultivationStage.Foundation
            && cultivation.cultivationStage < CultivationStage.DaoSevering && worldFailure.Length > 0)
            return WithCommission(player, $"{reputation} {Language.GetTextValue($"Mods.XianXia.Progression.{worldFailure}")}");

        if (cultivation.cultivationStage < CultivationStage.QiAwakening)
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("AwakenQi")}");
        }

        if (!DownedBossSystem.DownedSpiritVeinWyrm)
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("SpiritVeinWyrm")}");
        }

        if (cultivation.cultivationStage < CultivationStage.QiCondensation)
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("QiCondensation")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("garden_warden"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("GardenWarden")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("black_furnace_iron_golem"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("BlackFurnace")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("tribulation_cloud_avatar"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("TribulationCloud")}");
        }

        if (cultivation.cultivationStage < CultivationStage.Foundation)
            return WithCommission(player, $"{reputation} {GuidanceValue("Foundation")}");

        if (!DownedBossSystem.DownedBosses.Contains("thunder_marsh_jiao"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("ThunderJiao")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("abyssal_star_womb"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("StarWomb")}");
        }

        if (cultivation.cultivationStage < CultivationStage.GoldenCore)
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("GoldenCore")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("formless_sword_soul"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("SwordSoul")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("greenwood_medicine_king_echo"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("MedicineKing")}");
        }

        if (cultivation.cultivationStage < CultivationStage.NascentSoul)
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("NascentSoul")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("heaven_tablet_guardian"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("HeavenTablet")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("broken_heaven_inspector"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("BrokenInspector")}");
        }

        if (cultivation.cultivationStage < CultivationStage.SpiritSevering)
            return WithCommission(player, $"{reputation} {GuidanceValue("SpiritSevering")}");

        if (cultivation.cultivationStage < CultivationStage.Tribulation)
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("TribulationStage")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("moonbone_immortal"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("MoonboneImmortal")}");
        }

        if (!DownedBossSystem.DownedBosses.Contains("old_heaven_dao_core"))
        {
            return WithCommission(player, $"{reputation} {GuidanceValue("OldHeavenCore")}");
        }

        if (cultivation.cultivationStage < CultivationStage.DaoSevering)
        {
            return WithCommission(player, $"{reputation} {Guidance("DaoSevering").Format(DownedBossSystem.SectReputation)}");
        }

        return WithCommission(player, $"{reputation} {Guidance("Endgame").Format(DownedBossSystem.SectReputation)}");
    }

    private static string WithCommission(Player player, string guidance)
    {
        return $"{guidance} {GetCommissionGuidance(player)}";
    }

    private static string GetCommissionGuidance(Player player)
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

        string followup = FollowupCommissions.ReadyGuidance(player);
        return followup.Length > 0 ? followup : GuidanceValue("CommissionNoneReady");
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
