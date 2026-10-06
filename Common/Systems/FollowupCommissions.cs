using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;

public static class FollowupCommissions
{
    private sealed record Entry(string Prior, string Id, CultivationStage Stage, string Boss, int Reputation, (int Type, int Stack)[] Rewards);
    private static Entry Find(string npc) => npc switch {
        "HerbSectApprentice" => new("herb_sect_apprentice_garden", "herb_sect_apprentice_king", CultivationStage.NascentSoul, "greenwood_medicine_king_echo", 12, new[] { (ModContent.ItemType<QiRecoveryPill>(), 5), (ModContent.ItemType<WindStepPill>(), 3) }),
        "WanderingArtificer" => new("wandering_artificer_furnace", "wandering_artificer_sword", CultivationStage.NascentSoul, "formless_sword_soul", 16, new[] { (ModContent.ItemType<ArtifactQuenchingCrystal>(), 2), (ModContent.ItemType<ArtifactBlankShard>(), 6) }),
        "TribulationObserver" => new("tribulation_observer_thunder", "tribulation_observer_inspector", CultivationStage.SpiritSevering, "broken_heaven_inspector", 18, new[] { (ModContent.ItemType<ThunderBurstPill>(), 3), (ModContent.ItemType<TribulationResistingPill>(), 4) }),
        "ArchiveScrollSpirit" => new("archive_scroll_spirit_trial", "archive_scroll_spirit_moon", CultivationStage.Tribulation, "moonbone_immortal", 24, new[] { (ModContent.ItemType<ArchiveRemnantLight>(), 4), (ModContent.ItemType<ColdMoonDust>(), 6) }),
        "FallenHeavenMessenger" => new("fallen_heaven_messenger_tablet", "fallen_heaven_messenger_core", CultivationStage.Tribulation, "old_heaven_dao_core", 30, new[] { (ModContent.ItemType<HeavenDaoFragment>(), 8), (ModContent.ItemType<QiRecoveryPill>(), 6) }),
        _ => null,
    };
    private static bool Eligible(Player player, Entry entry) =>
        player.GetModPlayer<XianXiaPlayer>().cultivationStage >= entry.Stage
        && player.GetModPlayer<XianXiaPlayer>().cultivationStage <= CultivationStage.DaoSevering
        && CultivationRules.GetWorldFailure(entry.Stage, Main.hardMode, NPC.downedPlantBoss,
            NPC.downedGolemBoss, NPC.downedMoonlord).Length == 0
        && DownedBossSystem.DownedBosses.Contains(entry.Boss);
    public static string Hint(string npc, Player player)
    {
        Entry entry = Find(npc);
        if (entry == null || !DownedBossSystem.ClaimedCommissions.Contains(entry.Prior)) return "";
        string status = DownedBossSystem.ClaimedCommissions.Contains(entry.Id) ? "Completed"
            : Eligible(player, entry) ? "Ready" : "Locked";
        return "\n\n" + Language.GetTextValue($"Mods.XianXia.FollowupCommissions.{npc}.{status}");
    }
    public static bool TryClaim(Player player, NPC npc, string npcName, out NetworkText response)
    {
        response = NetworkText.FromKey("Mods.XianXia.NPCs.Commission.Unavailable");
        Entry entry = Find(npcName);
        if (Main.netMode == NetmodeID.MultiplayerClient || player == null || !player.active || player.dead
            || player.CCed || player.noItems || npc == null || !npc.active || npc.ModNPC?.Name != npcName
            || player.talkNPC != npc.whoAmI || Microsoft.Xna.Framework.Vector2.DistanceSquared(player.Center, npc.Center) > 600f * 600f
            || entry == null || !DownedBossSystem.ClaimedCommissions.Contains(entry.Prior)) return false;
        string key = $"Mods.XianXia.FollowupCommissions.{npcName}.";
        if (DownedBossSystem.ClaimedCommissions.Contains(entry.Id))
        {
            response = NetworkText.FromKey(key + "Completed"); return false;
        }
        if (!Eligible(player, entry))
        {
            response = NetworkText.FromKey(key + "Locked"); return false;
        }
        if (!DownedBossSystem.TryClaimCommission(entry.Id, entry.Reputation)) return false;
        foreach (var reward in entry.Rewards) player.QuickSpawnItem(npc.GetSource_FromThis(), reward.Type, reward.Stack);
        response = NetworkText.FromKey(key + "Claimed", entry.Reputation, DownedBossSystem.SectReputation);
        return true;
    }
}
