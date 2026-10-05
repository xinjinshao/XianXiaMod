using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Systems;

public class DownedBossSystem : ModSystem
{
    public static bool DownedSpiritVeinWyrm { get; set; }
    public static HashSet<string> DownedBosses { get; } = new();
    public static HashSet<string> ClaimedCommissions { get; } = new();
    public static int SectReputation { get; private set; }

    public enum EndgameRoute { None = 0, RebuildHeaven = 1, SeverHeaven = 2, AcceptStarAbyss = 3 }
    public static EndgameRoute ChosenRoute { get; private set; }

    public static bool TryChooseRoute(EndgameRoute route)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || ChosenRoute != EndgameRoute.None
            || route < EndgameRoute.RebuildHeaven || route > EndgameRoute.AcceptStarAbyss
            || !DownedBosses.Contains("old_heaven_dao_core")) return false;
        ChosenRoute = route;
        SyncWorldProgress();
        return true;
    }

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
        ChosenRoute = EndgameRoute.None;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["downedSpiritVeinWyrm"] = DownedSpiritVeinWyrm;
        tag["downedBosses"] = DownedBosses.ToList();
        tag["claimedCommissions"] = ClaimedCommissions.ToList();
        tag["sectReputation"] = SectReputation;
        tag["chosenRoute"] = (int)ChosenRoute;
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
        int route = tag.GetInt("chosenRoute");
        ChosenRoute = route >= 0 && route <= (int)EndgameRoute.AcceptStarAbyss
            ? (EndgameRoute)route : EndgameRoute.None;
        RecalculateSectReputation();
    }

    // The dictionaries define a stable wire order; never use HashSet enumeration order.
    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(GetFlags(DownedBosses, ReputationByBoss.Keys));
        writer.Write(GetFlags(ClaimedCommissions, ReputationByCommission.Keys));
        writer.Write((byte)ChosenRoute);
    }

    public override void NetReceive(BinaryReader reader)
    {
        ushort bosses = reader.ReadUInt16();
        ushort commissions = reader.ReadUInt16();
        byte route = reader.ReadByte();
        ReadFlags(bosses, DownedBosses, ReputationByBoss.Keys);
        ReadFlags(commissions, ClaimedCommissions, ReputationByCommission.Keys);
        DownedSpiritVeinWyrm = DownedBosses.Contains("spirit_vein_wyrm");
        ChosenRoute = route <= (byte)EndgameRoute.AcceptStarAbyss ? (EndgameRoute)route : EndgameRoute.None;
        RecalculateSectReputation();
    }

    private static ushort GetFlags(HashSet<string> values, IEnumerable<string> keys)
    {
        ushort flags = 0;
        int bit = 0;
        foreach (string key in keys.OrderBy(key => key, System.StringComparer.Ordinal))
        {
            if (values.Contains(key))
                flags |= (ushort)(1 << bit);
            bit++;
        }
        return flags;
    }

    private static void ReadFlags(ushort flags, HashSet<string> values, IEnumerable<string> keys)
    {
        values.Clear();
        int bit = 0;
        foreach (string key in keys.OrderBy(key => key, System.StringComparer.Ordinal))
        {
            if ((flags & (1 << bit)) != 0)
                values.Add(key);
            bit++;
        }
    }

    private static void SyncWorldProgress()
    {
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.WorldData);
    }

    public static void MarkDowned(string bossId)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !ReputationByBoss.ContainsKey(bossId))
            return;
        bool newlyDowned = DownedBosses.Add(bossId);
        if (bossId == "spirit_vein_wyrm")
        {
            DownedSpiritVeinWyrm = true;
        }
        if (newlyDowned && ReputationByBoss.TryGetValue(bossId, out int value))
        {
            SectReputation += value;
            SyncWorldProgress();
        }
    }

    public static bool HasSectReputation(int required)
    {
        return SectReputation >= required;
    }

    public static bool TryClaimCommission(string commissionId, int reputation)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient
            || !ReputationByCommission.TryGetValue(commissionId, out int expectedReputation)
            || reputation != expectedReputation)
            return false;
        if (!ClaimedCommissions.Add(commissionId))
        {
            return false;
        }

        SectReputation += expectedReputation;
        SyncWorldProgress();
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
