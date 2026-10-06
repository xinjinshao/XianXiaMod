namespace Microsoft.Xna.Framework {public struct Vector2 {}}
namespace Terraria.DataStructures {public struct FishingAttempt {public bool inLava,inHoney,crate,rare,veryrare,legendary;}}
namespace Terraria {
public struct AdvancedPopupRequest {}
public class Player {public bool active=true,dead;public HashSet<Type> Biomes=new();public bool InModBiome<T>()=>Biomes.Contains(typeof(T));}
public class NPC {public static bool downedPlantBoss,downedGolemBoss,downedMoonlord;}
public class RandomStub {public bool Result=true;public int Calls;public bool NextBool(){Calls++;return Result;}}
public static class Main {public static bool hardMode;public static RandomStub rand=new();}
public class Item {public const int CommonMaxStack=9999;public int ResearchUnlockCount,width,height,maxStack,value,rare;public bool consumable;public static int buyPrice(int silver)=>silver*100;}
}
namespace Terraria.ID {public static class ItemID {public const int WoodenCrate=2334;public static class Sets {public static bool[] IsFishingCrate=new bool[5000],IsFishingCrateHardmode=new bool[5000];}}public static class ItemRarityID {public const int Green=2,LightRed=4;}}
namespace Terraria.GameContent.ItemDropRules {public record Rule(int Item,int Denominator,int Min,int Max);public static class ItemDropRule {public static Rule Common(int item,int chance=1,int min=1,int max=1)=>new(item,chance,min,max);}}
namespace Terraria.ModLoader {
public class ModPlayer {public Terraria.Player Player=new();public virtual void CatchFish(Terraria.DataStructures.FishingAttempt a,ref int item,ref int npc,ref Terraria.AdvancedPopupRequest sonar,ref Microsoft.Xna.Framework.Vector2 pos){}}
public class ModItem {public Terraria.Item Item=new();public int Type=>ModContent.Id(GetType());public virtual string Texture=>"";public virtual void SetStaticDefaults(){}public virtual void SetDefaults(){}public virtual bool CanRightClick()=>false;public virtual void ModifyItemLoot(ItemLoot l){}}
public class ItemLoot {public List<Terraria.GameContent.ItemDropRules.Rule> Rules=new();public void Add(Terraria.GameContent.ItemDropRules.Rule r)=>Rules.Add(r);}
public static class ModContent {private static Dictionary<Type,int> ids=new();public static int Id(Type t){if(!ids.ContainsKey(t))ids[t]=100+ids.Count;return ids[t];}public static int ItemType<T>()=>Id(typeof(T));}
}
namespace XianXia.Content.Biomes {public class ShallowSpiritVeinsBiome{}public class GreenwoodHerbGardenBiome{}public class SunkenFurnaceVeinBiome{}public class StarAbyssRiftBiome{}public class ThunderMarshCloudsBiome{}public class TenThousandSectsRuinsBiome{}public class FallenHeavenPalaceBiome{}public class MoonboneAbyssBiome{}}
namespace XianXia.Content.Items.Materials {public class LowGradeSpiritStone{}public class SpiritGel{}public class GreenwoodRoot{}public class FurnaceSlagIron{}public class StarAbyssMembrane{}public class SectTrialToken{}public class HeavenTabletRubbing{}}
namespace XianXia.Content.Items.HandGenerated {public class ThunderPatternFeather{}public class ColdMoonDust{}}
