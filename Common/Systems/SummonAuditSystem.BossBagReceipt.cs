using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditBossBagReceipt(Action<bool,string> check)
    {
        string[] bosses = { "SpiritVeinWyrm","GardenWarden","BlackFurnaceIronGolem","TribulationCloudAvatar","ThunderMarshJiao","AbyssalStarWomb","FormlessSwordSoul","MoonboneImmortal","GreenwoodMedicineKingEcho","HeavenTabletGuardian","BrokenHeavenInspector","OldHeavenDaoCore" };
        var mod = ModLoader.GetMod("XianXia"); var players = Main.player.Take(2).ToArray();
        var drops = (Item[])Main.item.Clone(); var clients = (RemoteClient[])Netplay.Clients.Clone(); var connection = Netplay.Connection;
        var broadcast = NetMessage.buffer.Take(2).Select(b => b.broadcast).ToArray();
        int mode = Main.netMode, local = Main.myPlayer, difficulty = Main.GameMode; bool text = Main.showItemText; var random = Main.rand;
        var npcs = Main.npc.Where(n => n.active).ToHashSet();
        var grab = typeof(Player).GetMethod("GrabItems",BindingFlags.Instance | BindingFlags.NonPublic)!;
        void EmptyWorldItems() { for (int i = 0; i < Main.item.Length; i++) Main.item[i] = new Item { whoAmI = i }; }
        try {
            Main.showItemText = false; Main.rand = new UnifiedRandom(374);
            var outputs = new[] { new AuditOutputSocket(true),new AuditOutputSocket(true) };
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i < 2 ? outputs[i] : new AuditOutputSocket(), State = i < 2 ? 10 : 0, IsActive = i < 2 };
            foreach (int gameMode in new[] { 1,2 }) foreach (string name in bosses) {
                Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Main.GameMode = gameMode; EmptyWorldItems();
                for (int peer = 0; peer < 2; peer++) { Main.player[peer] = new Player { active = true,whoAmI = peer,statLife = 100 }; Main.player[peer].Center = new Vector2(Main.maxTilesX * 8,400); NetMessage.buffer[peer].broadcast = true; }
                int bag = mod.Find<ModItem>(name + "Bag").Type;
                int slot = NPC.NewNPC(new Terraria.DataStructures.EntitySource_Misc("BossBagReceiptAudit"),Main.maxTilesX * 8,400,mod.Find<ModNPC>(name).Type);
                if (slot >= Main.maxNPCs) throw new InvalidOperationException("Missing boss bag receipt fixture slot");
                var npc = Main.npc[slot]; Array.Clear(npc.playerInteraction); npc.playerInteraction[0] = npc.playerInteraction[1] = true;
                foreach (var output in outputs) output.Sent.Clear();
                Main.ItemDropSolver.TryDropping(new Terraria.GameContent.ItemDropRules.DropAttemptInfo { npc = npc,player = Main.player[0],rng = Main.rand,IsExpertMode = true,IsMasterMode = gameMode == 2 });
                var packets = outputs.Select(output => output.Sent.Where(packet => packet.Length >= 5 && packet[2] == MessageID.InstancedItem
                    && Main.item[BitConverter.ToInt16(packet,3)].type == bag).ToArray()).ToArray();
                npc.active = false;
                for (int peer = 0; peer < 2; peer++) {
                    check(packets[peer].Length == 1,"native boss bag receipt captures one actual instanced packet for each participant");
                    foreach (bool full in new[] { false,true }) {
                        Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = peer; Netplay.Connection = new RemoteServer { Socket = new AuditOutputSocket(true) }; EmptyWorldItems();
                        var receiver = new Player { active = true,whoAmI = peer,statLife = 100 }; Main.player[peer] = receiver;
                        if (full) for (int i = 0; i < 58; i++) receiver.inventory[i] = new Item(ItemID.DirtBlock,9999);
                        foreach (int replay in new[] { 0,1 }) foreach (var packet in packets[peer]) {
                            byte[] payload = packet[2..]; using var reader = new BinaryReader(new MemoryStream(payload));
                            var buffer = new MessageBuffer { whoAmI = 256,readBuffer = payload,reader = reader }; buffer.GetData(0,payload.Length,out int kind);
                            check(kind == MessageID.InstancedItem,"official client decoder accepts native participant boss bag message");
                        }
                        var received = Main.item.Take(Main.maxItems).Where(item => item.active).ToArray();
                        check(received.Length == 1 && received[0].type == bag && received[0].stack == 1
                            && ItemID.Sets.BossBag[received[0].type] && received[0].ModItem.CanRightClick(),
                            "official instanced bag decoding and pre-pickup replay preserve one openable registered bag");
                        int receivedSlot = Array.FindIndex(Main.item,item => ReferenceEquals(item,received[0]));
                        receiver.Center = received[0].Center; received[0].noGrabDelay = 0;
                        grab.Invoke(receiver,new object[] { peer });
                        int inventoryCount = receiver.inventory.Take(58).Where(item => item.type == bag).Sum(item => item.stack);
                        check(full ? inventoryCount == 0 && Main.item[receivedSlot].active && Main.item[receivedSlot].stack == 1
                            : inventoryCount == 1 && !Main.item[receivedSlot].active,
                            "native decoded boss bag pickup enters empty inventory once or remains in world when inventory is full: " + name + "/" + gameMode + "/" + peer + "/" + full + "/count=" + inventoryCount + "/active=" + received[0].active + "/reserved=" + received[0].playerIndexTheItemIsReservedFor + "/delay=" + received[0].noGrabDelay + "/owner=" + received[0].ownIgnore);
                        if (full) {
                            receiver.inventory[0].TurnToAir(); grab.Invoke(receiver,new object[] { peer });
                            check(receiver.inventory.Take(58).Where(item => item.type == bag).Sum(item => item.stack) == 1 && !Main.item[receivedSlot].active,
                                "native decoded boss bag remains collectible after freeing inventory space");
                        }
                    }
                }
            }
        } finally {
            for (int i = 0; i < 2; i++) { Main.player[i] = players[i]; NetMessage.buffer[i].broadcast = broadcast[i]; }
            drops.CopyTo(Main.item,0); clients.CopyTo(Netplay.Clients,0); Netplay.Connection = connection;
            foreach (var npc in Main.npc.Where(n => n.active && !npcs.Contains(n))) npc.active = false;
            Main.netMode = mode; Main.myPlayer = local; Main.GameMode = difficulty; Main.showItemText = text; Main.rand = random;
        }
        check(Main.player.Take(2).SequenceEqual(players) && Main.item.SequenceEqual(drops) && ReferenceEquals(Netplay.Connection,connection),
            "native bag receipt restores players complete item pool and connection");
    }
}
