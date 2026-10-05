using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Items;

public abstract class EndgameRouteReward : ModItem
{
    protected abstract DownedBossSystem.EndgameRoute Route { get; }
    protected abstract int UniqueMaterial { get; }
    protected abstract int Material { get; }
    protected abstract int MaterialCount { get; }
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = Item.height = 32; Item.maxStack = 1; Item.accessory = true;
        Item.rare = ItemRarityID.Purple; Item.value = Item.buyPrice(gold: 5);
    }
    protected bool Active => DownedBossSystem.ChosenRoute == Route && DownedBossSystem.DownedBosses.Contains("old_heaven_dao_core");
    public override void AddRecipes() => CreateRecipe().AddIngredient<RouteMaterial>()
        .AddIngredient(UniqueMaterial).AddIngredient(Material,MaterialCount)
        .AddTile(ModContent.TileType<DaoSeveringAltarTile>())
        .AddCondition(new Condition(Language.GetText("Mods.XianXia.Routes." + Route + "Condition"),
            () => DownedBossSystem.ChosenRoute == Route && DownedBossSystem.DownedBosses.Contains("old_heaven_dao_core")))
        .Register();
}
