using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.HandGenerated;

public class SpiritVeinWyrmTrophy : ModItem
{
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "DecorationPlacement", Terraria.Localization.Language.GetTextValue("Mods.XianXia.BossDecorations.PlaceableTooltip")));
    }

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.maxStack = 99;
        Item.value = Item.buyPrice(gold: 1);
        Item.rare = ItemRarityID.Blue;
        Item.consumable = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 10;
        Item.useAnimation = 15;
        Item.autoReuse = Item.useTurn = true;
        Item.createTile = ModContent.TileType<global::XianXia.Content.Tiles.BossDecorations.SpiritVeinWyrmTrophyTile>();
    }
}
