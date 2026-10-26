using System;
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
    private static void AuditPillQualityPickup(Action<bool, string> check)
    {
        var previous = Main.player[0]; var drops = (Item[])Main.item.Clone();
        int mode = Main.netMode, local = Main.myPlayer; bool text = Main.showItemText;
        var connection = Netplay.Connection;
        var grab = typeof(Player).GetMethod("GrabItems", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try {
            Main.showItemText = false;
            foreach (int type in new[] { ModContent.ItemType<QiCondensingPill>(), ModContent.ItemType<FoundationPill>(),
                ModContent.ItemType<QiRecoveryPill>(), ModContent.ItemType<SpringReturnPill>(), ModContent.ItemType<TribulationResistingPill>(),
                ModContent.ItemType<FurnaceGuardPill>(), ModContent.ItemType<WindStepPill>(), ModContent.ItemType<ThunderBurstPill>() })
            foreach (PillQuality inventoryGrade in Enum.GetValues<PillQuality>()) foreach (PillQuality dropGrade in Enum.GetValues<PillQuality>())
            foreach (int layout in new[] { 0, 1, 2 }) foreach (int pickupMode in new[] { NetmodeID.SinglePlayer, NetmodeID.MultiplayerClient }) {
                Main.netMode = pickupMode; Main.myPlayer = 0; Netplay.Connection = new RemoteServer();
                for (int i = 0; i < Main.item.Length; i++) Main.item[i] = new Item { whoAmI = i };
                var actor = new Player { whoAmI = 0, active = true, statLife = 100 }; Main.player[0] = actor;
                actor.Center = new Vector2(Main.maxTilesX * 8, 400);
                for (int i = 0; i < actor.inventory.Length; i++) { actor.inventory[i] = new Item(ItemID.StoneBlock); actor.inventory[i].stack = actor.inventory[i].maxStack; }
                int initial = layout == 2 ? 30 : 28;
                actor.inventory[0] = new Item(type, initial);
                actor.inventory[0].GetGlobalItem<PillQualitySystem>().LoadData(actor.inventory[0], new TagCompound { ["quality"] = (int)inventoryGrade });
                if (layout == 1) actor.inventory[1] = new Item();
                var drop = new Item(type, 5) { active = true, whoAmI = 0, position = actor.position, velocity = Vector2.Zero, noGrabDelay = 0, playerIndexTheItemIsReservedFor = 0 };
                drop.GetGlobalItem<PillQualitySystem>().LoadData(drop, new TagCompound { ["quality"] = (int)dropGrade, ["crafted"] = true });
                Main.item[0] = drop;
                bool same = inventoryGrade == dropGrade; int picked = layout == 1 ? 5 : layout == 0 && same ? 2 : 0;
                grab.Invoke(actor, new object[] { 0 });
                int total = actor.inventory.Take(58).Where(item => item.type == type).Sum(item => item.stack);
                check(total == initial + picked && actor.inventory[0].stack == initial + (same ? Math.Min(30 - initial, picked) : 0),
                    "native pill GrabItems picks only available equal-grade capacity or an empty slot: " + actor.inventory[0].ModItem.Name + "/" + inventoryGrade + "/" + dropGrade + "/" + layout + "/" + pickupMode);
                check(actor.inventory[0].GetGlobalItem<PillQualitySystem>().Quality == inventoryGrade
                    && actor.inventory[0].GetGlobalItem<PillQualitySystem>().Crafted == (same && picked > 0)
                    && actor.inventory.Take(58).Where(item => item.type == type && !ReferenceEquals(item, actor.inventory[0])).All(item => item.GetGlobalItem<PillQualitySystem>().Quality == dropGrade && item.GetGlobalItem<PillQualitySystem>().Crafted),
                    "native pill pickup keeps separate grades and merges only actual crafting provenance");
                var remainder = Main.item[0];
                check(picked == 5 ? !remainder.active : remainder.active && remainder.stack == 5 - picked && remainder.type == type
                    && remainder.GetGlobalItem<PillQualitySystem>().Quality == dropGrade && remainder.GetGlobalItem<PillQualitySystem>().Crafted,
                    "native pill pickup preserves every unaccepted world pill and its quality metadata");
                grab.Invoke(actor, new object[] { 0 });
                check(actor.inventory.Take(58).Where(item => item.type == type).Sum(item => item.stack) == total
                    && Main.item.Take(Main.maxItems).Where(item => item.active && item.type == type).Sum(item => item.stack) == 5 - picked,
                    "native repeated pill pickup cannot duplicate rewards or consume a blocked remainder");
            }
        } finally {
            Main.player[0] = previous; drops.CopyTo(Main.item, 0); Main.netMode = mode; Main.myPlayer = local;
            Main.showItemText = text; Netplay.Connection = connection;
        }
        check(ReferenceEquals(Main.player[0], previous) && Main.item.SequenceEqual(drops) && Main.netMode == mode
            && Main.myPlayer == local && Main.showItemText == text && ReferenceEquals(Netplay.Connection, connection),
            "native pill pickup audit restores participant world items and engine context");
    }
}
