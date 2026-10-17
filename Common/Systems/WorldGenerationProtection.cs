using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
namespace XianXia.Common.Systems;

internal static class WorldGenerationProtection
{
    private static bool[] allTypes;
    // This checks structure reservations only. Callers still validate their own terrain/occupancy.
    // The one-tile buffer also keeps SquareTileFrame away from reserved structure cells.
    internal static bool CanChange(int x, int y, StructureMap structures)
    {
        if (structures == null || !WorldGen.InWorld(x,y,10)) return false;
        if (allTypes == null || allTypes.Length != TileLoader.TileCount) {
            allTypes = new bool[TileLoader.TileCount]; Array.Fill(allTypes,true);
        }
        return structures.CanPlace(new Rectangle(x,y,1,1),allTypes,1);
    }
}
