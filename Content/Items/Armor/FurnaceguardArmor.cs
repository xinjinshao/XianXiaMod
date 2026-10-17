using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Armor;

public abstract class FurnaceguardArmor : CultivationArmorItem
{
    protected override int Rarity => ItemRarityID.Green;
    protected abstract int BarCost { get; }
    protected abstract int SlagCost { get; }
    public override void AddRecipes() => CreateRecipe().AddRecipeGroup(RecipeGroupID.IronBar, BarCost)
        .AddIngredient<FurnaceSlagIron>(SlagCost).AddIngredient<OldFurnaceEmber>()
        .AddTile(ModContent.TileType<ArtifactForgeTile>()).Register();
}

public class FurnaceguardHelmet : FurnaceguardArmor
{
    public const int ResistancePercent = 5, MovementPenaltyPercent = 10, CostPenaltyPercent = 10;
    protected override EquipType Kind => EquipType.Head;
    protected override int Visual => ArmorIDs.Head.IronHelmet;
    protected override int Icon => ItemID.IronHelmet;
    protected override int Defense => 3;
    protected override int BarCost => 6;
    protected override int SlagCost => 8;
    public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == Type
        && body.type == ModContent.ItemType<FurnaceguardBreastplate>() && legs.type == ModContent.ItemType<FurnaceguardGreaves>();
    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = this.GetLocalization("SetBonus").Format(ResistancePercent, MovementPenaltyPercent, CostPenaltyPercent);
        player.endurance += ResistancePercent / 100f;
        player.moveSpeed *= 1f - MovementPenaltyPercent / 100f;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyCostMultiplier *= 1f + CostPenaltyPercent / 100f;
    }
}

public class FurnaceguardBreastplate : FurnaceguardArmor
{
    protected override EquipType Kind => EquipType.Body;
    protected override int Visual => ArmorIDs.Body.IronChainmail;
    protected override int Icon => ItemID.IronChainmail;
    protected override int Defense => 5;
    protected override int BarCost => 10;
    protected override int SlagCost => 12;
}

public class FurnaceguardGreaves : FurnaceguardArmor
{
    protected override EquipType Kind => EquipType.Legs;
    protected override int Visual => ArmorIDs.Legs.IronGreaves;
    protected override int Icon => ItemID.IronGreaves;
    protected override int Defense => 4;
    protected override int BarCost => 8;
    protected override int SlagCost => 10;
}
