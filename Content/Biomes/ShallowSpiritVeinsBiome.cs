using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.Biomes;

public class ShallowSpiritVeinsBiome : ModBiome
{
    public override int Music => 0;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BestiaryIcon => "XianXia/Content/Tiles/SpiritOreTile";
    public override string BackgroundPath => MapBackground;
    public override string MapBackground => "Terraria/Images/MapBG1";
    public override Color? BackgroundColor => new(78, 201, 162);

    public override bool IsBiomeActive(Player player)
    {
        return ModContent.GetInstance<SpiritVeinTileCountSystem>().spiritVeinTileCount >= 40;
    }
}
