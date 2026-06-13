using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Tiles;

public class BlackFurnaceWall : ModWall

{

    public override void SetStaticDefaults()

    {

        Main.wallHouse[Type] = false;

        DustType = DustID.Stone;

        AddMapEntry(new Color(90, 82, 76), CreateMapEntryName());

    }

}
