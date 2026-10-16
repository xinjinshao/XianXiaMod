using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.Consumables;

public class QiDrawingTalisman : ModItem
{
    public const int SpiritStoneCost = 3, SpiritGelCost = 2;
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

    public override bool CanUseItem(Player player) => player != null && player.active && !player.dead
        && !player.noItems && !player.CCed && player.TryGetModPlayer<XianXiaPlayer>(out var state)
        && state.CanUseBreakthroughItem(CultivationStage.QiAwakening);

    public override bool? UseItem(Player player)
    {
        if (!CanUseItem(player)) return false;
        if (global::XianXia.Common.Systems.CultivationItemTransactions.RequestIfMultiplayer(player, Item))
            return true;

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
            .AddIngredient<LowGradeSpiritStone>(SpiritStoneCost)
            .AddIngredient<SpiritGel>(SpiritGelCost)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
