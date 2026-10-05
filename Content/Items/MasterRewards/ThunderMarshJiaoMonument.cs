using Terraria.ModLoader;
using XianXia.Common.Items;

namespace XianXia.Content.Items.MasterRewards;

public class ThunderMarshJiaoMonument : MasterBossMonument
{
    public override string Texture => "XianXia/Content/Tiles/SwordTabletTile";
    protected override int MonumentTile => ModContent.TileType<global::XianXia.Content.Tiles.MasterRewards.ThunderMarshJiaoMonumentTile>();
}
