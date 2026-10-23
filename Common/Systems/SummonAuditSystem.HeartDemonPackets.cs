using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.NPCs.Enemies;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditHeartDemonPackets(Action<bool,string> check)
    {
        int slot=Enumerable.Range(0,Main.maxNPCs).First(i=>!Main.npc[i].active);NPC oldNpc=Main.npc[slot];Player oldPlayer=Main.player[0];
        int mode=Main.netMode,local=Main.myPlayer,difficulty=Main.GameMode;var clients=(RemoteClient[])Netplay.Clients.Clone();var connection=Netplay.Connection;var random=Main.rand;
        var history=(long[])NpcEncounterIdentitySystem.ClientLastSeen.Clone();var trialHistory=(long[])TribulationEpochSystem.ClientLastSeen.Clone();Array.Clear(TribulationEpochSystem.ClientLastSeen);var output=new AuditOutputSocket(true);
        try {
            Main.rand=new Terraria.Utilities.UnifiedRandom(320);
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient { Id=i,Socket=i==0?output:new AuditOutputSocket(),State=i==0?10:0,IsActive=i==0 };
            var actor=new Player { whoAmI=0,active=true,statLife=100 };actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,400);Main.player[0]=actor;
            var state=actor.GetModPlayer<XianXiaPlayer>();state.tribulationKind=XianXiaPlayer.TribulationKind.HeartDemon;state.tribulationTimer=120;
            var spawn=typeof(XianXiaPlayer).GetMethod("SpawnHeartDemonEnemy",BindingFlags.Instance|BindingFlags.NonPublic)!;
            foreach(int gameMode in new[]{0,1,2})foreach(int stage in new[]{5,3})foreach(bool demon in new[]{true,false}) {
                Main.GameMode=gameMode;Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;Main.npc[slot]=new NPC { whoAmI=slot };state.tribulationStage=stage;
                int type=stage>=5?ModContent.NPCType<StarEclipsedCultivator>():ModContent.NPCType<TribulationCloudling>();var baseline=new NPC();baseline.SetDefaults(type);
                if(demon)spawn.Invoke(state,null);else NPC.NewNPC(new Terraria.DataStructures.EntitySource_SpawnNPC(),(int)actor.Center.X,(int)actor.Center.Y,type);NPC authority=Main.npc[slot];
                check(authority.active&&authority.type==type&&authority.lifeMax==(demon?Math.Max(1,(int)(baseline.lifeMax*0.5f)):baseline.lifeMax)&&authority.damage==(demon?Math.Max(1,baseline.damage*2/3):baseline.damage),"native heart demon reduction and natural spawn control preserve intended combat stats: "+gameMode+"/"+stage+"/"+demon);
                authority.target=0; authority.AI();
                check(authority.active&&authority.life==authority.lifeMax&&authority.damage==authority.defDamage,"native authoritative heart demon and natural spawn keeps permanent combat stats through actual AI");
                foreach(bool wounded in new[]{false,true}) {
                    Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;Main.npc[slot]=authority;authority.life=wounded?Math.Max(1,authority.lifeMax/3):authority.lifeMax;output.Sent.Clear();
                    long session=state.TribulationSession;
                    NetMessage.SendData(MessageID.SyncNPC,0,-1,null,slot);byte[] packet=output.Sent.Last(p=>p.Length>2&&p[2]==MessageID.SyncNPC),payload=packet[2..];
                    Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=0;Netplay.Connection=new RemoteServer();Array.Clear(NpcEncounterIdentitySystem.ClientLastSeen);Main.npc[slot]=new NPC { whoAmI=slot };state.TryReceiveTribulationEpoch(0,session);
                    var buffer=new MessageBuffer { whoAmI=256,readBuffer=payload,reader=new BinaryReader(new MemoryStream(payload)) };
                    try { buffer.GetData(0,payload.Length,out int kind);check(kind==MessageID.SyncNPC,"official client decoder accepts native heart demon NPC packet"); } finally { buffer.reader.Dispose(); }
                    NPC receiver=Main.npc[slot];
                    check(receiver.active&&receiver.type==type&&receiver.lifeMax==authority.lifeMax&&receiver.damage==authority.damage&&receiver.defense==authority.defense&&receiver.defDamage==authority.defDamage,"official heart demon and natural NPC decoding retains intended maximum life and damage: "+gameMode+"/"+stage+"/"+demon+$" life={receiver.lifeMax}/{authority.lifeMax} damage={receiver.damage}/{authority.damage}");
                    check(receiver.life==authority.life,"official heart demon NPC decoding retains current life for full and wounded state");
                    receiver.target=0; receiver.AI();
                    check(receiver.active&&receiver.lifeMax==authority.lifeMax&&receiver.life==authority.life&&receiver.damage==authority.damage,"native heart demon and natural receiver keeps combat stats through actual NPC AI: "+gameMode+"/"+stage+"/"+demon+$" damage={receiver.damage}/{authority.damage}");
                }
            }
        } finally {
            Main.npc[slot]=oldNpc;Main.player[0]=oldPlayer;clients.CopyTo(Netplay.Clients,0);Netplay.Connection=connection;Main.netMode=mode;Main.myPlayer=local;Main.GameMode=difficulty;Main.rand=random;history.CopyTo(NpcEncounterIdentitySystem.ClientLastSeen,0);trialHistory.CopyTo(TribulationEpochSystem.ClientLastSeen,0);
        }
        check(ReferenceEquals(Main.npc[slot],oldNpc)&&ReferenceEquals(Main.player[0],oldPlayer)&&NpcEncounterIdentitySystem.ClientLastSeen.SequenceEqual(history),"native heart demon packet audit restores NPC player difficulty and identity history");
    }
}
