using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Content.NPCs.Enemies;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditSpiritTidePackets(Action<bool,string> check)
    {
        int slot = Enumerable.Range(0,Main.maxNPCs).First(i => !Main.npc[i].active);
        NPC original = Main.npc[slot]; int mode = Main.netMode, local = Main.myPlayer; bool day = Main.dayTime;
        var clients = (RemoteClient[])Netplay.Clients.Clone(); var connection = Netplay.Connection;
        var history = (long[])NpcEncounterIdentitySystem.ClientLastSeen.Clone();
        var eventFields = typeof(SpiritTideSystem).GetFields(BindingFlags.Static | BindingFlags.NonPublic).Where(f => !f.IsLiteral && !f.IsInitOnly).Select(f => (Field:f,Value:f.GetValue(null))).ToArray();
        var system = ModContent.GetInstance<SpiritTideSystem>(); var output = new AuditOutputSocket(true);
        try {
            Main.dayTime = false;
            for (int wave = 1; wave <= 3; wave++) foreach (int type in new[] { ModContent.NPCType<WanderingSpiritSlime>(),ModContent.NPCType<ShatteredJadeWorm>(),ModContent.NPCType<TalismanBat>() }) {
                Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
                for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id=i,Socket=i == 0 ? output : new AuditOutputSocket(),State=i == 0 ? 10 : 0,IsActive=i == 0 };
                system.LoadWorldData(new TagCompound { ["spiritTideActive"]=true,["spiritTideWave"]=wave,["spiritTideKills"]=0 });
                Main.npc[slot] = new NPC { whoAmI=slot }; NPC authority = Main.npc[slot]; authority.SetDefaults(type); authority.active=true; authority.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX * 8,100);
                int baseline = authority.lifeMax; authority.GetGlobalNPC<SpiritTideEnemies>().OnSpawn(authority,new EntitySource_SpawnNPC());
                check(authority.lifeMax == baseline * (wave + 1),"native tide fixture boosts actual registered enemy by wave");
                foreach (bool wounded in new[] { false,true }) {
                    Main.netMode=NetmodeID.Server; Main.myPlayer=Main.maxPlayers; Main.npc[slot]=authority;
                    authority.life = wounded ? Math.Max(1,authority.lifeMax / 3) : authority.lifeMax; output.Sent.Clear();
                    NetMessage.SendData(MessageID.SyncNPC,0,-1,null,slot);
                    byte[] packet=output.Sent.Last(p => p.Length > 2 && p[2] == MessageID.SyncNPC), payload=packet[2..];
                    Main.netMode=NetmodeID.MultiplayerClient; Main.myPlayer=0; Netplay.Connection=new RemoteServer(); Array.Clear(NpcEncounterIdentitySystem.ClientLastSeen);
                    Main.npc[slot]=new NPC { whoAmI=slot };
                    var buffer=new MessageBuffer { whoAmI=256,readBuffer=payload,reader=new BinaryReader(new MemoryStream(payload)) };
                    try { buffer.GetData(0,payload.Length,out int kind); check(kind == MessageID.SyncNPC,"official client decoder accepts captured native tide NPC packet"); } finally { buffer.reader.Dispose(); }
                    NPC receiver=Main.npc[slot];
                    check(receiver.active && receiver.type == type && receiver.lifeMax == authority.lifeMax && receiver.damage == authority.damage && receiver.defense == authority.defense,"official tide NPC decoding preserves boosted type and combat stats");
                    check(receiver.life == authority.life,"official tide NPC decoding preserves current life for full and wounded enemies: " + type + "/" + wave + "/" + wounded + $" actual={receiver.life} expected={authority.life}");
                }
            }
        } finally {
            Main.npc[slot]=original; clients.CopyTo(Netplay.Clients,0); Netplay.Connection=connection; Main.netMode=mode;Main.myPlayer=local;Main.dayTime=day;
            history.CopyTo(NpcEncounterIdentitySystem.ClientLastSeen,0); foreach (var field in eventFields) field.Field.SetValue(null,field.Value);
        }
        check(ReferenceEquals(Main.npc[slot],original) && SpiritTideSystem.Active == (bool)eventFields.First(f => f.Field.Name.Contains("<Active>")).Value,"native tide packet audit restores event NPC and networking context");
    }
}
