using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Tiles;

public class SpiritOreTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        TileID.Sets.Ore[Type] = true;
        DustType = DustID.GemEmerald;
        MineResist = 1.2f;
        MinPick = 0;
        AddMapEntry(new Color(78, 201, 162), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<LowGradeSpiritStone>());
    }
}
