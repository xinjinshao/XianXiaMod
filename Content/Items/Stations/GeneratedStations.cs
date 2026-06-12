using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Stations;

public class EarthClayFurnace : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 48; Item.height = 48;
        Item.maxStack = 99; Item.useTurn = true; Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = 10; Item.useAnimation = 15;
        Item.consumable = true; Item.value = Item.buyPrice(silver: 50);
        Item.rare = ItemRarityID.White;
        Item.createTile = ModContent.TileType<global::XianXia.Content.Tiles.Stations.EarthClayFurnaceTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient(ItemID.ClayBlock, 20).AddIngredient(ItemID.StoneBlock, 15).AddIngredient(ItemID.Wood, 10).AddTile(TileID.WorkBenches).Register();
    }
}

public class SimpleTalismanTable : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 48; Item.height = 32;
        Item.maxStack = 99; Item.useTurn = true; Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = 10; Item.useAnimation = 15;
        Item.consumable = true; Item.value = Item.buyPrice(silver: 50);
        Item.rare = ItemRarityID.White;
        Item.createTile = ModContent.TileType<global::XianXia.Content.Tiles.Stations.SimpleTalismanTableTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient(ItemID.Wood, 15).AddIngredient(ItemID.Book, 1).AddTile(TileID.WorkBenches).Register();
    }
}

public class StarPatternCauldron : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 64; Item.height = 64;
        Item.maxStack = 99; Item.useTurn = true; Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = 10; Item.useAnimation = 15;
        Item.consumable = true; Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.LightRed;
        Item.createTile = ModContent.TileType<global::XianXia.Content.Tiles.Stations.StarPatternCauldronTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(8)
            .AddIngredient(ItemID.HellstoneBar, 10)
            .AddTile(ModContent.TileType<AlchemyCauldronTile>()).Register();
    }
}

public class ThunderPatternForge : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 64; Item.height = 48;
        Item.maxStack = 99; Item.useTurn = true; Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = 10; Item.useAnimation = 15;
        Item.consumable = true; Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.LightRed;
        Item.createTile = ModContent.TileType<global::XianXia.Content.Tiles.Stations.ThunderPatternForgeTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<global::XianXia.Content.Items.Generated.TribulationCloudDew>(8)
            .AddIngredient(ItemID.MythrilAnvil).AddTile(TileID.MythrilAnvil).Register();
    }
}

public class SectTrialAltar : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 64; Item.height = 48;
        Item.maxStack = 99; Item.useTurn = true; Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = 10; Item.useAnimation = 15;
        Item.consumable = true; Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Lime;
        Item.createTile = ModContent.TileType<global::XianXia.Content.Tiles.Stations.SectTrialAltarTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<global::XianXia.Content.Items.Generated.SectTrialToken>(4)
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(8)
            .AddTile(ModContent.TileType<ArtifactForgeTile>()).Register();
    }
}

public class HeavenFireFurnace : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 64; Item.height = 64;
        Item.maxStack = 99; Item.useTurn = true; Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = 10; Item.useAnimation = 15;
        Item.consumable = true; Item.value = Item.buyPrice(gold: 5);
        Item.rare = ItemRarityID.Yellow;
        Item.createTile = ModContent.TileType<global::XianXia.Content.Tiles.Stations.HeavenFireFurnaceTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(12)
            .AddIngredient(ItemID.AdamantiteForge).AddTile(TileID.MythrilAnvil).Register();
    }
}

public class DaoSeveringAltar : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 80; Item.height = 48;
        Item.maxStack = 99; Item.useTurn = true; Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = 10; Item.useAnimation = 15;
        Item.consumable = true; Item.value = Item.buyPrice(gold: 10);
        Item.rare = ItemRarityID.Red;
        Item.createTile = ModContent.TileType<global::XianXia.Content.Tiles.Stations.DaoSeveringAltarTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<global::XianXia.Content.Items.Generated.DaoSeveringDust>(12)
            .AddIngredient<global::XianXia.Content.Items.Generated.Moonbone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.HeavenFireFurnaceTile>()).Register();
    }
}
