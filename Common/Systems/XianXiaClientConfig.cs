using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace XianXia.Common.Systems;

public class XianXiaClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    [DefaultValue(28f), Range(0f, 4000f), Increment(1f)]
    public float EnergyBarX { get; set; } = 28f;
    [DefaultValue(84f), Range(0f, 4000f), Increment(1f)]
    public float EnergyBarY { get; set; } = 84f;
}
