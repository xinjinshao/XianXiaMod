using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.HandGenerated;

public class TribulationTrainingToken : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 30;
        Item.UseSound = SoundID.Item4;
        Item.value = Item.buyPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player)
    {
        if (!player.dead && player.GetModPlayer<XianXiaPlayer>().CanRetryTribulation())
            return true;
        if (Main.myPlayer == player.whoAmI)
            Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.TribulationRetryUnavailable"), 255, 210, 120);
        return false;
    }

    public override bool? UseItem(Player player)
    {
        if (global::XianXia.Common.Systems.CultivationItemTransactions.RequestIfMultiplayer(player, Item))
            return true;
        return player.GetModPlayer<XianXiaPlayer>().TryRetryTribulation();
    }

    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<LowGradeSpiritStone>(6).AddIngredient<SpiritGel>(4)
            .AddTile(TileID.WorkBenches).Register();
    }
}
