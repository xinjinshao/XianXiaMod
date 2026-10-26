using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Buffs;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditPillBuffs(Action<bool, string> check)
    {
        Player oldPlayer = Main.player[0]; var oldClients = (RemoteClient[])Netplay.Clients.Clone();
        int oldMode = Main.netMode, oldLocal = Main.myPlayer; var oldConnection = Netplay.Connection;
        var cases = new[] {
            (ModContent.ItemType<QiRecoveryPill>(), ModContent.BuffType<QiRecoveryCooldownBuff>(), 1800, false, false),
            (ModContent.ItemType<FurnaceGuardPill>(), ModContent.BuffType<FurnaceGuardBuff>(), 3600, true, true),
            (ModContent.ItemType<WindStepPill>(), ModContent.BuffType<WindStepBuff>(), 2700, true, true),
            (ModContent.ItemType<ThunderBurstPill>(), ModContent.BuffType<ThunderBurstBuff>(), 1200, true, true),
            (ModContent.ItemType<SpringReturnPill>(), ModContent.BuffType<SpringReturnBuff>(), 3600, true, false),
            (ModContent.ItemType<TribulationResistingPill>(), ModContent.BuffType<TribulationResistanceBuff>(), 5400, true, false)
        };
        try {
            foreach (var entry in cases) foreach (PillQuality quality in Enum.GetValues<PillQuality>()) {
                Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
                var output = new AuditOutputSocket(true);
                for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(), State = i == 0 ? 10 : 0, IsActive = i == 0 };
                var actor = new Player { whoAmI = 0, active = true, dead = false, selectedItem = 0 }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>();
                state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
                state.ResetEffects(); state.spiritualEnergy = 0; state.spiritPressure = 50;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()), "native pill fixture accepts valid authority character");
                var pill = new Item(); pill.SetDefaults(entry.Item1); pill.stack = 2; actor.inventory[0] = pill;
                pill.GetGlobalItem<PillQualitySystem>().LoadData(pill, new TagCompound { ["quality"] = (int)quality, ["crafted"] = true });
                CultivationItemTransactions.HandleRequest(actor, 0, pill.type);
                check(pill.stack == 1, "native authority pill transaction consumes exactly one");
                int index = actor.FindBuffIndex(entry.Item2), expected = entry.Item4 ? PillQualityRules.Scale(entry.Item3, quality) : entry.Item3;
                check(index >= 0 && actor.buffTime[index] == expected, "native authority pill applies expected quality-scaled primary buff");
                if (pill.ModItem.Name is "SpringReturnPill" or "TribulationResistingPill" or "FurnaceGuardPill" or "WindStepPill" or "ThunderBurstPill") {
                    var tips = new System.Collections.Generic.List<TooltipLine>(); pill.ModItem.ModifyTooltips(tips);
                    string key = "Mods.XianXia.PillQuality.EffectDuration", text = tips.Single(tip => tip.Name == "PillEffectDuration").Text;
                    check(Terraria.Localization.Language.GetTextValue(key) != key && text == Terraria.Localization.Language.GetTextValue(key, actor.buffTime[index] / 60f),
                        "native quality duration tooltip matches actual applied buff: " + pill.ModItem.Name + "/" + quality + " ticks=" + actor.buffTime[index] + " text=" + text);
                }
                if (pill.ModItem is QiRecoveryPill) {
                    var tips = new System.Collections.Generic.List<TooltipLine>(); pill.ModItem.ModifyTooltips(tips);
                    string key = "Mods.XianXia.PillQuality.RecoveryAmounts";
                    int recovery = PillQualityRules.Scale(QiRecoveryRules.BaseRecovery, quality), bonus = PillQualityRules.BonusEnergy(quality);
                    string text = tips.Single(tip => tip.Name == "QiRecoveryAmounts").Text;
                    check(Terraria.Localization.Language.GetTextValue(key) != key
                        && text == Terraria.Localization.Language.GetTextValue(key, recovery, bonus, QiRecoveryRules.PressureCost, QiRecoveryRules.CooldownTicks / 60)
                        && state.spiritualEnergy == recovery + bonus && state.spiritPressure == 60 && actor.buffTime[index] == 1800,
                        "native recovery pill tooltip matches actual energy pressure and fixed fatigue: " + quality + " energy=" + state.spiritualEnergy + " text=" + text);
                }
                if (entry.Item5) check(actor.HasBuff(ModContent.BuffType<QiRecoveryCooldownBuff>()) && actor.buffTime[actor.FindBuffIndex(ModContent.BuffType<QiRecoveryCooldownBuff>())] == 1800,
                    "native combat pill also applies fixed shared fatigue");
                int regeneration = PillQualityRules.RegenerationTicks(quality);
                check(actor.HasBuff(BuffID.Regeneration) == (regeneration > 0), "native pill quality bonus regeneration eligibility matches stored grade");
                var authorityBuffs = AuthoritativeBuffSync.Capture(actor);
                byte[][] packets = output.Sent.Where(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff).ToArray();
                check(packets.Length == authorityBuffs.Count, "native pill sends exactly one owner packet per changed buff including quality bonus");
                try {
                    Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = 0; Netplay.Connection = new RemoteServer();
                    var receiver = new Player { whoAmI = 0, active = true }; Main.player[0] = receiver;
                    foreach (byte[] packet in packets) {
                        byte[] payload = packet.Skip(2).ToArray();
                        var buffer = new MessageBuffer { whoAmI = 256, readBuffer = payload, reader = new BinaryReader(new MemoryStream(payload)) };
                        try { buffer.GetData(0, payload.Length, out int kind); check(kind == MessageID.AddPlayerBuff, "official owner decoder accepts native pill buff packet"); }
                        finally { buffer.reader.Dispose(); }
                    }
                    var receivedBuffs = AuthoritativeBuffSync.Capture(receiver);
                    check(receivedBuffs.Count == authorityBuffs.Count && authorityBuffs.All(pair => receivedBuffs.TryGetValue(pair.Key, out int time) && time == pair.Value),
                        "official owner receiver matches every pill effect fatigue and quality duration");
                } finally { Main.player[0] = actor; Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Netplay.Connection = oldConnection; }
                Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = Main.maxPlayers;
                var single = new Player { whoAmI = 0, active = true, selectedItem = 0 }; Main.player[0] = single;
                var singleState = single.GetModPlayer<XianXiaPlayer>(); singleState.cultivationStage = CultivationStage.DaoSevering; singleState.discoveredSpiritualEnergy = true; singleState.ResetEffects(); singleState.spiritualEnergy = 0; singleState.spiritPressure = 50;
                var singlePill = new Item(entry.Item1,2); single.inventory[0] = singlePill; singlePill.GetGlobalItem<PillQualitySystem>().LoadData(singlePill,new TagCompound { ["quality"] = (int)quality, ["crafted"] = true });
                bool? singleUsed = ItemLoader.UseItem(singlePill,single); var singleBuffs = AuthoritativeBuffSync.Capture(single);
                check(singleUsed == true && (singleState.CaptureSnapshot() with { Revision = 0 }).Equals(state.CaptureSnapshot() with { Revision = 0 }) && singleBuffs.Count == authorityBuffs.Count && authorityBuffs.All(kv => singleBuffs.TryGetValue(kv.Key,out int time) && time == kv.Value),
                    "native six-pills single-player item loader matches authority recovery pressure fatigue and quality effects");
                Main.player[0] = actor; Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
                int sentBeforeReplay = output.Sent.Count;
                CultivationItemTransactions.HandleRequest(actor, 0, pill.type);
                check(pill.stack == 1 && output.Sent.Count == sentBeforeReplay, "native duplicate pill request neither consumes nor resends buffs");
                state.ProgressionItemCooldown = 0; actor.dead = true;
                CultivationItemTransactions.HandleRequest(actor, 0, pill.type);
                check(pill.stack == 1 && output.Sent.Count == sentBeforeReplay, "native rejected dead-player pill request sends no new effects");
            }
        } finally {
            Main.player[0] = oldPlayer; oldClients.CopyTo(Netplay.Clients, 0);
            Main.netMode = oldMode; Main.myPlayer = oldLocal; Netplay.Connection = oldConnection;
        }
        check(ReferenceEquals(Main.player[0], oldPlayer) && Netplay.Clients.SequenceEqual(oldClients)
            && Main.netMode == oldMode && Main.myPlayer == oldLocal && ReferenceEquals(Netplay.Connection, oldConnection),
            "native pill audit restores player client and engine contexts");
    }
}
