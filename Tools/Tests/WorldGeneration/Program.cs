using Terraria;using Microsoft.Xna.Framework;using XianXia.Common.Systems;using System.Reflection;
int assertions=0;void Check(bool ok,string msg){assertions++;if(!ok)throw new Exception(msg);}void Reset(){for(int x=0;x<60;x++)for(int y=0;y<60;y++)Main.tile[x,y]=new();WorldGen.genRand=new();WorldGen.Calls=0;WorldGen.Positions.Clear();WorldGen.Allow=true;WorldGen.Partial=WorldGen.BadFrames=WorldGen.Throws=WorldGen.FrameThrows=false;Terraria.WorldBuilding.GenVars.structures=new();}
var type=typeof(GeneratedBiomeWorldGenSystem);var place=type.GetMethod("PlaceObjects",BindingFlags.NonPublic|BindingFlags.Instance);var paint=type.GetMethod("PaintEllipse",BindingFlags.NonPublic|BindingFlags.Static);List<Rectangle> regions=[new(12,12,20,20)];
Reset();for(int x=12;x<32;x++){Main.tile[x,25].HasTile=true;Main.tile[x,25].TileType=88;}var system=new GeneratedBiomeWorldGenSystem();place.Invoke(system,new object[]{77,2,regions,88,3});Check(system.Mod.Logger.InfoMessages[0].Contains("2/2"),"Count complete placed objects");Check(WorldGen.Positions.All(p=>p.X>=12&&p.X+1<32&&p.Y-2>=12&&p.Y<32),"Placement stays in matching region");Check(WorldGen.Positions.Count==2,"Stops after enough complete objects");
Reset();for(int x=12;x<32;x++){Main.tile[x,25].HasTile=true;Main.tile[x,25].TileType=99;}for(int x=12;x<32;x++)for(int y=12;y<32;y++){Main.tile[x,y].HasTile=true;Main.tile[x,y].TileType=99;}system=new();place.Invoke(system,new object[]{77,2,regions,88,3});Check(WorldGen.Calls==0&&system.Mod.Logger.WarnMessages.Count==1,"Wrong biome support rejected and reported");
Reset();for(int x=12;x<32;x++){Main.tile[x,25].HasTile=true;Main.tile[x,25].TileType=88;}WorldGen.Allow=false;system=new();place.Invoke(system,new object[]{77,2,regions,88,3});Check(system.Mod.Logger.InfoMessages[0].Contains("0/2")&&system.Mod.Logger.WarnMessages.Count==1,"Engine failures never counted as success");
Reset();for(int x=12;x<32;x++){Main.tile[x,25].HasTile=true;Main.tile[x,25].TileType=88;Main.tile[x,24].LiquidAmount=255;}for(int x=12;x<32;x++)for(int y=12;y<32;y++){Main.tile[x,y].LiquidAmount=255;}system=new();place.Invoke(system,new object[]{77,2,regions,88,3});Check(WorldGen.Calls==0,"Liquid-filled object space rejected");
Reset();for(int x=10;x<30;x++)for(int y=10;y<30;y++){Main.tile[x,y].HasTile=true;Main.tile[x,y].TileType=2;}int painted=(int)paint.Invoke(null,new object[]{20,20,5,5,100,101,101});Check(painted>0,"Painting reports actual tile count");Check(Main.tile[20,20].WallType==101&&(Main.tile[20,20].TileType==100||Main.tile[20,20].TileType==101),"Tile and wall IDs have independent meaning even when equal");Reset();Check((int)paint.Invoke(null,new object[]{20,20,5,5,100,101,-1})==0,"Empty regions do not pretend to paint ground");
Reset();WorldGen.genRand.LowOnly=true;for(int x=20;x<22;x++){Main.tile[x,25].HasTile=true;Main.tile[x,25].TileType=88;}system=new();place.Invoke(system,new object[]{77,1,regions,88,3});Check(system.Mod.Logger.InfoMessages[0].Contains("1/1")&&WorldGen.Positions.Single()==(20,24),"Exhaustive fallback finds legal ledge missed by every random sample");
var pedestal=type.GetMethod("TryPlacePedestal",BindingFlags.NonPublic|BindingFlags.Static);
Reset();Check((bool)pedestal.Invoke(null,new object[]{77,88,3,20,25})&&Main.tile[20,25].TileType==88,"Empty dry site receives matching pedestal and object");
Reset();Main.tile[20,23].HasTile=true;Main.tile[20,23].TileType=99;Check(!(bool)pedestal.Invoke(null,new object[]{77,88,3,20,25})&&Main.tile[20,23].TileType==99&&!Main.tile[20,25].HasTile,"Pedestal never overwrites occupied site");
Reset();WorldGen.Allow=false;Check(!(bool)pedestal.Invoke(null,new object[]{77,88,3,20,25})&&!Main.tile[20,25].HasTile&&!Main.tile[21,25].HasTile,"Failed object placement removes newly added pedestal");
var passes = new List<Terraria.WorldBuilding.GenPass> { new() { Name="Shinies" }, new() { Name="Cleanup" } }; double weight=0;
system.ModifyWorldGenTasks(passes,ref weight);Check(passes.Last().Name=="XianXia Cultivation Objects","Objects follow every existing terrain/cleanup pass");
Reset();SectRuinHallSystem.Fixtures.Add(new Rectangle(12,12,41,19));
for(int x=12;x<53;x++){Main.tile[x,28].HasTile=true;Main.tile[x,28].TileType=88;}
system=new();place.Invoke(system,new object[]{42,2,new List<Rectangle>{new(12,12,41,19)},88,3});
Check(WorldGen.Positions.SequenceEqual(new[]{(22,27),(41,27)}),"Sword tablets prefer two deliberate hall floor positions before random fallback");
SectRuinHallSystem.Fixtures.Clear();
// Link all three real paint paths; only the engine structure map and tile storage are mocked.
Reset();for(int x=10;x<30;x++)for(int y=10;y<30;y++){Main.tile[x,y].HasTile=true;Main.tile[x,y].TileType=2;Main.tile[x,y].WallType=7;}
var reserved=new Terraria.WorldBuilding.StructureMap();reserved.AddProtectedStructure(new Rectangle(19,10,3,20));
Check(GeneratedBiomeWorldGenSystem.PaintProtectedEllipse(20,20,7,7,100,101,43,reserved)>0,"Ground ellipse still paints outside reservation");
Check(Enumerable.Range(19,3).All(x=>Enumerable.Range(10,20).All(y=>Main.tile[x,y].TileType==2&&Main.tile[x,y].WallType==7)),"Ground ellipse preserves reserved natural tiles and walls");
Reset();for(int x=10;x<30;x++)for(int y=10;y<30;y++){Main.tile[x,y].HasTile=true;Main.tile[x,y].TileType=2;}
Check(SpiritVeinWorldGenSystem.PaintVein(20,20,7,7,reserved)>0,"Shallow veins still paint outside reservation");
Check(Enumerable.Range(19,3).All(x=>Enumerable.Range(10,20).All(y=>Main.tile[x,y].TileType==2)),"Shallow veins preserve reserved natural cells");
Reset();Check(GeneratedBiomeWorldGenSystem.PaintCloudEllipse(20,20,7,7,42,reserved)>0,"Clouds still fill unreserved dry sky");
Check(Enumerable.Range(19,3).All(x=>Enumerable.Range(10,20).All(y=>!Main.tile[x,y].HasTile)),"Clouds preserve reserved empty structure space");
Reset();Check(!WorldGenerationProtection.CanChange(20,20,null),"Missing structure map does not grant permission");
Check(!WorldGenerationProtection.CanChange(-1,20,new()),"Out of world cell rejected");
var full=new Terraria.WorldBuilding.StructureMap();full.AddProtectedStructure(new Rectangle(10,10,20,20));
Check(GeneratedBiomeWorldGenSystem.PaintCloudEllipse(20,20,7,7,42,full)==0,"Fully protected clouds report zero placed cells");
Check(!WorldGenerationProtection.CanChange(18,20,reserved)&&WorldGenerationProtection.CanChange(17,20,reserved),"One-cell framing buffer respected");
Reset();var occupied=new Terraria.WorldBuilding.StructureMap();occupied.AddProtectedStructure(new Rectangle(20,20,2,6));
Check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(77,88,3,20,25,occupied)&&WorldGen.Calls==0&&!Main.tile[20,25].HasTile,"Protected empty pedestal site never changes terrain or calls engine");
Reset();for(int x=20;x<22;x++){Main.tile[x,25].HasTile=true;Main.tile[x,25].TileType=88;}
Check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(77,88,3,20,25,occupied)&&WorldGen.Calls==0,"Protected direct object placement rejected");
Reset();Main.tile[20,23].RedWire=true;
Check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(77,88,3,20,25,new())&&WorldGen.Calls==0,"Wired empty site rejected");
Reset();Check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(77,88,2,20,25,new())&&WorldGen.Calls==0,"Mismatched engine dimensions rejected before writing outside footprint");
foreach(int failure in new[]{0,1,2,3,4}) {
 Reset();var support=Main.tile[20,25];support.TileType=99;support.TileFrameX=36;support.TileFrameY=54;support.Slope=(Terraria.ID.SlopeType)1;support.IsHalfBlock=true;
 WorldGen.Allow=failure!=0;WorldGen.Partial=failure==1;WorldGen.BadFrames=failure==2;WorldGen.Throws=failure==3;WorldGen.FrameThrows=failure==4;
 bool failed=false;try{failed=!GeneratedBiomeWorldGenSystem.TryPlaceProtectedPedestal(77,88,3,20,25,new());}catch(InvalidOperationException){failed=true;}
 Check(failed&&support.TileType==99&&support.TileFrameX==36&&support.TileFrameY==54&&support.Slope==(Terraria.ID.SlopeType)1&&support.IsHalfBlock
  &&Enumerable.Range(20,2).All(x=>Enumerable.Range(22,4).All(y=>!Main.tile[x,y].HasTile)),"Engine refusal/partial/wrong-frame/placement or frame exception restores full original footprint including foundation metadata");
}
Reset();SectRuinHallSystem.Fixtures.Add(new Rectangle(12,12,41,19));occupied=new();occupied.AddProtectedStructure(SectRuinHallSystem.Fixtures[0]);
for(int x=22;x<24;x++){Main.tile[x,28].HasTile=true;Main.tile[x,28].TileType=42;}
Check(GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(42,42,3,22,28,occupied),"Owned hall interior allows its planned sword tablet despite own reservation");
SectRuinHallSystem.Fixtures.Clear();
Console.WriteLine($"Actual world-generation source regression passed: {assertions} assertions; mocked tile placement and structure-map boundaries, no full world generated.");
