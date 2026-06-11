using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Systems;

public class DownedBossSystem : ModSystem
{
    public static bool DownedSpiritVeinWyrm { get; set; }

    public override void ClearWorld()
    {
        DownedSpiritVeinWyrm = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["downedSpiritVeinWyrm"] = DownedSpiritVeinWyrm;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        DownedSpiritVeinWyrm = tag.GetBool("downedSpiritVeinWyrm");
    }
}
