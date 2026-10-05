using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Construction;

namespace XianXia.Content.Items.Construction;

public abstract class ConstructedBiomeBlock : ModItem
{
    protected abstract int Tile { get; }
    protected abstract int Ingredient { get; }
    protected abstract int Amount { get; }
    protected virtual Condition Gate => null;
    protected virtual int CoreIngredient => ModContent.ItemType<LowGradeSpiritStone>();
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 100;
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(Tile);
        Item.width = Item.height = 32;
        Item.maxStack = 9999;
        Item.value = 0;
        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(Amount).AddIngredient(Ingredient, Amount)
            .AddIngredient(CoreIngredient).AddTile(TileID.WorkBenches);
        if (Gate != null) recipe.AddCondition(Gate);
        recipe.Register();
    }
}

public class SpiritVeinBiomeBlock : ConstructedBiomeBlock
{
    public override string Texture => "XianXia/Content/Items/Materials/LowGradeSpiritStone";
    protected override int CoreIngredient => ItemID.FallenStar;
    protected override int Tile => ModContent.TileType<SpiritVeinConstructedTile>();
    protected override int Ingredient => ItemID.StoneBlock;
    protected override int Amount => 40;
}

public class GreenwoodBiomeBlock : ConstructedBiomeBlock
{
    public override string Texture => "XianXia/Content/Items/Materials/GreenwoodRoot";
    protected override int Tile => ModContent.TileType<GreenwoodConstructedTile>();
    protected override int Ingredient => ItemID.DirtBlock;
    protected override int Amount => 100;
}

public class FurnaceBiomeBlock : ConstructedBiomeBlock
{
    public override string Texture => "XianXia/Content/Items/Materials/FurnaceSlagIron";
    protected override int Tile => ModContent.TileType<FurnaceConstructedTile>();
    protected override int Ingredient => ItemID.AshBlock;
    protected override int Amount => 100;
}

public class ThunderBiomeBlock : ConstructedBiomeBlock
{
    public override string Texture => "XianXia/Content/Items/Materials/TribulationCloudDew";
    protected override int Tile => ModContent.TileType<ThunderConstructedTile>();
    protected override int Ingredient => ItemID.Cloud;
    protected override int Amount => 100;
    protected override Condition Gate => Condition.Hardmode;
}

public class StarAbyssBiomeBlock : ConstructedBiomeBlock
{
    public override string Texture => "XianXia/Content/Items/Materials/StarEclipseCrystal";
    protected override int Tile => ModContent.TileType<StarAbyssConstructedTile>();
    protected override int Ingredient => ItemID.StoneBlock;
    protected override int Amount => 100;
    protected override Condition Gate => Condition.Hardmode;
}

public class SectRuinBiomeBlock : ConstructedBiomeBlock
{
    public override string Texture => "XianXia/Content/Items/Materials/ArtifactBlankShard";
    protected override int Tile => ModContent.TileType<SectRuinConstructedTile>();
    protected override int Ingredient => ItemID.StoneBlock;
    protected override int Amount => 100;
    protected override Condition Gate => Condition.DownedPlantera;
}

public class FallenHeavenBiomeBlock : ConstructedBiomeBlock
{
    public override string Texture => "XianXia/Content/Items/Materials/HeavenDaoFragment";
    protected override int Tile => ModContent.TileType<FallenHeavenConstructedTile>();
    protected override int Ingredient => ItemID.StoneBlock;
    protected override int Amount => 100;
    protected override Condition Gate => Condition.DownedGolem;
}

public class MoonboneBiomeBlock : ConstructedBiomeBlock
{
    public override string Texture => "XianXia/Content/Items/Materials/Moonbone";
    protected override int Tile => ModContent.TileType<MoonboneConstructedTile>();
    protected override int Ingredient => ItemID.StoneBlock;
    protected override int Amount => 100;
    protected override Condition Gate => Condition.DownedMoonLord;
}
