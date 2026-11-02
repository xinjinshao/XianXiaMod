using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Items.Guides;
using XianXia.Content.NPCs.Town;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditClassPathShops(Action<bool,string> check)
    {
        Player[] previous = Main.player.Take(2).ToArray(); int mode = Main.netMode, local = Main.myPlayer;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        try {
            int manual = ModContent.ItemType<ClassPathManual>(); string name = Lang.GetItemNameValue(manual);
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient })
            foreach (int route in new[] { 0,1,2,3 }) foreach (int realm in new[] { 0,5,8 }) foreach (int viewer in new[] { 0,1 })
            foreach (int seller in new[] { ModContent.NPCType<HerbSectApprentice>(),ModContent.NPCType<ArchiveScrollSpirit>() }) {
                Main.netMode = authority; Main.myPlayer = viewer; world.LoadWorldData(new TagCompound { ["chosenRoute"] = route });
                for (int i = 0; i < 2; i++) {
                    Main.player[i] = new Player { whoAmI = i, active = true, statLife = 100 };
                    Main.player[i].GetModPlayer<XianXiaPlayer>().cultivationStage = (CultivationStage)(i == viewer ? realm : 8 - realm);
                }
                var npc = new NPC(); npc.SetDefaults(seller);
                var shop = NPCShopDatabase.AllShops.OfType<NPCShop>().Single(s => s.NpcType == seller && s.Name == "Shop");
                var rows = new List<Item>(); shop.FillShop(rows,npc);
                Item[] slots = Enumerable.Range(0,40).Select(_ => new Item()).ToArray(); rows.CopyTo(slots,0);
                NPCLoader.ModifyActiveShop(npc,"Shop",slots);
                var copies = slots.Where(i => i != null && i.type == manual).ToArray();
                check(copies.Length == 1 && copies[0].value == Item.buyPrice(silver:2) && !copies[0].IsAir,
                    "native both Four Paths sellers retain exactly one base-price book across realm route and viewer differences");
                var state = Main.LocalPlayer.GetModPlayer<XianXiaPlayer>(); var before = state.CaptureSnapshot();
                string chat = npc.ModNPC.GetChat();
                check(chat.Contains(name) && !chat.Contains("Mods.XianXia.") && !chat.Contains("{0}"),
                    "native both Four Paths sellers advertise the manual in early late and all route dialogues");
                check(state.CaptureSnapshot() == before && Main.LocalPlayer.inventory.All(i => i.IsAir),
                    "native Four Paths service dialogue grants no item and changes no cultivation state");
            }
        } finally {
            for (int i = 0; i < 2; i++) Main.player[i] = previous[i];
            Main.netMode = mode; Main.myPlayer = local; world.LoadWorldData(saved);
        }
    }
}
