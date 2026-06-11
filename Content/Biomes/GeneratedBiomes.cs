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
        greenwoodHerbGardenBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.GreenwoodSoilTile>()] + tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.SpiritHerbTile>()];
        sunkenFurnaceVeinBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.FurnaceSlagTile>()];
        thunderMarshCloudsBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.ThunderCloudTile>()];
        starAbyssRiftBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.StarAbyssCrystalTile>()];
        tenThousandSectsRuinsBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.SectRuinBrickTile>()];
        fallenHeavenPalaceBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.FallenHeavenJadeTile>()];
        moonboneAbyssBiomeTileCount = tileCounts[ModContent.TileType<global::XianXia.Content.Tiles.Generated.MoonboneTile>()];
    }
}

public class GreenwoodHerbGardenBiome : ModBiome
{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(90, 170, 150);

    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().greenwoodHerbGardenBiomeTileCount >= 120;
    }
}


public class SunkenFurnaceVeinBiome : ModBiome
{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(90, 170, 150);

    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().sunkenFurnaceVeinBiomeTileCount >= 120;
    }
}


public class ThunderMarshCloudsBiome : ModBiome
{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(90, 170, 150);

    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().thunderMarshCloudsBiomeTileCount >= 100;
    }
}


public class StarAbyssRiftBiome : ModBiome
{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(90, 170, 150);

    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().starAbyssRiftBiomeTileCount >= 140;
    }
}


public class TenThousandSectsRuinsBiome : ModBiome
{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(90, 170, 150);

    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().tenThousandSectsRuinsBiomeTileCount >= 180;
    }
}


public class FallenHeavenPalaceBiome : ModBiome
{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(90, 170, 150);

    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().fallenHeavenPalaceBiomeTileCount >= 160;
    }
}


public class MoonboneAbyssBiome : ModBiome
{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(90, 170, 150);

    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().moonboneAbyssBiomeTileCount >= 200;
    }
}
