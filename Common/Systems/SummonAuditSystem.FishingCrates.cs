using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditFishingCrates(Action<bool,string> check)
    {
        var specs=new[] { ("SpiritVeinFishingCrate",0,"SpiritGel"),("GreenwoodFishingCrate",0,"GreenwoodRoot"),("FurnaceFishingCrate",0,"FurnaceSlagIron"),("StarAbyssFishingCrate",1,"StarAbyssMembrane"),("ThunderFishingCrate",1,"ThunderPatternFeather"),("SectFishingCrate",2,"SectTrialToken"),("HeavenFishingCrate",3,"HeavenTabletRubbing"),("MoonboneFishingCrate",4,"ColdMoonDust") };
        var mod=ModContent.GetInstance<global::XianXia.XianXia>();
        Player previous=Main.player[0]; int mode=Main.netMode,local=Main.myPlayer;
        bool hard=Main.hardMode,plant=NPC.downedPlantBoss,golem=NPC.downedGolemBoss,moon=NPC.downedMoonlord; var random=Main.rand;
        var existing=Main.item.Where(i => i.active).ToHashSet();
        try {
            Main.netMode=NetmodeID.SinglePlayer;Main.myPlayer=0;Main.rand=new Terraria.Utilities.UnifiedRandom(314);
            var actor=new Player { whoAmI=0,active=true,statLife=100 };actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX * 8,100);Main.player[0]=actor;
            int stone=mod.Find<ModItem>("LowGradeSpiritStone").Type;
            foreach(var spec in specs) {
                int type=mod.Find<ModItem>(spec.Item1).Type,material=mod.Find<ModItem>(spec.Item3).Type;var original=new Item(type);
                check(ItemID.Sets.IsFishingCrate[type] && ItemID.Sets.IsFishingCrateHardmode[type]==(spec.Item2>0) && original.consumable,"native registered fishing crate has actual crate tier and consumable flags: "+spec.Item1);
                Item[] variants={ original,original.Clone(),ItemIO.Load(ItemIO.Save(original)) };
                for(int flags=0;flags<16;flags++) {
                    Main.hardMode=(flags&1)!=0;NPC.downedPlantBoss=(flags&2)!=0;NPC.downedGolemBoss=(flags&4)!=0;NPC.downedMoonlord=(flags&8)!=0;
                    bool expected=(spec.Item2==0||Main.hardMode)&&(spec.Item2<2||NPC.downedPlantBoss)&&(spec.Item2<3||NPC.downedGolemBoss)&&(spec.Item2<4||NPC.downedMoonlord);
                    check(variants.All(i => ItemLoader.CanRightClick(i)==expected),"native right-click gate rechecks current world progression for original clone and saved crate: "+spec.Item1+"/"+flags);
                }
                Main.hardMode=NPC.downedPlantBoss=NPC.downedGolemBoss=NPC.downedMoonlord=true;
                for(int sample=0;sample<8;sample++) {
                    actor.OpenFishingCrate(type);Item[] drops=Main.item.Where(i => i.active&&!existing.Contains(i)).ToArray();
                    int Count(int id)=>drops.Where(i => i.type==id).Sum(i => i.stack);
                    check(Count(stone) is >=4 and <=8,"native actual fishing crate opening yields advertised spirit stone range: "+spec.Item1);
                    check(Count(material) is >=2 and <=4,"native actual fishing crate opening yields independent expected biome material range: "+spec.Item1);
                    check(drops.Length==2&&drops.All(i => i.type==stone||i.type==material),"native actual fishing crate opening emits exactly two intended rewards without extra rare progression loot: "+spec.Item1);
                    foreach(var drop in drops)drop.active=false;
                }
            }
        } finally {
            foreach(var item in Main.item.Where(i => i.active&&!existing.Contains(i)))item.active=false;
            Main.player[0]=previous;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;Main.hardMode=hard;NPC.downedPlantBoss=plant;NPC.downedGolemBoss=golem;NPC.downedMoonlord=moon;
        }
        check(ReferenceEquals(Main.player[0],previous)&&Main.item.Where(i => i.active).ToHashSet().SetEquals(existing)&&Main.hardMode==hard&&NPC.downedPlantBoss==plant&&NPC.downedGolemBoss==golem&&NPC.downedMoonlord==moon,"native fishing crate audit restores player rewards randomness and world progression");
    }
}
