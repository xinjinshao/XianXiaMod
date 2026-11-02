using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditMaterialSources(Action<bool,string> check)
    {
        bool hard = Main.hardMode, plant = NPC.downedPlantBoss, golem = NPC.downedGolemBoss, moon = NPC.downedMoonlord;
        var catalog = ModContent.GetInstance<MaterialDropSources>(); var mod = ModLoader.GetMod("XianXia");
        try {
            var enemies = mod.GetContent<ModNPC>().Where(n => EnemySpawnRules.RequiredTier(n.Name) >= 0).ToArray();
            check(enemies.Length == 17,"native material source catalog targets all seventeen ordinary enemy definitions");
            foreach (var enemy in enemies) {
                check(EnemySpawnRules.BiomeName(enemy.Name).Length > 0 && mod.Find<ModBiome>(EnemySpawnRules.BiomeName(enemy.Name)) != null,
                    "native material source enemy resolves a registered exploration biome");
                var drops = new List<DropRateInfo>();
                foreach (var rule in Main.ItemDropsDB.GetRulesForNPCID(enemy.Type,false)) rule.ReportDroprates(drops,new DropRateInfoChainFeed(1f));
                check(drops.Count > 0 && drops.Where(d => d.dropRate > 0).All(d => catalog.ForItem(d.itemId).Count(s => s.NpcType == enemy.Type) == 1),
                    "native material source catalog follows actual registered drops once per enemy and item");
            }
            check(catalog.ForItem(ModContent.ItemType<SpiritGel>()).Count == 3 && catalog.ForItem(ModContent.ItemType<LowGradeSpiritStone>()).Count == 2,
                "native material source catalog preserves multiple early enemy sources");
            check(catalog.ForItem(ModContent.ItemType<ArchiveRemnantLight>()).Single().Tier == 4 && catalog.ForItem(ItemID.DirtBlock).Count == 0,
                "native material source catalog identifies late archive material without inventing unrelated drops");
            var materials = mod.GetContent<ModItem>().Select(i => new Item(i.Type)).Where(i => catalog.ForItem(i.type).Count > 0).ToArray();
            foreach (var item in materials) for (int flags = 0; flags < 16; flags++) {
                Main.hardMode = (flags & 1) != 0; NPC.downedPlantBoss = (flags & 2) != 0;
                NPC.downedGolemBoss = (flags & 4) != 0; NPC.downedMoonlord = (flags & 8) != 0;
                var sources = catalog.ForItem(item.type); var lines = new List<TooltipLine>();
                check(item.TryGetGlobalItem<MaterialSourceHelp>(out var help),"native source tooltip is attached to actual material instances");
                help.ModifyTooltips(item,lines);
                check(lines.Count == sources.Count + 1 && lines.All(l => !l.Text.Contains("Mods.XianXia.") && !l.Text.Contains("{0}")),
                    "native source tooltip resolves all source lines and explicitly limits its scope");
                foreach (var source in sources) {
                    string line = lines.Single(l => l.Name == "EnemySource" + source.NpcType).Text;
                    bool unlocked = EnemySpawnRules.Allows(source.Enemy,Main.hardMode,NPC.downedPlantBoss,NPC.downedGolemBoss,NPC.downedMoonlord);
                    check(line.Contains(Lang.GetNPCNameValue(source.NpcType)) && line.Contains(mod.Find<ModBiome>(source.Biome).DisplayName.Value)
                        && line.Contains(Language.GetTextValue("Mods.XianXia.MaterialSources." + (unlocked ? "Unlocked" : "Locked"))),
                        "native source tooltip tracks actual world gates while retaining enemy and biome names");
                }
            }
            int stone = ModContent.ItemType<LowGradeSpiritStone>(); var before = catalog.ForItem(stone).ToArray();
            catalog.OnWorldUnload(); check(catalog.ForItem(stone).SequenceEqual(before),"native source cache rebuilds after world unload without duplicate entries");
            catalog.OnWorldLoad(); check(catalog.ForItem(stone).SequenceEqual(before),"native source cache rebuilds after world load");
        } finally { Main.hardMode = hard; NPC.downedPlantBoss = plant; NPC.downedGolemBoss = golem; NPC.downedMoonlord = moon; }
    }
}
