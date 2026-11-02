using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Items.Guides;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditClassPaths(Action<bool,string> check)
    {
        bool hard = Main.hardMode, plant = NPC.downedPlantBoss, golem = NPC.downedGolemBoss, moon = NPC.downedMoonlord;
        bool wyrm = DownedBossSystem.DownedSpiritVeinWyrm;
        int mode = Main.netMode, local = Main.myPlayer; var previous = Main.player[0];
        try {
            var manual = new Item(ModContent.ItemType<ClassPathManual>());
            check(!manual.consumable && manual.maxStack == 1 && manual.ModItem.AltFunctionUse(new Player()) && manual.useTime == 20,
                "native Four Paths manual is reusable with alternate class selection");
            check(ItemIO.Load(ItemIO.Save(manual)).type == manual.type && manual.Clone().type == manual.type,"native Four Paths manual saves and clones");
            var recipe = Main.recipe.Take(Recipe.numRecipes).Single(r => !r.Disabled && r.createItem.type == manual.type);
            check(recipe.requiredTile.Contains(TileID.WorkBenches) && recipe.Conditions.Count == 0 && recipe.acceptedGroups.Contains(RecipeGroupID.Wood)
                && recipe.requiredItem.Any(i => i.type == ItemID.FallenStar && i.stack == 1),"native Four Paths manual is craftable at a workbench before bosses");
            for (int flags = 0; flags < 32; flags++) {
                Main.hardMode = (flags & 1) != 0; NPC.downedPlantBoss = (flags & 2) != 0;
                NPC.downedGolemBoss = (flags & 4) != 0; NPC.downedMoonlord = (flags & 8) != 0;
                DownedBossSystem.DownedSpiritVeinWyrm = (flags & 16) != 0;
                int expected = !Main.hardMode ? DownedBossSystem.DownedSpiritVeinWyrm ? 1 : 0
                    : !NPC.downedPlantBoss ? 2 : !NPC.downedGolemBoss ? 3 : !NPC.downedMoonlord ? 4 : 5;
                check(ClassPathManual.CurrentStage() == expected,"native Four Paths follows this world's prerequisite chain even with inconsistent later flags");
            }
            foreach (int authority in new[] { 0,1,2 }) foreach (int realm in new[] { 0,5,8 })
            for (int stage = 0; stage < 6; stage++) for (int path = 0; path < 4; path++) {
                Main.netMode = authority; Main.myPlayer = 0;
                var actor = new Player { whoAmI = 0, active = true, statLife = 100 }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = (CultivationStage)realm;
                state.spiritualEnergy = 10; state.spiritPressure = 15; state.spiritualEnergyCostMultiplier = .75f;
                var snapshot = state.CaptureSnapshot(); int health = actor.statLife, active = Main.projectile.Count(p => p.active);
                int type = ClassPathManual.Candidate(stage,path); var item = new Item(type);
                var expectedClass = path == 0 ? DamageClass.Melee : path == 1 ? DamageClass.Ranged : path == 2 ? DamageClass.Magic : DamageClass.Summon;
                check(type > 0 && item.DamageType == expectedClass,"native Four Paths all twenty-four stage candidates retain actual class ownership");
                string[] lines = ClassPathManual.Guidance(actor,stage,path).ToArray(); string joined = string.Join("\n",lines);
                check(lines.Length >= 4 && joined.Contains(Lang.GetItemNameValue(type)) && !joined.Contains("Mods.XianXia.") && !joined.Contains("{0}")
                    && !joined.Contains(Language.GetTextValue("Mods.XianXia.ClassPaths.SpecialStation")),
                    "native Four Paths renders registered item names and resolved guide text");
                foreach (var entry in Main.recipe.Take(Recipe.numRecipes).Where(r => !r.Disabled && r.createItem.type == type)) {
                    check(entry.requiredItem.Where(i => !i.IsAir).All(i => joined.Contains("×" + i.stack))
                        && entry.Conditions.All(c => joined.Contains(c.Description.Value)),"native Four Paths reports registered ingredient quantities and every crafting condition");
                }
                check(joined.Contains(Language.GetTextValue("Mods.XianXia.ClassPaths.Caution")) && state.CaptureSnapshot() == snapshot
                    && actor.statLife == health && Main.projectile.Count(p => p.active) == active && actor.inventory.All(i => i.IsAir),
                    "native Four Paths guidance is read-only and does not claim candidate readiness");
            }
            check(ClassPathManual.Candidate(-1,0) == 0 && ClassPathManual.Candidate(6,0) == 0 && ClassPathManual.Candidate(0,4) == 0,
                "native Four Paths invalid selection has no candidate");
        } finally {
            Main.hardMode = hard; NPC.downedPlantBoss = plant; NPC.downedGolemBoss = golem; NPC.downedMoonlord = moon;
            DownedBossSystem.DownedSpiritVeinWyrm = wyrm; Main.netMode = mode; Main.myPlayer = local; Main.player[0] = previous;
        }
    }
}
