using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Content.Items.Fishing;

public class SpiritVeinFishingCrate : CultivationFishingCrate
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodenCrate}";
    public override int ProgressionTier => 0;
    protected override int MaterialType => ModContent.ItemType<SpiritGel>();
}

public class GreenwoodFishingCrate : CultivationFishingCrate
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodenCrate}";
    public override int ProgressionTier => 0;
    protected override int MaterialType => ModContent.ItemType<GreenwoodRoot>();
}

public class FurnaceFishingCrate : CultivationFishingCrate
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodenCrate}";
    public override int ProgressionTier => 0;
    protected override int MaterialType => ModContent.ItemType<FurnaceSlagIron>();
}

public class StarAbyssFishingCrate : CultivationFishingCrate
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodenCrate}";
    public override int ProgressionTier => 1;
    protected override int MaterialType => ModContent.ItemType<StarAbyssMembrane>();
}

public class ThunderFishingCrate : CultivationFishingCrate
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodenCrate}";
    public override int ProgressionTier => 1;
    protected override int MaterialType => ModContent.ItemType<ThunderPatternFeather>();
}

public class SectFishingCrate : CultivationFishingCrate
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodenCrate}";
    public override int ProgressionTier => 2;
    protected override int MaterialType => ModContent.ItemType<SectTrialToken>();
}

public class HeavenFishingCrate : CultivationFishingCrate
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodenCrate}";
    public override int ProgressionTier => 3;
    protected override int MaterialType => ModContent.ItemType<HeavenTabletRubbing>();
}

public class MoonboneFishingCrate : CultivationFishingCrate
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.WoodenCrate}";
    public override int ProgressionTier => 4;
    protected override int MaterialType => ModContent.ItemType<ColdMoonDust>();
}
