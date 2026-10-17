using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditGenerationProtection(Action<bool,string> check)
    {
        int cx = Main.maxTilesX / 2 + 400, cy = 40;
        var saved = new List<(int X,int Y,bool Has,ushort Type,ushort Wall,short FX,short FY,SlopeType Slope,bool Half)>();
        for (int x = cx - 12; x <= cx + 12; x++) for (int y = cy - 12; y <= cy + 12; y++) {
            Tile tile = Main.tile[x,y]; saved.Add((x,y,tile.HasTile,tile.TileType,tile.WallType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        var baselineFrames = new Dictionary<(int,int),(short,short)>();
        void Reset(bool solid) { foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = solid; tile.TileType = TileID.Stone; tile.WallType = WallID.Stone; tile.TileFrameX = tile.TileFrameY = 0; tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; }
            // Fully frame the natural fixture before measuring whether a later paint changes reserved cells.
            for (int x = cx - 10; x <= cx + 10; x++) for (int y = cy - 10; y <= cy + 10; y++) WorldGen.SquareTileFrame(x,y);
            baselineFrames.Clear();
            for (int x = cx - 1; x <= cx + 1; x++) for (int y = cy - 10; y <= cy + 10; y++) {
                Tile tile = Main.tile[x,y]; baselineFrames[(x,y)] = (tile.TileFrameX,tile.TileFrameY);
            }
        }
        string mismatch = "";
        bool Untouched(bool solid) {
            for (int x = cx - 1; x <= cx + 1; x++) for (int y = cy - 10; y <= cy + 10; y++) {
                Tile tile = Main.tile[x,y]; if (tile.HasTile != solid || tile.TileType != TileID.Stone || tile.WallType != WallID.Stone || tile.TileFrameX != baselineFrames[(x,y)].Item1 || tile.TileFrameY != baselineFrames[(x,y)].Item2) { mismatch = $"at {x - cx},{y - cy}: has={tile.HasTile} type={tile.TileType} wall={tile.WallType} frame={tile.TileFrameX},{tile.TileFrameY}"; return false; }
            }
            return true;
        }
        var map = new StructureMap(); map.AddProtectedStructure(new Rectangle(cx - 1,cy - 10,3,21),0);
        var full = new StructureMap(); full.AddProtectedStructure(new Rectangle(cx - 12,cy - 12,25,25),0);
        int slag = ModContent.TileType<FurnaceSlagTile>(), wall = ModContent.WallType<BlackFurnaceWall>(), cloud = ModContent.TileType<ThunderCloudTile>();
        try {
            Reset(true);
            check(GeneratedBiomeWorldGenSystem.PaintProtectedEllipse(cx,cy,8,6,slag,slag,wall,map) > 0,"native unreserved ecological terrain still painted");
            check(Untouched(true),"native ecological paint preserves reserved natural tiles walls and frames " + mismatch);
            Reset(true); check(SpiritVeinWorldGenSystem.PaintVein(cx,cy,8,6,map) > 0,"native unreserved shallow vein terrain still painted");
            check(Untouched(true),"native shallow vein paint preserves reserved natural tiles walls and frames");
            Reset(false); check(GeneratedBiomeWorldGenSystem.PaintCloudEllipse(cx,cy,8,6,cloud,map) > 0,"native clouds still fill unreserved empty sky");
            check(Untouched(false),"native cloud fill preserves reserved empty space walls and frames");
            check(!WorldGenerationProtection.CanChange(cx - 2,cy,map) && WorldGenerationProtection.CanChange(cx - 3,cy,map),"native reservation buffer keeps tile framing away from protected cells");
            Reset(true);
            check(GeneratedBiomeWorldGenSystem.PaintProtectedEllipse(cx,cy,8,6,slag,slag,wall,full) == 0 && Untouched(true),"native fully reserved ecological patch has zero changes");
            check(SpiritVeinWorldGenSystem.PaintVein(cx,cy,8,6,full) == 0 && Untouched(true),"native fully reserved shallow vein has zero changes");
            Reset(false); check(GeneratedBiomeWorldGenSystem.PaintCloudEllipse(cx,cy,8,6,cloud,full) == 0 && Untouched(false),"native fully reserved cloud patch has zero changes");
            check(!WorldGenerationProtection.CanChange(cx,cy,null) && !WorldGenerationProtection.CanChange(-1,cy,map),"native absent protection map and bad coordinates do not authorize writes");
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type; tile.WallType = cell.Wall; tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
        }
    }
}
