using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.BossSummons.Generated;

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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.GardenWarden>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.QiAwakening;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.GardenWarden>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GardenBrokenKey>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BlackFurnaceIronGolem>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.QiAwakening;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BlackFurnaceIronGolem>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.OldFurnaceEmber>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.TribulationCloudAvatar>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.QiCondensation;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.TribulationCloudAvatar>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ThunderCallingJade>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.ThunderMarshJiao>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.Foundation;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.ThunderMarshJiao>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ThunderCallingJade>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.AbyssalStarWomb>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.Foundation;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.AbyssalStarWomb>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.StarAbyssMembrane>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.FormlessSwordSoul>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.GoldenCore;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.FormlessSwordSoul>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.SectTrialToken>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.GreenwoodMedicineKingEcho>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.GoldenCore;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.GreenwoodMedicineKingEcho>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.SectTrialToken>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.HeavenTabletGuardian>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.NascentSoul;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.HeavenTabletGuardian>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.HeavenTabletRubbing>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BrokenHeavenInspector>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.NascentSoul;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BrokenHeavenInspector>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.HeavenTabletRubbing>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.MoonboneImmortal>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.Tribulation;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.MoonboneImmortal>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.MoonboneRitualTalisman>()
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
        return !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.OldHeavenDaoCore>())
            && player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage >= global::XianXia.Common.Players.CultivationStage.DaoSevering;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.OldHeavenDaoCore>());
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.MoonboneRitualTalisman>()
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(40)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}
