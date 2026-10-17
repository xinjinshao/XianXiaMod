using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Items.Armor;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditStormstrideArmor(Action<bool, string> check)
    {
        int[] types = { ModContent.ItemType<StormstrideHood>(), ModContent.ItemType<StormstrideRobe>(), ModContent.ItemType<StormstrideLeggings>() };
        int[] defenses = { 6, 9, 7 }, costs = { 8, 12, 10 };
        for (int i = 0; i < 3; i++) {
            var item = new Item(); item.SetDefaults(types[i]);
            check(item.defense == defenses[i] && !item.vanity && (i == 0 ? item.headSlot : i == 1 ? item.bodySlot : item.legSlot) >= 0, "native Stormstride functional armor and equip slot");
            check(ItemIO.Load(ItemIO.Save(item)).type == item.type && item.Clone().defense == defenses[i], "native Stormstride serialization and clone");
            var recipe = Main.recipe.Take(Recipe.numRecipes).Single(r => !r.Disabled && r.createItem.type == types[i]);
            check(recipe.createItem.stack == 1 && recipe.Conditions.Contains(Condition.Hardmode)
                && recipe.requiredTile.Contains(ModContent.TileType<ThunderPatternForgeTile>())
                && recipe.requiredItem.Count(x => !x.IsAir) == 3
                && recipe.requiredItem.Any(x => x.type == ItemID.Silk && x.stack == costs[i])
                && recipe.requiredItem.Any(x => x.type == ModContent.ItemType<ThunderPatternFeather>() && x.stack == costs[i])
                && recipe.requiredItem.Any(x => x.type == ModContent.ItemType<TribulationCloudDew>() && x.stack == 2), "native Stormstride exact recipe materials station and hardmode gate");
            var lines = new System.Collections.Generic.List<TooltipLine>(); item.ModItem.ModifyTooltips(lines);
            check(lines.Single().Text.Contains(StormstrideHood.BonusText()) && !lines[0].Text.Contains("{0}") && !lines[0].Text.Contains("Mods.XianXia."), "native Stormstride unworn preview localized");
        }
        Player previous = Main.player[0]; int oldMode = Main.netMode, oldLocal = Main.myPlayer;
        try {
            foreach (int mode in new[] { 0, 1, 2 }) for (int mask = 0; mask < 8; mask++) {
                Main.netMode = mode; Main.myPlayer = 0;
                var actor = new Player { active = true, whoAmI = 0 }; Main.player[0] = actor;
                for (int i = 0; i < 3; i++) if ((mask & (1 << i)) != 0) actor.armor[i].SetDefaults(types[i]);
                for (int frame = 0; frame < 2; frame++) {
                    actor.ResetEffects(); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                    int expectedDefense = Enumerable.Range(0, 3).Where(i => (mask & (1 << i)) != 0).Sum(i => defenses[i]) - (mask == 7 ? 6 : 0);
                    check(actor.statDefense == expectedDefense && Math.Abs(actor.moveSpeed - (mask == 7 ? 1.15f : 1f)) < .0001f
                        && actor.GetModPlayer<XianXiaPlayer>().GetSpiritualEnergyCost(100) == (mask == 7 ? 88 : 100)
                        && Math.Abs(actor.GetDamage(DamageClass.Generic).ApplyTo(100) - 100) < .01f,
                        "native Stormstride authority and mixed-piece repeated-frame mobility cost defense tradeoff");
                    if (mask == 7) check(actor.setBonus == StormstrideHood.BonusText(), "native Stormstride preview matches active set");
                }
                for (int i = 0; i < 3; i++) { actor.armor[i + 10] = actor.armor[i]; actor.armor[i] = new Item(); }
                actor.ResetEffects(); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                check(actor.statDefense == 0 && actor.moveSpeed == 1f && actor.GetModPlayer<XianXiaPlayer>().GetSpiritualEnergyCost(100) == 100, "native Stormstride vanity or removal clears all set effects");
            }
        } finally { Main.player[0] = previous; Main.netMode = oldMode; Main.myPlayer = oldLocal; }
    }
}
