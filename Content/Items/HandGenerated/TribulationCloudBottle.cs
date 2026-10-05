using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.HandGenerated;

public class TribulationCloudBottle : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.LightRed;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetJumpState(ExtraJump.CloudInABottle).Enable();
        player.buffImmune[BuffID.Electrified] = true;
    }
}
