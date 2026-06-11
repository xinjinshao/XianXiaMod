using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Tiles;

public class SpiritMossTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = false;
        DustType = DustID.Grass;
        MineResist = 0.6f;
        AddMapEntry(new Color(80, 190, 130), CreateMapEntryName());
    }
}
