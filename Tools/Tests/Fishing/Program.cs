using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Biomes;
using XianXia.Content.Items.Fishing;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
int count=0;void Check(bool ok,string msg){count++;if(!ok)throw new Exception(msg);}
var entries=new (CultivationFishingCrate Crate,Type Biome,int Material)[]{
 (new SpiritVeinFishingCrate(),typeof(ShallowSpiritVeinsBiome),ModContent.ItemType<SpiritGel>()),
 (new GreenwoodFishingCrate(),typeof(GreenwoodHerbGardenBiome),ModContent.ItemType<GreenwoodRoot>()),
 (new FurnaceFishingCrate(),typeof(SunkenFurnaceVeinBiome),ModContent.ItemType<FurnaceSlagIron>()),
 (new StarAbyssFishingCrate(),typeof(StarAbyssRiftBiome),ModContent.ItemType<StarAbyssMembrane>()),
 (new ThunderFishingCrate(),typeof(ThunderMarshCloudsBiome),ModContent.ItemType<ThunderPatternFeather>()),
 (new SectFishingCrate(),typeof(TenThousandSectsRuinsBiome),ModContent.ItemType<SectTrialToken>()),
 (new HeavenFishingCrate(),typeof(FallenHeavenPalaceBiome),ModContent.ItemType<HeavenTabletRubbing>()),
 (new MoonboneFishingCrate(),typeof(MoonboneAbyssBiome),ModContent.ItemType<ColdMoonDust>())};
var hook=new CultivationFishingPlayer();var normal=new FishingAttempt{crate=true,rare=true};
int Catch(FishingAttempt attempt,int initial=50,int npc=0){var sonar=new AdvancedPopupRequest();var pos=new Vector2();hook.CatchFish(attempt,ref initial,ref npc,ref sonar,ref pos);Check(npc==0||initial==50,"Enemy catch must not change");return initial;}
for(int flags=0;flags<16;flags++) {
 Main.hardMode=(flags&1)!=0;NPC.downedPlantBoss=(flags&2)!=0;NPC.downedGolemBoss=(flags&4)!=0;NPC.downedMoonlord=(flags&8)!=0;
 foreach(var entry in entries) {
  int tier=entry.Crate.ProgressionTier;
  bool expected=(tier<1||Main.hardMode)&&(tier<2||NPC.downedPlantBoss)&&(tier<3||NPC.downedGolemBoss)&&(tier<4||NPC.downedMoonlord);
  Check(entry.Crate.CanRightClick()==expected,"World transfer opening gate");hook.Player.Biomes.Clear();hook.Player.Biomes.Add(entry.Biome);
  Check(Catch(normal)==(expected?entry.Crate.Type:50),"Biome and current world catch gate");
 }
}
Main.hardMode=NPC.downedPlantBoss=NPC.downedGolemBoss=NPC.downedMoonlord=true;
foreach(var entry in entries){entry.Crate.SetStaticDefaults();entry.Crate.SetDefaults();var loot=new ItemLoot();entry.Crate.ModifyItemLoot(loot);
 Check(ItemID.Sets.IsFishingCrate[entry.Crate.Type]&&ItemID.Sets.IsFishingCrateHardmode[entry.Crate.Type]==(entry.Crate.ProgressionTier>0),"Native crate registration");
 Check(entry.Crate.Item.ResearchUnlockCount==10&&entry.Crate.Item.consumable&&entry.Crate.Item.maxStack==Item.CommonMaxStack,"Native item opening metadata");
 Check(loot.Rules.Count==2&&loot.Rules[0]==new Terraria.GameContent.ItemDropRules.Rule(ModContent.ItemType<LowGradeSpiritStone>(),1,4,8)&&loot.Rules[1]==new Terraria.GameContent.ItemDropRules.Rule(entry.Material,1,2,4),"Exact two native loot rules, no boss-only rewards");
}
hook.Player.Biomes.Clear();Check(Catch(normal)==50,"Outside ecology preserves vanilla catch");
foreach(var entry in entries)hook.Player.Biomes.Add(entry.Biome);Check(Catch(normal)==ModContent.ItemType<MoonboneFishingCrate>(),"Highest unlocked overlapping biome wins");
NPC.downedMoonlord=false;Check(Catch(normal)==ModContent.ItemType<HeavenFishingCrate>(),"Locked overlapping biome falls back");
Main.rand.Result=false;Check(Catch(normal)==50,"Half-roll failure keeps original crate");Main.rand.Result=true;
foreach(var attempt in new[]{new FishingAttempt{rare=true},new FishingAttempt{crate=true},new FishingAttempt{crate=true,rare=true,inLava=true},new FishingAttempt{crate=true,rare=true,inHoney=true},new FishingAttempt{crate=true,rare=true,veryrare=true},new FishingAttempt{crate=true,rare=true,legendary=true}}){int calls=Main.rand.Calls;Check(Catch(attempt)==50&&Main.rand.Calls==calls,"Excluded catches untouched and do not roll");}
Check(Catch(normal,-1)==-1&&Catch(normal,0)==0&&Catch(normal,50,12)==50,"Failed or enemy catches preserved");
hook.Player.dead=true;Check(Catch(normal)==50,"Dead player does not fish");hook.Player.dead=false;hook.Player.active=false;Check(Catch(normal)==50,"Inactive player does not fish");
Check(!CultivationFishingRules.Allows(-1,true,true,true,true)&&!CultivationFishingRules.Allows(5,true,true,true,true),"Invalid tier rejected");
Console.WriteLine($"Fishing actual catch/crate source hooks passed: {count} assertions; biome scan, native fishing, crate consumption and loot engine mocked.");
