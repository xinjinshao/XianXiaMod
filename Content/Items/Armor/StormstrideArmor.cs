using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Armor;

public abstract class StormstrideArmor : CultivationArmorItem
{
    protected override string SetPreview => StormstrideHood.BonusText();
    protected override int Rarity => ItemRarityID.LightRed;
    protected abstract int FeatherCost { get; }
    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.Silk, FeatherCost)
        .AddIngredient<ThunderPatternFeather>(FeatherCost).AddIngredient<TribulationCloudDew>(2)
        .AddTile(ModContent.TileType<ThunderPatternForgeTile>()).AddCondition(Condition.Hardmode).Register();
}

public class StormstrideHood : StormstrideArmor
{
    public const int MovementPercent = 15, CostReductionPercent = 12, DefensePenalty = 6;
    internal static string BonusText() => ModContent.GetInstance<StormstrideHood>().GetLocalization("SetBonus")
        .Format(MovementPercent, CostReductionPercent, DefensePenalty);
    protected override EquipType Kind => EquipType.Head;
    protected override int Visual => ArmorIDs.Head.MeteorHelmet;
    public override string Texture => $"Terraria/Images/Item_{ItemID.MeteorHelmet}";
    protected override int Defense => 6;
    protected override int FeatherCost => 8;
    public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == Type
        && body.type == ModContent.ItemType<StormstrideRobe>() && legs.type == ModContent.ItemType<StormstrideLeggings>();
    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = BonusText();
        player.moveSpeed *= 1f + MovementPercent / 100f;
        player.statDefense -= DefensePenalty;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyCostMultiplier *= 1f - CostReductionPercent / 100f;
    }
}

public class StormstrideRobe : StormstrideArmor
{
    protected override EquipType Kind => EquipType.Body;
    protected override int Visual => ArmorIDs.Body.MeteorSuit;
    public override string Texture => $"Terraria/Images/Item_{ItemID.MeteorSuit}";
    protected override int Defense => 9;
    protected override int FeatherCost => 12;
}

public class StormstrideLeggings : StormstrideArmor
{
    protected override EquipType Kind => EquipType.Legs;
    protected override int Visual => ArmorIDs.Legs.MeteorLeggings;
    public override string Texture => $"Terraria/Images/Item_{ItemID.MeteorLeggings}";
    protected override int Defense => 7;
    protected override int FeatherCost => 10;
}
