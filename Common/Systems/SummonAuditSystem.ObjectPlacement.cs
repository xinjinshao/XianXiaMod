using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditProtectedObjectPlacement(Action<bool,string> check)
    {
        var hall = new Rectangle(Main.maxTilesX / 2 + 500,30,41,19); int x = hall.X + 10, ground = hall.Bottom - 3;
        int sword = ModContent.TileType<SwordTabletTile>(), brick = ModContent.TileType<SectRuinBrickTile>();
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>(); bool dry = true;
        for (int tx = hall.Left - 4; tx < hall.Right + 4; tx++) for (int ty = hall.Top - 4; ty < hall.Bottom + 4; ty++) {
            Tile tile = Main.tile[tx,ty]; dry &= tile.LiquidAmount == 0 && tile.WallType == 0 && !tile.RedWire && !tile.BlueWire && !tile.GreenWire && !tile.YellowWire && !tile.HasActuator;
            saved.Add((tx,ty,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        check(dry,"native protected object fixture dry empty sky");
        var halls = ModContent.GetInstance<SectRuinHallSystem>(); var original = new TagCompound(); halls.SaveWorldData(original);
        void Reset() { foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = false; tile.TileType = TileID.Stone; tile.TileFrameX = tile.TileFrameY = 0; tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; } }
        bool Complete() { for (int col = 0; col < 2; col++) for (int row = 0; row < 3; row++) { Tile tile = Main.tile[x + col,ground - 3 + row]; if (!tile.HasTile || tile.TileType != sword || tile.TileFrameX != col * 16 || tile.TileFrameY != row * 16) return false; } return true; }
        try {
            halls.ClearWorld(); Reset(); var protectedMap = new StructureMap(); protectedMap.AddProtectedStructure(new Rectangle(x,ground - 3,2,4),0);
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(sword,brick,3,x,ground,protectedMap) && !Main.tile[x,ground].HasTile,"native protected empty pedestal leaves no foundation");
            for (int col = 0; col < 2; col++) { Tile support = Main.tile[x + col,ground]; support.HasTile = true; support.TileType = (ushort)brick; }
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(sword,brick,3,x,ground,protectedMap) && !Main.tile[x,ground - 1].HasTile,"native direct placement cannot populate reserved foreign space");
            Reset(); Tile obstacle = Main.tile[x,ground - 2]; obstacle.RedWire = true;
            try { check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(sword,brick,3,x,ground,new StructureMap()),"native fallback pedestal refuses wiring"); } finally { obstacle.RedWire = false; }
            obstacle.HasActuator = true;
            try { check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(sword,brick,3,x,ground,new StructureMap()),"native fallback pedestal refuses actuator"); } finally { obstacle.HasActuator = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(sword,brick,3,x,ground,new StructureMap()),"native fallback pedestal refuses housing walls"); } finally { obstacle.WallType = 0; }
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(sword,brick,2,x,ground,new StructureMap()) && !Main.tile[x,ground].HasTile,"native registered dimensions prevent incorrect footprint placement");
            check(GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(sword,brick,3,x,ground,new StructureMap()) && Complete()
                && Main.tile[x,ground].HasTile && Main.tile[x,ground].TileType == brick,"native safe pedestal creates full framed sword and flat support");
            Reset(); Tile stale = Main.tile[x,ground]; stale.TileFrameX = 36; stale.TileFrameY = 54; stale.Slope = (SlopeType)1; stale.IsHalfBlock = true;
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(sword,ModContent.TileType<CultivatedSpiritHerbTile>(),3,x,ground,new StructureMap()),"native engine rejects unsupported non-solid pedestal anchor");
            bool empty = true; for (int col = 0; col < 2; col++) for (int row = 0; row <= 3; row++) empty &= !Main.tile[x + col,ground - row].HasTile;
            check(empty && stale.TileType == TileID.Stone && stale.TileFrameX == 36 && stale.TileFrameY == 54 && stale.Slope == (SlopeType)1 && stale.IsHalfBlock,
                "native failed pedestal restores full footprint and original type frame slope half-block metadata");
            Reset(); var ownMap = new StructureMap(); check(SectRuinHallSystem.TryBuild(hall,ownMap),"native owned hall reservation fixture built");
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(sword,brick,3,x,ground,ownMap),"native unrecognized hall reservation still rejects placement");
            halls.Halls.Add(hall);
            check(GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(sword,brick,3,x,ground,ownMap) && Complete(),"native owned hall interior accepts its complete planned sword tablet");
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(sword,brick,3,hall.Left,ground,ownMap) && !Main.tile[hall.Left,ground - 1].HasTile,"native owned hall exemption keeps entrance columns empty");
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(sword,brick,3,hall.Right - 1,ground,ownMap),"native hall exemption cannot spill outside its bounds");
            obstacle = Main.tile[hall.X + 20,ground - 2]; obstacle.RedWire = true;
            try { check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(sword,brick,3,hall.X + 20,ground,ownMap),"native owned hall exemption still refuses wiring"); } finally { obstacle.RedWire = false; }
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(sword,brick,3,hall.X + 20,ground,null),"native missing map refused even in recognized hall");
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type; tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
            halls.LoadWorldData(original);
        }
    }
}
