using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

// Progression uses the actual held inventory item, never a client-supplied realm.
public class CultivationItemTransactions : GlobalItem
{
    public static bool IsProgressionItem(Item item) => item.ModItem?.Mod is global::XianXia.XianXia
        && item.ModItem.Name is "QiDrawingTalisman" or "QiCondensingPill" or "FoundationPill"
            or "StarEclipseCrystal" or "OldHeavenDaoScroll" or "HeavenDaoFragment"
            or "Moonbone" or "DaoSeveringDust" or "TribulationTrainingToken" or "LightningAvoidanceRune"
            or "ThunderBurstPill" or "WindStepPill" or "FurnaceGuardPill" or "QiRecoveryPill" or "SpringReturnPill" or "TribulationResistingPill" or "StarAbyssForbiddenTalisman";

    public override bool ConsumeItem(Item item, Player player) =>
        Main.netMode != NetmodeID.MultiplayerClient || !IsProgressionItem(item);

    // Called by the specific ModItem before applying its normal effect.
    public static bool RequestIfMultiplayer(Player player, Item item)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return false;
        if (Main.netMode == NetmodeID.Server)
            return !player.GetModPlayer<XianXiaPlayer>().ApplyingProgressionItem;
        if (player.whoAmI == Main.myPlayer && player.HeldItem == item)
        {
            ModPacket packet = item.ModItem.Mod.GetPacket();
            packet.Write((byte)5);
            packet.Write((byte)player.selectedItem);
            packet.Write(item.type);
            packet.Send();
        }
        return true;
    }

    public static void HandleRequest(Player player, int slot, int type)
    {
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        if (Main.netMode != NetmodeID.Server || !player.active || player.dead || player.noItems || player.CCed
            || !state.NetworkInitialized || slot < 0 || slot != player.selectedItem
            || slot >= player.inventory.Length || state.ProgressionItemCooldown > 0) return;
        Item item = player.inventory[slot];
        if (item.type != type || item.stack <= 0 || !IsProgressionItem(item)) return;
        state.ProgressionItemCooldown = System.Math.Max(20, item.useTime);
        if (CombinedHooks.CanUseItem(player, item))
        {
            CultivationSnapshot before = state.CaptureSnapshot();
            int[] oldBuffTimes = (int[])player.buffTime.Clone();
            int[] oldBuffTypes = (int[])player.buffType.Clone();
            var oldBuffs = AuthoritativeBuffSync.Capture(player);
            state.ApplyingProgressionItem = true;
            try { item.ModItem.UseItem(player); }
            finally { state.ApplyingProgressionItem = false; }
            if (state.CaptureSnapshot() != before || !System.Linq.Enumerable.SequenceEqual(oldBuffTimes, player.buffTime)
                || !System.Linq.Enumerable.SequenceEqual(oldBuffTypes, player.buffType))
            {
                PillQualitySystem.ApplyUseBonus(item, player);
                AuthoritativeBuffSync.SendChanges(player, oldBuffs);
                state.AdvanceResourceRevision();
                if (item.consumable)
                {
                    item.stack--;
                    if (item.stack <= 0) item.TurnToAir();
                }
            }
        }
        // Success or failure: return canonical inventory and state to the owner.
        AuthoritativeInventorySync.Send(player, slot);
        state.SyncPlayer(-1, -1, false);
    }
}
