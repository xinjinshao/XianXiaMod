using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Tiles.Stations;

public class EarthClayFurnaceTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true; Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddMapEntry(new Color(160, 110, 80), CreateMapEntryName());
        DustType = DustID.Stone; MineResist = 1f; MinPick = 0;
    }
}

public class SimpleTalismanTableTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true; Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddMapEntry(new Color(200, 180, 120), CreateMapEntryName());
        DustType = DustID.WoodFurniture; MineResist = 1f; MinPick = 0;
    }
}

public class StarPatternCauldronTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true; Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddMapEntry(new Color(40, 50, 110), CreateMapEntryName());
        DustType = DustID.GemSapphire; MineResist = 2f; MinPick = 110;
        AdjTiles = [TileID.AlchemyTable];
    }
}

public class ThunderPatternForgeTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true; Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddMapEntry(new Color(130, 120, 210), CreateMapEntryName());
        DustType = DustID.Electric; MineResist = 2f; MinPick = 110;
        AdjTiles = [TileID.MythrilAnvil];
    }
}

public class SectTrialAltarTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true; Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddMapEntry(new Color(180, 175, 140), CreateMapEntryName());
        DustType = DustID.Stone; MineResist = 3f; MinPick = 150;
        AdjTiles = [TileID.MythrilAnvil, TileID.AlchemyTable];
    }
}

public class HeavenFireFurnaceTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true; Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddMapEntry(new Color(220, 210, 160), CreateMapEntryName());
        DustType = DustID.GoldCoin; MineResist = 4f; MinPick = 200;
        AdjTiles = [TileID.AdamantiteForge, TileID.AlchemyTable];
    }
}

public class DaoSeveringAltarTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true; Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddMapEntry(new Color(40, 40, 40), CreateMapEntryName());
        DustType = DustID.Obsidian; MineResist = 5f; MinPick = 250;
        AdjTiles = [TileID.LunarCraftingStation];
    }
}
