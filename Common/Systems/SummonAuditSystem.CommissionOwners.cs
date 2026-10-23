using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.NPCs.Town;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditCommissionOwners(Action<bool,string> check)
    {
        Player[] players=Main.player.Take(2).ToArray();Item[] items=(Item[])Main.item.Clone();int[] reuse=(int[])Main.timeItemSlotCannotBeReusedFor.Clone();
        int slot=Enumerable.Range(0,Main.maxNPCs).First(i=>!Main.npc[i].active);NPC previous=Main.npc[slot];int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        bool hard=Main.hardMode,plant=NPC.downedPlantBoss,golem=NPC.downedGolemBoss,moon=NPC.downedMoonlord;bool itemText=Main.showItemText;
        var clients=(RemoteClient[])Netplay.Clients.Clone();var connection=Netplay.Connection;bool[] broadcast=NetMessage.buffer.Take(2).Select(b=>b.broadcast).ToArray();
        var world=ModContent.GetInstance<DownedBossSystem>();var saved=new TagCompound();world.SaveWorldData(saved);var mod=ModContent.GetInstance<global::XianXia.XianXia>();
        var outputs=new[]{new AuditOutputSocket(true),new AuditOutputSocket(true)};
        var grab=typeof(Player).GetMethod("GrabItems",BindingFlags.Instance|BindingFlags.NonPublic)!;
        try {
            Main.hardMode=NPC.downedPlantBoss=NPC.downedGolemBoss=NPC.downedMoonlord=true;Main.showItemText=false;
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i<2?outputs[i]:new AuditOutputSocket(),State=i<2?10:0,IsActive=i<2};for(int i=0;i<2;i++)NetMessage.buffer[i].broadcast=true;
            foreach(var q in CommissionQuestFixtures())foreach(int owner in new[]{0,1}) {
                Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(330);
                world.LoadWorldData(new TagCompound());DownedBossSystem.DownedBosses.Add(q.Boss);if(q.Prior.Length>0)DownedBossSystem.TryClaimCommission(q.Prior,q.PriorReputation);
                for(int i=0;i<Main.item.Length;i++)Main.item[i]=new Item {whoAmI=i};Array.Clear(Main.timeItemSlotCannotBeReusedFor);
                Main.npc[slot]=new NPC {whoAmI=slot};Main.npc[slot].SetDefaults(mod.Find<ModNPC>(q.Npc).Type);Main.npc[slot].whoAmI=slot;Main.npc[slot].active=true;Main.npc[slot].Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,400);
                for(int i=0;i<2;i++) {var actor=new Player {whoAmI=i,active=true,statLife=100};actor.Center=Main.npc[slot].Center;Main.player[i]=actor;actor.GetModPlayer<XianXiaPlayer>().cultivationStage=CultivationStage.DaoSevering;actor.SetTalkNPC(slot);outputs[i].Sent.Clear();}
                ((CultivationTownNPC)Main.npc[slot].ModNPC).ClaimCommissionOnServer(Main.player[owner]);
                var rewards=Enumerable.Range(0,Main.maxItems).Where(i=>Main.item[i].active).Select(i=>(whoAmI:i,Main.item[i].type,Main.item[i].stack,Main.item[i].prefix)).ToArray();
                check(rewards.Length==2&&Main.item.Take(Main.maxItems).Where(i=>i.active).All(i=>i.playerIndexTheItemIsReservedFor==owner),"native commission all ten quests reserve both real reward stacks for either claimant slot");
                byte[][] packets=outputs[0].Sent.Where(p=>p.Length>2&&(p[2]==MessageID.SyncItem||p[2]==MessageID.ItemOwner)).ToArray();
                check(packets.Select(p=>p[2]).SequenceEqual(new byte[]{21,22,21,22})&&outputs[1].Sent.Where(p=>p.Length>2&&(p[2]==21||p[2]==22)).Select(p=>Convert.ToHexString(p)).SequenceEqual(packets.Select(p=>Convert.ToHexString(p))),"native commission broadcasts one spawn then ownership packet per stack identically to both peers");
                for(int peer=0;peer<2;peer++) {
                    Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=peer;Netplay.Connection=new RemoteServer();
                    for(int i=0;i<Main.item.Length;i++)Main.item[i]=new Item {whoAmI=i};
                    var receiver=new Player {whoAmI=peer,active=true,statLife=100};receiver.Center=Main.npc[slot].Center;Main.player[peer]=receiver;
                    for(int replay=0;replay<2;replay++)foreach(byte[] packet in packets) {
                        byte[] payload=packet[2..];using var reader=new BinaryReader(new MemoryStream(payload));var buffer=new MessageBuffer {whoAmI=256,readBuffer=payload,reader=reader};buffer.GetData(0,payload.Length,out int kind);
                        check(kind==packet[2],"official commission receiver decodes actual item spawn and owner packets");
                    }
                    check(rewards.All(r=>Main.item[r.whoAmI].active&&Main.item[r.whoAmI].type==r.type&&Main.item[r.whoAmI].stack==r.stack&&Main.item[r.whoAmI].prefix==r.prefix&&Main.item[r.whoAmI].playerIndexTheItemIsReservedFor==owner)&&Main.item.Take(Main.maxItems).Count(i=>i.active)==2,"official owner and observer replay preserve exactly two correctly reserved rewards");
                    foreach(var reward in rewards){Main.item[reward.whoAmI].position=receiver.position;Main.item[reward.whoAmI].velocity=Microsoft.Xna.Framework.Vector2.Zero;Main.item[reward.whoAmI].noGrabDelay=0;}
                    grab.Invoke(receiver,new object[]{peer});
                    if(peer==owner)check(rewards.All(r=>receiver.inventory.Take(58).Where(i=>i.type==r.type).Sum(i=>i.stack)==r.stack)&&Main.item.Take(Main.maxItems).All(i=>!i.active),"native claimant GrabItems actually picks up both decoded reserved rewards");
                    else check(receiver.inventory.Take(58).All(i=>i.IsAir)&&Main.item.Take(Main.maxItems).Count(i=>i.active)==2,"native bystander GrabItems cannot take decoded rewards reserved for claimant");
                }
            }
        } finally {
            for(int i=0;i<items.Length;i++)Main.item[i]=items[i];reuse.CopyTo(Main.timeItemSlotCannotBeReusedFor,0);for(int i=0;i<2;i++){Main.player[i]=players[i];NetMessage.buffer[i].broadcast=broadcast[i];}Main.npc[slot]=previous;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;clients.CopyTo(Netplay.Clients,0);Netplay.Connection=connection;world.LoadWorldData(saved);
            Main.hardMode=hard;NPC.downedPlantBoss=plant;NPC.downedGolemBoss=golem;NPC.downedMoonlord=moon;Main.showItemText=itemText;
        }
        check(Main.player.Take(2).SequenceEqual(players)&&Main.item.SequenceEqual(items)&&Main.timeItemSlotCannotBeReusedFor.SequenceEqual(reuse),"native commission ownership audit restores participants drops and reuse timers");
    }
}
