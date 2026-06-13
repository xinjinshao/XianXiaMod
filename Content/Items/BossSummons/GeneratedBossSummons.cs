using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.BossSummons;

public class SummonGardenBrokenKey : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.GardenWarden>(),
                global::XianXia.Common.Players.CultivationStage.QiAwakening,
                "spirit_vein_wyrm")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "garden_warden");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.GardenWarden>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.GardenBrokenKey>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}


public class SummonOldFurnaceEmber : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.BlackFurnaceIronGolem>(),
                global::XianXia.Common.Players.CultivationStage.QiAwakening,
                "spirit_vein_wyrm")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "black_furnace_iron_golem");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.BlackFurnaceIronGolem>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.OldFurnaceEmber>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(13)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}


public class SummonThunderCallingJade : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.TribulationCloudAvatar>(),
                global::XianXia.Common.Players.CultivationStage.QiCondensation,
                "garden_warden")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "tribulation_cloud_avatar");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.TribulationCloudAvatar>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.ThunderCallingJade>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(16)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}


public class SummonThunderCallingJadeThunderMarshJiao : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.ThunderMarshJiao>(),
                global::XianXia.Common.Players.CultivationStage.Foundation,
                "tribulation_cloud_avatar")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "thunder_marsh_jiao");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.ThunderMarshJiao>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.ThunderCallingJade>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(19)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}


public class SummonStarAbyssMembrane : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.AbyssalStarWomb>(),
                global::XianXia.Common.Players.CultivationStage.Foundation,
                "black_furnace_iron_golem")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "abyssal_star_womb");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.AbyssalStarWomb>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.StarAbyssMembrane>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(22)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}


public class SummonSectTrialToken : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.FormlessSwordSoul>(),
                global::XianXia.Common.Players.CultivationStage.GoldenCore,
                "thunder_marsh_jiao")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "formless_sword_soul");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.FormlessSwordSoul>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.SectTrialToken>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(25)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}


public class SummonSectTrialTokenGreenwoodMedicineKingEcho : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.GreenwoodMedicineKingEcho>(),
                global::XianXia.Common.Players.CultivationStage.GoldenCore,
                "garden_warden")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "greenwood_medicine_king_echo");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.GreenwoodMedicineKingEcho>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.SectTrialToken>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(28)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}


public class SummonHeavenTabletRubbing : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.HeavenTabletGuardian>(),
                global::XianXia.Common.Players.CultivationStage.NascentSoul,
                "formless_sword_soul")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "heaven_tablet_guardian");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.HeavenTabletGuardian>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.HeavenTabletRubbing>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(31)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}


public class SummonHeavenTabletRubbingBrokenHeavenInspector : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.BrokenHeavenInspector>(),
                global::XianXia.Common.Players.CultivationStage.NascentSoul,
                "heaven_tablet_guardian")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "broken_heaven_inspector");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.BrokenHeavenInspector>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.HeavenTabletRubbing>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(34)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}


public class SummonMoonboneRitualTalisman : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.MoonboneImmortal>(),
                global::XianXia.Common.Players.CultivationStage.Tribulation,
                "broken_heaven_inspector")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "moonbone_immortal");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.MoonboneImmortal>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.MoonboneRitualTalisman>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(37)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}


public class SummonMoonboneRitualTalismanOldHeavenDaoCore : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBossSummon(
                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.OldHeavenDaoCore>(),
                global::XianXia.Common.Players.CultivationStage.DaoSevering,
                "moonbone_immortal")
            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "old_heaven_dao_core");
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.OldHeavenDaoCore>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Materials.MoonboneRitualTalisman>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(40)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}
