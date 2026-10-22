using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditCrateInventory(Action<bool,string> check)
    {
        Player previous=Main.player[0]; int mode=Main.netMode,local=Main.myPlayer,split=Main.stackSplit;
        bool right=Main.mouseRight,release=Main.mouseRightRelease; Item mouse=Main.mouseItem;
        bool hard=Main.hardMode,plant=NPC.downedPlantBoss,golem=NPC.downedGolemBoss,moon=NPC.downedMoonlord; var random=Main.rand;
        var existing=Main.item.Where(i => i.active).ToHashSet();
        int[] recipes=(int[])Main.availableRecipe.Clone(); int recipeCount=Main.numAvailableRecipes,focus=Main.focusRecipe;
        try {
            Main.netMode=NetmodeID.SinglePlayer;Main.myPlayer=0;Main.rand=new Terraria.Utilities.UnifiedRandom(315);
            Main.hardMode=NPC.downedPlantBoss=NPC.downedGolemBoss=NPC.downedMoonlord=true;
            var actor=new Player { whoAmI=0,active=true,statLife=100 };actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX * 8,100);Main.player[0]=actor;
            var mod=ModContent.GetInstance<global::XianXia.XianXia>();

            var specs=new[] { ("SpiritVeinFishingCrate",0,"SpiritGel"),("GreenwoodFishingCrate",0,"GreenwoodRoot"),("FurnaceFishingCrate",0,"FurnaceSlagIron"),("StarAbyssFishingCrate",1,"StarAbyssMembrane"),("ThunderFishingCrate",1,"ThunderPatternFeather"),("SectFishingCrate",2,"SectTrialToken"),("HeavenFishingCrate",3,"HeavenTabletRubbing"),("MoonboneFishingCrate",4,"ColdMoonDust") };
            int stone=mod.Find<ModItem>("LowGradeSpiritStone").Type;
            void Ready() { Main.mouseItem=new Item();Main.mouseRight=Main.mouseRightRelease=true;Main.stackSplit=0; }
            void ClearRewards() { foreach(var drop in Main.item.Where(i => i.active&&!existing.Contains(i)))drop.active=false; }
            foreach(var spec in specs) {
                int type=mod.Find<ModItem>(spec.Item1).Type, material=mod.Find<ModItem>(spec.Item3).Type;
                foreach(var scenario in new[] { (Stack:2,Full:false),(Stack:1,Full:false),(Stack:2,Full:true) }) {
                    Main.hardMode=NPC.downedPlantBoss=NPC.downedGolemBoss=NPC.downedMoonlord=true;
                    for(int i=0;i<actor.inventory.Length;i++)actor.inventory[i]=scenario.Full?new Item(ItemID.StoneBlock,Item.CommonMaxStack):new Item();
                    actor.inventory[0]=new Item(type,scenario.Stack);Ready();
                    ItemSlot.RightClick(actor.inventory,ItemSlot.Context.InventoryItem,0);
                    check(scenario.Stack==1?actor.inventory[0].IsAir:actor.inventory[0].type==type&&actor.inventory[0].stack==1,"native inventory right click consumes one crate and clears final item: "+spec.Item1+"/"+scenario);
                    Item[] drops=Main.item.Where(i => i.active&&!existing.Contains(i)).ToArray();
                    int Count(int id)=>drops.Where(i => i.type==id).Sum(i => i.stack);
                    check(drops.Length==2&&Count(stone) is >=4 and <=8&&Count(material) is >=2 and <=4,"native inventory crate opening releases exact intended rewards including full inventory: "+spec.Item1+"/"+scenario);
                    ItemSlot.RightClick(actor.inventory,ItemSlot.Context.InventoryItem,0);
                    check((scenario.Stack==1?actor.inventory[0].IsAir:actor.inventory[0].stack==1)&&Main.item.Count(i => i.active&&!existing.Contains(i))==2,"native held right click cannot bypass opening cooldown or duplicate crate reward: "+spec.Item1+"/"+scenario);
                    ClearRewards();
                }
                if(spec.Item2>0) {
                    Main.hardMode=NPC.downedPlantBoss=NPC.downedGolemBoss=NPC.downedMoonlord=false;
                    for(int i=0;i<actor.inventory.Length;i++)actor.inventory[i]=new Item();
                    actor.inventory[0]=new Item(type,2);Ready();
                    ItemSlot.RightClick(actor.inventory,ItemSlot.Context.InventoryItem,0);
                    int units=actor.inventory.Where(i => i.type==type).Sum(i => i.stack)+(Main.mouseItem.type==type?Main.mouseItem.stack:0);
                    check(units==2,"native locked crate inventory right click preserves all item units including cursor transfers: "+spec.Item1);
                    check(!Main.item.Any(i => i.active&&!existing.Contains(i)),"native locked crate inventory right click cannot release progression rewards: "+spec.Item1);
                }
            }

        } finally {
            foreach(var item in Main.item.Where(i => i.active&&!existing.Contains(i)))item.active=false;
            Main.player[0]=previous;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;Main.hardMode=hard;NPC.downedPlantBoss=plant;NPC.downedGolemBoss=golem;NPC.downedMoonlord=moon;
            Main.stackSplit=split;Main.mouseRight=right;Main.mouseRightRelease=release;Main.mouseItem=mouse;
            recipes.CopyTo(Main.availableRecipe,0);Main.numAvailableRecipes=recipeCount;Main.focusRecipe=focus;
        }
        check(ReferenceEquals(Main.player[0],previous)&&ReferenceEquals(Main.mouseItem,mouse)&&Main.stackSplit==split&&Main.mouseRight==right&&Main.mouseRightRelease==release&&Main.availableRecipe.SequenceEqual(recipes)&&Main.numAvailableRecipes==recipeCount&&Main.focusRecipe==focus,"native crate inventory audit restores player cursor click and recipe state");
    }
}
