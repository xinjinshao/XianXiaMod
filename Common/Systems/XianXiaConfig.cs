using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace XianXia.Common.Systems;

public class XianXiaConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(true)]
    public bool EnableWorldGeneration { get; set; } = true;

    [DefaultValue(1f)]
    [Range(0f, 2f)]
    [Increment(0.05f)]
    public float PermanentGrowthMultiplier { get; set; } = 1f;

    [DefaultValue(false)]
    public bool DebugDrops { get; set; } = false;
}
