using System;
using System.Collections.Generic;
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
    private static void AuditForgeRequirements(Action<bool,string> check)
    {
        Player original=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        var clients=(RemoteClient[])Netplay.Clients.Clone();bool broadcast=NetMessage.buffer[0].broadcast;
        var world=ModContent.GetInstance<DownedBossSystem>();var savedWorld=new TagCompound();world.SaveWorldData(savedWorld);
        int x=Main.maxTilesX/2,y=12;Tile tile=Main.tile[x,y];bool has=tile.HasTile;ushort oldType=tile.TileType;
        var output=new AuditOutputSocket(true);var mod=ModContent.GetInstance<global::XianXia.XianXia>();using var probe=mod.GetPacket();int offset=(int)probe.BaseStream.Position;
        int Type(string name)=>mod.Find<ModItem>(name).Type;
        byte[] Inventory(Player actor) {using var stream=new MemoryStream();TagIO.ToStream(new TagCompound {["items"]=actor.inventory.Take(58).Select(ItemIO.Save).ToList()},stream,false);return stream.ToArray();}
        int Stones(Player actor)=>actor.inventory.Take(58).Where(i=>i.type==Type("LowGradeSpiritStone")).Sum(i=>i.stack);
        var operations=new List<(string Tool,string Target,byte Level,InscriptionKind Before,InscriptionKind After,int Cost,bool Awake)>();
        string[] needles={"GreenwoodInscriptionNeedle","FurnaceInscriptionNeedle","ThunderInscriptionNeedle","StarAbyssInscriptionNeedle","BrokenHeavenInscriptionNeedle"};
        for(int kind=1;kind<=5;kind++)operations.Add((needles[kind-1],"CloudpiercerFlyingSword",0,InscriptionKind.None,(InscriptionKind)kind,3,false));
        operations.Add(("InscriptionRemovalStone","CloudpiercerFlyingSword",2,InscriptionKind.Greenwood,InscriptionKind.None,0,false));
        for(byte level=0;level<3;level++)operations.Add(("ArtifactQuenchingCrystal","CloudpiercerFlyingSword",level,InscriptionKind.Greenwood,InscriptionKind.Greenwood,6+level*6,false));
        foreach(string target in new[]{"CloudpiercerFlyingSword","GreenwoodArrayPlate","HeavenTabletWardSeal","MoonboneDharmaSword","FormlessSwordWheel","BlackFurnaceWarhammer","ThunderPatternSwordCase","CinnabarTalismanFlameItem","StarEclipseArbalest","ThunderTalismanArrayPlate","BrokenHeavenDecree","GreenwoodMedicineCauldron","SectMechanismCrossbow","HeavenLawArbalest","StarCalamityMechanismCase"})operations.Add(("ArtifactAwakeningSeal",target,3,InscriptionKind.Greenwood,InscriptionKind.Greenwood,24,true));
        string[] bosses={"black_furnace_iron_golem","thunder_marsh_jiao","formless_sword_soul","greenwood_medicine_king_echo","moonbone_immortal"};
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i==0?output:new AuditOutputSocket(),State=i==0?10:0,IsActive=i==0};NetMessage.buffer[0].broadcast=true;
            foreach(int netMode in new[]{NetmodeID.SinglePlayer,NetmodeID.Server})foreach(var op in operations)for(int scenario=0;scenario<9;scenario++) {
                Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(326);
                world.LoadWorldData(savedWorld);foreach(string boss in bosses)DownedBossSystem.DownedBosses.Add(boss);
                tile.HasTile=true;string[] stations={"ArtifactForgeTile","ThunderPatternForgeTile","HeavenFireFurnaceTile","DaoSeveringAltarTile"};tile.TileType=(ushort)mod.Find<ModTile>(stations[Math.Min(scenario,3)]).Type;
                var actor=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};actor.Center=new Microsoft.Xna.Framework.Vector2(x*16+8,y*16+8);Main.player[0]=actor;
                var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=CultivationStage.DaoSevering;state.discoveredSpiritualEnergy=true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native forge fixture imports valid registered player state");
                actor.inventory[0]=new Item(Type(op.Tool),2);actor.inventory[1]=new Item(Type(op.Target));Item target=actor.inventory[1];target.Prefix(PrefixID.Godly);int prefix=target.prefix;
                target.GetGlobalItem<InscribedEquipment>().SetKind(op.Before);target.GetGlobalItem<RefinedArtifact>().SetLevel(op.Level);
                int total=op.Cost+2;string expected="";
                if(scenario==4){tile.HasTile=false;expected="Mods.XianXia.Inscriptions.NeedForge";}
                if(scenario==5) {
                    if(op.Cost>0){total=op.Cost-1;expected="Mods.XianXia.Inscriptions.NeedStones";}
                    else {target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.None);expected="Mods.XianXia.Inscriptions.NoChange";}
                }
                if(scenario==6) {
                    if(op.Awake){DownedBossSystem.DownedBosses.Remove("greenwood_medicine_king_echo");expected="Mods.XianXia.Refinement.AwakeningRequirements";}
                    else if(op.Tool=="ArtifactQuenchingCrystal"){DownedBossSystem.DownedBosses.Remove(RefinementRules.RequiredBoss(op.Level));expected="Mods.XianXia.Refinement.Requirements";}
                    else {target.GetGlobalItem<InscribedEquipment>().SetKind(op.After);expected="Mods.XianXia.Inscriptions.NoChange";}
                }
                if(scenario==7) {
                    if(op.Target=="MoonboneDharmaSword"){state.cultivationStage=CultivationStage.SpiritSevering;expected="Mods.XianXia.Refinement.MoonRequirements";}
                    else if(op.Awake){state.cultivationStage=CultivationStage.None;expected="Mods.XianXia.Refinement.AwakeningRequirements";}
                    else if(op.Tool=="ArtifactQuenchingCrystal"){state.cultivationStage=CultivationStage.None;expected="Mods.XianXia.Refinement.Requirements";}
                    else {target.stack=2;expected="Mods.XianXia.Inscriptions.ChangedInventory";}
                }
                if(scenario==8) {
                    if(op.Target=="MoonboneDharmaSword"){DownedBossSystem.DownedBosses.Remove("moonbone_immortal");expected="Mods.XianXia.Refinement.MoonRequirements";}
                    else if(op.Awake){target.GetGlobalItem<RefinedArtifact>().TryAwaken();expected="Mods.XianXia.Refinement.AwakeningRequirements";}
                    else if(op.Tool=="ArtifactQuenchingCrystal"){target.GetGlobalItem<RefinedArtifact>().SetLevel(3);expected="Mods.XianXia.Refinement.Requirements";}
                    else {target.stack=2;expected="Mods.XianXia.Inscriptions.ChangedInventory";}
                }
                if(total>0)actor.inventory[2]=new Item(Type("LowGradeSpiritStone"),1);if(total>1)actor.inventory[3]=new Item(Type("LowGradeSpiritStone"),total-1);
                var material=(InscriptionToolItem)actor.inventory[0].ModItem;byte prior=(byte)InscribedEquipment.GetKind(target);byte level=RefinedArtifact.GetLevel(target);bool awakened=RefinedArtifact.IsAwakened(target);
                byte[] before=Inventory(actor);var resources=state.CaptureSnapshot();
                check(InscriptionTransactions.RequirementFailure(actor,material,target)==expected,"native forge requirement preview matches ready station material and progression gates");
                Main.netMode=NetmodeID.MultiplayerClient;check(InscriptionTransactions.RequirementFailure(actor,material,target)==expected,"native client forge preview shares authority requirement result");Main.netMode=netMode;
                check(Inventory(actor).SequenceEqual(before)&&state.CaptureSnapshot().Equals(resources),"native forge preview never spends materials or changes equipment resources");
                output.Sent.Clear();InscriptionTransactions.HandleRequest(actor,0,actor.inventory[0].type,1,target.type,target.prefix,prior,level,awakened);
                if(expected.Length>0)check(Inventory(actor).SequenceEqual(before),"native rejected forge request preserves all fifty-eight inventory slots");
                else {
                    check(actor.inventory[0].stack==1&&Stones(actor)==total-op.Cost&&actor.inventory[2].IsAir==(op.Cost>0),"native forge transaction consumes exactly one tool and specified stones across stacks");
                    check(ReferenceEquals(actor.inventory[1],target)&&target.prefix==prefix&&InscribedEquipment.GetKind(target)==op.After&&RefinedArtifact.GetLevel(target)==(op.Tool=="ArtifactQuenchingCrystal"?op.Level+1:op.Level)&&RefinedArtifact.IsAwakened(target)==op.Awake,"native forge success preserves target prefix and intended inscription refinement awakening");
                }
                if(netMode==NetmodeID.Server) {
                    byte[] reply=output.Sent.Single(p=>p.Length>offset&&p[offset]==7);using var reader=new BinaryReader(new MemoryStream(reply[(offset+1)..]));
                    string key=expected.Length>0?expected:op.Awake?"Mods.XianXia.Refinement.AwakeningSuccess":op.Tool=="ArtifactQuenchingCrystal"?"Mods.XianXia.Refinement.Success":op.After==InscriptionKind.None?"Mods.XianXia.Inscriptions.Removed":"Mods.XianXia.Inscriptions.Success";
                    check(NetworkText.Deserialize(reader).ToString()==NetworkText.FromKey(key).ToString(),"native server forge reply matches preview refusal or actual success result");
                }
                byte[] after=Inventory(actor);int packets=output.Sent.Count;InscriptionTransactions.HandleRequest(actor,0,actor.inventory[0].type,1,target.type,target.prefix,prior,level,awakened);
                check(Inventory(actor).SequenceEqual(after)&&output.Sent.Count==packets,"native forge repeated same-frame confirmation neither spends again nor sends duplicate result");
            }
        } finally {
            tile.HasTile=has;tile.TileType=oldType;world.LoadWorldData(savedWorld);Main.player[0]=original;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;clients.CopyTo(Netplay.Clients,0);NetMessage.buffer[0].broadcast=broadcast;
        }
        check(ReferenceEquals(Main.player[0],original)&&tile.HasTile==has&&tile.TileType==oldType,"native forge requirement audit restores player tile and world context");
    }
}
