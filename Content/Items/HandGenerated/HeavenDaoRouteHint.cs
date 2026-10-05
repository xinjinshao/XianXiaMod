using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.Items.HandGenerated;

public class HeavenDaoRouteHint : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 5); Item.rare = ItemRarityID.Yellow;
        Item.useStyle = ItemUseStyleID.HoldUp; Item.useTime = Item.useAnimation = 30;
        Item.UseSound = SoundID.Item4; Item.consumable = false;
    }
    public override bool? UseItem(Player player)
    {
        if (Main.dedServ || player.whoAmI != Main.myPlayer || !player.active || player.dead) return false;
        string current = Language.GetTextValue(EndgameRouteTransactions.NameKey(DownedBossSystem.ChosenRoute));
        Main.NewText(Language.GetTextValue("Mods.XianXia.RouteGuide.Current", current), 220, 210, 120);
        foreach (string route in new[] { "RebuildHeaven", "SeverHeaven", "AcceptStarAbyss" })
            Main.NewText(Language.GetTextValue("Mods.XianXia.Routes." + route) + ": "
                + Language.GetTextValue("Mods.XianXia.Routes." + route + "Description"), 220, 210, 120);
        Main.NewText(Language.GetTextValue("Mods.XianXia.Routes.Warning"), 220, 210, 120);
        Main.NewText(Language.GetTextValue("Mods.XianXia.RouteGuide.Materials"), 220, 210, 120);
        return true;
    }
}
