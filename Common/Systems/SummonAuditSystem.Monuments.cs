using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditMasterMonuments(Action<bool,string> check)
    {
        string[] names = { "SpiritVeinWyrm","GardenWarden","BlackFurnaceIronGolem","TribulationCloudAvatar","ThunderMarshJiao","AbyssalStarWomb","FormlessSwordSoul","MoonboneImmortal","GreenwoodMedicineKingEcho","HeavenTabletGuardian","BrokenHeavenInspector","OldHeavenDaoCore" };
        int x = Main.maxTilesX / 2, y = 12, mode = Main.netMode;
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,byte Liquid,SlopeType Slope,bool Half,bool Actuated)>();
        var existing = Main.item.Where(i => i.active).ToHashSet();
        for (int dx = -3; dx <= 4; dx++) for (int dy = -5; dy <= 3; dy++) {
            Tile t = Main.tile[x + dx,y + dy]; saved.Add((x + dx,y + dy,t.HasTile,t.TileType,t.TileFrameX,t.TileFrameY,t.LiquidAmount,t.Slope,t.IsHalfBlock,t.IsActuated));
        }
        void Clear() {
            foreach (var cell in saved) { Tile t = Main.tile[cell.X,cell.Y]; t.HasTile = false; t.LiquidAmount = 0; t.TileFrameX = t.TileFrameY = 0; t.Slope = SlopeType.Solid; t.IsHalfBlock = false; t.IsActuated = false; }
        }
        bool Complete(int type) => Enumerable.Range(0,2).All(dx => Enumerable.Range(0,3).All(dy => {
            Tile t = Main.tile[x + dx,y - 2 + dy]; return t.HasTile && t.TileType == type && t.TileFrameX == dx * 16 && t.TileFrameY == dy * 16;
        }));
        try {
            Main.netMode = NetmodeID.SinglePlayer;
            var mod = ModContent.GetInstance<global::XianXia.XianXia>();
            foreach (string name in names) {
                int item = mod.Find<ModItem>(name + "Monument").Type, type = mod.Find<ModTile>(name + "MonumentTile").Type;
                var data = TileObjectData.GetTileData(type,0); var prototype = new Item(item);
                check(prototype.createTile == type && prototype.master && prototype.consumable && data.Width == 2 && data.Height == 3,"native master monument item targets registered two by three object: " + name);
                Clear(); WorldGen.PlaceObject(x,y,type);
                check(!Complete(type) && !Main.tile[x,y].HasTile,"native master monument refuses placement without foundation: " + name);
                for (int cell = 0; cell < 6; cell++) {
                    Clear();
                    for (int dx = 0; dx < 2; dx++) { Tile ground = Main.tile[x + dx,y + 1]; ground.HasTile = true; ground.TileType = TileID.Stone; }
                    WorldGen.PlaceObject(x,y,type);
                    check(Complete(type),"native master monument places all six cells with correct frames: " + name + "/" + cell);
                    WorldGen.KillTile(x + cell % 2,y - 2 + cell / 2);
                    check(Enumerable.Range(0,2).All(dx => Enumerable.Range(0,3).All(dy => !Main.tile[x + dx,y - 2 + dy].HasTile)),"native destroying each monument cell removes whole object: " + name + "/" + cell);
                    var drops = Main.item.Where(i => i.active && !existing.Contains(i)).ToArray();
                    check(drops.Length == 1 && drops[0].type == item && drops[0].stack == 1,"native monument destruction returns one matching item: " + name + "/" + cell);
                    WorldGen.KillTile(x + cell % 2,y - 2 + cell / 2);
                    check(Main.item.Count(i => i.active && !existing.Contains(i)) == 1,"native repeated monument destruction does not duplicate reward: " + name + "/" + cell);
                    foreach (var drop in drops) drop.active = false;
                }
            }
        } finally {
            foreach (var cell in saved) { Tile t = Main.tile[cell.X,cell.Y]; t.HasTile = cell.Has; t.TileType = cell.Type; t.TileFrameX = cell.FX; t.TileFrameY = cell.FY; t.LiquidAmount = cell.Liquid; t.Slope = cell.Slope; t.IsHalfBlock = cell.Half; t.IsActuated = cell.Actuated; }
            foreach (var drop in Main.item.Where(i => i.active && !existing.Contains(i))) drop.active = false;
            Main.netMode = mode;
        }
        check(saved.All(c => { Tile t = Main.tile[c.X,c.Y]; return t.HasTile == c.Has && t.TileType == c.Type && t.TileFrameX == c.FX && t.TileFrameY == c.FY && t.LiquidAmount == c.Liquid && t.Slope == c.Slope && t.IsHalfBlock == c.Half && t.IsActuated == c.Actuated; }),"native monument fixture restores all patched tile fields");
    }
}
