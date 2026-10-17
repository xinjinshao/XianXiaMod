using System;
using Terraria;
using Terraria.ModLoader;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditHerbPlacement(Action<bool, string> check)
    {
        int x = Main.maxTilesX / 2, y = 12;
        var saved = new System.Collections.Generic.List<(int X, int Y, bool Has, ushort Type, short FX, short FY, byte Liquid, Terraria.ID.SlopeType Slope, bool Half)>();
        for (int dx = -3; dx <= 3; dx++) for (int dy = -3; dy <= 3; dy++) {
            Tile tile = Main.tile[x + dx, y + dy];
            saved.Add((x + dx, y + dy, tile.HasTile, tile.TileType, tile.TileFrameX, tile.TileFrameY, tile.LiquidAmount, tile.Slope, tile.IsHalfBlock));
        }
        int herb = ModContent.TileType<CultivatedSpiritHerbTile>();
        try {
            foreach (int support in new int[] { Terraria.ID.TileID.Dirt, Terraria.ID.TileID.Grass, Terraria.ID.TileID.JungleGrass,
                ModContent.TileType<GreenwoodSoilTile>(), ModContent.TileType<SpiritMossTile>(), Terraria.ID.TileID.ClayPot, Terraria.ID.TileID.PlanterBox, Terraria.ID.TileID.Stone }) {
                foreach (var cell in saved) {
                    Tile tile = Main.tile[cell.X, cell.Y]; tile.HasTile = false; tile.LiquidAmount = 0;
                    tile.TileFrameX = tile.TileFrameY = 0; tile.Slope = Terraria.ID.SlopeType.Solid; tile.IsHalfBlock = false;
                }
                Tile ground = Main.tile[x, y + 1]; ground.HasTile = true; ground.TileType = (ushort)support;
                if (support == Terraria.ID.TileID.ClayPot || support == Terraria.ID.TileID.PlanterBox) {
                    Tile floor = Main.tile[x, y + 2]; floor.HasTile = true; floor.TileType = Terraria.ID.TileID.Dirt;
                }
                WorldGen.PlaceObject(x, y, herb);
                Tile crop = Main.tile[x, y]; bool planted = crop.HasTile && crop.TileType == herb;
                check(planted == (support != Terraria.ID.TileID.Stone), "native object placement matches advertised herb soil/pot/planter anchors; support=" + support + "; support shape=" + Terraria.ObjectData.TileObjectData.GetTileData(support, 0)?.Width + "x" + Terraria.ObjectData.TileObjectData.GetTileData(support, 0)?.Height + "; anchor=" + Terraria.ObjectData.TileObjectData.GetTileData(herb, 0).AnchorBottom.type);
                if (planted) check(crop.TileFrameX == 0, "native object placement starts the herb as an immature seedling");
            }
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X, cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.LiquidAmount = cell.Liquid; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
        }
        check(System.Linq.Enumerable.All(saved, cell => {
            Tile tile = Main.tile[cell.X, cell.Y];
            return tile.HasTile == cell.Has && tile.TileType == cell.Type && tile.TileFrameX == cell.FX && tile.TileFrameY == cell.FY
                && tile.LiquidAmount == cell.Liquid && tile.Slope == cell.Slope && tile.IsHalfBlock == cell.Half;
        }), "native herb placement audit restores the complete bounded terrain fixture fields");
    }

    private static void AuditHerbMap(Action<bool, string> check)
    {
        int x = Main.maxTilesX / 2, y = 12;
        Tile tile = Main.tile[x, y]; bool oldHas = tile.HasTile; ushort oldType = tile.TileType; short oldFrame = tile.TileFrameX;
        var herb = ModContent.GetInstance<CultivatedSpiritHerbTile>();
        try {
            tile.HasTile = true; tile.TileType = (ushort)herb.Type;
            string[] keys = { "Seedling", "Growing", "Mature" };
            for (int stage = 0; stage < 3; stage++) {
                tile.TileFrameX = (short)(stage * 18);
                ushort option = herb.GetMapOption(x, y);
                check(option == stage && tile.TileFrameX == stage * 18, "native herb map stage follows real frame without growing the plant");
                string key = "Mods.XianXia.Tiles.CultivatedSpiritHerbTile." + keys[stage];
                string expected = Terraria.Localization.Language.GetTextValue(key);
                check(expected != key && herb.GetLocalization(keys[stage]).Value == expected,
                    "native registered herb resolves the localized label for each real growth stage; client map atlas is not initialized on dedicated servers");
            }
        } finally { tile.HasTile = oldHas; tile.TileType = oldType; tile.TileFrameX = oldFrame; }
        check(tile.HasTile == oldHas && tile.TileType == oldType && tile.TileFrameX == oldFrame, "native herb map audit restores all patched terrain fields");
    }

    private static void AuditHerbGrowth(Action<bool, string> check)
    {
        int x = Main.maxTilesX / 2, y = 12, mode = Main.netMode;
        Tile tile = Main.tile[x, y]; bool has = tile.HasTile; ushort type = tile.TileType;
        short frame = tile.TileFrameX; byte liquid = tile.LiquidAmount;
        var herb = ModContent.GetInstance<CultivatedSpiritHerbTile>();
        int seeds = ModContent.ItemType<Content.Items.HandGenerated.SpiritHerbSeeds>();
        int root = ModContent.ItemType<Content.Items.Materials.GreenwoodRoot>();
        try {
            tile.HasTile = false; tile.LiquidAmount = 0;
            check(herb.CanPlace(x, y), "native dry empty planting cell passes crop placement hook");
            tile.LiquidAmount = 1; check(!herb.CanPlace(x, y), "native wet planting cell is refused");
            tile.HasTile = true; tile.TileType = (ushort)herb.Type; tile.TileFrameX = 0;
            check(!herb.CanPlace(x, y), "native existing crop cannot be overwritten by another seed");
            Main.netMode = Terraria.ID.NetmodeID.Server;
            herb.RandomUpdate(x, y); check(tile.TileFrameX == 0, "native flooded crop cannot grow");
            tile.LiquidAmount = 0; Main.netMode = Terraria.ID.NetmodeID.MultiplayerClient;
            herb.RandomUpdate(x, y); check(tile.TileFrameX == 0, "native client cannot advance the crop frame");
            var young = System.Linq.Enumerable.ToArray(herb.GetItemDrops(x, y));
            check(young.Length == 1 && young[0].type == seeds && young[0].stack == 1, "native immature crop returns exactly its seed and no root");
            Main.netMode = Terraria.ID.NetmodeID.Server;
            int updates = 0; bool ordered = true;
            while (tile.TileFrameX != 36 && updates++ < 4096) {
                short before = tile.TileFrameX; herb.RandomUpdate(x, y);
                ordered &= tile.TileFrameX == before || tile.TileFrameX == before + 18;
            }
            check(ordered, "native random growth never skips a stage or rewinds");
            check(tile.TileFrameX == 36 && herb.IsTileSpelunkable(x, y), "native crop reaches mature registered frame and spelunker eligibility");
            var mature = System.Linq.Enumerable.ToArray(herb.GetItemDrops(x, y));
            check(mature.Length == 2 && mature[0].type == root && mature[0].stack == 1
                && mature[1].type == seeds && mature[1].stack >= 1 && mature[1].stack <= 3,
                "native mature drop hook creates one root and one to three replanting seeds");
            herb.RandomUpdate(x, y); check(tile.TileFrameX == 36, "native mature crop does not overgrow");
        } finally { tile.HasTile = has; tile.TileType = type; tile.TileFrameX = frame; tile.LiquidAmount = liquid; Main.netMode = mode; }
        check(tile.HasTile == has && tile.TileType == type && tile.TileFrameX == frame && tile.LiquidAmount == liquid && Main.netMode == mode,
            "native crop audit restores terrain liquid and authority context");
    }
}
