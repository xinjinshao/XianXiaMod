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
