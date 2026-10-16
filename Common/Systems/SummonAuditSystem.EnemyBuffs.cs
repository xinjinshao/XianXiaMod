using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.NPCs.Enemies;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditEnemyBuffs(Action<bool, string> check, List<NPC> owned)
    {
        Player[] oldPlayers = Main.player.Take(2).ToArray(); var oldClients = (RemoteClient[])Netplay.Clients.Clone();
        int oldMode = Main.netMode, oldLocal = Main.myPlayer; var oldConnection = Netplay.Connection;
        var output = new AuditOutputSocket(true);
        int BuffPackets() => output.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff);
        void ReceiveLast(int buff, int duration) {
            byte[] packet = output.Sent.Last(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff);
            Player authority = Main.player[0];
            try {
                Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = 0; Netplay.Connection = new RemoteServer();
                var receiver = new Player { whoAmI = 0, active = true }; Main.player[0] = receiver;
                byte[] payload = packet.Skip(2).ToArray();
                var buffer = new MessageBuffer { whoAmI = 256, readBuffer = payload, reader = new BinaryReader(new MemoryStream(payload)) };
                try { buffer.GetData(0, payload.Length, out int kind); check(kind == MessageID.AddPlayerBuff, "official owner decoder accepts native enemy debuff packet"); }
                finally { buffer.reader.Dispose(); }
                check(receiver.HasBuff(buff) && receiver.buffTime[receiver.FindBuffIndex(buff)] == duration, "official owner receives exact native enemy debuff duration");
            } finally { Main.player[0] = authority; Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Netplay.Connection = oldConnection; }
        }
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(), State = i == 0 ? 10 : 0, IsActive = i == 0 };
            var actor = new Player { whoAmI = 0, active = true }; var distant = new Player { whoAmI = 1, active = true };
            Main.player[0] = actor; Main.player[1] = distant;
            int mothIndex = NPC.NewNPC(new EntitySource_Misc("XianXiaEnemyBuffAudit"), Main.maxTilesX * 8, 200, ModContent.NPCType<MiasmaFlowerMoth>());
            check(mothIndex >= 0 && mothIndex < Main.maxNPCs, "native registered moth created for poison pulse");
            NPC moth = Main.npc[mothIndex]; owned.Add(moth);
            actor.Center = moth.Center + new Vector2(64, 0); distant.Center = moth.Center + new Vector2(256, 0);
            moth.localAI[0] = 44; moth.ModNPC.PostAI();
            check(actor.HasBuff(BuffID.Poisoned) && actor.buffTime[actor.FindBuffIndex(BuffID.Poisoned)] == 90 && !distant.HasBuff(BuffID.Poisoned) && BuffPackets() == 1,
                "native moth pulse poisons only nearby living player and sends one packet");
            ReceiveLast(BuffID.Poisoned, 90);
            actor.AddBuff(BuffID.Poisoned, 900); moth.localAI[0] = 44; moth.ModNPC.PostAI();
            check(actor.buffTime[actor.FindBuffIndex(BuffID.Poisoned)] == 900 && BuffPackets() == 1, "native moth neither shortens nor resends longer poison");
            actor.ClearBuff(BuffID.Poisoned); actor.buffImmune[BuffID.Poisoned] = true; moth.localAI[0] = 44; moth.ModNPC.PostAI();
            check(!actor.HasBuff(BuffID.Poisoned) && BuffPackets() == 1, "native poison immune player receives no ineffective sync"); actor.buffImmune[BuffID.Poisoned] = false;
            output.Sent.Clear();
            int larvaIndex = NPC.NewNPC(new EntitySource_Misc("XianXiaEnemyBuffAudit"), Main.maxTilesX * 8, 200, ModContent.NPCType<StarAbyssLarva>());
            check(larvaIndex >= 0 && larvaIndex < Main.maxNPCs, "native registered larva created for close slow");
            NPC larva = Main.npc[larvaIndex]; owned.Add(larva); larva.target = 0; larva.localAI[1] = 90;
            actor.Center = larva.Center + new Vector2(20, 0); larva.ModNPC.PostAI();
            check(actor.HasBuff(BuffID.Slow) && actor.buffTime[actor.FindBuffIndex(BuffID.Slow)] == 30 && !distant.HasBuff(BuffID.Slow) && BuffPackets() == 1,
                "native larva close contact grants thirty-frame slow only to target owner");
            ReceiveLast(BuffID.Slow, 30);
            larva.ModNPC.PostAI(); check(BuffPackets() == 1, "native larva does not broadcast healthy slow every frame");
            actor.buffTime[actor.FindBuffIndex(BuffID.Slow)] = 15; larva.ModNPC.PostAI();
            check(actor.buffTime[actor.FindBuffIndex(BuffID.Slow)] == 30 && BuffPackets() == 2, "native larva renews slow exactly at fifteen-frame threshold");
            actor.Center = larva.Center + new Vector2(40, 0); actor.buffTime[actor.FindBuffIndex(BuffID.Slow)] = 15; larva.ModNPC.PostAI();
            check(actor.buffTime[actor.FindBuffIndex(BuffID.Slow)] == 15 && BuffPackets() == 2, "native larva forty-pixel boundary stops slow renewal");
            actor.Center = larva.Center + new Vector2(20, 0); actor.ClearBuff(BuffID.Slow); actor.buffImmune[BuffID.Slow] = true; larva.ModNPC.PostAI();
            check(!actor.HasBuff(BuffID.Slow) && BuffPackets() == 2, "native larva honors slow immunity without misleading packet"); actor.buffImmune[BuffID.Slow] = false;
            actor.dead = true; moth.localAI[0] = 44; moth.ModNPC.PostAI(); larva.ModNPC.PostAI();
            check(!actor.HasBuff(BuffID.Poisoned) && !actor.HasBuff(BuffID.Slow) && BuffPackets() == 2, "native enemy aura cannot apply buffs to dead owner");
            actor.dead = false; Main.netMode = NetmodeID.MultiplayerClient; moth.localAI[0] = 44; moth.ModNPC.PostAI(); larva.ModNPC.PostAI();
            check(!actor.HasBuff(BuffID.Poisoned) && !actor.HasBuff(BuffID.Slow) && BuffPackets() == 2, "native clients cannot apply or resend authority enemy auras");
            Main.netMode = NetmodeID.SinglePlayer; larva.target = 0; larva.localAI[1] = 90;
            actor.Center = larva.Center + new Vector2(20, 0); larva.ModNPC.PostAI();
            check(actor.HasBuff(BuffID.Slow) && actor.buffTime[actor.FindBuffIndex(BuffID.Slow)] == 2 && BuffPackets() == 2,
                "native singleplayer larva retains two-frame local slow without packets");
            actor.Center = moth.Center + new Vector2(64, 0); moth.localAI[0] = 44; moth.ModNPC.PostAI();
            check(actor.HasBuff(BuffID.Poisoned) && actor.buffTime[actor.FindBuffIndex(BuffID.Poisoned)] == 90 && BuffPackets() == 2,
                "native singleplayer moth retains ninety-frame poison without packets");
            Main.netMode = NetmodeID.Server; actor.Center = larva.Center + new Vector2(20, 0);
            larva.target = -1; larva.netUpdate = false;
            check(EnemyTargeting.TryGetLivingTarget(larva, out var healthyTarget) && ReferenceEquals(healthyTarget, actor) && larva.netUpdate,
                "native ordinary enemy retains healthy-table vanilla target acquisition");
            Main.player[0] = null; larva.target = 0; larva.netUpdate = false;
            check(EnemyTargeting.TryGetLivingTarget(larva, out var replacement) && ReferenceEquals(replacement, distant) && larva.target == 1 && larva.netUpdate,
                "native ordinary enemy safely replaces a missing player slot");
            Main.netMode = NetmodeID.MultiplayerClient; larva.target = 0; larva.netUpdate = false;
            check(!EnemyTargeting.TryGetLivingTarget(larva, out _) && larva.target == 0 && !larva.netUpdate,
                "native client waits for authority when ordinary enemy target slot is missing");
            Main.netMode = NetmodeID.Server; Main.player[0] = actor; actor.Center = new Vector2(float.NaN, 0); larva.target = 0;
            check(EnemyTargeting.TryGetLivingTarget(larva, out replacement) && ReferenceEquals(replacement, distant),
                "native ordinary enemy ignores malformed player position and chooses valid peer");
            Main.player[0] = null; Main.player[1] = null; larva.target = 0;
            check(!EnemyTargeting.TryGetLivingTarget(larva, out _), "native ordinary enemy has no target when all active slots disappear");
            Main.player[0] = actor; actor.Center = larva.Center + new Vector2(20, 0); larva.Center = new Vector2(float.NaN, 0);
            check(!EnemyTargeting.TryGetLivingTarget(larva, out _), "native malformed ordinary enemy cannot acquire target");
            Main.player[0] = actor; Main.player[1] = distant;
            int gardenIndex = NPC.NewNPC(new EntitySource_Misc("XianXiaGardenRetargetAudit"), Main.maxTilesX * 8, 200, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.GardenWarden>());
            check(gardenIndex >= 0 && gardenIndex < Main.maxNPCs, "registered garden warden created for shared targeting regression");
            NPC garden = Main.npc[gardenIndex]; owned.Add(garden);
            actor.Center = garden.Center + new Vector2(300, 0); distant.Center = garden.Center + new Vector2(400, 0); garden.target = 0;
            var gardenAI = (global::XianXia.Content.NPCs.Bosses.GardenWarden)garden.ModNPC; gardenAI.AI(); int oldHazardSession = gardenAI.HazardSession;
            actor.Center = new Vector2(float.NaN, 0); gardenAI.AI();
            check(gardenAI.BattleTarget == 1 && gardenAI.HazardSession > 0 && gardenAI.HazardSession != oldHazardSession,
                "native garden warden safely selects valid peer and replaces prior hazard identity");
            distant.Center = new Vector2(float.NaN, 0); gardenAI.AI();
            check(gardenAI.HazardSession == 0, "native garden warden cancels hazards when all targets are malformed");
        } finally {
            for (int i = 0; i < oldPlayers.Length; i++) Main.player[i] = oldPlayers[i]; oldClients.CopyTo(Netplay.Clients, 0);
            Main.netMode = oldMode; Main.myPlayer = oldLocal; Netplay.Connection = oldConnection;
        }
        check(Main.player.Take(2).SequenceEqual(oldPlayers) && Netplay.Clients.SequenceEqual(oldClients)
            && Main.netMode == oldMode && Main.myPlayer == oldLocal && ReferenceEquals(Netplay.Connection, oldConnection), "native enemy buff audit restores player client and engine contexts");
    }
}
