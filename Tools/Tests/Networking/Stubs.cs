// Minimal engine boundary for executing the actual packet and world-progress source.
// This harness does not replace dedicated-server or client integration testing.
using System.IO;

namespace Microsoft.Xna.Framework
{
    public struct Color { public static Color Gray, CornflowerBlue, Gold, LimeGreen; public Color(int r,int g,int b) { } }
    public record struct Point(int X,int Y);
    public struct Vector2
    {
        public float X, Y;
        public Vector2(float x, float y) { X = x; Y = y; }
        public Point ToTileCoordinates() => new((int)(X / 16),(int)(Y / 16));
        public Vector2 RotatedBy(double radians) => new(X*(float)Math.Cos(radians)-Y*(float)Math.Sin(radians),X*(float)Math.Sin(radians)+Y*(float)Math.Cos(radians));
        public float ToRotation()=>MathF.Atan2(Y,X);
        public static Vector2 UnitY => new(0,1);
        public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.X+b.X,a.Y+b.Y);
        public static Vector2 Zero => new(0,0);
        public static Vector2 UnitX => new(1,0);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X-b.X,a.Y-b.Y);
        public static Vector2 operator *(Vector2 a, float b) => new(a.X*b,a.Y*b);
        public float LengthSquared() => X*X+Y*Y;
        public Vector2 SafeNormalize(Vector2 fallback) => LengthSquared() > 0 ? this * (1f/MathF.Sqrt(LengthSquared())) : fallback;
        public static float DistanceSquared(Vector2 a, Vector2 b) =>
            (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
    }
}

namespace Terraria
{
    public static class Main
    {
        public static ulong GameUpdateCount;
        public static int netMode, maxPlayers = 4, maxNPCs = 4, myPlayer;
        public static int maxTilesX=30,maxTilesY=30;
        public static Tile[,] tile = CreateTiles();
        private static Tile[,] CreateTiles() { var cells=new Tile[maxTilesX,maxTilesY]; for(int x=0;x<maxTilesX;x++) for(int y=0;y<maxTilesY;y++) cells[x,y]=new Tile(); return cells; }
        public static bool[] buffNoSave=new bool[22]; public static Item mouseItem=new();
        public static Player[] player = Enumerable.Range(0, maxPlayers).Select(i => new Player { whoAmI = i }).ToArray();
        public static NPC[] npc = Enumerable.Range(0, maxNPCs).Select(_ => new NPC()).ToArray();
        public static Player LocalPlayer => player[myPlayer];
        public static IEnumerable<Player> ActivePlayers=>player.Where(p=>p.active);
        public static string npcChatText;
        public static bool hardMode, dayTime,dedServ;
        public static int maxProjectiles = 8;
        public static RandomStub rand = new();
        public static Microsoft.Xna.Framework.Vector2 MouseWorld;
        public static Projectile[] projectile = Enumerable.Range(0, maxProjectiles).Select(_ => new Projectile()).ToArray();
        public static readonly List<string> Chat = new();
        public static void NewText(string value, byte r, byte g, byte b) => Chat.Add(value);
    }
    public class RandomStub { public float Roll = 0.9f; public float NextFloat() => Roll; public float NextFloat(float min,float max)=>min+(max-min)*Roll; }
    public class Player
    {
        public bool active = true, dead, noItems, CCed;
        public int talkNPC = -1;
        public int selectedItem, whoAmI, direction = 1;
        public int altFunctionUse,statLife=100,statLifeMax2=100,team;
        public bool hostile;
        public Item[] armor=Enumerable.Range(0,20).Select(_=>new Item()).ToArray();
        public bool IsItemSlotUnlockedAndUsable(int slot)=>slot<8;
        public void Heal(int amount)=>statLife=Math.Min(statLifeMax2,statLife+amount);
        public int lifeRegen,statDefense,armorPenetration;
        public float moveSpeed,attackSpeed,critChance;
        public Terraria.ModLoader.StatModifier damageModifier,knockbackModifier;
        public ref int GetArmorPenetration(Terraria.ModLoader.DamageClass damage)=>ref armorPenetration;
        public ref float GetAttackSpeed(Terraria.ModLoader.DamageClass damage)=>ref attackSpeed;
        public ref float GetCritChance(Terraria.ModLoader.DamageClass damage)=>ref critChance;
        public ref Terraria.ModLoader.StatModifier GetDamage(Terraria.ModLoader.DamageClass damage)=>ref damageModifier;
        public ref Terraria.ModLoader.StatModifier GetKnockback(Terraria.ModLoader.DamageClass damage)=>ref knockbackModifier;
        public int[] buffTime = new int[22], ownedProjectileCounts = new int[16];
        public void AddBuff(int type, int time) => buffTime[type] = time; public bool HasBuff(int type)=>buffTime[type]>0;
        public Microsoft.Xna.Framework.Rectangle Hitbox => new((int)Center.X-10,(int)Center.Y-20,20,40);
        public Microsoft.Xna.Framework.Vector2 MountedCenter => Center;
        public Microsoft.Xna.Framework.Vector2 RotatedRelativePoint(Microsoft.Xna.Framework.Vector2 value) => value;
        public int GetWeaponDamage(Item item) => item.damage + 5;
        public float GetWeaponKnockback(Item item, float value) => value + 1;

        public Item[] inventory = Enumerable.Range(0, 59).Select(_ => new Item()).ToArray();
        public Item HeldItem => inventory[selectedItem];
        public Microsoft.Xna.Framework.Vector2 Center;
        public XianXia.Common.Players.XianXiaPlayer State = new();
        private XianXia.Common.Players.InscriptionPlayer inscription;
        public T GetModPlayer<T>() {
            if(typeof(T)==typeof(XianXia.Common.Players.InscriptionPlayer))
                return (T)(object)(inscription ??= new XianXia.Common.Players.InscriptionPlayer { Player=this });
            return (T)(object)State;
        }
        public HashSet<Type> Biomes = new();
        public bool InModBiome<T>() => Biomes.Contains(typeof(T));
    }
    public class Item
    {
        public Terraria.ModLoader.ModItem ModItem;
        public int type, stack, shoot = 2, damage = 20, useTime = 30;
        public int maxStack=1,useAmmo,ammo,prefix,width,height,value,rare,useStyle,useAnimation,ResearchUnlockCount;
        public object UseSound;
        public bool accessory,vanity,noMelee,autoReuse; public int crit; public object DamageType;
        public static int buyPrice(int gold=0,int silver=0)=>gold*10000+silver*100;
        public bool IsAir=>type==0||stack<=0;
        public float shootSpeed = 10f, knockBack = 2f;
        public bool consumable;
        private readonly Dictionary<Type, Terraria.ModLoader.GlobalItem> globals = new();
        public T GetGlobalItem<T>() where T : Terraria.ModLoader.GlobalItem, new()
        {
            if (!globals.TryGetValue(typeof(T), out var value)) globals[typeof(T)] = value = new T { Mod = ModItem?.Mod };
            return (T)value;
        }
        public void TurnToAir() { type = stack = 0; }
    }
    public class Tile { public bool HasTile; public ushort TileType; }
    public class Projectile
    {
        public bool active,friendly,hostile,tileCollide,ignoreWater,usesLocalNPCImmunity,netUpdate,arrow,netImportant;
        public int width,height,penetrate,timeLeft,localNPCHitCooldown; public float rotation; public float[] ai=new float[2]; public object DamageType;
        public Terraria.ModLoader.ModProjectile ModProjectile;public int whoAmI;
        public int owner, type, damage, identity; public float knockBack;
        public Microsoft.Xna.Framework.Rectangle Hitbox=>new((int)Center.X-width/2,(int)Center.Y-height/2,width,height);
        public object GetSource_FromAI()=>new(); public void Kill(){if(!active)return;new XianXia.Common.Systems.ServerPlayerProjectileSync().OnKill(this,timeLeft);active=false;}
        private static int nextIdentity;
        public Microsoft.Xna.Framework.Vector2 velocity;
        public Microsoft.Xna.Framework.Vector2 Center;
        public static bool AllowSpawn = true;
        public static int SpawnBudget = -1;
        public static int NewProjectile(object source, Microsoft.Xna.Framework.Vector2 position,
            Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback, int owner)
        {
            if (!AllowSpawn || SpawnBudget == 0) return Main.maxProjectiles;
            if (SpawnBudget > 0) SpawnBudget--;
            int index = Array.FindIndex(Main.projectile, p => !p.active);
            if (index < 0) return Main.maxProjectiles;
            Main.projectile[index] = new() { active = true, owner = owner, type = type, damage = damage, velocity = velocity, identity = nextIdentity++ };
            Main.player[owner].ownedProjectileCounts[type]++;
            return index;
        }
    }
    public class NPC
    {
        public struct HitInfo{} public int LastBuff,BuffDuration; public void AddBuff(int type,int duration){LastBuff=type;BuffDuration=duration;}
        public bool active = true, friendly, dontTakeDamage;
        public bool CanBeChasedBy(object source)=>active&&!friendly&&!dontTakeDamage;
        public Microsoft.Xna.Framework.Vector2 Center;
        public object ModNPC;
        public int type;
        public static bool downedPlantBoss, downedGolemBoss, downedMoonlord, AllowSpawn = true;
        public static int SpawnCalls;
        public static bool AnyNPCs(int type) => Main.npc.Any(n => n.active && n.type == type);
        public static void SpawnOnPlayer(int player, int type)
        {
            SpawnCalls++;
            if (!AllowSpawn) return;
            NPC available = Main.npc.FirstOrDefault(n => !n.active);
            if (available != null) { available.active = true; available.type = type; }
        }
    }
    public static class NetMessage
    {
        public static readonly List<(int Message,int Number,float Number2)> Sent=new();
        public static readonly List<(int Message,int To,int Ignore,int Number,float Amount)> Routed=new();
        public static int Broadcasts;
        public static int WorldSends;
        public static void SendData(int message, int toWho = -1, int fromWho = -1, object text = null, int number = 0, float number2 = 0) { Sent.Add((message,number,number2));Routed.Add((message,toWho,fromWho,number,number2));Broadcasts++; if(message==Terraria.ID.MessageID.WorldData) WorldSends++; }
    }
}

namespace Terraria.ID
{
    public static class ItemUseStyleID { public const int HoldUp=1,Shoot=2,Swing=3,DrinkLiquid=4; }
    public static class SoundID { public const int Item4=1,Item20=2,Item5=3,Item3=4; }
    public static class BuffID { public const int Regeneration = 1,Ichor=2,OnFire3=3; }
    public static class NetmodeID { public const int SinglePlayer = 0, MultiplayerClient = 1, Server = 2; }
    public static class MessageID { public const int WorldData = 7, SyncEquipment = 5,SpiritHeal=66,KillProjectile=29,SyncProjectile=27; }
}

namespace Terraria.ModLoader
{
    public class ModItem
    {
        public Mod Mod;
        public virtual string Texture=>""; public virtual void AddRecipes(){} public Terraria.Recipe CreateRecipe(int amount=1)=>new();
        public Terraria.Item Item = new();
        public string Name;
        public bool Allowed = true;
        public Action<Terraria.Player> Effect;
        public int Uses;
        public virtual void SetDefaults() { }
        public virtual void SetStaticDefaults() { }
        public virtual void ModifyTooltips(List<TooltipLine> tips) { }
        public virtual bool ConsumeItem(Terraria.Player player) => true;
        public virtual bool AltFunctionUse(Terraria.Player player)=>false;
        public virtual void ModifyWeaponDamage(Terraria.Player player, ref StatModifier damage) {}
        public virtual bool Shoot(Terraria.Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback) => true;
        public virtual bool CanUseItem(Terraria.Player player) => Allowed;
        public virtual bool? UseItem(Terraria.Player player) { Uses++; Effect?.Invoke(player); return true; }
    }
    public static class ProjectileLoader { public const int ProjectileCount = 16; }
    public static class CombinedHooks
    {
        public static bool AllowShoot = true, SuppressDefault;
        public static int TotalUseTime(float useTime, Terraria.Player player, Terraria.Item item) => Math.Max(1, (int)useTime);
        public static bool CanUseItem(Terraria.Player player, Terraria.Item item) => item.ModItem.CanUseItem(player);
        public static bool CanShoot(Terraria.Player player, Terraria.Item item) => AllowShoot;
        public static void ModifyShootStats(Terraria.Player player, Terraria.Item item, ref Microsoft.Xna.Framework.Vector2 pos,
            ref Microsoft.Xna.Framework.Vector2 velocity, ref int type, ref int damage, ref float knockback) { }
        public static bool Shoot(Terraria.Player player, Terraria.Item item, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Microsoft.Xna.Framework.Vector2 pos, Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback) =>
            !SuppressDefault && item.ModItem.Shoot(player, source, pos, velocity, type, damage, knockback);
    }
    public class TooltipLine
    {
        public Microsoft.Xna.Framework.Color? OverrideColor;
        public string Text;
        public TooltipLine(Mod mod, string name, string text) => Text = text;
    }
    public class GlobalItem
    {
        public Mod Mod;
        public virtual bool InstancePerEntity => false;
        public virtual bool AppliesToEntity(Terraria.Item item, bool lateInstantiation) => true;
        public virtual void OnCreated(Terraria.Item item, Terraria.DataStructures.ItemCreationContext context) { }
        public virtual bool CanStack(Terraria.Item destination, Terraria.Item source) => true;
        public virtual bool CanStackInWorld(Terraria.Item destination, Terraria.Item source) => true;
        public virtual void OnStack(Terraria.Item destination, Terraria.Item source, int num) { }
        public virtual void SplitStack(Terraria.Item destination, Terraria.Item source, int num) { }
        public virtual void SaveData(Terraria.Item item, IO.TagCompound tag) { }
        public virtual void LoadData(Terraria.Item item, IO.TagCompound tag) { }
        public virtual void NetSend(Terraria.Item item, BinaryWriter writer) { }
        public virtual void NetReceive(Terraria.Item item, BinaryReader reader) { }
        public virtual void ModifyTooltips(Terraria.Item item, List<TooltipLine> tips) { }
        public virtual void UpdateAccessory(Terraria.Item item,Terraria.Player player,bool hideVisual) { }
        public virtual void ModifyWeaponDamage(Terraria.Item item,Terraria.Player player,ref StatModifier damage) { }
        public virtual void ModifyWeaponCrit(Terraria.Item item,Terraria.Player player,ref float crit) { }
        public virtual void ModifyWeaponKnockback(Terraria.Item item,Terraria.Player player,ref StatModifier knockback) { }
        public virtual bool ConsumeItem(Terraria.Item item, Terraria.Player player) => true;
        public virtual bool? UseItem(Terraria.Item item, Terraria.Player player) => null;
    }
    public struct StatModifier { public float Bonus,Base; public static StatModifier operator +(StatModifier value,float amount) { value.Bonus+=amount; return value; } }
    public class DamageClass { public static DamageClass Generic=new(),Magic=new(),Ranged=new(),Melee=new(); }
    public class ModPlayer {
        public Terraria.Player Player;
        public virtual void Initialize() { }
        public virtual void ResetEffects() { }
        public virtual void PostUpdateEquips() { }
        public virtual void PostUpdate() { }
        public virtual void UpdateLifeRegen() { }
        public virtual void UpdateDead() { }
    }
    public static class ModContent {
        public static int TileType<T>() => typeof(T).Name switch { "ArtifactForgeTile"=>1,"ThunderPatternForgeTile"=>2,"HeavenFireFurnaceTile"=>3,_=>4 };
        public static int ItemType<T>()=>200;
        public static int ProjectileType<T>()=>typeof(T).Name=="GreenwoodArrayField"?3:typeof(T).Name=="MedicineCauldronField"?4:typeof(T).Name=="MedicineSpiritBolt"?5:typeof(T).Name=="FurnaceHammerProjectile"?6:typeof(T).Name=="FurnaceImpactBurst"?7:typeof(T).Name=="HeavenTabletWardProjectile"?8:typeof(T).Name=="ThunderTalismanArray"?9:typeof(T).Name=="MinorThunderboltProjectile"?10:2;
        public static int BuffType<T>()=>typeof(T).Name=="FurnaceGuardBuff"?3:typeof(T).Name=="WindStepBuff"?4:typeof(T).Name=="ThunderBurstBuff"?5:2;
        public static T GetInstance<T>() where T:new()=>new T();
    }
    public class ModSystem
    {
        public virtual void ClearWorld() { }
        public virtual void PostUpdatePlayers() { }
        public virtual void SaveWorldData(IO.TagCompound tag) { }
        public virtual void LoadWorldData(IO.TagCompound tag) { }
        public virtual void NetSend(BinaryWriter writer) { }
        public virtual void NetReceive(BinaryReader reader) { }
    }
    public class Mod
    {
        public virtual void HandlePacket(BinaryReader reader, int whoAmI) { }
        public ModPacket GetPacket() => new();
    }
    public class ModPacket : BinaryWriter
    {
        public static readonly List<(byte[] Data, int Recipient, int Excluded)> Sent = new();
        public ModPacket() : base(new MemoryStream()) { }
        public void Send(int toWho = -1, int fromWho = -1) =>
            Sent.Add((((MemoryStream)BaseStream).ToArray(), toWho, fromWho));
    }
}

namespace Terraria.ModLoader.IO
{
    public class TagCompound : Dictionary<string, object>
    {
        public bool GetBool(string key) => TryGetValue(key, out object v) && (bool)v;
        public int GetInt(string key) => TryGetValue(key, out object v) ? (int)v : 0;
        public IList<T> GetList<T>(string key) => TryGetValue(key, out object v) ? (IList<T>)v : new List<T>();
    }
}

namespace Terraria.Localization
{
    public static class Language { public static object GetText(string key)=>key; public static string GetTextValue(string key, params object[] values) => key; }
    public class NetworkText
    {
        private readonly string text;
        public NetworkText(string value) => text = value;
        public void Serialize(BinaryWriter writer) => writer.Write(text);
        public static NetworkText Deserialize(BinaryReader reader) => new(reader.ReadString());
        public static NetworkText FromKey(string key, params object[] substitutions) => new(key);
        public override string ToString() => text;
    }
}

namespace XianXia.Common.Players
{
    public class XianXiaPlayer
    {
        public int spiritualEnergy;
        public int activeSkillCooldown,wardGuardTimer,skillRequestCooldown;
        public int spiritPressure,spiritualEnergyRegenBonus;
        public float spiritualEnergyCostMultiplier=1f;
        public CultivationStage cultivationStage;
        public int maxSpiritualEnergy = 40, arrayDeploymentCooldown;
        public bool discoveredSpiritualEnergy=true; public bool NetworkInitialized, ApplyingProgressionItem, NetworkWasActive;
        public void ResetNetworkSession() { NetworkInitialized = NetworkWasActive = false; ProgressionItemCooldown = BossSummonCooldown = 0; }
        public int ProgressionItemCooldown, BossSummonCooldown, WeaponShotCooldown;
        public bool ApplyingWeaponShot;
        public void RestoreSpiritualEnergy(int amount) => spiritualEnergy = Math.Clamp(spiritualEnergy + amount, 0, maxSpiritualEnergy);
        public bool TryConsumeSpiritualEnergy(int amount) { if (spiritualEnergy < amount) return false; spiritualEnergy -= amount; return true; }
        private ulong lastRecoveryTick=ulong.MaxValue; public bool TryArrayRecovery(ulong tick){if(Terraria.Main.netMode==Terraria.ID.NetmodeID.MultiplayerClient||tick==lastRecoveryTick)return false;lastRecoveryTick=tick;return true;}
        public bool CanConsumeSpiritualEnergy(int amount)=>spiritualEnergy>=amount;
        public bool CanDeployArray(int type,int amount)=>arrayDeploymentCooldown==0&&Terraria.Main.player[0].ownedProjectileCounts[type]==0&&CanConsumeSpiritualEnergy(amount);
        public bool TryDeployArray(int type, int amount)
        {
            if (arrayDeploymentCooldown != 0 || Terraria.Main.player[0].ownedProjectileCounts[type] > 0 || !TryConsumeSpiritualEnergy(amount)) return false;
            arrayDeploymentCooldown = 480; return true;
        }
        public uint ResourceRevision;
        public void AdvanceResourceRevision() => ResourceRevision++;
        public CultivationSnapshot Snapshot;
        public CultivationSnapshot CaptureSnapshot() => Snapshot with { Energy = spiritualEnergy, Stage = (byte)cultivationStage, ArrayCooldown = (ushort)arrayDeploymentCooldown, Revision = ResourceRevision,SkillCooldown=(ushort)activeSkillCooldown,WardTimer=(ushort)wardGuardTimer };
        public void NotifySnapshot(CultivationSnapshot state) { }
        public void ApplySnapshot(CultivationSnapshot state)
        {
            Snapshot = state;
            ResourceRevision = state.Revision;
            spiritualEnergy = state.Energy;
            cultivationStage = (CultivationStage)state.Stage;
            maxSpiritualEnergy = CultivationSnapshot.MaxEnergy(state.Stage) + state.Comprehension * 5;
            arrayDeploymentCooldown = state.ArrayCooldown;
            activeSkillCooldown=state.SkillCooldown;wardGuardTimer=state.WardTimer;
        }
        public bool TryInitializeNetwork(CultivationSnapshot state)
        {
            if (NetworkInitialized || !state.IsValid()) return false;
            ApplySnapshot(state); ResourceRevision = 0; NetworkInitialized = true; return true;
        }
        public void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            var packet = new Terraria.ModLoader.ModPacket();
            packet.Write((byte)4); packet.Write((byte)0); CaptureSnapshot().Write(packet);
            packet.Send(toWho, fromWho);
        }
    }
}

namespace XianXia.Content.NPCs.Town
{
    public class CultivationTownNPC
    {
        public int Claims;
        public Terraria.Localization.NetworkText ClaimCommissionOnServer(Terraria.Player player)
        {
            Claims++;
            return new("commission response");
        }
    }
}

namespace XianXia.Content.Biomes
{
    public class GreenwoodHerbGardenBiome { }
    public class SunkenFurnaceVeinBiome { }
    public class ThunderMarshCloudsBiome { }
    public class StarAbyssRiftBiome { }
    public class TenThousandSectsRuinsBiome { }
    public class FallenHeavenPalaceBiome { }
    public class MoonboneAbyssBiome { }
}

namespace Terraria.DataStructures
{
    public interface IEntitySource{}
    public class ItemCreationContext { }
    public class RecipeItemCreationContext : ItemCreationContext { }
    public class EntitySource_ItemUse_WithAmmo : IEntitySource
    {
        public EntitySource_ItemUse_WithAmmo(Terraria.Player player, Terraria.Item item, int ammo) { }
    }
}

namespace XianXia.Common.Systems { public class InscriptionUISystem { public void Open(int slot) { } } }
namespace XianXia.Content.Items.Materials { public class LowGradeSpiritStone { } }
namespace XianXia.Content.Tiles.Stations { public class AlchemyCauldronTile {} public class ArtifactForgeTile { } public class ThunderPatternForgeTile { } public class HeavenFireFurnaceTile { } public class DaoSeveringAltarTile { } }
namespace XianXia.Content.Items.HandGenerated { public class RouteMaterial { } }
namespace XianXia.Content.Items.Accessories { public class LightningWardJade { } }
namespace XianXia.Content.Projectiles { public class CloudpiercerSwordProjectile { } }
namespace XianXia.Content.Buffs { public class QiRecoveryCooldownBuff {} public class ArtifactWardBuff { } }

namespace Terraria {
 public class Condition {public Condition(){} public Condition(object description,Func<bool> predicate){} public static Condition DownedMoonLord=new(),DownedPlantera=new(),DownedGolem=new();}
 public class Recipe {public Recipe AddIngredient<T>(int amount=1)=>this;public Recipe AddIngredient(int type,int amount=1)=>this;public Recipe AddTile(int tile)=>this;public Recipe AddCondition(Condition condition)=>this;public void Register(){}}
 public static class Lighting {public static int Calls;public static void AddLight(Microsoft.Xna.Framework.Vector2 center,float r,float g,float b)=>Calls++;}
}
namespace Terraria.ID {public static class ItemRarityID {public const int Red=10,Lime=8,Yellow=9,Green=2,Blue=1;}}
namespace Terraria.ModLoader {
 public class ModProjectile {public Mod Mod;public Terraria.Projectile Projectile=new();public ModProjectile(){Projectile.ModProjectile=this;}public virtual string Texture=>"";public virtual void SetDefaults(){}public virtual bool? CanDamage()=>null;public virtual void AI(){} public virtual void OnSpawn(Terraria.DataStructures.IEntitySource source){} public virtual void SendExtraAI(System.IO.BinaryWriter writer){} public virtual void ReceiveExtraAI(System.IO.BinaryReader reader){} public virtual void OnKill(int timeLeft){} public virtual bool? CanHitNPC(Terraria.NPC target)=>null;public virtual bool OnTileCollide(Microsoft.Xna.Framework.Vector2 velocity)=>true;public virtual void OnHitNPC(Terraria.NPC npc,Terraria.NPC.HitInfo hit,int damage){}}
}
namespace XianXia.Content.Items.Materials {public class Moonbone{}}
namespace XianXia.Content.Items.HandGenerated {public class ArchiveRemnantLight{} public class ImperialDecreeItem{}}

namespace XianXia.Content.Items.HandGenerated {public class TornScrollPage{} public class HeavenTabletSeal{} public class StarCalamityCore{}}
namespace XianXia.Content.Items.Materials {public class HeavenTabletRubbing{}}
namespace XianXia.Content.Tiles.Stations {public class SectTrialAltarTile{}}

namespace XianXia.Content.Items.Materials {public class ArtifactBlankShard{}}

namespace Terraria {public static class Collision {public static bool Visible=true;public static bool CanHitLine(Microsoft.Xna.Framework.Vector2 start,int w,int h,Microsoft.Xna.Framework.Vector2 end,int ew,int eh)=>Visible;}}
namespace XianXia.Content.Items.HandGenerated {public class MedicineKingWoodHeart{}}
namespace XianXia.Content.Items.Materials {public class GreenwoodRoot{}}

namespace XianXia.Content.Items.Materials {public class OldFurnaceEmber{} public class FurnaceSlagIron{}}

namespace Terraria.ID {public static class ItemID { public const int BottledWater=6,Feather=7;public const int FlamingArrow=41,WoodenArrow=40;}public static class ProjectileID {public const int WoodenArrowFriendly=1,FireArrow=2;}public static class AmmoID {public const int Arrow=40;}}
namespace XianXia.Content.Items.HandGenerated {public class TornTalismanPaper{} public class CinnabarPowder{}}
namespace XianXia.Content.Tiles.Stations {public class SimpleTalismanTableTile{}}

namespace XianXia.Content.Items.Materials {public class HeavenDaoFragment{}}

namespace Terraria.ModLoader {public class GlobalProjectile {public virtual void OnSpawn(Terraria.Projectile p,Terraria.DataStructures.IEntitySource source){} public virtual void PostAI(Terraria.Projectile p){} public virtual void OnKill(Terraria.Projectile p,int timeLeft){}}}

namespace Microsoft.Xna.Framework {public record struct Rectangle(int X,int Y,int Width,int Height) {public bool Intersects(Rectangle r)=>X<r.X+r.Width&&X+Width>r.X&&Y<r.Y+r.Height&&Y+Height>r.Y;}}
namespace Terraria.DataStructures {public class EntitySource_Parent : IEntitySource {public object Entity;public EntitySource_Parent(object entity){Entity=entity;}}}

namespace Terraria.ModLoader { public class ModBuff { public int Type=3;public virtual string Texture=>"";public virtual void SetStaticDefaults(){}public virtual void Update(Terraria.Player player,ref int buffIndex){} } }

namespace XianXia.Content.Items.Materials { public class TribulationCloudDew {} }

namespace XianXia.Content.Projectiles{public class MoonboneShardProjectile{}}

namespace XianXia.Common.Players{public static class CultivationStatusText{public static string StageName(CultivationStage stage)=>stage.ToString();}}
