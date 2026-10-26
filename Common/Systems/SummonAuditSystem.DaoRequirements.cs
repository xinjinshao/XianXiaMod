using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditDaoRequirements(Action<bool,string> check)
    {
        Player original=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        var clients=(RemoteClient[])Netplay.Clients.Clone();bool broadcast=NetMessage.buffer[0].broadcast;
        var world=ModContent.GetInstance<DownedBossSystem>();var savedWorld=new TagCompound();world.SaveWorldData(savedWorld);
        int x=Main.maxTilesX/2,y=12;Tile tile=Main.tile[x,y];bool has=tile.HasTile;ushort oldType=tile.TileType;
        var output=new AuditOutputSocket(true);var mod=ModContent.GetInstance<global::XianXia.XianXia>();using var probe=mod.GetPacket();int offset=(int)probe.BaseStream.Position;
        int Type(string name)=>mod.Find<ModItem>(name).Type;
        byte[] Inventory(Player actor) {using var stream=new MemoryStream();TagIO.ToStream(new TagCompound {["items"]=actor.inventory.Take(58).Select(ItemIO.Save).ToList()},stream,false);return stream.ToArray();}
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i==0?output:new AuditOutputSocket(),State=i==0?10:0,IsActive=i==0};NetMessage.buffer[0].broadcast=true;
            foreach(int netMode in new[]{NetmodeID.SinglePlayer,NetmodeID.Server})foreach(int route in new[]{1,2,3})
            foreach(string name in new[]{"CloudpiercerFlyingSword","GreenwoodArrayPlate","MoonboneDharmaSword","FormlessSwordWheel","BlackFurnaceWarhammer","ThunderPatternSwordCase","HeavenTabletWardSeal","CinnabarTalismanFlameItem","WoodgrainFlyingSword","SpiritwoodCrossbow","StarEclipseArbalest","BrokenHeavenDecree","ArchiveStarCodex"})
            for(int scenario=0;scenario<12;scenario++) {
                Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(327);
                world.LoadWorldData(new TagCompound {["chosenRoute"]=route});DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                tile.HasTile=true;tile.TileType=(ushort)mod.Find<ModTile>("DaoSeveringAltarTile").Type;
                var actor=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};actor.Center=new Microsoft.Xna.Framework.Vector2(x*16+8,y*16+8);Main.player[0]=actor;
                var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=CultivationStage.DaoSevering;state.discoveredSpiritualEnergy=true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Dao readiness fixture imports valid registered player state");
                actor.inventory[0]=new Item(Type("DaoTransformationSeal"),2);actor.inventory[1]=new Item(Type(name));Item target=actor.inventory[1];target.Prefix(PrefixID.Godly);int prefix=target.prefix;
                target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);var growth=target.GetGlobalItem<RefinedArtifact>();growth.SetLevel(3);growth.TryAwaken();
                int total=38;string expected="";const string key="Mods.XianXia.DaoArtifacts.";
                switch(scenario) {
                    case 1:tile.HasTile=false;expected=key+"NeedAltar";break;
                    case 2:total=35;expected=key+"NeedStones";break;
                    case 3:target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.None);expected=key+"NeedInscription";break;
                    case 4:growth.SetLevel(2);expected=key+"NeedAwakening";break;
                    case 5:growth.SetLevel(2);growth.SetLevel(3);expected=key+"NeedAwakening";break;
                    case 6:growth.TryTransform((DownedBossSystem.EndgameRoute)route);expected=key+"AlreadyTransformed";break;
                    case 7:state.cultivationStage=CultivationStage.Tribulation;expected=key+"NeedRealm";break;
                    case 8:DownedBossSystem.DownedBosses.Remove("old_heaven_dao_core");expected=key+"NeedCore";break;
                    case 9:world.LoadWorldData(new TagCompound());DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");expected=key+"NeedWorldRoute";break;
                    case 10:target.stack=2;expected=key+"ChangedInventory";break;
                }
                actor.inventory[2]=new Item(Type("LowGradeSpiritStone"),1);actor.inventory[3]=new Item(Type("LowGradeSpiritStone"),total-1);
                byte kind=(byte)InscribedEquipment.GetKind(target),level=RefinedArtifact.GetLevel(target),previous=(byte)RefinedArtifact.GetDaoRoute(target),previewRoute=(byte)DownedBossSystem.ChosenRoute;bool awake=RefinedArtifact.IsAwakened(target);
                byte[] before=Inventory(actor);var resources=state.CaptureSnapshot();
                check(DaoArtifactTransactions.RequirementFailure(actor,target)==expected,"native Dao preview returns precise first unmet condition or ready");
                Main.netMode=NetmodeID.MultiplayerClient;check(DaoArtifactTransactions.RequirementFailure(actor,target)==expected,"native client Dao preview shares canonical authority gates");Main.netMode=netMode;
                check(Inventory(actor).SequenceEqual(before)&&state.CaptureSnapshot().Equals(resources),"native Dao preview preserves all inventory slots and resource state");
                if(scenario==11) {world.LoadWorldData(new TagCompound {["chosenRoute"]=route%3+1});DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");expected=key+"ChangedInventory";}
                var chosen=DownedBossSystem.ChosenRoute;
                output.Sent.Clear();DaoArtifactTransactions.Handle(actor,0,actor.inventory[0].type,1,target.type,target.prefix,kind,level,awake,previous,previewRoute);
                if(expected.Length>0)check(Inventory(actor).SequenceEqual(before),"native Dao unmet condition or stale world route spends no materials and preserves all slots");
                else check(actor.inventory[0].stack==1&&actor.inventory[2].IsAir&&actor.inventory[3].stack==2&&target.prefix==prefix&&ReferenceEquals(target,actor.inventory[1])&&growth.Level==3&&growth.Awakened&&growth.DaoRoute==(DownedBossSystem.EndgameRoute)route&&InscribedEquipment.GetKind(target)==InscriptionKind.Greenwood,"native Dao ready confirmation consumes exact materials and preserves growth inscription and prefix");
                check(DownedBossSystem.ChosenRoute==chosen&&state.CaptureSnapshot().Equals(resources),"native Dao item transaction never changes world route or player resources");
                if(netMode==NetmodeID.Server) {
                    byte[] reply=output.Sent.Single(p=>p.Length>offset&&p[offset]==7);using var reader=new BinaryReader(new MemoryStream(reply[(offset+1)..]));
                    check(NetworkText.Deserialize(reader).ToString()==NetworkText.FromKey(expected.Length>0?expected:key+"Success").ToString(),"native Dao server reply agrees with precise requirement or stale preview refusal");
                }
                byte[] after=Inventory(actor);int packets=output.Sent.Count;DaoArtifactTransactions.Handle(actor,0,actor.inventory[0].type,1,target.type,target.prefix,kind,level,awake,previous,previewRoute);
                check(Inventory(actor).SequenceEqual(after)&&output.Sent.Count==packets,"native Dao repeat confirmation cannot spend twice or repeat reply");
            }
        } finally {
            tile.HasTile=has;tile.TileType=oldType;world.LoadWorldData(savedWorld);Main.player[0]=original;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;clients.CopyTo(Netplay.Clients,0);NetMessage.buffer[0].broadcast=broadcast;
        }
        check(ReferenceEquals(Main.player[0],original)&&tile.HasTile==has&&tile.TileType==oldType,"native Dao readiness audit restores player tile world and connections");
    }
}
