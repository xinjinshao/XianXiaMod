using System;
using Terraria;
using Terraria.ModLoader;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditHerbMap(Action<bool, string> check)
    {
        int x = Main.maxTilesX / 2, y = 12;
        Tile tile = Main.tile[x, y]; bool oldHas = tile.HasTile; ushort oldType = tile.TileType; short oldFrame = tile.TileFrameX;
        var herb = ModContent.GetInstance<CultivatedSpiritHerbTile>();
        try {
            tile.HasTile = true; tile.TileType = (ushort)herb.Type;
            string[] keys = { "Seedling", "Growing", "Mature" };
            for (int stage = 0; stage < 3; stage++) {
                tile.TileFrameX = (short)(stage * 18);
                ushort option = herb.GetMapOption(x, y);
                check(option == stage && tile.TileFrameX == stage * 18, "native herb map stage follows real frame without growing the plant");
                string key = "Mods.XianXia.Tiles.CultivatedSpiritHerbTile." + keys[stage];
                string expected = Terraria.Localization.Language.GetTextValue(key);
                check(expected != key && herb.GetLocalization(keys[stage]).Value == expected,
                    "native registered herb resolves the localized label for each real growth stage; client map atlas is not initialized on dedicated servers");
            }
        } finally { tile.HasTile = oldHas; tile.TileType = oldType; tile.TileFrameX = oldFrame; }
        check(tile.HasTile == oldHas && tile.TileType == oldType && tile.TileFrameX == oldFrame, "native herb map audit restores all patched terrain fields");
    }
}
