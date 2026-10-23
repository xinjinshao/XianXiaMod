using System;
using Terraria;

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
}
