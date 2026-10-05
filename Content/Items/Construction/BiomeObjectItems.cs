using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.Construction;

public abstract class BiomeObjectItem : ModItem
{
    protected abstract int Tile { get; }
    protected abstract int StoneCost { get; }
    protected abstract int ExtraIngredient { get; }
    protected abstract int ExtraCost { get; }
    protected abstract Condition Gate { get; }

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(Tile);
        Item.width = Item.height = 32;
        Item.maxStack = 99;
        Item.value = 0;
        Item.rare = ItemRarityID.LightRed;
    }
    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.StoneBlock, StoneCost)
        .AddIngredient(ExtraIngredient, ExtraCost)
        .AddIngredient<LowGradeSpiritStone>(5)
        .AddTile(TileID.WorkBenches).AddCondition(Gate).Register();
}

public class SwordTabletPlaceable : BiomeObjectItem
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/BrokenSwordIntent";
    protected override int Tile => ModContent.TileType<global::XianXia.Content.Tiles.SwordTabletTile>();
    protected override int StoneCost => 20;
    protected override int ExtraIngredient => ItemID.Bone;
    protected override int ExtraCost => 5;
    protected override Condition Gate => Condition.DownedPlantera;
}

public class SingingThunderStonePlaceable : BiomeObjectItem
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/SingingThunderStoneItem";
    protected override int Tile => ModContent.TileType<global::XianXia.Content.Tiles.SingingThunderStoneTile>();
    protected override int StoneCost => 20;
    protected override int ExtraIngredient => ItemID.FallenStar;
    protected override int ExtraCost => 5;
    protected override Condition Gate => Condition.Hardmode;
}

public class RiftMembranePlaceable : BiomeObjectItem
{
    public override string Texture => "XianXia/Content/Items/Materials/StarAbyssMembrane";
    protected override int Tile => ModContent.TileType<global::XianXia.Content.Tiles.RiftMembraneTile>();
    protected override int StoneCost => 20;
    protected override int ExtraIngredient => ItemID.FallenStar;
    protected override int ExtraCost => 5;
    protected override Condition Gate => Condition.Hardmode;
}

public class BrokenHeavenTabletPlaceable : BiomeObjectItem
{
    public override string Texture => "XianXia/Content/Items/Materials/HeavenTabletRubbing";
    protected override int Tile => ModContent.TileType<global::XianXia.Content.Tiles.BrokenHeavenTabletTile>();
    protected override int StoneCost => 40;
    protected override int ExtraIngredient => ItemID.GoldBar;
    protected override int ExtraCost => 10;
    protected override Condition Gate => Condition.DownedGolem;
}

public class ArchiveLightPillarPlaceable : BiomeObjectItem
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/ArchiveRemnantLight";
    protected override int Tile => ModContent.TileType<global::XianXia.Content.Tiles.ArchiveLightPillarTile>();
    protected override int StoneCost => 60;
    protected override int ExtraIngredient => ItemID.FallenStar;
    protected override int ExtraCost => 10;
    protected override Condition Gate => Condition.DownedMoonLord;
}
