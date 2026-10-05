using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public static class BossSummonTransactions
{
    public static bool TrySummon(Player player, CultivationBossSummonItem summon)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !player.active || player.dead || player.noItems || player.CCed
            || NPC.AnyNPCs(summon.BossType) || !CombinedHooks.CanUseItem(player, summon.Item)) return false;
        NPC.SpawnOnPlayer(player.whoAmI, summon.BossType);
        // SpawnOnPlayer can fail when all NPC slots are occupied. A failed
        // spawn must not consume the item even if its eligibility check passed.
        return NPC.AnyNPCs(summon.BossType);
    }

    public static void HandleRequest(Player player, int slot, int type)
    {
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        if (Main.netMode != NetmodeID.Server || !player.active || player.dead || player.noItems || player.CCed
            || !state.NetworkInitialized || slot < 0 || slot != player.selectedItem
            || slot >= player.inventory.Length || state.BossSummonCooldown > 0) return;
        Item item = player.inventory[slot];
        if (item.type != type || item.stack <= 0
            || item.ModItem is not CultivationBossSummonItem summon) return;
        state.BossSummonCooldown = Math.Max(20, item.useTime);
        bool spawned = TrySummon(player, summon);
        if (spawned && item.consumable)
        {
            item.stack--;
            if (item.stack <= 0) item.TurnToAir();
        }
        NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, slot);
        if (!spawned)
        {
            var reply = summon.Mod.GetPacket();
            reply.Write((byte)7);
            NetworkText.FromKey("Mods.XianXia.Progression.BossSummonFailed").Serialize(reply);
            reply.Send(player.whoAmI);
        }
    }
}
