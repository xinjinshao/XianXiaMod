using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem {
    private static void AuditWorkshopPaths(Action<bool,string> check) {
        var room = new Rectangle(Main.maxTilesX / 2,60,FurnaceWorkshopSystem.Width,FurnaceWorkshopSystem.Height); int floor = room.Bottom - 4;
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>();
        for (int x = room.Left - 44; x < room.Right + 44; x++) for (int y = room.Top - 12; y < room.Bottom + 12; y++) { Tile t = Main.tile[x,y]; saved.Add((x,y,t.HasTile,t.TileType,t.TileFrameX,t.TileFrameY,t.Slope,t.IsHalfBlock)); }
        var paths = ModContent.GetInstance<FurnaceWorkshopPathSystem>(); var original = new TagCompound(); paths.SaveWorldData(original); int mode = Main.netMode; var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        try {
            check(saved.TrueForAll(c => Main.tile[c.X,c.Y].LiquidAmount == 0 && Main.tile[c.X,c.Y].WallType == 0 && !Main.tile[c.X,c.Y].HasActuator && !Main.tile[c.X,c.Y].RedWire && !Main.tile[c.X,c.Y].BlueWire && !Main.tile[c.X,c.Y].GreenWire && !Main.tile[c.X,c.Y].YellowWire),"native workshop path fixture has dry unoccupied sky fields");
            foreach(var c in saved) { Tile t = Main.tile[c.X,c.Y]; t.HasTile = true; t.TileType = TileID.Stone; t.IsHalfBlock = false; t.Slope = SlopeType.Solid; }
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true; var map = new StructureMap(); check(FurnaceWorkshopSystem.TryBuild(room,map,registerProtection:false),"native workshop path fixture builds actual room");
            check(!FurnaceWorkshopPathSystem.TryConnect(room,map,out _),"native workshop path refuses solid terrain without a real cave");
            for(int x = room.Right + 5; x < room.Right + 10; x++)for(int y = floor - 4; y < floor; y++){Tile t = Main.tile[x,y];t.HasTile=false;}
            check(!FurnaceWorkshopPathSystem.TryConnect(room,map,out _),"native workshop path refuses a tiny closed cavity");
            for(int x = room.Right + 10; x < room.Right + 22; x++)for(int y = floor - 10; y < floor; y++){Tile t = Main.tile[x,y];t.HasTile=false;}
            config.EnableWorldGeneration=false;check(!FurnaceWorkshopPathSystem.TryConnect(room,map,out _),"native disabled generation never excavates workshop path");config.EnableWorldGeneration=true;
            Main.netMode=NetmodeID.MultiplayerClient;check(!FurnaceWorkshopPathSystem.TryConnect(room,map,out _),"native client never excavates workshop path");Main.netMode=NetmodeID.Server;
            Tile obstacle=Main.tile[room.Right+2,floor-2];obstacle.RedWire=true;try{check(!FurnaceWorkshopPathSystem.TryConnect(room,map,out _),"native workshop path preserves wired route");}finally{obstacle.RedWire=false;}
            obstacle.LiquidAmount=1;try{check(!FurnaceWorkshopPathSystem.TryConnect(room,map,out _),"native workshop path refuses liquid route");}finally{obstacle.LiquidAmount=0;}
            var protectedMap=new StructureMap();protectedMap.AddProtectedStructure(new Rectangle(room.Right+2,floor-4,2,4),2);check(!FurnaceWorkshopPathSystem.TryConnect(room,protectedMap,out _),"native workshop path preserves unrelated structure reservation");
            check(FurnaceWorkshopPathSystem.TryConnect(room,map,out Rectangle path)&&path.Width>=5&&path.Width<=40&&path.Height==6,"native workshop path connects within bounded excavation");
            bool shape=true;for(int x=path.Left;x<path.Right;x++)for(int y=path.Top;y<path.Bottom;y++){Tile t=Main.tile[x,y];bool shell=y==path.Top||y==path.Bottom-1;shape&=t.HasTile==shell&&(!shell||(t.TileType==ModContent.TileType<global::XianXia.Content.Tiles.FurnaceSlagTile>()&&t.Slope==SlopeType.Solid&&!t.IsHalfBlock));}
            check(shape&&FurnaceWorkshopPathSystem.HasOpenExit(path,room),"native path has four-cell walk space and real cave remains connected outside tunnel");
            check(FurnaceWorkshopSystem.StationsIntact(room)&&!map.CanPlace(path,2),"native path preserves workstations and reserves itself");
            paths.Paths.Clear();paths.Paths.Add(path);var tag=new TagCompound();paths.SaveWorldData(tag);paths.ClearWorld();paths.LoadWorldData(tag);check(paths.Paths.Count==1&&paths.Paths[0]==path,"native workshop path saves and loads its bounds");
            Tile removed=Main.tile[path.X,path.Bottom-1];removed.HasTile=false;paths.LoadWorldData(tag);check(!removed.HasTile,"native path load never repairs removed floor");
        } finally {foreach(var c in saved){Tile t=Main.tile[c.X,c.Y];t.HasTile=c.Has;t.TileType=c.Type;t.TileFrameX=c.FX;t.TileFrameY=c.FY;t.Slope=c.Slope;t.IsHalfBlock=c.Half;}paths.LoadWorldData(original);Main.netMode=mode;config.EnableWorldGeneration=enabled;}
    }
}
