using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditRetryItem(Action<bool, string> check)
    {
        Player previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        var clients = (RemoteClient[])Netplay.Clients.Clone();
        try {
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(), State = 0 };
            foreach (int authority in new[] { NetmodeID.SinglePlayer, NetmodeID.Server }) for (int stage = 3; stage <= 8; stage++) {
                Main.netMode = authority; Main.myPlayer = Main.maxPlayers;
                var actor = new Player { whoAmI = 0, active = true, selectedItem = 0 }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = (CultivationStage)stage; state.discoveredSpiritualEnergy = true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()), "native retry item fixture imports a valid player state");
                var token = new Item(ModContent.ItemType<TribulationTrainingToken>()); actor.inventory[0] = token;
                bool Use() {
                    if (authority == NetmodeID.Server) { CultivationItemTransactions.HandleRequest(actor, 0, token.type); return state.tribulationTimer > 0; }
                    return ItemLoader.UseItem(token, actor) == true;
                }
                check(!token.consumable && token.stack == 1 && ItemLoader.CanUseItem(token, actor), "native retry token is reusable and can start an unfinished realm trial");
                check(Use() && state.tribulationTimer > 0 && state.tribulationStage == stage && token.stack == 1 && ReferenceEquals(actor.inventory[0], token),
                    "native actual retry item starts a trial while preserving held token identity and stack");
                long session = state.TribulationSession; var active = state.CaptureSnapshot();
                Use();
                check(state.CaptureSnapshot() == active && state.TribulationSession == session && token.stack == 1,
                    "native repeated retry item use cannot restart active trial or consume token");
                state.HandleTribulationFailure(); state.ProgressionItemCooldown = 0;
                check(Use() && state.TribulationSession > session && state.tribulationAttempts == 1 && state.tribulationWeakness == 10800 && token.stack == 1,
                    "native same retry token starts a fresh session after failure without clearing weakness or insight attempts");
                if (authority != NetmodeID.Server) continue;
                state.HandleTribulationFailure();
                foreach (int invalid in new[] { 0, 1, 2, 3, 4 }) {
                    state.ProgressionItemCooldown = 0; actor.dead = invalid == 0; actor.noItems = invalid == 1; actor.frozen = invalid == 2; actor.active = invalid != 3;
                    var before = state.CaptureSnapshot();
                    CultivationItemTransactions.HandleRequest(actor, invalid == 4 ? 1 : 0, token.type);
                    check(state.CaptureSnapshot() == before && token.stack == 1 && state.TribulationSession == 0,
                        "native retry transaction rejects dead disabled controlled inactive or mismatched-slot requests without inventory or trial changes");
                }
            }
        }
        finally { Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; clients.CopyTo(Netplay.Clients, 0); }
    }
}
