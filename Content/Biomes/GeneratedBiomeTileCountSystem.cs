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

        greenwoodHerbGardenBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.GreenwoodSoilTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Construction.GreenwoodConstructedTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.SpiritHerbTile>()];

        sunkenFurnaceVeinBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.FurnaceSlagTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Construction.FurnaceConstructedTile>()];

        thunderMarshCloudsBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.ThunderCloudTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Construction.ThunderConstructedTile>()];

        starAbyssRiftBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.StarAbyssCrystalTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Construction.StarAbyssConstructedTile>()];

        tenThousandSectsRuinsBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.SectRuinBrickTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Construction.SectRuinConstructedTile>()];

        fallenHeavenPalaceBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.FallenHeavenJadeTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Construction.FallenHeavenConstructedTile>()];

        moonboneAbyssBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.MoonboneTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Construction.MoonboneConstructedTile>()];

    }

}
