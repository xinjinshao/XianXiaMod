using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using XianXia.Content.Items.Construction;

namespace XianXia.Content.Tiles.Construction;

public abstract class ConstructedBiomeTile : ModTile
{
    protected abstract int Drop { get; }
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1f;
        AddMapEntry(new Color(100, 160, 140), CreateMapEntryName());
        RegisterItemDrop(Drop);
    }
}

public class SpiritVeinConstructedTile : ConstructedBiomeTile
{
    public override string Texture => "XianXia/Content/Tiles/SpiritMossTile";
    protected override int Drop => ModContent.ItemType<SpiritVeinBiomeBlock>();
}

public class GreenwoodConstructedTile : ConstructedBiomeTile
{
    public override string Texture => "XianXia/Content/Tiles/GreenwoodSoilTile";
    protected override int Drop => ModContent.ItemType<GreenwoodBiomeBlock>();
}

public class FurnaceConstructedTile : ConstructedBiomeTile
{
    public override string Texture => "XianXia/Content/Tiles/FurnaceSlagTile";
    protected override int Drop => ModContent.ItemType<FurnaceBiomeBlock>();
}

public class ThunderConstructedTile : ConstructedBiomeTile
{
    public override string Texture => "XianXia/Content/Tiles/ThunderCloudTile";
    protected override int Drop => ModContent.ItemType<ThunderBiomeBlock>();
}

public class StarAbyssConstructedTile : ConstructedBiomeTile
{
    public override string Texture => "XianXia/Content/Tiles/StarAbyssCrystalTile";
    protected override int Drop => ModContent.ItemType<StarAbyssBiomeBlock>();
}

public class SectRuinConstructedTile : ConstructedBiomeTile
{
    public override string Texture => "XianXia/Content/Tiles/SectRuinBrickTile";
    protected override int Drop => ModContent.ItemType<SectRuinBiomeBlock>();
}

public class FallenHeavenConstructedTile : ConstructedBiomeTile
{
    public override string Texture => "XianXia/Content/Tiles/FallenHeavenJadeTile";
    protected override int Drop => ModContent.ItemType<FallenHeavenBiomeBlock>();
}

public class MoonboneConstructedTile : ConstructedBiomeTile
{
    public override string Texture => "XianXia/Content/Tiles/MoonboneTile";
    protected override int Drop => ModContent.ItemType<MoonboneBiomeBlock>();
}
