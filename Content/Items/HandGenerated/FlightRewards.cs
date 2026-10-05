using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.HandGenerated;

// Independent equip slots keep these rewards from changing vanilla wing stats.
public abstract class FlightReward : ModItem
{
    private int wingSlot;
    protected abstract int FlightTime { get; }
    protected abstract float FlightSpeed { get; }
    protected abstract float FlightAcceleration { get; }
    protected abstract int VisualWing { get; }
    protected abstract int Price { get; }
    protected abstract int Rarity { get; }

    public override void Load() => wingSlot = EquipLoader.AddEquipTexture(
        Mod, $"Terraria/Images/Wings_{VisualWing}", EquipType.Wings, this);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ArmorIDs.Wing.Sets.Stats[wingSlot] = new WingStats(FlightTime, FlightSpeed, FlightAcceleration);
    }

    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: Price);
        Item.rare = Rarity;
        Item.accessory = true;
        Item.wingSlot = wingSlot;
    }

    public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling,
        ref float ascentWhenRising, ref float maxCanAscendMultiplier,
        ref float maxAscentMultiplier, ref float constantAscend)
    {
        ascentWhenFalling = 0.85f;
        ascentWhenRising = 0.15f;
        maxCanAscendMultiplier = 1f;
        maxAscentMultiplier = 3f;
        constantAscend = 0.135f;
    }
}

public class ThunderMarshJiaoWing : FlightReward
{
    protected override int FlightTime => 120;
    protected override float FlightSpeed => 6f;
    protected override float FlightAcceleration => 1.5f;
    protected override int VisualWing => ArmorIDs.Wing.FrozenWings;
    protected override int Price => 3;
    protected override int Rarity => ItemRarityID.LightRed;
}

public class MoonboneImmortalWingAccessory : FlightReward
{
    protected override int FlightTime => 180;
    protected override float FlightSpeed => 9f;
    protected override float FlightAcceleration => 2.5f;
    protected override int VisualWing => ArmorIDs.Wing.BoneWings;
    protected override int Price => 5;
    protected override int Rarity => ItemRarityID.Red;
}
