using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Systems;
int count=0;void Check(bool ok,string text){count++;if(!ok)throw new Exception(text);}
var system=new CalamityRecipeSystem();
void Reset(bool enabled=true,bool loaded=true){Recipe.Registered.Clear();ModContent.Config.EnableSoftCompatibilityHooks=enabled;ModLoader.Calls=0;system.Mod.Logger.Warnings.Clear();ModLoader.Calamity=loaded?new Mod():null;}
void Populate(){int id=2001;foreach(string name in new[]{"EssenceofEleum","EssenceofHavoc","EssenceofSunlight"})ModLoader.Calamity.Items[name]=new ModItem{Type=id++};}
Reset(false);Populate();system.AddRecipes();Check(Recipe.Registered.Count==0&&CalamityRecipeSystem.RegisteredRecipeCount==0&&ModLoader.Calls==0,"Disabled compatibility neither looks up dependency nor registers recipes");
Reset(loaded:false);system.AddRecipes();Check(Recipe.Registered.Count==0&&CalamityRecipeSystem.RegisteredRecipeCount==0,"Missing Calamity leaves standalone recipes untouched");
Reset();Populate();Main.hardMode=false;system.AddRecipes();Check(Recipe.Registered.Count==3&&CalamityRecipeSystem.RegisteredRecipeCount==3,"All three supported materials register actual recipe builders");
for(int i=0;i<3;i++) {
 var recipe=Recipe.Registered[i];Check(recipe.Output==1001&&recipe.Stack==8&&recipe.Ingredients.SequenceEqual(new[]{2001+i})&&recipe.Tiles.SequenceEqual(new[]{1002}),"Each essence converts to eight stones at the alchemy cauldron with one exact ingredient");
 Check(recipe.Conditions.Count==1&&!recipe.Conditions[0].IsMet(),"Recipes remain unavailable before Hardmode even with imported essence");
 Main.hardMode=true;Check(recipe.Conditions[0].IsMet(),"Recipe condition object is retained for current-world progression evaluation");Main.hardMode=false;
}
foreach(string missing in new[]{"EssenceofEleum","EssenceofHavoc","EssenceofSunlight"}) {
 Reset();Populate();ModLoader.Calamity.Items.Remove(missing);system.AddRecipes();Check(Recipe.Registered.Count==2&&CalamityRecipeSystem.RegisteredRecipeCount==2,"Missing optional item only skips its own recipe");
 Check(system.Mod.Logger.Warnings.Count==1&&system.Mod.Logger.Warnings[0].Contains(missing),"Skipped item is identified without crashing load");
}
Reset();system.AddRecipes();Check(Recipe.Registered.Count==0&&system.Mod.Logger.Warnings.Count==3,"Unsupported dependency version has no invalid ingredient recipes");
system.Unload();Check(CalamityRecipeSystem.RegisteredRecipeCount==0,"Unload clears recipe registration status");
Console.WriteLine($"Calamity recipe source integration passed: {count} assertions; optional mod registry and recipe engine boundaries mocked.");
