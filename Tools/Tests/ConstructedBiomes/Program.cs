using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Biomes;
using XianXia.Content.Items.Construction;
using XianXia.Content.Tiles.Construction;
using Game=Terraria.Main;
int assertions=0;
void Check(bool ok,string message){assertions++;if(!ok)throw new Exception(message);}
var domains=new (ConstructedBiomeBlock Block,ConstructedBiomeTile Tile,ModBiome Biome,int Threshold,int Tier)[]{
 (new SpiritVeinBiomeBlock(),new SpiritVeinConstructedTile(),new ShallowSpiritVeinsBiome(),40,0),
 (new GreenwoodBiomeBlock(),new GreenwoodConstructedTile(),new GreenwoodHerbGardenBiome(),120,0),
 (new FurnaceBiomeBlock(),new FurnaceConstructedTile(),new SunkenFurnaceVeinBiome(),120,0),
 (new ThunderBiomeBlock(),new ThunderConstructedTile(),new ThunderMarshCloudsBiome(),100,1),
 (new StarAbyssBiomeBlock(),new StarAbyssConstructedTile(),new StarAbyssRiftBiome(),140,1),
 (new SectRuinBiomeBlock(),new SectRuinConstructedTile(),new TenThousandSectsRuinsBiome(),180,2),
 (new FallenHeavenBiomeBlock(),new FallenHeavenConstructedTile(),new FallenHeavenPalaceBiome(),160,3),
 (new MoonboneBiomeBlock(),new MoonboneConstructedTile(),new MoonboneAbyssBiome(),200,4)
};
void SetCounts(int tile,int amount){var counts=new int[512];counts[tile]=amount;ModContent.GetInstance<SpiritVeinTileCountSystem>().TileCountsAvailable(counts);ModContent.GetInstance<GeneratedBiomeTileCountSystem>().TileCountsAvailable(counts);}
string[] naturalNames={"SpiritMossTile","GreenwoodSoilTile","FurnaceSlagTile","ThunderCloudTile","StarAbyssCrystalTile","SectRuinBrickTile","FallenHeavenJadeTile","MoonboneTile"};
int domainIndex=0;
foreach(var domain in domains){
 domain.Block.SetDefaults();domain.Tile.SetStaticDefaults();domain.Block.AddRecipes();
 Check(domain.Block.Item.createTile==domain.Tile.Type,"Block places matching constructed tile");
 Check(domain.Tile.RegisteredDrop==ModContent.Id(domain.Block.GetType()),"Mining returns construction item, never material");
 Check(domain.Block.Item.value==0,"Construction blocks cannot be sold for crafting profit");
 var recipe=Recipe.All.Last();
 Check(recipe.Ingredients.Count==2&&recipe.Ingredients[1]==(domain.Block is SpiritVeinBiomeBlock?Terraria.ID.ItemID.FallenStar:ModContent.ItemType<XianXia.Content.Items.Materials.LowGradeSpiritStone>(),1),"Recipe uses accessible catalyst and vanilla blocks");
 Check(recipe.Amount==recipe.Ingredients[0].Item2&&recipe.Tiles.Single()==Terraria.ID.TileID.WorkBenches,"Recipe cannot multiply vanilla building resources");
 for(int flags=0;flags<16;flags++){
  Game.hardMode=(flags&1)!=0;NPC.downedPlantBoss=(flags&2)!=0;NPC.downedGolemBoss=(flags&4)!=0;NPC.downedMoonlord=(flags&8)!=0;
  bool expected=domain.Tier==0||(flags&(1<<(domain.Tier-1)))!=0;
  Check(recipe.Conditions.All(c=>c.Check())==expected,"Recipe respects its exact vanilla stage gate");
 }
 foreach(int count in new[]{0,domain.Threshold-1,domain.Threshold}){
  Game.netMode=0;SetCounts(domain.Tile.Type,count);
  var player=new Player();player.Biome=new ServerBiomePlayer{Player=player};
  Check(domain.Biome.IsBiomeActive(player)==(count>=domain.Threshold),"Client recognizes constructed biome at exact threshold");
  Game.netMode=2;Array.Clear(Game.tile);
  for(int i=0;i<count;i++)Game.tile[10+i%20,10+i/20]=new Tile{HasTile=true,TileType=(ushort)domain.Tile.Type};
  Game.GameUpdateCount+=60;
  Check(domain.Biome.IsBiomeActive(player)==(count>=domain.Threshold),"Actual server scan recognizes constructed biome at exact threshold");
 }
 int natural=ModContent.Id(typeof(SpiritVeinBiomeBlock).Assembly.GetType("XianXia.Content.Tiles."+naturalNames[domainIndex++],true));
 int[] mixed=new int[512];mixed[natural]=domain.Threshold/2;mixed[domain.Tile.Type]=domain.Threshold-mixed[natural];
 ModContent.GetInstance<SpiritVeinTileCountSystem>().TileCountsAvailable(mixed);ModContent.GetInstance<GeneratedBiomeTileCountSystem>().TileCountsAvailable(mixed);
 var mixedPlayer=new Player();mixedPlayer.Biome=new ServerBiomePlayer{Player=mixedPlayer};
 Game.netMode=0;Check(domain.Biome.IsBiomeActive(mixedPlayer),"Natural and constructed blocks combine on client");
 Game.netMode=2;Array.Clear(Game.tile);for(int i=0;i<domain.Threshold;i++)Game.tile[10+i%20,10+i/20]=new Tile{HasTile=true,TileType=(ushort)(i<mixed[natural]?natural:domain.Tile.Type)};
 Game.GameUpdateCount+=60;Check(domain.Biome.IsBiomeActive(mixedPlayer),"Natural and constructed blocks combine on actual server scan");
}
Console.WriteLine($"Constructed biome regression passed: {assertions} assertions against actual block/tile/recipe/biome/scan sources with engine stubs.");

var objects=new (BiomeObjectItem Item,ModTile Tile,int Height,int Pick,int Tier)[]{
 (new SwordTabletPlaceable(),new XianXia.Content.Tiles.SwordTabletTile(),3,150,2),
 (new SingingThunderStonePlaceable(),new XianXia.Content.Tiles.SingingThunderStoneTile(),2,110,1),
 (new RiftMembranePlaceable(),new XianXia.Content.Tiles.RiftMembraneTile(),2,110,1),
 (new BrokenHeavenTabletPlaceable(),new XianXia.Content.Tiles.BrokenHeavenTabletTile(),4,200,3),
 (new ArchiveLightPillarPlaceable(),new XianXia.Content.Tiles.ArchiveLightPillarTile(),6,225,4)
};
foreach(var entry in objects){
 entry.Item.SetStaticDefaults();entry.Item.SetDefaults();entry.Item.AddRecipes();entry.Tile.SetStaticDefaults();
 Check(entry.Item.Item.createTile==entry.Tile.Type,"Object item places matching tile");
 Check(entry.Tile.RegisteredDrop==ModContent.Id(entry.Item.GetType()),"Object mining returns one matching placeable");
 Check(entry.Item.Item.maxStack==99&&entry.Item.Item.ResearchUnlockCount==1&&entry.Item.Item.value==0,"Object stacks, researches once and cannot be sold for profit");
 var data=Terraria.ObjectData.TileObjectData.Registered[entry.Tile.Type];
 Check(data.Width==2&&data.Height==entry.Height&&data.Origin.Y==entry.Height-1,"Object preserves correct native footprint and origin");
 Check(data.CoordinatePadding==0&&data.CoordinateHeights.Length==entry.Height&&data.CoordinateHeights.All(h=>h==16),"Object preserves original 16-pixel tile sheet layout");
 Check(entry.Tile.MinPick==entry.Pick&&entry.Pick<=225,"Object can be recovered with original-game progression tools");
 var recipe=Recipe.All.Last();Check(recipe.Ingredients.Count==3&&recipe.Amount==1&&recipe.Tiles.Single()==Terraria.ID.TileID.WorkBenches,"Object recipe is accessible without a generated structure");
 for(int flags=0;flags<16;flags++){
  Game.hardMode=(flags&1)!=0;NPC.downedPlantBoss=(flags&2)!=0;NPC.downedGolemBoss=(flags&4)!=0;NPC.downedMoonlord=(flags&8)!=0;
  Check(recipe.Conditions.All(c=>c.Check())==((flags&(1<<(entry.Tier-1)))!=0),"Object recipe retains intended stage gate");
 }
}
Console.WriteLine($"Including placeable biome objects: {assertions} assertions.");
