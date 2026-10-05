using Terraria.ModLoader;
using XianXia.Common.Items;

namespace XianXia.Content.Tiles.MasterRewards;

public class BlackFurnaceIronGolemMonumentTile : MasterBossMonumentTile
{
    public override string Texture => "XianXia/Content/Tiles/SwordTabletTile";
    protected override int MonumentItem => ModContent.ItemType<global::XianXia.Content.Items.MasterRewards.BlackFurnaceIronGolemMonument>();
}
