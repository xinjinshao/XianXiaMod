using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditBossBagInventory(Action<bool,string> check)
    {
        var specs = new[] {
            ("SpiritVeinWyrm","VeinCoilCharm","LowGradeSpiritStone",12,18,"SpiritGel",20,35),
            ("GardenWarden","GardenLifeCharm","GreenwoodRoot",24,44,"LowGradeSpiritStone",8,16),
            ("BlackFurnaceIronGolem","FurnaceHeatGuard","FurnaceSlagIron",16,28,"LowGradeSpiritStone",8,16),
            ("TribulationCloudAvatar","TribulationConductor","TribulationCloudDew",24,44,"LowGradeSpiritStone",8,16),
            ("ThunderMarshJiao","JiaoStormStride","TribulationCloudDew",16,28,"ThunderPatternFeather",8,16),
            ("AbyssalStarWomb","AbyssSpiritLens","StarEclipseCrystal",24,44,"LowGradeSpiritStone",8,16),
            ("FormlessSwordSoul","FormlessSwordEmblem","SectTrialToken",16,28,"LowGradeSpiritStone",8,16),
            ("MoonboneImmortal","MoonboneSpiritBond","Moonbone",16,28,"DaoSeveringDust",8,16),
            ("GreenwoodMedicineKingEcho","MedicineKingLifeSeal","GreenwoodRoot",16,28,"SpringReturnPill",8,16),
            ("HeavenTabletGuardian","HeavenTabletBulwark","HeavenDaoFragment",24,44,"LowGradeSpiritStone",8,16),
            ("BrokenHeavenInspector","InspectorQuiverSeal","HeavenDaoFragment",24,44,"LowGradeSpiritStone",8,16),
            ("OldHeavenDaoCore","OldHeavenResonator","DaoSeveringDust",24,44,"LowGradeSpiritStone",8,16),
        };
        Player previous=Main.player[0]; int mode=Main.netMode,local=Main.myPlayer,difficulty=Main.GameMode,split=Main.stackSplit;
        bool right=Main.mouseRight,release=Main.mouseRightRelease;Item mouse=Main.mouseItem;var random=Main.rand;
        var existing=Main.item.Where(i => i.active).ToHashSet();int[] recipes=(int[])Main.availableRecipe.Clone();int recipeCount=Main.numAvailableRecipes,focus=Main.focusRecipe;
        var mod=ModContent.GetInstance<global::XianXia.XianXia>();int Find(string name)=>mod.Find<ModItem>(name).Type;
        try {
            Main.netMode=NetmodeID.SinglePlayer;Main.myPlayer=0;Main.rand=new Terraria.Utilities.UnifiedRandom(316);
            var actor=new Player { whoAmI=0,active=true,statLife=100 };actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX * 8,100);Main.player[0]=actor;
            foreach(int gameMode in new[]{0,1,2}) {
                Main.GameMode=gameMode;
                foreach(var spec in specs) foreach(var scenario in new[]{(Stack:2,Full:false),(Stack:1,Full:false),(Stack:2,Full:true)}) {
                    int type=Find(spec.Item1+"Bag"),expert=Find(spec.Item2);
                    for(int i=0;i<actor.inventory.Length;i++)actor.inventory[i]=scenario.Full?new Item(ItemID.StoneBlock,Item.CommonMaxStack):new Item();
                    actor.inventory[0]=new Item(type,scenario.Stack);Main.mouseItem=new Item();Main.mouseRight=Main.mouseRightRelease=true;Main.stackSplit=0;
                    ItemSlot.RightClick(actor.inventory,ItemSlot.Context.InventoryItem,0);
                    check(scenario.Stack==1?actor.inventory[0].IsAir:actor.inventory[0].type==type&&actor.inventory[0].stack==1,"native inventory boss bag right click consumes exactly one and clears final item: "+spec.Item1+"/"+gameMode+"/"+scenario);
                    Item[] drops=Main.item.Where(i => i.active&&!existing.Contains(i)).ToArray();int Count(int id)=>drops.Where(i => i.type==id).Sum(i => i.stack);
                    check(Count(expert)==1,"native inventory boss bag emits one expert accessory even with full inventory: "+spec.Item1+"/"+gameMode+"/"+scenario);
                    check(Count(Find(spec.Item3))>=spec.Item4&&Count(Find(spec.Item3))<=spec.Item5&&Count(Find(spec.Item6))>=spec.Item7&&Count(Find(spec.Item6))<=spec.Item8,"native inventory boss bag preserves independent core and secondary material ranges: "+spec.Item1+"/"+gameMode+"/"+scenario);
                    if(spec.Item1=="SpiritVeinWyrm")check(Count(Find("LowGradeSpiritCore"))==1&&Count(Find("SpiritVeinScale")) is >=12 and <=18,"native inventory wyrm bag retains guaranteed spirit core and scales");
                    if(spec.Item1=="OldHeavenDaoCore")check(Count(Find("RouteMaterial"))==1&&Count(Find("SilentTabletDecoration"))==1,"native inventory final boss bag retains route and decoration rewards");
                    var before=drops.Select(i => (i.type,i.stack)).OrderBy(i => i.type).ThenBy(i => i.stack).ToArray();
                    ItemSlot.RightClick(actor.inventory,ItemSlot.Context.InventoryItem,0);
                    var after=Main.item.Where(i => i.active&&!existing.Contains(i)).Select(i => (i.type,i.stack)).OrderBy(i => i.type).ThenBy(i => i.stack).ToArray();
                    check((scenario.Stack==1?actor.inventory[0].IsAir:actor.inventory[0].stack==1)&&before.SequenceEqual(after),"native repeated same-frame inventory boss bag right click cannot duplicate consumption or any loot: "+spec.Item1+"/"+gameMode+"/"+scenario);
                    foreach(var item in Main.item.Where(i => i.active&&!existing.Contains(i)))item.active=false;
                }
            }
        } finally {
            foreach(var item in Main.item.Where(i => i.active&&!existing.Contains(i)))item.active=false;
            Main.player[0]=previous;Main.netMode=mode;Main.myPlayer=local;Main.GameMode=difficulty;Main.rand=random;
            Main.stackSplit=split;Main.mouseRight=right;Main.mouseRightRelease=release;Main.mouseItem=mouse;
            recipes.CopyTo(Main.availableRecipe,0);Main.numAvailableRecipes=recipeCount;Main.focusRecipe=focus;
        }
        check(ReferenceEquals(Main.player[0],previous)&&ReferenceEquals(Main.mouseItem,mouse)&&Main.GameMode==difficulty&&Main.availableRecipe.SequenceEqual(recipes)&&Main.item.Where(i => i.active).ToHashSet().SetEquals(existing),"native boss bag inventory audit restores player cursor difficulty recipe and item state");
    }
}
