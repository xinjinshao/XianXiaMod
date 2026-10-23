using System;
using Terraria;
using Terraria.ID;

namespace XianXia.Common.Systems;

public static class CommissionRewards
{
    // QuickSpawnItem needs an allocatable world slot for each reward stack, even with inventory space.
    public static bool HasCapacity(int stacks)
    {
        if (stacks < 0 || Main.item == null || Main.timeItemSlotCannotBeReusedFor == null) return false;
        int slots = Math.Min(Main.maxItems, Math.Min(Main.item.Length, Main.timeItemSlotCannotBeReusedFor.Length));
        int available = 0;
        for (int i = 0; i < slots && available < stacks; i++)
            if (Main.item[i] != null && !Main.item[i].active && Main.timeItemSlotCannotBeReusedFor[i] == 0) available++;
        return available >= stacks;
    }
    public static void Spawn(Player player, NPC npc, int type, int stack)
    {
        if (Main.netMode != NetmodeID.Server) { player.QuickSpawnItem(npc.GetSource_FromThis(), type, stack); return; }
        // Set ownership before any broadcast; do not expose an initially unreserved reward.
        int slot = Item.NewItem(npc.GetSource_FromThis(), player.getRect(), type, stack, noBroadcast: true);
        if (slot < 0 || slot >= Main.maxItems || !Main.item[slot].active) return;
        Item reward = Main.item[slot];
        reward.playerIndexTheItemIsReservedFor = player.whoAmI;
        reward.timeSinceTheItemHasBeenReservedForSomeone = 0;
        NetMessage.SendData(MessageID.SyncItem, number: slot);
        NetMessage.SendData(MessageID.ItemOwner, number: slot);
    }
}
