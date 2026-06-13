using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Tiles;

public class StarAbyssCrystalTile : ModTile

{

    public override void SetStaticDefaults()

    {

        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.Stone;

        MineResist = 1.1f;

        AddMapEntry(new Color(120, 180, 150), CreateMapEntryName());

        RegisterItemDrop(ModContent.ItemType<global::XianXia.Content.Items.Materials.StarEclipseCrystal>());

    }

}
