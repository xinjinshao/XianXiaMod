using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Systems;

namespace XianXia.Content.Tiles.BossDecorations;

public class EndgameRouteFrameTile : MasterBossMonumentTile
{
    public override string Texture => "XianXia/Content/Tiles/SwordTabletTile";
    protected override int MonumentItem => ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.EndgameRouteFrame>();
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults(); Main.tileLighted[Type] = true;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        (r, g, b) = DownedBossSystem.ChosenRoute switch
        {
            DownedBossSystem.EndgameRoute.RebuildHeaven => (0.12f, 0.3f, 0.18f),
            DownedBossSystem.EndgameRoute.SeverHeaven => (0.32f, 0.08f, 0.06f),
            DownedBossSystem.EndgameRoute.AcceptStarAbyss => (0.18f, 0.1f, 0.35f),
            _ => (0.08f, 0.08f, 0.08f)
        };
    }
    public override bool RightClick(int i, int j)
    {
        if (Main.dedServ) return true;
        var route = DownedBossSystem.ChosenRoute;
        string name = Language.GetTextValue(EndgameRouteTransactions.NameKey(route));
        Main.NewText(Language.GetTextValue("Mods.XianXia.RouteGuide.Current", name), 220, 210, 120);
        if (route != DownedBossSystem.EndgameRoute.None)
            Main.NewText(Language.GetTextValue(EndgameRouteTransactions.NameKey(route) + "Description"), 220, 210, 120);
        return true;
    }
}
