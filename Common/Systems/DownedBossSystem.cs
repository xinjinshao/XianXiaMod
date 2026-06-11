using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Systems;

public class DownedBossSystem : ModSystem
{
    public static bool DownedSpiritVeinWyrm { get; set; }
    public static HashSet<string> DownedBosses { get; } = new();
    public static int SectReputation { get; private set; }

    private static readonly Dictionary<string, int> ReputationByBoss = new()
    {
        ["spirit_vein_wyrm"] = 5,
        ["garden_warden"] = 10,
        ["black_furnace_iron_golem"] = 10,
        ["tribulation_cloud_avatar"] = 12,
        ["thunder_marsh_jiao"] = 18,
        ["abyssal_star_womb"] = 18,
        ["formless_sword_soul"] = 24,
        ["greenwood_medicine_king_echo"] = 24,
        ["heaven_tablet_guardian"] = 36,
        ["broken_heaven_inspector"] = 36,
        ["moonbone_immortal"] = 60,
        ["old_heaven_dao_core"] = 80,
    };

    public override void ClearWorld()
    {
        DownedSpiritVeinWyrm = false;
        DownedBosses.Clear();
        SectReputation = 0;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["downedSpiritVeinWyrm"] = DownedSpiritVeinWyrm;
        tag["downedBosses"] = DownedBosses.ToList();
        tag["sectReputation"] = SectReputation;
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
        RecalculateSectReputation();
    }

    public static void MarkDowned(string bossId)
    {
        bool newlyDowned = DownedBosses.Add(bossId);
        if (bossId == "spirit_vein_wyrm")
        {
            DownedSpiritVeinWyrm = true;
        }
        if (newlyDowned && ReputationByBoss.TryGetValue(bossId, out int value))
        {
            SectReputation += value;
        }
    }

    public static bool HasSectReputation(int required)
    {
        return SectReputation >= required;
    }

    private static void RecalculateSectReputation()
    {
        SectReputation = 0;
        foreach (string bossId in DownedBosses)
        {
            if (ReputationByBoss.TryGetValue(bossId, out int value))
            {
                SectReputation += value;
            }
        }
    }
}
