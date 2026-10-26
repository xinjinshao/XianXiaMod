using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Content.Items.Weapons;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditSkillTooltips(Action<bool, string> check)
    {
        Player previous = Main.player[0]; int local = Main.myPlayer;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new Terraria.ModLoader.IO.TagCompound(); world.SaveWorldData(saved);
        try {
            Main.myPlayer = 0;
            var actor = new Player { whoAmI = 0, active = true }; Main.player[0] = actor;
            var state = actor.GetModPlayer<XianXiaPlayer>();
            foreach (var sample in ModLoader.GetMod("XianXia").GetContent<ModItem>().OfType<CultivationWeaponItem>()) {
                var dormant = new Item(); dormant.SetDefaults(sample.Type);
                var dormantLines = new List<TooltipLine>(); dormant.GetGlobalItem<CultivationDamageClassHelp>().ModifyTooltips(dormant, dormantLines);
                check(dormantLines.All(line => line.Name != "CurrentArtifactSkill"), "native dormant and non-awakenable weapons do not advertise a usable active skill");
                if (!RefinedArtifact.SupportsAwakening(dormant)) continue;
                foreach (int route in new[] { 0, 1, 2, 3 }) {
                    if (route != 0 && !RefinedArtifact.SupportsDaoTransformation(dormant)) continue;
                    var routeTag = new Terraria.ModLoader.IO.TagCompound(); world.SaveWorldData(routeTag); routeTag["chosenRoute"] = route; world.LoadWorldData(routeTag);
                    DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                    var item = new Item(); item.SetDefaults(sample.Type); actor.inventory[0] = item; actor.selectedItem = 0;
                    var refined = item.GetGlobalItem<RefinedArtifact>(); refined.SetLevel(3); check(refined.TryAwaken(), "native skill tooltip fixture uses actual awakening state");
                    if (route != 0) check(refined.TryTransform((DownedBossSystem.EndgameRoute)route), "native skill tooltip fixture uses actual matching-world Dao state");
                    var selectedSkill = ArtifactSkillTransactions.HeldSkill(item);
                    if (item.ModItem is CinnabarTalismanFlameItem or StarEclipseArbalest or ThunderTalismanArrayPlate or BrokenHeavenDecree or GreenwoodMedicineCauldron or SectMechanismCrossbow or HeavenLawArbalest) {
                        var flameLines = new List<TooltipLine>(); item.GetGlobalItem<CultivationDamageClassHelp>().ModifyTooltips(item, flameLines);
                        check(!selectedSkill.HasValue && !item.ModItem.AltFunctionUse(actor)
                            && flameLines.All(line => line.Name != "CurrentArtifactSkill"), "native awakened passive-growth weapon does not advertise an undefined active skill");
                        continue;
                    }
                    if (item.ModItem is ThunderPatternSwordCase)
                        check(selectedSkill == ArtifactSkill.ThunderVolley && item.ModItem.AltFunctionUse(actor) && RefinedArtifact.SupportsDaoTransformation(item),
                            "native awakened thunder case exposes its active skill with supported Dao transformation");
                    check(selectedSkill.HasValue, "native existing active artifacts retain their skill mappings");
                    var skill = selectedSkill.Value;
                    foreach (float multiplier in new[] { 0.92f, 1f, 1.1f }) foreach (int cooldown in new[] { 0, 61 }) {
                        state.spiritualEnergyCostMultiplier = multiplier; state.activeSkillCooldown = cooldown; var before = state.CaptureSnapshot();
                        var lines = new List<TooltipLine>(); item.GetGlobalItem<CultivationDamageClassHelp>().ModifyTooltips(item, lines);
                        string preview = lines.Single(line => line.Name == "CurrentArtifactSkill").Text;
                        int cost = state.GetSpiritualEnergyCost(DaoArtifactRules.SkillCost(skill, RefinedArtifact.ActiveDaoRoute(item)));
                        string name = skill switch { ArtifactSkill.SwordBurst => "Sword", ArtifactSkill.ArrayPulse => "Array", ArtifactSkill.MoonCrescent => "Moon",
                            ArtifactSkill.WheelFormation => "Wheel", ArtifactSkill.HammerVolley => "Hammer", ArtifactSkill.ThunderVolley => "Thunder", _ => "Ward" };
                        string expected = Terraria.Localization.Language.GetTextValue("Mods.XianXia.DamageClasses.CurrentSkillCost",
                            Terraria.Localization.Language.GetTextValue("Mods.XianXia.Skills." + name), cost, ArtifactSkillRules.Cooldown(skill) / 60, cooldown == 0 ? 0 : 2);
                        check(preview == expected && !preview.Contains("{0}") && !preview.Contains("Mods.XianXia."), "native active skill preview resolves matching route costs and rounded remaining shared cooldown");
                        if (item.ModItem is ThunderPatternSwordCase)
                            check(lines.Single(line => line.Name == "CurrentSpiritCost").Text == Terraria.Localization.Language.GetTextValue("Mods.XianXia.DamageClasses.CurrentCastCost",state.GetSpiritualEnergyCost(((ThunderPatternSwordCase)item.ModItem).GetSpiritCost(actor))),
                                "native thunder active tooltip keeps separate rounded ordinary route-specific cost");
                        check(state.CaptureSnapshot() == before && actor.HeldItem == item, "native skill preview does not activate a skill spend energy or change inventory");
                    }
                }
            }
        } finally { world.LoadWorldData(saved); Main.player[0] = previous; Main.myPlayer = local; }
        check(Main.player[0] == previous && Main.myPlayer == local, "native skill tooltip audit restores world and local occupant contexts");
    }

    private static void AuditWeaponCostTooltips(Action<bool, string> check)
    {
        Player previous = Main.player[0]; int local = Main.myPlayer;
        try {
            Main.myPlayer = 0;
            var actor = new Player { whoAmI = 0, active = true }; Main.player[0] = actor;
            var state = actor.GetModPlayer<XianXiaPlayer>();
            var weapons = ModLoader.GetMod("XianXia").GetContent<ModItem>().OfType<CultivationWeaponItem>().ToArray();
            check(weapons.Length == 18 && weapons.Count(weapon => weapon.DeploysArray) == 3, "native tooltip matrix covers all eighteen spiritual weapons including three arrays");
            foreach (var sample in weapons) foreach (float multiplier in new[] { 0.92f, 1f, 1.1f }) {
                var item = new Item(); item.SetDefaults(sample.Type); actor.inventory[0] = item; actor.selectedItem = 0;
                state.spiritualEnergyCostMultiplier = multiplier; var before = state.CaptureSnapshot();
                var lines = new List<TooltipLine>(); item.GetGlobalItem<CultivationDamageClassHelp>().ModifyTooltips(item, lines);
                var costLine = lines.Single(line => line.Name == "CurrentSpiritCost");
                var weapon = (CultivationWeaponItem)item.ModItem;
                int cost = state.GetSpiritualEnergyCost(weapon.GetSpiritCost(actor));
                string key = "Mods.XianXia.DamageClasses." + (weapon.DeploysArray ? "CurrentArrayCost" : "CurrentCastCost");
                check(costLine.Text == Terraria.Localization.Language.GetTextValue(key, cost) && !costLine.Text.Contains("{0}") && !costLine.Text.Contains("Mods.XianXia."),
                    "native spiritual weapon tooltip uses actual current rounded cost and distinguishes array deployment");
                check(state.CaptureSnapshot() == before && actor.HeldItem == item, "native cost tooltip does not spend energy or change held inventory");
                if (weapon is WoodgrainFlyingSword) check(cost == (multiplier == 1.1f ? 5 : 4), "native four-energy sword displays unchanged discounted rounding and increased guarded cost");
                if (weapon is SpiritwoodCrossbow) check(cost == (multiplier == 1.1f ? 3 : 2), "native two-energy crossbow displays its actual small-cost rounding");
            }
            var probe = new Item(); probe.SetDefaults(ModContent.ItemType<WoodgrainFlyingSword>());
            foreach (int context in new[] { 0, 1, 2 }) {
                Main.myPlayer = context == 0 ? Main.maxPlayers : 0;
                Main.player[0] = context == 1 ? null : actor; actor.dead = context == 2;
                var lines = new List<TooltipLine>(); probe.GetGlobalItem<CultivationDamageClassHelp>().ModifyTooltips(probe, lines);
                check(lines.All(line => line.Name != "CurrentSpiritCost"), "native invalid local-player context skips cost preview safely");
            }
        } finally { Main.player[0] = previous; Main.myPlayer = local; }
        check(Main.player[0] == previous && Main.myPlayer == local, "native weapon tooltip audit restores local occupant context");
    }
}
