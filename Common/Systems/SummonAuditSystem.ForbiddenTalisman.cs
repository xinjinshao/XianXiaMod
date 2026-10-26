using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Buffs;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditForbiddenTalisman(Action<bool,string> check)
    {
        Player previous=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var clients=(RemoteClient[])Netplay.Clients.Clone();var connection=Netplay.Connection;var random=Main.rand;bool broadcast=NetMessage.buffer[0].broadcast;
        var mod=ModContent.GetInstance<global::XianXia.XianXia>();using var probe=mod.GetPacket();int offset=(int)probe.BaseStream.Position;var output=new AuditOutputSocket(true);
        int type=ModContent.ItemType<StarAbyssForbiddenTalisman>(),disorder=ModContent.BuffType<SpiritualPressureDisorderBuff>();
        void Deliver(byte[] payload,int sender) {using var reader=new BinaryReader(new MemoryStream(payload));mod.HandlePacket(reader,sender);}
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i==0?output:new AuditOutputSocket(),State=i==0?10:0,IsActive=i==0};NetMessage.buffer[0].broadcast=true;
            foreach(int energy in new[]{0,40,119,120})foreach(int pressure in new[]{0,54,55,75,100})foreach(int stacks in new[]{1,2}) {
                Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(333);
                var actor=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};Main.player[0]=actor;var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=CultivationStage.Foundation;state.discoveredSpiritualEnergy=true;state.ResetEffects();state.spiritualEnergy=energy;state.spiritPressure=pressure;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native forbidden talisman fixture imports valid resource state");var before=state.CaptureSnapshot();actor.inventory[0]=new Item(type,stacks);
                Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=0;var requests=new AuditOutputSocket(true);Netplay.Connection=new RemoteServer();Netplay.Connection.Socket=requests;
                var requester=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};Main.player[0]=requester;requester.GetModPlayer<XianXiaPlayer>().ApplySnapshot(before);requester.inventory[0]=new Item(type,stacks);requester.inventory[0].ModItem.UseItem(requester);
                check(requester.GetModPlayer<XianXiaPlayer>().CaptureSnapshot().Equals(before)&&requester.inventory[0].stack==stacks&&!ModContent.GetInstance<CultivationItemTransactions>().ConsumeItem(requester.inventory[0],requester),"native client forbidden talisman sends request without local recovery pressure or consumption");
                byte[] request=requests.Sent.Single(p=>p.Length>offset&&p[offset]==5)[offset..];
                Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;Main.player[0]=actor;output.Sent.Clear();Deliver(request,0);
                bool success=energy<120;int expectedEnergy=success?Math.Min(120,energy+80):energy,expectedPressure=success?Math.Min(100,pressure+25):pressure,remaining=stacks-(success?1:0);
                check(state.spiritualEnergy==expectedEnergy&&state.spiritPressure==expectedPressure&&(remaining==0?actor.inventory[0].IsAir:actor.inventory[0].type==type&&actor.inventory[0].stack==remaining),"native forbidden talisman restores capped energy and charges item and pressure only for useful use");
                bool risk=success&&expectedPressure>=80;int index=actor.FindBuffIndex(disorder);
                check((index>=0)==risk&&(!risk||actor.buffTime[index]==480),"native forbidden talisman preserves exact eighty-pressure boundary and eight-second disorder");
                check(state.ResourceRevision==before.Revision+(success?1:0)&&!state.ApplyingProgressionItem&&!PillQualitySystem.IsPill(new Item(type)),"native forbidden talisman updates resource revision once without pill bonuses or stuck reentry");
                byte[][] buffs=output.Sent.Where(p=>p.Length>2&&p[2]==MessageID.AddPlayerBuff).ToArray();
                check(buffs.Length==(risk?1:0),"native forbidden talisman sends disorder only when useful recovery crosses or retains risk threshold");
                byte[][] canonical=output.Sent.Where(p=>p.Length>offset&&(p[offset]==13||p[offset]==4)).ToArray();
                check(canonical.Count(p=>p[offset]==13)==1&&canonical.Count(p=>p[offset]==4)==1,"native forbidden talisman success and full-energy refusal both return canonical item and resources");
                Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=0;Netplay.Connection=new RemoteServer();
                var receiver=new Player {whoAmI=0,active=true,statLife=100};receiver.inventory[0]=new Item(type,stacks);Main.player[0]=receiver;receiver.GetModPlayer<XianXiaPlayer>().ApplySnapshot(before);
                for(int replay=0;replay<2;replay++) {
                    foreach(byte[] packet in canonical)Deliver(packet[offset..],256);
                    foreach(byte[] packet in buffs) {byte[] payload=packet[2..];using var reader=new BinaryReader(new MemoryStream(payload));var buffer=new MessageBuffer {whoAmI=256,readBuffer=payload,reader=reader};buffer.GetData(0,payload.Length,out int kind);check(kind==55,"official forbidden talisman owner decodes actual disorder buff packet");}
                }
                var received=receiver.GetModPlayer<XianXiaPlayer>();int receivedBuff=receiver.FindBuffIndex(disorder);
                check(received.CaptureSnapshot().Equals(state.CaptureSnapshot())&&(remaining==0?receiver.inventory[0].IsAir:receiver.inventory[0].stack==remaining)&&(receivedBuff>=0)==risk&&(!risk||receiver.buffTime[receivedBuff]==480),"actual owner packet replay converges forbidden talisman inventory resources and risk effect without duplication");
                Main.player[0]=actor;Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;int sent=output.Sent.Count;var after=state.CaptureSnapshot();Deliver(request,0);
                check(state.CaptureSnapshot().Equals(after)&&output.Sent.Count==sent&&(remaining==0?actor.inventory[0].IsAir:actor.inventory[0].stack==remaining),"native forbidden talisman repeated request cannot duplicate recovery cost or risk effects");
            }
            Main.netMode=NetmodeID.SinglePlayer;Main.myPlayer=Main.maxPlayers;
            var beginner=new Player {whoAmI=0,active=true,statLife=100};Main.player[0]=beginner;var beginnerState=beginner.GetModPlayer<XianXiaPlayer>();Item firstUse=new Item(type);
            check(firstUse.ModItem.CanUseItem(beginner)&&firstUse.ModItem.UseItem(beginner)==true&&beginnerState.discoveredSpiritualEnergy&&beginnerState.spiritualEnergy==40&&beginnerState.spiritPressure==25,"native forbidden talisman retains original first-use discovery without adding a realm gate");
        } finally {Main.player[0]=previous;Main.netMode=mode;Main.myPlayer=local;clients.CopyTo(Netplay.Clients,0);Netplay.Connection=connection;Main.rand=random;NetMessage.buffer[0].broadcast=broadcast;}
        check(ReferenceEquals(Main.player[0],previous)&&ReferenceEquals(Netplay.Connection,connection)&&NetMessage.buffer[0].broadcast==broadcast,"native forbidden talisman audit restores owner connection and engine context");
    }
}
