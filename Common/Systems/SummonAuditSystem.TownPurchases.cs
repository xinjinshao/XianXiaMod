using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditTownPurchases(Action<bool,string> check)
    {
        int npcSlot=Enumerable.Range(0,Main.maxNPCs).First(i=>!Main.npc[i].active);NPC oldNpc=Main.npc[npcSlot];Player oldPlayer=Main.player[0];
        int mode=Main.netMode,local=Main.myPlayer,npcShop=Main.npcShop,oldShop=Main.oldNPCShop,split=Main.stackSplit;
        bool left=Main.mouseLeft,release=Main.mouseLeftRelease;Item mouse=Main.mouseItem;var shops=Main.instance.shop;var sellback=Main.shopSellbackHelper;var random=Main.rand;string chat=Main.npcChatText;bool inventory=Main.playerInventory;
        var existing=Main.item.Where(i=>i.active).ToHashSet();
        int[] recipes=(int[])Main.availableRecipe.Clone();int recipeCount=Main.numAvailableRecipes,focus=Main.focusRecipe;
        long Coins(Player actor) => actor.inventory.Concat(actor.bank.item).Concat(actor.bank2.item).Concat(actor.bank3.item).Concat(actor.bank4.item).Sum(i=>i.type switch { ItemID.CopperCoin=>(long)i.stack,ItemID.SilverCoin=>100L*i.stack,ItemID.GoldCoin=>10000L*i.stack,ItemID.PlatinumCoin=>1000000L*i.stack,_=>0L });
        try {
            Main.netMode=NetmodeID.SinglePlayer;Main.myPlayer=0;Main.instance.shop=shops==null?new Chest[100]:(Chest[])shops.Clone();Main.shopSellbackHelper=new ItemShopSellbackHelper();

            Main.rand=new Terraria.Utilities.UnifiedRandom(319);
            var mod=ModContent.GetInstance<global::XianXia.XianXia>();
            var sellers=new[]{("HerbSectApprentice","AlchemyCauldron"),("WanderingArtificer","ArtifactForge"),("TribulationObserver","TribulationGauge"),("ArchiveScrollSpirit","SectLedger"),("FallenHeavenMessenger","HeavenDaoFragment")};
            foreach(var seller in sellers)foreach(var profile in new[]{(Adjustment:1.0,Discount:false),(Adjustment:0.75,Discount:false),(Adjustment:0.75,Discount:true)})foreach(int wallet in new[]{0,1,2,3}) {
                var npc=new NPC { whoAmI=npcSlot };npc.SetDefaults(mod.Find<ModNPC>(seller.Item1).Type);npc.active=true;npc.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,100);Main.npc[npcSlot]=npc;
                NPCShop shop=NPCShopDatabase.AllShops.OfType<NPCShop>().Single(s=>s.FullName=="XianXia/"+seller.Item1+"/Shop");
                var actor=new Player { whoAmI=0,active=true,statLife=100 };actor.Center=npc.Center;Main.player[0]=actor;actor.SetTalkNPC(npcSlot,false);Main.SetNPCShopIndex(1);
                actor.currentShoppingSettings=new ShoppingSettings { PriceAdjustment=profile.Adjustment };actor.discountEquipped=actor.discountAvailable=profile.Discount;Main.shopSellbackHelper=new ItemShopSellbackHelper();
                var rows=new List<Item>();shop.FillShop(rows,npc);var chest=new Chest();rows.CopyTo(chest.item,0);NPCLoader.ModifyActiveShop(npc,"Shop",chest.item);Main.instance.shop[1]=chest;
                int product=mod.Find<ModItem>(seller.Item2).Type,index=Array.FindIndex(chest.item,i=>i!=null&&i.type==product);Item item=chest.item[index];actor.GetItemExpectedPrice(item,out _,out long buying);
                check(buying>0&&buying<10000000,"native town purchase quote is positive and within funded wallet fixture: "+seller.Item1+"/"+profile);
                long value=wallet switch { 0=>0L,1=>buying-1,2=>buying,_=>10000000L };
                int[] denominations={ItemID.CopperCoin,ItemID.SilverCoin,ItemID.GoldCoin,ItemID.PlatinumCoin};
                for(int unit=0;unit<4;unit++) { int amount=(int)(unit==3?value:value%100);actor.inventory[50+unit]=amount>0?new Item(denominations[unit],amount):new Item();value/=100; }
                long before=Coins(actor);bool funded=wallet>=2;int stock=item.stack;
                Main.mouseItem=new Item();Main.mouseLeft=Main.mouseLeftRelease=true;Main.stackSplit=0;
                ItemSlot.LeftClick(chest.item,ItemSlot.Context.ShopItem,index);
                check(funded?Main.mouseItem.type==product&&Main.mouseItem.stack==1:Main.mouseItem.IsAir,"native town purchase click delivers one service item only with enough coins: "+seller.Item1+"/"+profile+"/"+wallet);
                check(Coins(actor)==before-(funded?buying:0),"native town purchase click applies exact quote at zero short exact and change boundaries: "+seller.Item1+"/"+profile+"/"+wallet);
                ItemSlot.LeftClick(chest.item,ItemSlot.Context.ShopItem,index);
                check((funded?Main.mouseItem.type==product&&Main.mouseItem.stack==1:Main.mouseItem.IsAir)&&Coins(actor)==before-(funded?buying:0)&&chest.item[index].type==product&&chest.item[index].stack==stock,"native immediate repeated town purchase click cannot double-spend or alter unlimited stock: "+seller.Item1+"/"+profile+"/"+wallet);
                check(Main.item.Where(i=>i.active).ToHashSet().SetEquals(existing),"native town purchase transfers to cursor without duplicate world drops");
            }

        } finally {
            Main.npc[npcSlot]=oldNpc;Main.player[0]=oldPlayer;Main.netMode=mode;Main.myPlayer=local;Main.SetNPCShopIndex(npcShop);Main.oldNPCShop=oldShop;Main.stackSplit=split;Main.mouseLeft=left;Main.mouseLeftRelease=release;Main.mouseItem=mouse;Main.instance.shop=shops;Main.shopSellbackHelper=sellback;Main.rand=random;Main.npcChatText=chat;Main.playerInventory=inventory;
            recipes.CopyTo(Main.availableRecipe,0);Main.numAvailableRecipes=recipeCount;Main.focusRecipe=focus;
        }
        check(ReferenceEquals(Main.npc[npcSlot],oldNpc)&&ReferenceEquals(Main.player[0],oldPlayer)&&ReferenceEquals(Main.mouseItem,mouse)&&ReferenceEquals(Main.instance.shop,shops)&&ReferenceEquals(Main.shopSellbackHelper,sellback)&&Main.availableRecipe.SequenceEqual(recipes),"native town purchase audit restores seller buyer cursor shop sellback and recipe context");
    }
}
