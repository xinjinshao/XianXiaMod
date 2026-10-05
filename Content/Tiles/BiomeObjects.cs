using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;

namespace XianXia.Content.Tiles;

public class SwordTabletTile : ModTile
{
    public override void SetStaticDefaults()
    {
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Origin = new Point16(0, 2);
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Construction.SwordTabletPlaceable>());
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; AddMapEntry(new Color(160, 200, 220), CreateMapEntryName());
        DustType = DustID.Stone; MineResist = 3f; MinPick = 150;
    }
    public override bool CanExplode(int i, int j) => false;
}

public class BrokenHeavenTabletTile : ModTile
{
    public override void SetStaticDefaults()
    {
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.Origin = new Point16(0, 3);
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Construction.BrokenHeavenTabletPlaceable>());
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileLighted[Type] = true;
        AddMapEntry(new Color(220, 210, 160), CreateMapEntryName());
        DustType = DustID.GoldCoin; MineResist = 4f; MinPick = 200;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = 0.3f; g = 0.28f; b = 0.15f; }
}

public class ArchiveLightPillarTile : ModTile
{
    public override void SetStaticDefaults()
    {
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 6;
        TileObjectData.newTile.Origin = new Point16(0, 5);
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 };
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Construction.ArchiveLightPillarPlaceable>());
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileLighted[Type] = true;
        AddMapEntry(new Color(220, 220, 240), CreateMapEntryName());
        DustType = DustID.IceTorch; MineResist = 5f; MinPick = 225;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = 0.2f; g = 0.22f; b = 0.35f; }
}

public class SingingThunderStoneTile : ModTile
{
    public override void SetStaticDefaults()
    {
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Construction.SingingThunderStonePlaceable>());
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileLighted[Type] = true;
        AddMapEntry(new Color(140, 130, 220), CreateMapEntryName());
        DustType = DustID.Electric; MineResist = 2f; MinPick = 110;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = 0.15f; g = 0.12f; b = 0.3f; }
}

public class RiftMembraneTile : ModTile
{
    public override void SetStaticDefaults()
    {
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Construction.RiftMembranePlaceable>());
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileLighted[Type] = true;
        AddMapEntry(new Color(40, 40, 100), CreateMapEntryName());
        DustType = DustID.GemSapphire; MineResist = 2f; MinPick = 110;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = 0.05f; g = 0.05f; b = 0.2f; }
}
