using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using XianXia.Common.Items;
int assertions=0;void Check(bool ok,string message){assertions++;if(!ok)throw new Exception(message);}
var item=new TestItem();item.SetStaticDefaults();item.SetDefaults();var i=item.Item;
Check(i.master&&i.rare==ItemRarityID.Master,"Master collectible identity");Check(i.ResearchUnlockCount==1,"Research one collectible");Check(i.createTile==7&&i.consumable,"Placement consumes one item");Check(i.useStyle==ItemUseStyleID.Swing&&i.useTime==10&&i.useAnimation==15,"Native placement timings");Check(i.autoReuse&&i.useTurn&&i.maxStack==Item.CommonMaxStack,"Normal furniture behavior");
var tile=new TestTile();tile.SetStaticDefaults();var d=TileObjectData.Registered;
Check(d.Width==2&&d.Height==3,"2x3 object footprint");Check(d.Origin.X==0&&d.Origin.Y==2,"Placement origin at bottom");Check(d.CoordinatePadding==0&&d.CoordinateWidth==16&&d.CoordinateHeights.SequenceEqual(new[]{16,16,16}),"32x48 unpadded source matches six frames");Check(d.AnchorBottom.Width==2&&d.AnchorBottom.Offset==0&&d.AnchorBottom.Type==(AnchorType.SolidTile|AnchorType.SolidWithTop|AnchorType.Table),"Two supporting cells required");Check(Main.tileFrameImportant[7]&&Main.tileNoAttach[7],"Furniture frame flags");Check(!Main.tileLavaDeath[7]&&!d.LavaDeath,"Collectible survives lava");Check(tile.Drop==91,"Native destruction returns original collectible");
var lamp=new XianXia.Content.Tiles.BossDecorations.AbyssalStarWombLampTile();lamp.SetStaticDefaults();float r=0,g=0,b=0;lamp.ModifyLight(0,0,ref r,ref g,ref b);Check(Main.tileLighted[lamp.Type]&&Math.Abs(r-.18f)<.001f&&Math.Abs(g-.10f)<.001f&&Math.Abs(b-.35f)<.001f,"Lamp registers violet emission");
var tileSpiritVeinWyrmTrophy=new XianXia.Content.Tiles.BossDecorations.SpiritVeinWyrmTrophyTile();tileSpiritVeinWyrmTrophy.SetStaticDefaults();Check(tileSpiritVeinWyrmTrophy.Drop==Terraria.ModLoader.ModContent.ItemType<XianXia.Content.Items.HandGenerated.SpiritVeinWyrmTrophy>(),"SpiritVeinWyrmTrophy returns same item");
var tileAbyssalStarWombLamp=new XianXia.Content.Tiles.BossDecorations.AbyssalStarWombLampTile();tileAbyssalStarWombLamp.SetStaticDefaults();Check(tileAbyssalStarWombLamp.Drop==Terraria.ModLoader.ModContent.ItemType<XianXia.Content.Items.HandGenerated.AbyssalStarWombLamp>(),"AbyssalStarWombLamp returns same item");
var tileMedicineKingCauldronDecoration=new XianXia.Content.Tiles.BossDecorations.MedicineKingCauldronDecorationTile();tileMedicineKingCauldronDecoration.SetStaticDefaults();Check(tileMedicineKingCauldronDecoration.Drop==Terraria.ModLoader.ModContent.ItemType<XianXia.Content.Items.HandGenerated.MedicineKingCauldronDecoration>(),"MedicineKingCauldronDecoration returns same item");
var tileSilentTabletDecoration=new XianXia.Content.Tiles.BossDecorations.SilentTabletDecorationTile();tileSilentTabletDecoration.SetStaticDefaults();Check(tileSilentTabletDecoration.Drop==Terraria.ModLoader.ModContent.ItemType<XianXia.Content.Items.HandGenerated.SilentTabletDecoration>(),"SilentTabletDecoration returns same item");
Console.WriteLine($"Master monument actual-hook regression passed: {assertions} assertions; mocked engine registration boundary.");
class TestItem:MasterBossMonument { protected override int MonumentTile=>7; }
class TestTile:MasterBossMonumentTile { protected override int MonumentItem=>91; }
