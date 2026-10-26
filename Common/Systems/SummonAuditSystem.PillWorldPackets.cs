using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditPillWorldPackets(Action<bool, string> check)
    {
        var previous = Main.player[0]; var drops = (Item[])Main.item.Clone();
        var clients = (RemoteClient[])Netplay.Clients.Clone(); var connection = Netplay.Connection;
        int mode = Main.netMode, local = Main.myPlayer; bool text = Main.showItemText;
        var grab = typeof(Player).GetMethod("GrabItems", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try {
            Main.showItemText = false;
            var output = new AuditOutputSocket(true);
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(), State = i == 0 ? 10 : 0, IsActive = i == 0 };
            foreach (int type in new[] { ModContent.ItemType<QiCondensingPill>(), ModContent.ItemType<FoundationPill>(),
                ModContent.ItemType<QiRecoveryPill>(), ModContent.ItemType<SpringReturnPill>(), ModContent.ItemType<TribulationResistingPill>(),
                ModContent.ItemType<FurnaceGuardPill>(), ModContent.ItemType<WindStepPill>(), ModContent.ItemType<ThunderBurstPill>() })
            foreach (PillQuality grade in Enum.GetValues<PillQuality>()) foreach (bool crafted in new[] { false, true }) {
                Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; output.Sent.Clear();
                for (int i = 0; i < Main.item.Length; i++) Main.item[i] = new Item { whoAmI = i };
                var sender = new Item(type, 5) { active = true, whoAmI = 0, position = new Vector2(Main.maxTilesX * 8, 400), velocity = Vector2.Zero };
                sender.GetGlobalItem<PillQualitySystem>().LoadData(sender, new TagCompound { ["quality"] = (int)grade, ["crafted"] = crafted });
                Main.item[0] = sender; NetMessage.SendData(MessageID.SyncItem, 0, -1, null, 0);
                byte[][] packets = output.Sent.Where(packet => packet.Length > 2 && packet[2] == MessageID.SyncItem).ToArray();
                check(packets.Length == 1, "native eight-pill world quality fixture captures one actual item packet");
                Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = 0; Netplay.Connection = new RemoteServer();
                Main.item[0] = new Item { whoAmI = 0 };
                var receiver = new Player { whoAmI = 0, active = true, statLife = 100, position = sender.position }; Main.player[0] = receiver;
                foreach (int replay in new[] { 0, 1 }) {
                    foreach (byte[] packet in packets) {
                        byte[] payload = packet[2..]; using var reader = new BinaryReader(new MemoryStream(payload));
                        var buffer = new MessageBuffer { whoAmI = 256, readBuffer = payload, reader = reader };
                        buffer.GetData(0, payload.Length, out int kind);
                        check(kind == MessageID.SyncItem, "official pill world decoder accepts actual server item packet");
                    }
                    Item received = Main.item[0];
                    check(received.active && received.type == type && received.stack == 5
                        && received.GetGlobalItem<PillQualitySystem>().Quality == grade && received.GetGlobalItem<PillQualitySystem>().Crafted == crafted
                        && Main.item.Take(Main.maxItems).Count(item => item.active) == 1,
                        "official pill world replay preserves one stack grade and provenance: " + sender.ModItem.Name + "/" + grade + "/" + crafted + "/" + replay);
                }
                Main.item[0].playerIndexTheItemIsReservedFor = 0; Main.item[0].noGrabDelay = 0;
                grab.Invoke(receiver, new object[] { 0 });
                var inventory = receiver.inventory.Take(58).Where(item => item.type == type).ToArray();
                check(!Main.item[0].active && inventory.Length == 1 && inventory[0].stack == 5
                    && inventory[0].GetGlobalItem<PillQualitySystem>().Quality == grade && inventory[0].GetGlobalItem<PillQualitySystem>().Crafted == crafted,
                    "native pickup of officially decoded world pill preserves wire quality and provenance");
            }
        } finally {
            Main.player[0] = previous; drops.CopyTo(Main.item, 0); clients.CopyTo(Netplay.Clients, 0);
            Netplay.Connection = connection; Main.netMode = mode; Main.myPlayer = local; Main.showItemText = text;
        }
        check(ReferenceEquals(Main.player[0], previous) && Main.item.SequenceEqual(drops)
            && ReferenceEquals(Netplay.Connection, connection), "native pill world packet audit restores items participant and connection");
    }
}
