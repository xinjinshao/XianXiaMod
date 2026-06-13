using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace XianXia.Content.Tiles.Stations;

public class AlchemyCauldronTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
        TileObjectData.addTile(Type);

        DustType = DustID.GemEmerald;
        AddMapEntry(new Color(70, 180, 145), CreateMapEntryName());
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (closer)
        {
            Main.LocalPlayer.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>(), 20);
        }
    }
}
