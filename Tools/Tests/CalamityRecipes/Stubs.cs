namespace Terraria {
 public static class Main {public static bool hardMode;}
 public class Condition {public static readonly Condition Hardmode=new();public bool IsMet()=>Main.hardMode;}
 public class Recipe {
  public static List<Recipe> Registered=new(); public int Output,Stack; public List<int> Ingredients=new(),Tiles=new();public List<Condition> Conditions=new();
  public static Recipe Create(int output,int stack)=>new(){Output=output,Stack=stack};
  public Recipe AddIngredient(int type){Ingredients.Add(type);return this;}public Recipe AddTile(int tile){Tiles.Add(tile);return this;}
  public Recipe AddCondition(Condition condition){Conditions.Add(condition);return this;}public void Register()=>Registered.Add(this);
 }
}
namespace Terraria.ModLoader {
 public class Logger {public List<string> Warnings=new(),Messages=new();public void Warn(string text)=>Warnings.Add(text);public void Info(string text)=>Messages.Add(text);}
 public class Mod {public Logger Logger=new();public Dictionary<string,ModItem> Items=new();public bool TryFind<T>(string name,out T item) where T:ModItem {bool found=Items.TryGetValue(name,out var value);item=found?(T)value:null;return found;}}
 public class ModItem {public int Type;}
 public class ModSystem {public Mod Mod=new();public virtual void AddRecipes(){}public virtual void Unload(){}}
 public static class ModLoader {public static Mod Calamity;public static int Calls;public static bool TryGetMod(string name,out Mod mod){Calls++;mod=name=="CalamityMod"?Calamity:null;return mod!=null;}}
 public static class ModContent {public static XianXia.Common.Systems.XianXiaConfig Config=new();public static T GetInstance<T>()=>(T)(object)Config;public static int ItemType<T>()=>1001;public static int TileType<T>()=>1002;}
}
namespace XianXia.Common.Systems {public class XianXiaConfig {public bool EnableSoftCompatibilityHooks=true;}}
namespace XianXia.Content.Items.Materials {public class LowGradeSpiritStone {}}
namespace XianXia.Content.Tiles.Stations {public class AlchemyCauldronTile {}}
