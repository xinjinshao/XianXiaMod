using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Armor;

public abstract class MoonbonePactArmor : CultivationArmorItem
{
    protected override string SetPreview => MoonbonePactCrown.BonusText();
    protected override int Rarity => ItemRarityID.Red;
    protected abstract int BoneCost { get; }
    protected abstract int LightCost { get; }
    public override void AddRecipes() => CreateRecipe().AddIngredient<Moonbone>(BoneCost)
        .AddIngredient<ArchiveRemnantLight>(LightCost).AddIngredient(ItemID.Silk, BoneCost * 2)
        .AddTile(ModContent.TileType<DaoSeveringAltarTile>()).AddCondition(Condition.DownedMoonLord).Register();
}

public class MoonbonePactCrown : MoonbonePactArmor
{
    public const int MinionBonus = 4, SummonDamagePercent = 35, CostPenaltyPercent = 25;
    internal static string BonusText() => ModContent.GetInstance<MoonbonePactCrown>().GetLocalization("SetBonus")
        .Format(MinionBonus, SummonDamagePercent, CostPenaltyPercent);
    protected override EquipType Kind => EquipType.Head;
    protected override int Visual => ArmorIDs.Head.SpookyHelmet;
    public override string Texture => $"Terraria/Images/Item_{ItemID.SpookyHelmet}";
    protected override int Defense => 8;
    protected override int BoneCost => 6;
    protected override int LightCost => 3;
    public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == Type
        && body.type == ModContent.ItemType<MoonbonePactRobe>() && legs.type == ModContent.ItemType<MoonbonePactLeggings>();
    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = BonusText();
        player.maxMinions += MinionBonus;
        player.GetDamage(DamageClass.Summon) += SummonDamagePercent / 100f;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyCostMultiplier *= 1f + CostPenaltyPercent / 100f;
    }
}

public class MoonbonePactRobe : MoonbonePactArmor
{
    protected override EquipType Kind => EquipType.Body;
    protected override int Visual => ArmorIDs.Body.SpookyBreastplate;
    public override string Texture => $"Terraria/Images/Item_{ItemID.SpookyBreastplate}";
    protected override int Defense => 16;
    protected override int BoneCost => 10;
    protected override int LightCost => 5;
}

public class MoonbonePactLeggings : MoonbonePactArmor
{
    protected override EquipType Kind => EquipType.Legs;
    protected override int Visual => ArmorIDs.Legs.SpookyLeggings;
    public override string Texture => $"Terraria/Images/Item_{ItemID.SpookyLeggings}";
    protected override int Defense => 12;
    protected override int BoneCost => 8;
    protected override int LightCost => 4;
}
