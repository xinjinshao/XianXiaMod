using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.HandGenerated;

public abstract class BossMaskReward : ModItem
{
    private int headSlot;
    protected abstract int VisualHead { get; }
    protected abstract int Price { get; }
    protected abstract int Rarity { get; }

    public override void Load() => headSlot = EquipLoader.AddEquipTexture(
        Mod, $"Terraria/Images/Armor_Head_{VisualHead}", EquipType.Head, this);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ArmorIDs.Head.Sets.DrawHead[headSlot] = false;
    }

    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: Price);
        Item.rare = Rarity;
        Item.vanity = true;
        Item.headSlot = headSlot;
    }
}

public class GardenWardenMask : BossMaskReward
{
    protected override int VisualHead => ArmorIDs.Head.PlanteraMask;
    protected override int Price => 1;
    protected override int Rarity => ItemRarityID.Green;
}

public class InspectorMask : BossMaskReward
{
    protected override int VisualHead => ArmorIDs.Head.GuyFawkesMask;
    protected override int Price => 4;
    protected override int Rarity => ItemRarityID.Yellow;
}

public class FormlessSwordSoulCostume : ModItem
{
    private int bodySlot;
    private int robeSlot;

    public override void Load()
    {
        bodySlot = EquipLoader.AddEquipTexture(Mod,
            $"Terraria/Images/Armor/Armor_{ArmorIDs.Body.ReaperRobe}", EquipType.Body, this);
        robeSlot = EquipLoader.AddEquipTexture(Mod,
            $"Terraria/Images/Armor_Legs_{ArmorIDs.Legs.ReaperRobe}", EquipType.Legs, this);
    }

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Lime;
        Item.vanity = true;
        Item.bodySlot = bodySlot;
    }

    public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
    {
        equipSlot = robeSlot;
        robes = true;
    }
}
