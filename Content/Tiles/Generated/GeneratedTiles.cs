using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Tiles.Generated;

public class GreenwoodSoilTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.GreenwoodRoot>());
    }
}


public class SpiritHerbTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.GreenwoodRoot>());
    }
}


public class FurnaceSlagTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.FurnaceSlagIron>());
    }
}


public class BlackFurnaceWall : ModWall
{
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = false;
        DustType = DustID.Stone;
        AddMapEntry(new Color(90, 82, 76), CreateMapEntryName());
    }
}


public class ThunderCloudTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.TribulationCloudDew>());
    }
}


public class StarAbyssCrystalTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.StarEclipseCrystal>());
    }
}


public class SectRuinBrickTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.ArtifactBlankShard>());
    }
}


public class FallenHeavenJadeTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.HeavenDaoFragment>());
    }
}


public class MoonboneTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Stone;
        MineResist = 1.1f;
        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Generated.Moonbone>());
    }
}
