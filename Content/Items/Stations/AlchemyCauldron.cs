using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Items.Accessories;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Stations;

public class AlchemyCauldron : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 99;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 40);
        Item.rare = ItemRarityID.Green;
        Item.createTile = ModContent.TileType<AlchemyCauldronTile>();
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<LowGradeSpiritStone>(8)
            .AddIngredient<GreenwoodRoot>(6)
            .AddIngredient(ItemID.Bottle)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
