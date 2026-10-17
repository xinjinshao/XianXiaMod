using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Items.Armor;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditSpiritweaveFrames(Action<bool, string> check)
    {
        Player previous = Main.player[0]; int oldMode = Main.netMode, oldLocal = Main.myPlayer;
        int[] types = { ModContent.ItemType<SpiritweaveHood>(), ModContent.ItemType<SpiritweaveRobe>(), ModContent.ItemType<SpiritweaveLeggings>() };
        try {
            foreach (int mode in new[] { NetmodeID.SinglePlayer, NetmodeID.Server, NetmodeID.MultiplayerClient }) {
                Main.netMode = mode; Main.myPlayer = 0;
                var actor = new Player { whoAmI = 0, active = true }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>();
                for (int phase = 0; phase < 4; phase++) {
                    for (int i = 0; i < 3; i++) actor.armor[i].SetDefaults(types[i]);
                    if (phase == 2) actor.armor[1].SetDefaults(ItemID.WoodBreastplate);
                    if (phase == 3) for (int i = 0; i < 3; i++) { actor.armor[i].TurnToAir(); actor.armor[10 + i].SetDefaults(types[i]); }
                    actor.ResetEffects(); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                    bool full = phase < 2;
                    check(state.spiritualEnergyRegenBonus == (full ? 1 : 0) && state.GetSpiritualEnergyCost(100) == (full ? 92 : 100),
                        "native reset/equipment/set sequence refreshes Spiritweave sustain without stacking and removes it on mixed/vanity-only equipment");
                    check(Math.Abs(actor.GetDamage(DamageClass.Generic).ApplyTo(100) - (full ? 90 : 100)) < 0.01f,
                        "native equipment frame removes the Spiritweave damage penalty when the functional set is broken");
                    if (full) check(actor.statDefense == 4, "native equipment frame adds exactly four defense from the complete Spiritweave set");
                }
            }
        } finally { Main.player[0] = previous; Main.netMode = oldMode; Main.myPlayer = oldLocal; }
        check(ReferenceEquals(Main.player[0], previous) && Main.netMode == oldMode && Main.myPlayer == oldLocal,
            "native Spiritweave equipment frame audit restores occupant and authority contexts");
    }

    private static void AuditSpiritweaveArmor(Action<bool, string> check)
    {
        int[] types = { ModContent.ItemType<SpiritweaveHood>(), ModContent.ItemType<SpiritweaveRobe>(), ModContent.ItemType<SpiritweaveLeggings>() };
        int[] vanilla = { ItemID.WoodHelmet, ItemID.WoodBreastplate, ItemID.WoodGreaves };
        int[] defense = { 1, 2, 1 }, wood = { 16, 24, 20 }, gel = { 6, 10, 8 }, stone = { 2, 4, 3 };
        for (int i = 0; i < types.Length; i++) {
            var item = new Item(); item.SetDefaults(types[i]);
            int slot = i == 0 ? item.headSlot : i == 1 ? item.bodySlot : item.legSlot;
            check(slot >= 0 && item.defense == defense[i] && !item.vanity && item.maxStack == 1,
                "native Spiritweave piece has functional armor defaults and a registered equip slot");
            Item restored = ItemIO.Load(ItemIO.Save(item)); Item clone = item.Clone();
            check(restored.type == item.type && restored.defense == item.defense && clone.type == item.type && clone.defense == item.defense,
                "native Spiritweave item serialization and cloning preserve armor identity");
            var recipes = Main.recipe.Take(Recipe.numRecipes).Where(recipe => !recipe.Disabled && recipe.createItem.type == types[i]).ToArray();
            check(recipes.Length == 1 && recipes[0].requiredTile.Contains(TileID.WorkBenches) && recipes[0].acceptedGroups.Contains(RecipeGroupID.Wood)
                && recipes[0].requiredItem.Any(ingredient => ingredient.type == ItemID.Wood && ingredient.stack == wood[i])
                && recipes[0].requiredItem.Any(ingredient => ingredient.type == ModContent.ItemType<SpiritGel>() && ingredient.stack == gel[i])
                && recipes[0].requiredItem.Any(ingredient => ingredient.type == ModContent.ItemType<LowGradeSpiritStone>() && ingredient.stack == stone[i]),
                "native Spiritweave finalized recipe has its advertised materials and vanilla wood alternatives");
        }
        int oldMode = Main.netMode;
        try {
            foreach (int mode in new[] { NetmodeID.SinglePlayer, NetmodeID.Server, NetmodeID.MultiplayerClient }) for (int mask = 0; mask < 8; mask++) {
                Main.netMode = mode;
                var actor = new Player { active = true };
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergyRegenBonus = 0; state.spiritualEnergyCostMultiplier = 1f;
                actor.GetDamage(DamageClass.Generic) = StatModifier.Default;
                for (int i = 0; i < 3; i++) actor.armor[i].SetDefaults((mask & (1 << i)) != 0 ? types[i] : vanilla[i]);
                ItemLoader.UpdateArmorSet(actor, actor.armor[0], actor.armor[1], actor.armor[2]);
                bool full = mask == 7;
                check(state.spiritualEnergyRegenBonus == (full ? 1 : 0) && state.GetSpiritualEnergyCost(100) == (full ? 92 : 100),
                    "native armor dispatcher applies energy sustain only to the complete three-piece set in each authority mode");
                check(Math.Abs(actor.GetDamage(DamageClass.Generic).ApplyTo(100) - (full ? 90 : 100)) < 0.01f,
                    "native Spiritweave full-set damage tradeoff applies once and mixed vanilla armor has no bonus or penalty");
                if (full) check(!string.IsNullOrWhiteSpace(actor.setBonus) && !actor.setBonus.Contains("{0}") && !actor.setBonus.Contains("Mods.XianXia."),
                    "native complete armor set displays formatted localized sustain and damage tradeoff");
            }
        } finally { Main.netMode = oldMode; }
        check(Main.netMode == oldMode, "native Spiritweave audit restores authority mode");
    }
}
