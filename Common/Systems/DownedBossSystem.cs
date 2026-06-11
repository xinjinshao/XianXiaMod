using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Systems;

public class DownedBossSystem : ModSystem
{
    public static bool DownedSpiritVeinWyrm { get; set; }
    public static HashSet<string> DownedBosses { get; } = new();

    public override void ClearWorld()
    {
        DownedSpiritVeinWyrm = false;
        DownedBosses.Clear();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["downedSpiritVeinWyrm"] = DownedSpiritVeinWyrm;
        tag["downedBosses"] = DownedBosses.ToList();
    }

    public override void LoadWorldData(TagCompound tag)
    {
        DownedSpiritVeinWyrm = tag.GetBool("downedSpiritVeinWyrm");
        DownedBosses.Clear();
        foreach (string boss in tag.GetList<string>("downedBosses"))
        {
            DownedBosses.Add(boss);
        }
        if (DownedSpiritVeinWyrm)
        {
            DownedBosses.Add("spirit_vein_wyrm");
        }
    }

    public static void MarkDowned(string bossId)
    {
        DownedBosses.Add(bossId);
        if (bossId == "spirit_vein_wyrm")
        {
            DownedSpiritVeinWyrm = true;
        }
    }
}
