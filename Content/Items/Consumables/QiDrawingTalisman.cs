using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.Consumables;

public class QiDrawingTalisman : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 2);
        Item.rare = ItemRarityID.White;
    }

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<XianXiaPlayer>().UnlockQiAwakening();
        if (Main.myPlayer == player.whoAmI)
        {
            Main.NewText(this.GetLocalization("Awakened").Value, 115, 255, 230);
        }
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<LowGradeSpiritStone>(3)
            .AddIngredient<SpiritGel>(2)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
