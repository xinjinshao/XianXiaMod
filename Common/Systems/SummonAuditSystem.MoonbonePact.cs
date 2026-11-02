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
    private static void AuditMoonbonePactArmor(Action<bool, string> check)
    {
        int[] types = { ModContent.ItemType<MoonbonePactCrown>(), ModContent.ItemType<MoonbonePactRobe>(), ModContent.ItemType<MoonbonePactLeggings>() };
        int[] defenses = { 8, 16, 12 }, costs = { 6, 10, 8 }, lights = { 3, 5, 4 };
        for (int i = 0; i < 3; i++) {
            var item = new Item(); item.SetDefaults(types[i]);
            check(item.defense == defenses[i] && !item.vanity && (i == 0 ? item.headSlot : i == 1 ? item.bodySlot : item.legSlot) >= 0, "native Moonbone Pact functional armor and equip slot");
            check(ItemIO.Load(ItemIO.Save(item)).type == item.type && item.Clone().defense == defenses[i], "native Moonbone Pact serialization and clone");
            var recipe = Main.recipe.Take(Recipe.numRecipes).Single(r => !r.Disabled && r.createItem.type == types[i]);
            check(recipe.createItem.stack == 1 && recipe.Conditions.Contains(Condition.DownedMoonLord)
                && recipe.requiredTile.Contains(ModContent.TileType<DaoSeveringAltarTile>())
                && recipe.requiredItem.Count(x => !x.IsAir) == 3
                && recipe.requiredItem.Any(x => x.type == ItemID.Silk && x.stack == costs[i] * 2)
                && recipe.requiredItem.Any(x => x.type == ModContent.ItemType<Moonbone>() && x.stack == costs[i])
                && recipe.requiredItem.Any(x => x.type == ModContent.ItemType<ArchiveRemnantLight>() && x.stack == lights[i]), "native Moonbone Pact exact recipe materials station and Moon Lord gate");
            var lines = new System.Collections.Generic.List<TooltipLine>(); item.ModItem.ModifyTooltips(lines);
            check(lines.Single().Text.Contains(MoonbonePactCrown.BonusText()) && !lines[0].Text.Contains("{0}") && !lines[0].Text.Contains("Mods.XianXia."), "native Moonbone Pact unworn preview localized");
        }
        Player previous = Main.player[0]; int oldMode = Main.netMode, oldLocal = Main.myPlayer;
        try {
            foreach (int mode in new[] { 0, 1, 2 }) for (int mask = 0; mask < 8; mask++) {
                Main.netMode = mode; Main.myPlayer = 0;
                var actor = new Player { active = true, whoAmI = 0 }; Main.player[0] = actor;
                for (int i = 0; i < 3; i++) if ((mask & (1 << i)) != 0) actor.armor[i].SetDefaults(types[i]);
                for (int frame = 0; frame < 2; frame++) {
                    actor.ResetEffects(); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                    int expectedDefense = Enumerable.Range(0, 3).Where(i => (mask & (1 << i)) != 0).Sum(i => defenses[i]);
                    check(actor.statDefense == expectedDefense && Math.Abs(actor.moveSpeed - 1f) < .0001f
                        && actor.GetModPlayer<XianXiaPlayer>().GetSpiritualEnergyCost(100) == (mask == 7 ? 125 : 100)
                        && actor.maxMinions == (mask == 7 ? 5 : 1) && actor.maxTurrets == 1
                        && Math.Abs(actor.GetDamage(DamageClass.Summon).ApplyTo(100) - (mask == 7 ? 135 : 100)) < .01f
                        && Math.Abs(actor.GetDamage(DamageClass.Generic).ApplyTo(100) - 100) < .01f,
                        "native Moonbone Pact authority and mixed-piece repeated-frame summon capacity damage and energy tradeoff");
                    if (mask == 7) check(actor.setBonus == MoonbonePactCrown.BonusText(), "native Moonbone Pact preview matches active set");
                }
                for (int i = 0; i < 3; i++) { actor.armor[i + 10] = actor.armor[i]; actor.armor[i] = new Item(); }
                actor.ResetEffects(); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                check(actor.statDefense == 0 && actor.moveSpeed == 1f && actor.maxMinions == 1 && actor.maxTurrets == 1
                    && Math.Abs(actor.GetDamage(DamageClass.Summon).ApplyTo(100) - 100) < .01f && actor.GetModPlayer<XianXiaPlayer>().GetSpiritualEnergyCost(100) == 100, "native Moonbone Pact vanity or removal clears all set effects");
            }
        } finally { Main.player[0] = previous; Main.netMode = oldMode; Main.myPlayer = oldLocal; }
    }
}
