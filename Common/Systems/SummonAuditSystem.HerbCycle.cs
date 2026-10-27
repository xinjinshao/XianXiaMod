using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Tiles;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Materials;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditHerbCycle(Action<bool,string> check)
    {
        int x = Main.maxTilesX / 2,y = 20,mode = Main.netMode,local = Main.myPlayer,tx = Player.tileTargetX,ty = Player.tileTargetY,mx = Main.mouseX,my = Main.mouseY;
        var previous = Main.player[0]; var items = (Item[])Main.item.Clone(); bool text = Main.showItemText,menu = Main.gameMenu,inventory = Main.playerInventory;
        var screen = Main.screenPosition; var mouse = Main.mouseItem;
        using var audio = new MonoMod.RuntimeDetour.Hook(typeof(Player).GetMethod("ItemCheck_PlayInstruments",BindingFlags.Instance | BindingFlags.NonPublic)!,
            (Action<Action<Player,Item>,Player,Item>)((original,actor,item) => { }));
        var cells = new List<(int X,int Y,AuditTileSnapshot Saved)>();
        for (int dx = -3; dx <= 3; dx++) for (int dy = -3; dy <= 3; dy++) cells.Add((x+dx,y+dy,AuditTileSnapshot.Capture(Main.tile[x+dx,y+dy])));
        var grab = typeof(Player).GetMethod("GrabItems",BindingFlags.Instance | BindingFlags.NonPublic)!;

        try {
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0; Main.showItemText = Main.gameMenu = Main.playerInventory = false; Main.mouseItem = new Item();
            for (int i = 0; i < Main.item.Length; i++) Main.item[i] = new Item { whoAmI = i };
            foreach (var cell in cells) { Tile tile = Main.tile[cell.X,cell.Y]; tile.ClearEverything(); }
            Tile ground = Main.tile[x,y+1]; ground.HasTile = true; ground.TileType = TileID.Dirt;
            var actor = new Player { active = true,whoAmI = 0,selectedItem = 0,statLife = 100 }; Main.player[0] = actor; actor.Center = new Vector2(x*16+8,y*16-40);
            int seed = ModContent.ItemType<SpiritHerbSeeds>(),root = ModContent.ItemType<GreenwoodRoot>(),cropType = ModContent.TileType<CultivatedSpiritHerbTile>();
            actor.inventory[0] = new Item(seed,2);
            Main.screenPosition = actor.Center - new Vector2(400,300); Main.mouseX = (int)(x*16+8-Main.screenPosition.X); Main.mouseY = (int)(y*16+8-Main.screenPosition.Y);
            void UseSeed() {
                Player.tileTargetX = x; Player.tileTargetY = y; actor.itemAnimation = actor.itemTime = 0; actor.releaseUseItem = true; actor.controlUseItem = true;
                actor.ItemCheck(); actor.controlUseItem = false;
            }
            Tile crop = Main.tile[x,y]; crop.LiquidAmount = 1; UseSeed();
            check(!crop.HasTile && actor.inventory[0].stack == 2,"native seed item use on wet cell refuses planting without consuming seed");
            crop.LiquidAmount = 0; ground.TileType = TileID.Stone; UseSeed();
            check(!crop.HasTile && actor.inventory[0].stack == 2,"native seed item use on unsupported ground refuses planting without consuming seed");
            ground.TileType = TileID.Dirt; UseSeed();
            check(crop.HasTile && crop.TileType == cropType && crop.TileFrameX == 0 && actor.inventory[0].stack == 1,
                "native Player.ItemCheck places real herb seedling and consumes exactly one held seed: placed="+crop.HasTile+" type="+crop.TileType+" stack="+actor.inventory[0].stack+" time="+actor.itemTime);
            UseSeed(); check(actor.inventory[0].stack == 1 && crop.TileFrameX == 0,"native seed use on existing herb does not consume another seed or reset growth");
            var herb = ModContent.GetInstance<CultivatedSpiritHerbTile>(); int updates = 0;
            while (crop.TileFrameX != 36 && updates++ < 4096) herb.RandomUpdate(x,y);
            check(crop.TileFrameX == 36,"native planted seed grows to maturity through actual random updates");
            WorldGen.KillTile(x,y); var drops = Main.item.Take(Main.maxItems).Where(item=>item.active).ToArray();
            check(!crop.HasTile && drops.Where(item=>item.type==root).Sum(item=>item.stack)==1
                && drops.Where(item=>item.type==seed).Sum(item=>item.stack)>=1 && drops.Where(item=>item.type==seed).Sum(item=>item.stack)<=3,
                "native mature planted herb harvest creates root and replantable seeds in world");
            int seeds = drops.Where(item=>item.type==seed).Sum(item=>item.stack);
            foreach (var drop in drops) { int slot = Array.FindIndex(Main.item,item=>ReferenceEquals(item,drop)); drop.noGrabDelay = 0; drop.FindOwner(slot); }
            actor.Center = drops[0].Center; grab.Invoke(actor,new object[] { 0 });
            check(Main.item.Take(Main.maxItems).All(item=>!item.active) && actor.inventory.Take(58).Where(item=>item.type==seed).Sum(item=>item.stack)==1+seeds
                && actor.inventory.Take(58).Where(item=>item.type==root).Sum(item=>item.stack)==1,"native harvested herbs and seeds are picked up through owner assignment and inventory hooks");
            actor.Center = new Vector2(x*16+8,y*16-40); UseSeed();
            check(crop.HasTile && crop.TileType==cropType && crop.TileFrameX==0 && actor.inventory.Take(58).Where(item=>item.type==seed).Sum(item=>item.stack)==seeds,
                "native harvested seed inventory successfully replants next seedling for exactly one seed");
        } finally {
            foreach (var cell in cells) cell.Saved.Restore(Main.tile[cell.X,cell.Y]); items.CopyTo(Main.item,0); Main.player[0]=previous;
            Main.netMode=mode; Main.myPlayer=local; Main.showItemText=text; Main.gameMenu=menu; Main.playerInventory=inventory; Main.screenPosition=screen; Main.mouseX=mx; Main.mouseY=my; Main.mouseItem=mouse; Player.tileTargetX=tx; Player.tileTargetY=ty;
        }
        check(cells.All(cell=>cell.Saved.Matches(Main.tile[cell.X,cell.Y]))&&Main.item.SequenceEqual(items),"native herb cycle restores complete terrain and world item pool");
    }
}
