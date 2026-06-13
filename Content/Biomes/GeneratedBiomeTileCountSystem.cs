using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ModLoader;

namespace XianXia.Content.Biomes;

public class GeneratedBiomeTileCountSystem : ModSystem

{

    public int greenwoodHerbGardenBiomeTileCount;

    public int sunkenFurnaceVeinBiomeTileCount;

    public int thunderMarshCloudsBiomeTileCount;

    public int starAbyssRiftBiomeTileCount;

    public int tenThousandSectsRuinsBiomeTileCount;

    public int fallenHeavenPalaceBiomeTileCount;

    public int moonboneAbyssBiomeTileCount;



    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)

    {

        greenwoodHerbGardenBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.GreenwoodSoilTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.SpiritHerbTile>()];

        sunkenFurnaceVeinBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.FurnaceSlagTile>()];

        thunderMarshCloudsBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.ThunderCloudTile>()];

        starAbyssRiftBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.StarAbyssCrystalTile>()];

        tenThousandSectsRuinsBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.SectRuinBrickTile>()];

        fallenHeavenPalaceBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.FallenHeavenJadeTile>()];

        moonboneAbyssBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.MoonboneTile>()];

    }

}
