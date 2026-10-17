using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Items.Materials;

public class LowGradeSpiritStone : ModItem
{
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
    {
        string hint = global::XianXia.Common.Systems.CalamityRecipeSystem.GetSourceHint();
        if (hint.Length > 0) tooltips.Add(new TooltipLine(Mod, "CalamitySource", hint));
    }
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

    private void Discover(Player player, bool allowClient)
    {
        if ((!allowClient && Main.netMode == NetmodeID.MultiplayerClient) || player == null || !player.active || player.dead || Item.IsAir
            || !player.TryGetModPlayer<XianXiaPlayer>(out var state) || state.discoveredSpiritualEnergy) return;
        state.discoveredSpiritualEnergy = true;
        if (Main.netMode != NetmodeID.MultiplayerClient) state.AdvanceResourceRevision();
    }

    // Inventory acquisition relies on authority; pickup keeps its existing immediate local feedback.
    public override void UpdateInventory(Player player) => Discover(player, false);
    public override bool OnPickup(Player player)
    {
        Discover(player, true);
        return true;
    }
}
