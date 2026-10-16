using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Consumables;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditQiAwakening(Action<bool, string> check)
    {
        Player oldPlayer = Main.player[0]; var oldClients = (RemoteClient[])Netplay.Clients.Clone();
        int oldMode = Main.netMode, oldLocal = Main.myPlayer;
        try {
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(i == 0), State = i == 0 ? 10 : 0, IsActive = i == 0 };
            foreach (int mode in new[] { NetmodeID.SinglePlayer, NetmodeID.Server }) foreach (int stage in Enumerable.Range(0, 9)) foreach (bool full in new[] { false, true }) {
                Main.netMode = mode; Main.myPlayer = Main.maxPlayers;
                var actor = new Player { whoAmI = 0, active = true, dead = false, selectedItem = 0 }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>();
                int energy = full ? CultivationSnapshot.MaxEnergy(stage) : 0;
                var initial = new CultivationSnapshot(energy, (byte)stage, stage > 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                check(state.TryInitializeNetwork(initial), "native awakening fixture imports valid realm and resource state");
                var talisman = new Item(); talisman.SetDefaults(ModContent.ItemType<QiDrawingTalisman>()); talisman.stack = 2; actor.inventory[0] = talisman;
                check(CombinedHooks.CanUseItem(actor, talisman) == (stage == 0), "native combined item-use gate allows Qi talisman only before entering first realm");
                if (mode == NetmodeID.Server) CultivationItemTransactions.HandleRequest(actor, 0, talisman.type);
                else check(talisman.ModItem.UseItem(actor) == (stage == 0), "native singleplayer use result matches first-realm eligibility");
                if (stage == 0) {
                    check(state.cultivationStage == CultivationStage.QiAwakening && state.discoveredSpiritualEnergy && state.spiritualEnergy == Math.Min(40, energy + 30),
                        "native first awakening still enters Qi realm and restores bounded thirty energy");
                    check(talisman.stack == (mode == NetmodeID.Server ? 1 : 2), "native server awakening consumes one while direct singleplayer hook leaves engine consumption separate");
                } else check(state.CaptureSnapshot() == initial && talisman.stack == 2, "native later-realm talisman rejection preserves cultivation resources and inventory");
                var after = state.CaptureSnapshot(); int stack = talisman.stack;
                state.ProgressionItemCooldown = 0;
                if (mode == NetmodeID.Server) CultivationItemTransactions.HandleRequest(actor, 0, talisman.type);
                else check(talisman.ModItem.UseItem(actor) is false, "native repeated singleplayer talisman use is refused");
                state.UnlockQiAwakening();
                check(state.CaptureSnapshot() == after && talisman.stack == stack, "native repeated request and direct unlock cannot refill energy or consume another talisman");
            }
            Main.netMode = NetmodeID.Server;
            foreach (int invalid in new[] { 0, 1, 2, 3 }) {
                var actor = new Player { active = invalid != 0, dead = invalid == 1, noItems = invalid == 2, stoned = invalid == 3 };
                var item = new Item(); item.SetDefaults(ModContent.ItemType<QiDrawingTalisman>()); item.stack = 1;
                var state = actor.GetModPlayer<XianXiaPlayer>(); var before = state.CaptureSnapshot();
                check(!item.ModItem.CanUseItem(actor) && item.ModItem.UseItem(actor) is false, "native inactive dead blocked or crowd-controlled actor cannot use awakening talisman");
                state.UnlockQiAwakening(); check(state.CaptureSnapshot() == before && item.stack == 1, "native guarded direct awakening respects invalid player lifecycle");
            }
        } finally { Main.player[0] = oldPlayer; oldClients.CopyTo(Netplay.Clients, 0); Main.netMode = oldMode; Main.myPlayer = oldLocal; }
        check(ReferenceEquals(Main.player[0], oldPlayer) && Netplay.Clients.SequenceEqual(oldClients) && Main.netMode == oldMode && Main.myPlayer == oldLocal,
            "native awakening audit restores player client and engine contexts");
    }
}
