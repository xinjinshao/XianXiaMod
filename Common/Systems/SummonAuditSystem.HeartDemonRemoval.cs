using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditHeartDemonRemoval(Action<bool,string> check)
    {
        int slot=Enumerable.Range(0,Main.maxNPCs).First(i=>!Main.npc[i].active);
        NPC original=Main.npc[slot];Player previous=Main.player[0];
        int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        var clients=(RemoteClient[])Netplay.Clients.Clone();var connection=Netplay.Connection;
        bool[] broadcast=NetMessage.buffer.Take(2).Select(b=>b.broadcast).ToArray();
        long[] identities=(long[])NpcEncounterIdentitySystem.ClientLastSeen.Clone(),epochs=(long[])TribulationEpochSystem.ClientLastSeen.Clone();
        var outputs=new[] {new AuditOutputSocket(true),new AuditOutputSocket(true)};
        void Decode(byte[] packet) {
            byte[] payload=packet[2..];
            using var reader=new BinaryReader(new MemoryStream(payload));
            var buffer=new MessageBuffer {whoAmI=256,readBuffer=payload,reader=reader};
            buffer.GetData(0,payload.Length,out int kind);
            check(kind==MessageID.SyncNPC,"official heart removal decoder accepts actual vanilla NPC packet");
        }
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i<2?outputs[i]:new AuditOutputSocket(),State=i<2?10:0,IsActive=i<2};
            for(int i=0;i<2;i++)NetMessage.buffer[i].broadcast=true;
            foreach(int stage in new[] {3,5})foreach(int invalid in Enumerable.Range(0,3)) {
                Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(322);
                var actor=new Player {whoAmI=0,active=true,statLife=100};actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,400);Main.player[0]=actor;
                var state=actor.GetModPlayer<XianXiaPlayer>();state.tribulationKind=XianXiaPlayer.TribulationKind.HeartDemon;state.tribulationTimer=120;state.tribulationStage=stage;
                Main.npc[slot]=new NPC {whoAmI=slot};
                typeof(XianXiaPlayer).GetMethod("SpawnHeartDemonEnemy",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(state,null);
                NPC authority=Main.npc[slot];authority.target=0;long session=state.TribulationSession;
                foreach(var output in outputs)output.Sent.Clear();
                for(int peer=0;peer<2;peer++)NetMessage.SendData(MessageID.SyncNPC,peer,-1,null,slot);
                byte[][] live=outputs.Select(o=>o.Sent.Single(p=>p.Length>2&&p[2]==MessageID.SyncNPC)).ToArray();
                check(live[0].SequenceEqual(live[1]),"native active heart NPC sends identical addressed spawn metadata to owner and observer sockets");
                foreach(var output in outputs)output.Sent.Clear();
                if(invalid==0)state.tribulationTimer=0;
                else if(invalid==1)actor.dead=true;
                else state.ResetNetworkSession();
                check(!NPCLoader.PreAI(authority)&&!authority.active,"native heart cancellation invokes authoritative removal before broadcast validation");
                byte[][] cancelled=outputs.Select(o=>o.Sent.Single(p=>p.Length>2&&p[2]==MessageID.SyncNPC)).ToArray();
                check(cancelled[0].SequenceEqual(cancelled[1]),"native heart cancellation broadcasts identical removal packet to owner and observer sockets");
                NPCLoader.PreAI(authority);
                check(outputs.All(o=>o.Sent.Count(p=>p.Length>2&&p[2]==MessageID.SyncNPC)==1),"native repeated heart cleanup emits exactly one removal per receiver");
                for(int peer=0;peer<2;peer++) {
                    Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=peer;Netplay.Connection=new RemoteServer();
                    Array.Clear(NpcEncounterIdentitySystem.ClientLastSeen);Array.Clear(TribulationEpochSystem.ClientLastSeen);
                    var receiver=new Player {whoAmI=0,active=true,statLife=100};receiver.Center=actor.Center;Main.player[0]=receiver;
                    var replica=receiver.GetModPlayer<XianXiaPlayer>();replica.tribulationKind=XianXiaPlayer.TribulationKind.HeartDemon;replica.tribulationTimer=120;
                    Main.npc[slot]=new NPC {whoAmI=slot};Decode(live[peer]);NPC npc=Main.npc[slot];int cooldown=0;
                    check(npc.active&&!NPCLoader.CanHitPlayer(npc,receiver,ref cooldown)&&!NPCLoader.PreAI(npc)&&npc.active,"official live heart receiver waits harmlessly for matching trial epoch without despawning");
                    check(replica.TryReceiveTribulationEpoch(0,session)&&NPCLoader.CanHitPlayer(npc,receiver,ref cooldown)&&NPCLoader.PreAI(npc),"official heart receiver resumes valid damage after matching epoch arrives");
                    Decode(cancelled[peer]);Decode(cancelled[peer]);
                    check(!Main.npc[slot].active&&replica.tribulationTimer==120,"official owner and observer decode removal idempotently even before local trial timer ends");
                }
            }
        } finally {
            Main.npc[slot]=original;Main.player[0]=previous;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;
            clients.CopyTo(Netplay.Clients,0);Netplay.Connection=connection;
            for(int i=0;i<2;i++)NetMessage.buffer[i].broadcast=broadcast[i];
            identities.CopyTo(NpcEncounterIdentitySystem.ClientLastSeen,0);epochs.CopyTo(TribulationEpochSystem.ClientLastSeen,0);
        }
        check(ReferenceEquals(Main.npc[slot],original)&&ReferenceEquals(Main.player[0],previous)&&identities.SequenceEqual(NpcEncounterIdentitySystem.ClientLastSeen)&&epochs.SequenceEqual(TribulationEpochSystem.ClientLastSeen),"native heart removal audit restores entity slots and both replay histories");
    }
}
