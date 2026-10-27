using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditBossBagEligibility(Action<bool,string> check)
    {
        string[] bosses = { "SpiritVeinWyrm","GardenWarden","BlackFurnaceIronGolem","TribulationCloudAvatar","ThunderMarshJiao","AbyssalStarWomb","FormlessSwordSoul","MoonboneImmortal","GreenwoodMedicineKingEcho","HeavenTabletGuardian","BrokenHeavenInspector","OldHeavenDaoCore" };
        var mod = ModLoader.GetMod("XianXia");
        var players = Main.player.Take(3).ToArray(); var clients = (RemoteClient[])Netplay.Clients.Clone();
        var broadcast = NetMessage.buffer.Take(3).Select(b => b.broadcast).ToArray();
        int mode = Main.netMode, local = Main.myPlayer, difficulty = Main.GameMode; var random = Main.rand;
        var items = Main.item.Where(i => i.active).ToHashSet(); var npcs = Main.npc.Where(n => n.active).ToHashSet();
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Main.rand = new UnifiedRandom(373);
            var outputs = new[] { new AuditOutputSocket(true),new AuditOutputSocket(true),new AuditOutputSocket(true) };
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i < 3 ? outputs[i] : new AuditOutputSocket(), State = i < 3 ? 10 : 0, IsActive = i < 3 };
            foreach (int gameMode in new[] { 1,2 }) foreach (string name in bosses)
            foreach (var scenario in new[] { (Interacted:1,Active:7,Dead:0),(Interacted:3,Active:5,Dead:0),(Interacted:3,Active:7,Dead:2),(Interacted:7,Active:7,Dead:0),(Interacted:0,Active:7,Dead:0) }) {
                Main.GameMode = gameMode;
                for (int peer = 0; peer < 3; peer++) {
                    bool active = (scenario.Active & (1 << peer)) != 0, dead = (scenario.Dead & (1 << peer)) != 0;
                    Main.player[peer] = new Player { whoAmI = peer, active = active, dead = dead, statLife = dead ? 0 : 100 };
                    Main.player[peer].Center = new Microsoft.Xna.Framework.Vector2(Main.maxTilesX * 8,400);
                    NetMessage.buffer[peer].broadcast = true; Netplay.Clients[peer].IsActive = active; Netplay.Clients[peer].State = active ? 10 : 0;
                }
                int bag = mod.Find<ModItem>(name + "Bag").Type;
                int slot = NPC.NewNPC(new Terraria.DataStructures.EntitySource_Misc("BossBagEligibilityAudit"),Main.maxTilesX * 8,400,mod.Find<ModNPC>(name).Type);
                if (slot >= Main.maxNPCs) throw new InvalidOperationException("Missing boss bag eligibility fixture slot");
                NPC npc = Main.npc[slot]; Array.Clear(npc.playerInteraction);
                for (int peer = 0; peer < 3; peer++) npc.playerInteraction[peer] = (scenario.Interacted & (1 << peer)) != 0;
                foreach (var output in outputs) output.Sent.Clear();
                Main.ItemDropSolver.TryDropping(new Terraria.GameContent.ItemDropRules.DropAttemptInfo { npc = npc,player = Main.player[0],rng = Main.rand,IsExpertMode = true,IsMasterMode = gameMode == 2 });
                for (int peer = 0; peer < 3; peer++) {
                    int count = outputs[peer].Sent.Count(packet => packet.Length >= 5 && packet[2] == MessageID.InstancedItem
                        && BitConverter.ToInt16(packet,3) >= 0 && BitConverter.ToInt16(packet,3) < Main.maxItems
                        && Main.item[BitConverter.ToInt16(packet,3)].type == bag);
                    bool eligible = (scenario.Interacted & scenario.Active & (1 << peer)) != 0;
                    check(count == (eligible ? 1 : 0),"native boss bag eligibility awards active participants including dead peers and excludes spectators disconnected peers or no-interaction players: " + name + "/" + gameMode + "/" + scenario + "/" + peer);
                }
                check(!Main.item.Any(item => item.active && !items.Contains(item) && item.type == bag),"native participant-specific boss bags never become duplicate shared active loot");
                foreach (var item in Main.item.Where(item => item.active && !items.Contains(item))) item.active = false;
                npc.active = false;
            }
        } finally {
            foreach (var item in Main.item.Where(item => item.active && !items.Contains(item))) item.active = false;
            foreach (var npc in Main.npc.Where(npc => npc.active && !npcs.Contains(npc))) npc.active = false;
            for (int i = 0; i < 3; i++) { Main.player[i] = players[i]; NetMessage.buffer[i].broadcast = broadcast[i]; }
            clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local; Main.GameMode = difficulty; Main.rand = random;
        }
        check(Main.player.Take(3).SequenceEqual(players) && Main.item.Where(i => i.active).ToHashSet().SetEquals(items)
            && Main.npc.Where(n => n.active).ToHashSet().SetEquals(npcs),"native boss bag eligibility audit restores players item pool NPC pool and network context");
    }
}
