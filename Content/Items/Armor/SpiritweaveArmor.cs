using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.Armor;

// Vanilla armor visuals are temporary; each piece owns its equip slot.
public abstract class CultivationArmorItem : ModItem
{
    private int slot;
    protected abstract EquipType Kind { get; }
    protected abstract int Visual { get; }
    protected abstract int Defense { get; }
    protected abstract string SetPreview { get; }
    protected virtual int Rarity => ItemRarityID.Blue;
    public override void Load() => slot = EquipLoader.AddEquipTexture(Mod, Kind switch {
        EquipType.Head => "Terraria/Images/Armor_Head_" + Visual,
        EquipType.Body => "Terraria/Images/Armor/Armor_" + Visual,
        _ => "Terraria/Images/Armor_Legs_" + Visual
    }, Kind, this);
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) =>
        tooltips.Add(new TooltipLine(Mod, "CultivationSetPreview", Terraria.Localization.Language.GetTextValue("Mods.XianXia.ArmorGuide.CompleteSet", SetPreview)));
    public override void SetDefaults()
    {
        Item.width = Item.height = 28; Item.maxStack = 1; Item.defense = Defense;
        Item.value = Item.buyPrice(silver: 2); Item.rare = Rarity;
        if (Kind == EquipType.Head) Item.headSlot = slot;
        else if (Kind == EquipType.Body) Item.bodySlot = slot;
        else Item.legSlot = slot;
    }
}

public abstract class SpiritweaveArmor : CultivationArmorItem
{
    protected override string SetPreview => SpiritweaveHood.BonusText();
    protected abstract int WoodCost { get; }
    protected abstract int GelCost { get; }
    protected abstract int StoneCost { get; }
    public override void AddRecipes() => CreateRecipe().AddRecipeGroup(RecipeGroupID.Wood, WoodCost)
        .AddIngredient<SpiritGel>(GelCost).AddIngredient<LowGradeSpiritStone>(StoneCost)
        .AddTile(TileID.WorkBenches).Register();
}

public class SpiritweaveHood : SpiritweaveArmor
{
    public const int RegenBonus = 1, CostReductionPercent = 8, DamagePenaltyPercent = 10;
    internal static string BonusText() => ModContent.GetInstance<SpiritweaveHood>().GetLocalization("SetBonus")
        .Format(RegenBonus, CostReductionPercent, DamagePenaltyPercent);
    protected override EquipType Kind => EquipType.Head;
    protected override int Visual => ArmorIDs.Head.WoodHelmet;
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodHelmet}";
    protected override int Defense => 1;
    protected override int WoodCost => 16;
    protected override int GelCost => 6;
    protected override int StoneCost => 2;
    public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == Type
        && body.type == ModContent.ItemType<SpiritweaveRobe>() && legs.type == ModContent.ItemType<SpiritweaveLeggings>();
    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = BonusText();
        var state = player.GetModPlayer<XianXiaPlayer>();
        state.spiritualEnergyRegenBonus += RegenBonus;
        state.spiritualEnergyCostMultiplier *= 1f - CostReductionPercent / 100f;
        player.GetDamage(DamageClass.Generic) -= DamagePenaltyPercent / 100f;
    }
}

public class SpiritweaveRobe : SpiritweaveArmor
{
    protected override EquipType Kind => EquipType.Body;
    protected override int Visual => ArmorIDs.Body.WoodBreastplate;
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodBreastplate}";
    protected override int Defense => 2;
    protected override int WoodCost => 24;
    protected override int GelCost => 10;
    protected override int StoneCost => 4;
}

public class SpiritweaveLeggings : SpiritweaveArmor
{
    protected override EquipType Kind => EquipType.Legs;
    protected override int Visual => ArmorIDs.Legs.WoodGreaves;
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodGreaves}";
    protected override int Defense => 1;
    protected override int WoodCost => 20;
    protected override int GelCost => 8;
    protected override int StoneCost => 3;
}
