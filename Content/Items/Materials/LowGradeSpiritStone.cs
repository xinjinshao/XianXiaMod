using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Items.Materials;

public class LowGradeSpiritStone : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(silver: 1);
        Item.rare = ItemRarityID.White;
    }

    public override void UpdateInventory(Player player)
    {
        // Chest transfers, crafting and old inventory saves need not raise a ground-pickup hook.
        if (Main.netMode == NetmodeID.MultiplayerClient || player == null || !player.active || player.dead || Item.IsAir
            || !player.TryGetModPlayer<XianXiaPlayer>(out var state) || state.discoveredSpiritualEnergy) return;
        state.discoveredSpiritualEnergy = true;
        state.AdvanceResourceRevision();
    }

    public override bool OnPickup(Player player)
    {
        player.GetModPlayer<XianXiaPlayer>().discoveredSpiritualEnergy = true;
        return true;
    }
}
