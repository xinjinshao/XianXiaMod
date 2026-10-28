using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Guides;
using XianXia.Content.Items.BossSummons;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditLedgerProgress(Action<bool, string> check)
    {
        bool oldHard = Main.hardMode, oldPlant = NPC.downedPlantBoss, oldGolem = NPC.downedGolemBoss, oldMoon = NPC.downedMoonlord;
        bool oldWyrm = DownedBossSystem.DownedSpiritVeinWyrm;
        var oldBosses = DownedBossSystem.DownedBosses.ToArray();
        try {
            int[] items = { ModContent.ItemType<SpiritVeinIncense>(), ModContent.ItemType<SummonGardenBrokenKey>(),
                ModContent.ItemType<SummonOldFurnaceEmber>(), ModContent.ItemType<SummonThunderCallingJade>(),
                ModContent.ItemType<SummonThunderCallingJadeThunderMarshJiao>(), ModContent.ItemType<SummonStarAbyssMembrane>(),
                ModContent.ItemType<SummonSectTrialToken>(), ModContent.ItemType<SummonSectTrialTokenGreenwoodMedicineKingEcho>(),
                ModContent.ItemType<SummonHeavenTabletRubbing>(), ModContent.ItemType<SummonHeavenTabletRubbingBrokenHeavenInspector>(),
                ModContent.ItemType<SummonMoonboneRitualTalisman>(), ModContent.ItemType<SummonMoonboneRitualTalismanOldHeavenDaoCore>(),
                ModContent.ItemType<TribulationTrainingToken>() };
            foreach (int type in items) {
                var recipe = Main.recipe.Take(Recipe.numRecipes).First(r => !r.Disabled && r.createItem.type == type && r.Conditions.Count == 0);
                string text = SectLedger.RecipeGuidance(type);
                check(text.Contains(Lang.GetItemNameValue(type)) && recipe.requiredItem.All(ingredient => text.Contains(Lang.GetItemNameValue(ingredient.type) + " ×" + ingredient.stack))
                    && !text.Contains("{0}") && !text.Contains("Mods.XianXia."),
                    "native ledger formats every actual summon and retry recipe with registered names and quantities");
            }
            string gardenRecipe = SectLedger.RecipeGuidance(ModContent.ItemType<SummonGardenBrokenKey>());
            check(gardenRecipe.Contains(Lang.GetItemNameValue(ModContent.ItemType<Content.Items.Materials.GreenwoodRoot>()) + " ×4"),
                "native ledger also shows the preceding craftable garden key recipe");
            var actor = new Player { active = true };
            actor.GetModPlayer<XianXiaPlayer>().LoadData(new TagCompound {
                ["cultivationStage"] = (int)CultivationStage.DaoSevering,
                ["clearedTribulationStages"] = new List<int> { 3, 4, 5, 6, 7, 8 }
            });
            DownedBossSystem.DownedBosses.Clear(); DownedBossSystem.DownedSpiritVeinWyrm = true;
            Main.hardMode = NPC.downedPlantBoss = NPC.downedGolemBoss = NPC.downedMoonlord = false;
            string guidance = SectLedger.GetNextGuidance(actor);
            check(guidance.Contains("[i:XianXia/SummonGardenBrokenKey]") && !guidance.Contains(Language.GetTextValue("Mods.XianXia.Progression.BreakthroughRequiresHardmode")),
                "native advanced player in a fresh world is guided to available early boss before Hardmode");
            foreach (string boss in new[] { "garden_warden", "black_furnace_iron_golem", "tribulation_cloud_avatar" }) DownedBossSystem.DownedBosses.Add(boss);
            check(SectLedger.GetNextGuidance(actor).Contains(Language.GetTextValue("Mods.XianXia.Progression.BreakthroughRequiresHardmode")), "native ledger asks for Hardmode at the thunder stage");
            Main.hardMode = true;
            check(SectLedger.GetNextGuidance(actor).Contains("[i:XianXia/SummonThunderCallingJadeThunderMarshJiao]"), "native ledger returns to thunder boss after Hardmode");
            DownedBossSystem.DownedBosses.UnionWith(new[] { "thunder_marsh_jiao", "abyssal_star_womb" });
            check(SectLedger.GetNextGuidance(actor).Contains(Language.GetTextValue("Mods.XianXia.Progression.BreakthroughRequiresPlantera")), "native ledger asks for Plantera only after thunder and star bosses");
            NPC.downedPlantBoss = true;
            check(SectLedger.GetNextGuidance(actor).Contains("[i:XianXia/SummonSectTrialToken]"), "native ledger returns to sword boss after Plantera");
            DownedBossSystem.DownedBosses.UnionWith(new[] { "formless_sword_soul", "greenwood_medicine_king_echo" });
            check(SectLedger.GetNextGuidance(actor).Contains(Language.GetTextValue("Mods.XianXia.Progression.BreakthroughRequiresGolem")), "native ledger asks for Golem after sword and medicine bosses");
            NPC.downedGolemBoss = true;
            check(SectLedger.GetNextGuidance(actor).Contains("[i:XianXia/SummonHeavenTabletRubbing]"), "native ledger returns to guardian after Golem");
            DownedBossSystem.DownedBosses.UnionWith(new[] { "heaven_tablet_guardian", "broken_heaven_inspector" });
            check(SectLedger.GetNextGuidance(actor).Contains(Language.GetTextValue("Mods.XianXia.Progression.BreakthroughRequiresMoonLord")), "native ledger asks for Moon Lord after guardian and inspector");
            NPC.downedMoonlord = true;
            check(SectLedger.GetNextGuidance(actor).Contains("[i:XianXia/SummonMoonboneRitualTalisman]"), "native ledger returns to moonbone boss after Moon Lord");
        }
        finally {
            Main.hardMode = oldHard; NPC.downedPlantBoss = oldPlant; NPC.downedGolemBoss = oldGolem; NPC.downedMoonlord = oldMoon;
            DownedBossSystem.DownedSpiritVeinWyrm = oldWyrm; DownedBossSystem.DownedBosses.Clear(); DownedBossSystem.DownedBosses.UnionWith(oldBosses);
        }
    }
}
