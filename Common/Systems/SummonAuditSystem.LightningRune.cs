using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Buffs;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditLightningRune(Action<bool,string> check)
    {
        Player previous=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var connection=Netplay.Connection;var random=Main.rand;
        var clients=(RemoteClient[])Netplay.Clients.Clone();bool broadcast=NetMessage.buffer[0].broadcast;
        var mod=ModContent.GetInstance<global::XianXia.XianXia>();using var probe=mod.GetPacket();int offset=(int)probe.BaseStream.Position;
        var output=new AuditOutputSocket(true);int type=ModContent.ItemType<LightningAvoidanceRune>(),buff=ModContent.BuffType<TribulationResistanceBuff>();
        void Deliver(byte[] payload,int sender) {using var reader=new BinaryReader(new MemoryStream(payload));mod.HandlePacket(reader,sender);}
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i==0?output:new AuditOutputSocket(),State=i==0?10:0,IsActive=i==0};NetMessage.buffer[0].broadcast=true;
            foreach(int scenario in Enumerable.Range(0,13)) {
                Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=0;Main.rand=new Terraria.Utilities.UnifiedRandom(331);
                var requestOutput=new AuditOutputSocket(true);Netplay.Connection=new RemoteServer();Netplay.Connection.Socket=requestOutput;
                var actor=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};Main.player[0]=actor;actor.inventory[0]=new Item(type,2);
                actor.inventory[0].ModItem.UseItem(actor);
                check(!actor.HasBuff(buff)&&actor.inventory[0].stack==2&&!ModContent.GetInstance<CultivationItemTransactions>().ConsumeItem(actor.inventory[0],actor),"native client rune requests authority without local buff or item consumption");
                byte[] request=requestOutput.Sent.Single(p=>p.Length>offset&&p[offset]==5)[offset..];
                check(request.Length==6&&request[1]==0&&BitConverter.ToInt32(request,2)==type,"native rune client emits existing held-slot and item-type request");
                Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;
                actor=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};Main.player[0]=actor;actor.inventory[0]=new Item(type,2);
                var state=actor.GetModPlayer<XianXiaPlayer>();check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native rune authority fixture imports canonical character state");
                if(scenario==1)actor.AddBuff(buff,1799);if(scenario==2)actor.AddBuff(buff,1800);if(scenario==3)actor.AddBuff(buff,5400);
                if(scenario==4)actor.buffImmune[buff]=true;if(scenario==5)actor.dead=true;if(scenario==6)actor.noItems=true;if(scenario==7)actor.frozen=true;
                if(scenario==8)actor.selectedItem=1;if(scenario==9)actor.inventory[0]=new Item(mod.Find<ModItem>("LowGradeSpiritStone").Type,2);
                if(scenario==10)actor.inventory[0].stack=0;if(scenario==11)state.ResetNetworkSession();if(scenario==12)actor.active=false;
                int oldStack=actor.inventory[0].stack,oldBuff=actor.FindBuffIndex(buff);int oldDuration=oldBuff<0?0:actor.buffTime[oldBuff];uint revision=state.ResourceRevision;
                bool success=scenario<2;output.Sent.Clear();Deliver(request,0);
                int index=actor.FindBuffIndex(buff);
                check(actor.inventory[0].stack==oldStack-(success?1:0)&&(index<0?0:actor.buffTime[index])==(success?1800:oldDuration),"native rune authority consumes once only for a new or refreshed thirty-second effect");
                check(state.ResourceRevision==revision+(success?1:0)&&!state.ApplyingProgressionItem&&!PillQualitySystem.IsPill(new Item(type)),"native rune preserves authority revision and reentry guard without pill quality classification");
                byte[][] buffs=output.Sent.Where(p=>p.Length>2&&p[2]==MessageID.AddPlayerBuff).ToArray();
                check(buffs.Length==(success?1:0),"native rune sends one changed resistance buff and no effect packet on refusal");
                byte[][] slots=output.Sent.Where(p=>p.Length>offset&&p[offset]==13).ToArray();
                if(success) {
                    check(slots.Length==1&&AuthoritativeBuffSync.Capture(actor).Count==1,"native rune success sends one owner correction with no fatigue or quality side effect");
                    Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=0;Netplay.Connection=new RemoteServer();
                    var receiver=new Player {whoAmI=0,active=true,statLife=100};receiver.inventory[0]=new Item(type,2);Main.player[0]=receiver;
                    for(int replay=0;replay<2;replay++) {
                        foreach(byte[] packet in buffs) {
                            byte[] payload=packet[2..];using var reader=new BinaryReader(new MemoryStream(payload));var buffer=new MessageBuffer {whoAmI=256,readBuffer=payload,reader=reader};buffer.GetData(0,payload.Length,out int kind);
                            check(kind==MessageID.AddPlayerBuff,"official rune owner decoder accepts actual authority buff packet");
                        }
                        foreach(byte[] packet in slots)Deliver(packet[offset..],256);
                    }
                    check(receiver.inventory[0].stack==1&&receiver.HasBuff(buff)&&receiver.buffTime[receiver.FindBuffIndex(buff)]==1800,"official rune owner replay converges to one remaining item and exact thirty-second resistance");
                    Main.player[0]=actor;Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;
                }
                int sent=output.Sent.Count;int stack=actor.inventory[0].stack;Deliver(request,0);
                check(actor.inventory[0].stack==stack&&output.Sent.Count==sent,"native rune repeated request neither consumes again nor resends effects");
            }
        } finally {
            Main.player[0]=previous;Main.netMode=mode;Main.myPlayer=local;Netplay.Connection=connection;Main.rand=random;clients.CopyTo(Netplay.Clients,0);NetMessage.buffer[0].broadcast=broadcast;
        }
        check(ReferenceEquals(Main.player[0],previous)&&ReferenceEquals(Netplay.Connection,connection)&&NetMessage.buffer[0].broadcast==broadcast,"native rune audit restores owner connection and broadcast context");
    }
}
