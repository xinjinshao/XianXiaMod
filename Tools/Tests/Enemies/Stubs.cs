namespace Microsoft.Xna.Framework {
public struct Vector2(float x,float y) { public float X=x,Y=y; public float ToRotation()=>MathF.Atan2(Y,X);public float LengthSquared()=>X*X+Y*Y;public static float Distance(Vector2 a,Vector2 b){var v=a-b;return MathF.Sqrt(v.X*v.X+v.Y*v.Y);} public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.X+b.X,a.Y+b.Y); public Vector2 RotatedBy(float a)=>new(X*MathF.Cos(a)-Y*MathF.Sin(a),X*MathF.Sin(a)+Y*MathF.Cos(a));public static Vector2 Zero=>new(0,0);public static Vector2 UnitX=>new(1,0);public static Vector2 UnitY=>new(0,1); public Vector2 SafeNormalize(Vector2 fallback){float length=MathF.Sqrt(X*X+Y*Y);return length>0?new(X/length,Y/length):fallback;} public static Vector2 operator -(Vector2 a,Vector2 b)=>new(a.X-b.X,a.Y-b.Y); public static Vector2 operator *(Vector2 a,float n)=>new(a.X*n,a.Y*n); }
}
namespace Terraria {
public static class Main {public static int maxTilesX=1000,maxTilesY=500;public static NPC[] npc=Array.Empty<NPC>();public static IEnumerable<NPC> ActiveNPCs=>npc.Where(n=>n.active);public static RandomStub rand=new();public static bool dedServ;public static IEnumerable<Player> ActivePlayers=>player.Where(p=>p.active);public static int netMode,maxPlayers=2;public static Player[] player=[new(),new()];public static bool hardMode;public static int[] npcFrameCount=new int[100];}
public class Player {public struct HurtInfo {}public Microsoft.Xna.Framework.Vector2 velocity;public int Buffs;public void AddBuff(int type,int time)=>Buffs++;public bool active=true,dead;public Microsoft.Xna.Framework.Vector2 Center;public bool InModBiome<T>()=>true;}
public class NPC {public bool active=true;public int whoAmI,type;public bool justHit;public struct HitInfo {public int HitDirection;} public int defDamage;public static bool downedPlantBoss,downedGolemBoss,downedMoonlord;public bool noGravity;public int life;public int width,height,lifeMax,damage,defense,aiStyle,target;public float value,knockBackResist;public object HitSound,DeathSound;public bool netUpdate;public float rotation;public float[] ai=new float[4];public float[] localAI=new float[4];public Microsoft.Xna.Framework.Vector2 velocity,Center,position;public object GetSource_FromAI()=>null;public void TargetClosest(bool face){target=255;for(int i=0;i<Main.maxPlayers;i++)if(Main.player[i].active&&!Main.player[i].dead){target=i;break;}}}
public class Projectile {public int owner;public static Microsoft.Xna.Framework.Vector2 LastPosition;public static int Spawns;public static int NewProjectile(object source,Microsoft.Xna.Framework.Vector2 position,Microsoft.Xna.Framework.Vector2 velocity,int type,int damage,float knockback){LastPosition=position;Spawns++;return 0;}}
}
namespace Terraria.ID {public static class NetmodeID {public const int MultiplayerClient=1,Server=2;}public static class NPCAIStyleID {public const int Fighter=3,Bat=14,Slime=1;}public static class NPCID {public const int Zombie=3,CaveBat=49,BlueSlime=1;}public static class SoundID {public static object NPCHit1=new(),NPCDeath1=new(),NPCDeath4=new();}}
namespace Terraria.ModLoader {public class ModNPC {public Terraria.NPC NPC=new();public int Type,AIType;public virtual void SetStaticDefaults(){}public virtual void SendExtraAI(System.IO.BinaryWriter writer){}public virtual void ReceiveExtraAI(System.IO.BinaryReader reader){}public virtual void SetDefaults(){}public virtual void OnHitByProjectile(Terraria.Projectile projectile,Terraria.NPC.HitInfo hit,int damage){}public virtual void HitEffect(Terraria.NPC.HitInfo hit){}public virtual void OnHitPlayer(Terraria.Player target,Terraria.Player.HurtInfo hit){}public virtual void AI(){}public virtual void PostAI(){}public virtual void SetBestiary(Terraria.GameContent.Bestiary.BestiaryDatabase database,Terraria.GameContent.Bestiary.BestiaryEntry entry){}public virtual float SpawnChance(NPCSpawnInfo info)=>0;public virtual void ModifyNPCLoot(Terraria.GameContent.ItemDropRules.NPCLoot loot){}public virtual void FindFrame(int height){}}public class NPCSpawnInfo {public Terraria.Player Player=new();}public static class ModContent {public static int ProjectileType<T>()=>1;public static int ItemType<T>()=>1;}}
namespace Terraria.GameContent.Bestiary {public class BestiaryDatabase {} public class BestiaryEntry {public List<object> Info=new();}public class FlavorTextBestiaryInfoElement(string text){public string Text=text;}}
namespace Terraria.GameContent.ItemDropRules {public class NPCLoot {public void Add(object rule){}}public static class ItemDropRule {public static object Common(int type,int chance,int min=1,int max=1)=>new();}}
namespace XianXia.Common.Animation {public static class NpcFrameAnimator {public const int EnemyFrameCount=4;public static void Animate(Terraria.NPC npc,int height,int count,int ticks){}}}
namespace XianXia.Common.Systems {public static class EnemySpawnRules {public static bool Allows(string name,bool hard,bool plant,bool golem,bool moon)=>true;}}
namespace XianXia.Content.Biomes {public class FallenHeavenPalaceBiome {}}
namespace XianXia.Content.Projectiles {public class BossSpiritBoltProjectile {}}
namespace XianXia.Content.Items.HandGenerated {public class BrokenHeavenJade {}}
namespace XianXia.Content.Items.Materials {public class HeavenDaoFragment {}}

namespace Microsoft.Xna.Framework {public static class MathHelper {public static float Lerp(float a,float b,float amount)=>a+(b-a)*amount;public static float ToRadians(float degrees)=>degrees*MathF.PI/180;public const float TwoPi=MathF.PI*2;}}
namespace Terraria {public static class Dust {public static int Calls;public static void NewDust(Microsoft.Xna.Framework.Vector2 p,int width,int height,int type,float vx,float vy,int alpha=0,object color=null,float scale=1)=>Calls++;}}
namespace Terraria.ID {public static class BuffID {public const int Poisoned=20,Slow=32,OnFire3=323;}public static class DustID {public const int Grass=1,Poisoned=2,GoldCoin=3,Electric=4,Stone=5,Torch=6,MagicMirror=7;}}
namespace XianXia.Content.Biomes {public class GreenwoodHerbGardenBiome {}}
namespace XianXia.Content.Projectiles {public class SpiritBoltProjectile {}}
namespace XianXia.Content.Items.HandGenerated {public class SpiritHerbRoot {} public class HerbDew {} public class CinnabarPowder {}}
namespace XianXia.Content.Items.Materials {public class GreenwoodRoot {}}

namespace XianXia.Content.Items.HandGenerated {public class BrokenSwordIntent {}}

namespace XianXia.Content.Biomes {public class TenThousandSectsRuinsBiome {}}
namespace XianXia.Content.Items.Materials {public class SectTrialToken {}}

namespace XianXia.Content.Items.HandGenerated {public class TornScrollPage {}}

namespace XianXia.Content.Biomes {public class ThunderMarshCloudsBiome {}}
namespace XianXia.Content.Items.HandGenerated {public class ThunderPatternFeather {}}
namespace XianXia.Content.Items.Materials {public class TribulationCloudDew {}}

namespace XianXia.Content.Biomes {public class StarAbyssRiftBiome {}}
namespace XianXia.Content.Items.Materials {public class StarEclipseCrystal {}}

namespace Terraria {public static class Lighting {public static int Calls;public static void AddLight(Microsoft.Xna.Framework.Vector2 center,float r,float g,float b)=>Calls++;}}
namespace XianXia.Content.Biomes {public class MoonboneAbyssBiome {}}
namespace XianXia.Content.Items.Materials {public class Moonbone {}}
namespace XianXia.Content.Items.HandGenerated {public class AbyssDust {}public class DarkBlueSpiritFluid {}public class ColdMoonDust {}}

namespace XianXia.Content.Items.HandGenerated {public class BrokenDecreeItem {}}

namespace XianXia.Content.Biomes {public class ShallowSpiritVeinsBiome {}}
namespace XianXia.Content.Items.Materials {public class LowGradeSpiritStone {}public class SpiritGel {}}

namespace XianXia.Content.Biomes {public class SunkenFurnaceVeinBiome {}}
namespace XianXia.Content.Items.Materials {public class ArtifactBlankShard {}public class FurnaceSlagIron {}}

namespace Terraria {public class RandomStub {public float Value;public int Calls;public float NextFloat(float min,float max){Calls++;return (min+max)/2;}public float NextFloat(){Calls++;return Value;}}}
namespace XianXia.Content.Projectiles {public class EnemySpiritBoltProjectile {}}

namespace XianXia.Content.Items.Materials {public class DaoSeveringDust {}}
namespace XianXia.Content.Items.HandGenerated {public class ArchiveRemnantLight {}}

namespace Terraria {public static class Collision {public static bool Blocked;public static int Calls;public static Microsoft.Xna.Framework.Vector2 LastPosition;public static bool SolidCollision(Microsoft.Xna.Framework.Vector2 position,int width,int height){Calls++;LastPosition=position;return Blocked;}}}
namespace XianXia.Content.Projectiles {public class TribulationWarningLineProjectile {}}
namespace XianXia.Content.Items.HandGenerated {public class SingingThunderStoneItem {}}

namespace XianXia.Content.Items.HandGenerated {public class FurnaceCharcoal {}}

namespace XianXia.Content.NPCs.Enemies {public partial class ArchivedImmortalSoul{private bool SynchronizeSummonTarget()=>true;}public partial class IronShardSpirit{private bool SynchronizeSummonTarget()=>true;}}

namespace XianXia.Content.NPCs.Enemies {public partial class TribulationCloudling{private bool SynchronizeSummonTarget()=>true;}}

namespace XianXia.Content.NPCs.Enemies{public partial class ObsessedSwordCultivator{private bool SynchronizeSummonTarget()=>true;private void WriteSummonBinding(System.IO.BinaryWriter writer){}private void ReadSummonBinding(System.IO.BinaryReader reader){}}}

namespace XianXia.Content.NPCs.Enemies{public partial class HerbGardenVineSpirit{private bool SynchronizeSummonTarget()=>true;}}
