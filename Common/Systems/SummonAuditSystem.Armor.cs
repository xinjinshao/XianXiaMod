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
    private static void AuditFurnaceguardArmor(Action<bool, string> check)
    {
        int[] types = { ModContent.ItemType<FurnaceguardHelmet>(), ModContent.ItemType<FurnaceguardBreastplate>(), ModContent.ItemType<FurnaceguardGreaves>() };
        int[] vanilla = { ItemID.IronHelmet, ItemID.IronChainmail, ItemID.IronGreaves };
        int[] defense = { 3, 5, 4 }, bars = { 6, 10, 8 }, slag = { 8, 12, 10 };
        for (int i = 0; i < 3; i++) {
            var item = new Item(); item.SetDefaults(types[i]);
            check(item.defense == defense[i] && !item.vanity && (i == 0 ? item.headSlot : i == 1 ? item.bodySlot : item.legSlot) >= 0,
                "native Furnaceguard piece has its functional defense and independent equip slot");
            check(ItemIO.Load(ItemIO.Save(item)).type == item.type && item.Clone().defense == item.defense,
                "native Furnaceguard serialization and cloning preserve item identity");
            var recipe = Main.recipe.Take(Recipe.numRecipes).Single(entry => !entry.Disabled && entry.createItem.type == types[i]);
            check(recipe.createItem.stack == 1 && recipe.requiredTile.Contains(ModContent.TileType<Content.Tiles.Stations.ArtifactForgeTile>())
                && recipe.requiredItem.Any(entry => entry.type == ItemID.IronBar && entry.stack == bars[i])
                && recipe.requiredItem.Any(entry => entry.type == ModContent.ItemType<FurnaceSlagIron>() && entry.stack == slag[i])
                && recipe.requiredItem.Any(entry => entry.type == ModContent.ItemType<OldFurnaceEmber>() && entry.stack == 1),
                "native Furnaceguard recipe preserves all advertised costs and station");
            check(recipe.acceptedGroups.Contains(RecipeGroupID.IronBar) && recipe.AcceptedByItemGroups(ItemID.LeadBar, ItemID.IronBar),
                "native Furnaceguard crafting accepts lead bars as the vanilla iron alternative");
        }
        Player previous = Main.player[0]; int oldMode = Main.netMode, oldLocal = Main.myPlayer;
        try {
            foreach (int mode in new[] { NetmodeID.SinglePlayer, NetmodeID.Server, NetmodeID.MultiplayerClient }) for (int mask = 0; mask < 8; mask++) {
                Main.netMode = mode; Main.myPlayer = 0;
                var actor = new Player { active = true, whoAmI = 0 }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>();
                for (int i = 0; i < 3; i++) actor.armor[i].SetDefaults((mask & (1 << i)) != 0 ? types[i] : vanilla[i]);
                actor.ResetEffects(); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                bool full = mask == 7;
                check(Math.Abs(actor.endurance - (full ? 0.05f : 0f)) < 0.0001f
                    && Math.Abs(actor.moveSpeed - (full ? 0.9f : 1f)) < 0.0001f && state.GetSpiritualEnergyCost(100) == (full ? 110 : 100),
                    "native Furnaceguard full set grants protection with both mobility and energy penalties; mixed sets do not");
                check(Math.Abs(actor.GetDamage(DamageClass.Generic).ApplyTo(100) - 100) < 0.01f,
                    "native Furnaceguard does not increase generic damage");
                if (full) {
                    check(actor.statDefense == 12 && !string.IsNullOrWhiteSpace(actor.setBonus) && !actor.setBonus.Contains("{0}"),
                        "native Furnaceguard provides twelve defense and formatted localized tradeoff text");
                    actor.ResetEffects(); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                    check(Math.Abs(actor.endurance - 0.05f) < 0.0001f && Math.Abs(actor.moveSpeed - 0.9f) < 0.0001f && state.GetSpiritualEnergyCost(100) == 110,
                        "native repeated equipment frame does not stack Furnaceguard bonuses or penalties");
                }
            }
        } finally { Main.player[0] = previous; Main.netMode = oldMode; Main.myPlayer = oldLocal; }
        check(Main.player[0] == previous && Main.netMode == oldMode && Main.myPlayer == oldLocal, "native Furnaceguard audit restores occupant and authority context");
    }

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
