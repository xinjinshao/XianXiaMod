using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditRouteChoices(Action<bool,string> check)
    {
        Player[] originals=Main.player.Take(2).ToArray();int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        var clients=(RemoteClient[])Netplay.Clients.Clone();bool[] broadcasts=NetMessage.buffer.Take(2).Select(b=>b.broadcast).ToArray();
        var world=ModContent.GetInstance<DownedBossSystem>();var savedWorld=new TagCompound();world.SaveWorldData(savedWorld);
        int x=Main.maxTilesX/2,y=12;Tile tile=Main.tile[x,y];bool has=tile.HasTile;ushort oldType=tile.TileType;
        var outputs=new[]{new AuditOutputSocket(true),new AuditOutputSocket(true)};var mod=ModContent.GetInstance<global::XianXia.XianXia>();using var probe=mod.GetPacket();int offset=(int)probe.BaseStream.Position;
        int material=mod.Find<ModItem>("RouteMaterial").Type;
        byte[] Inventory(Player actor) {using var stream=new MemoryStream();TagIO.ToStream(new TagCompound {["items"]=actor.inventory.Take(58).Select(ItemIO.Save).ToList()},stream,false);return stream.ToArray();}
        bool ReplyIs(int peer,string key) {byte[] packet=outputs[peer].Sent.Last(p=>p.Length>offset&&p[offset]==7);using var reader=new BinaryReader(new MemoryStream(packet[(offset+1)..]));return NetworkText.Deserialize(reader).ToString()==NetworkText.FromKey(key).ToString();}
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i<2?outputs[i]:new AuditOutputSocket(),State=i<2?10:0,IsActive=i<2};for(int i=0;i<2;i++)NetMessage.buffer[i].broadcast=true;
            foreach(int netMode in new[]{NetmodeID.SinglePlayer,NetmodeID.Server})foreach(int stage in new[]{7,8})
            foreach(byte first in new byte[]{1,2,3})foreach(byte second in new byte[]{1,2,3})for(int gate=0;gate<4;gate++) {
                Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(328);world.LoadWorldData(new TagCompound());DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                tile.HasTile=true;tile.TileType=(ushort)mod.Find<ModTile>("DaoSeveringAltarTile").Type;
                for(int peer=0;peer<2;peer++) {
                    var actor=new Player {whoAmI=peer,active=true,statLife=100,selectedItem=0};actor.Center=new Microsoft.Xna.Framework.Vector2(x*16+8,y*16+8);Main.player[peer]=actor;actor.inventory[0]=new Item(material);
                    var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=(CultivationStage)stage;state.discoveredSpiritualEnergy=true;check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native route confirmation fixture imports each participant state");outputs[peer].Sent.Clear();
                }
                string expected="";
                if(gate==1){DownedBossSystem.DownedBosses.Remove("old_heaven_dao_core");expected="Mods.XianXia.Routes.NeedCore";}
                if(gate==2){Main.player[0].GetModPlayer<XianXiaPlayer>().cultivationStage=CultivationStage.SpiritSevering;expected="Mods.XianXia.Routes.NeedRealm";}
                if(gate==3){Main.player[0].Center+=new Microsoft.Xna.Framework.Vector2(400,0);expected="Mods.XianXia.Routes.NeedAltar";}
                byte[][] before=Main.player.Take(2).Select(Inventory).ToArray();
                check(EndgameRouteTransactions.RequirementFailure(Main.player[0])==expected,"native route shared preview identifies core realm altar or ready condition");
                Main.netMode=NetmodeID.MultiplayerClient;check(EndgameRouteTransactions.RequirementFailure(Main.player[0])==expected,"native client route preview matches authority requirement result");Main.netMode=netMode;
                check(Main.player.Take(2).Select((p,i)=>Inventory(p).SequenceEqual(before[i])).All(v=>v)&&DownedBossSystem.ChosenRoute==DownedBossSystem.EndgameRoute.None,"native route browsing requirements neither spend materials nor choose a world route");
                EndgameRouteTransactions.Handle(Main.player[0],0,material,first);
                check(DownedBossSystem.ChosenRoute==(gate==0?(DownedBossSystem.EndgameRoute)first:DownedBossSystem.EndgameRoute.None)&&Main.player[0].inventory[0].IsAir==(gate==0)&&Main.player[1].inventory[0].type==material,"native first route request only commits and spends when all requirements are met");
                if(gate!=0&&netMode==NetmodeID.Server)check(ReplyIs(0,expected),"native route server refusal uses exact shared preview reason");
                byte[] afterFirst=Inventory(Main.player[0]);int sent=outputs.Sum(o=>o.Sent.Count);EndgameRouteTransactions.Handle(Main.player[0],0,material,first);
                check(Inventory(Main.player[0]).SequenceEqual(afterFirst)&&outputs.Sum(o=>o.Sent.Count)==sent,"native repeated first route request cannot repeat spending or output");
                DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                check(EndgameRouteTransactions.RequirementFailure(Main.player[1])==(gate==0?"Mods.XianXia.Routes.AlreadyChosen":""),"native second participant sees committed route lock or readiness after first refusal");
                EndgameRouteTransactions.Handle(Main.player[1],0,material,second);
                var chosen=(DownedBossSystem.EndgameRoute)(gate==0?first:second);
                check(DownedBossSystem.ChosenRoute==chosen&&Main.player[0].inventory[0].IsAir==(gate==0)&&Main.player[1].inventory[0].IsAir==(gate!=0),"native same-tick competing route requests consume exactly one participant material and preserve first valid choice");
                int loser=gate==0?1:0;Main.player[loser].GetModPlayer<InscriptionPlayer>().Initialize();byte[] loserBefore=Inventory(Main.player[loser]);
                EndgameRouteTransactions.Handle(Main.player[loser],0,material,(byte)((int)chosen%3+1));
                check(Inventory(Main.player[loser]).SequenceEqual(loserBefore)&&DownedBossSystem.ChosenRoute==chosen,"native route lock rejects losing participant after cooldown reset without material loss");
                if(netMode==NetmodeID.Server)check(ReplyIs(loser,"Mods.XianXia.Routes.AlreadyChosen"),"native losing participant receives permanent world lock feedback");
                var committed=new TagCompound();world.SaveWorldData(committed);world.LoadWorldData(committed);
                check(DownedBossSystem.ChosenRoute==chosen&&EndgameRouteTransactions.RequirementFailure(Main.player[loser])=="Mods.XianXia.Routes.AlreadyChosen","native chosen route remains locked after actual world-data save and reload hooks");
            }
        } finally {
            tile.HasTile=has;tile.TileType=oldType;world.LoadWorldData(savedWorld);for(int i=0;i<2;i++){Main.player[i]=originals[i];NetMessage.buffer[i].broadcast=broadcasts[i];}Main.netMode=mode;Main.myPlayer=local;Main.rand=random;clients.CopyTo(Netplay.Clients,0);
        }
        check(Main.player.Take(2).SequenceEqual(originals)&&tile.HasTile==has&&tile.TileType==oldType,"native competing route audit restores both participants tile world and connections");
    }
}
