using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Systems;

public class DownedBossSystem : ModSystem
{
    public static bool DownedSpiritVeinWyrm { get; set; }
    public static HashSet<string> DownedBosses { get; } = new();
    public static HashSet<string> ClaimedCommissions { get; } = new();
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

    private static readonly Dictionary<string, int> ReputationByCommission = new()
    {
        ["herb_sect_apprentice_garden"] = 8,
        ["wandering_artificer_furnace"] = 8,
        ["tribulation_observer_thunder"] = 12,
        ["archive_scroll_spirit_trial"] = 16,
        ["fallen_heaven_messenger_tablet"] = 24,
    };

    public override void ClearWorld()
    {
        DownedSpiritVeinWyrm = false;
        DownedBosses.Clear();
        ClaimedCommissions.Clear();
        SectReputation = 0;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["downedSpiritVeinWyrm"] = DownedSpiritVeinWyrm;
        tag["downedBosses"] = DownedBosses.ToList();
        tag["claimedCommissions"] = ClaimedCommissions.ToList();
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
        ClaimedCommissions.Clear();
        foreach (string commission in tag.GetList<string>("claimedCommissions"))
        {
            ClaimedCommissions.Add(commission);
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

    public static bool TryClaimCommission(string commissionId, int reputation)
    {
        if (!ClaimedCommissions.Add(commissionId))
        {
            return false;
        }

        SectReputation += reputation;
        return true;
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
        foreach (string commissionId in ClaimedCommissions)
        {
            if (ReputationByCommission.TryGetValue(commissionId, out int value))
            {
                SectReputation += value;
            }
        }
    }
}
