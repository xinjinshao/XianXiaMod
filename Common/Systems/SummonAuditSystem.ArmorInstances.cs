using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditArmorInstances(Action<bool,string> check)
    {
        var sets = new[] { new[] { "SpiritweaveHood","SpiritweaveRobe","SpiritweaveLeggings" },new[] { "FurnaceguardHelmet","FurnaceguardBreastplate","FurnaceguardGreaves" },new[] { "StormstrideHood","StormstrideRobe","StormstrideLeggings" } };
        int[][] defenses = { new[] { 1,2,1 },new[] { 3,5,4 },new[] { 6,9,7 } };
        Player previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        var mod = ModContent.GetInstance<global::XianXia.XianXia>();
        bool Close(float a,float b) => Math.Abs(a - b) < 0.0001f;
        try {
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
            for (int family = 0; family < sets.Length; family++) {
                var actor = new Player { whoAmI = 0, active = true, statLife = 100 }; Main.player[0] = actor;
                for (int part = 0; part < 3; part++) {
                    Item item = new Item(mod.Find<ModItem>(sets[family][part]).Type); actor.armor[part] = item;
                    EquipType kind = part == 0 ? EquipType.Head : part == 1 ? EquipType.Body : EquipType.Legs;
                    int Registered(Item i) => kind == EquipType.Head ? i.headSlot : kind == EquipType.Body ? i.bodySlot : i.legSlot;
                    int minimum = part == 0 ? ArmorIDs.Head.Count : part == 1 ? ArmorIDs.Body.Count : ArmorIDs.Legs.Count;
                    int slot = EquipLoader.GetEquipSlot(mod,item.ModItem.Name,kind);
                    check(slot >= minimum && Registered(item) == slot,"native new cultivation armor retains registered independent equip slot: " + item.ModItem.Name);
                    check(item.defense == defenses[family][part] && item.maxStack == 1,"native new cultivation armor retains independent defense specification: " + item.ModItem.Name);
                    foreach (Item copy in new[] { item.Clone(),ItemIO.Load(ItemIO.Save(item)) })
                        check(!ReferenceEquals(copy.ModItem,item.ModItem) && Registered(copy) == slot && copy.defense == item.defense,"native armor clone and saved item reload preserve equip slot and defense: " + item.ModItem.Name);
                }
                actor.ResetEffects(); PlayerLoader.ResetEffects(actor); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                var state = actor.GetModPlayer<XianXiaPlayer>();
                bool bonus = family switch {
                    0 => actor.statDefense == 4 && state.spiritualEnergyRegenBonus == 1 && Close(state.spiritualEnergyCostMultiplier,0.92f) && Close(actor.GetDamage(DamageClass.Generic).Additive,0.9f),
                    1 => actor.statDefense == 12 && Close(actor.endurance,0.05f) && Close(actor.moveSpeed,0.9f) && Close(state.spiritualEnergyCostMultiplier,1.1f),
                    _ => actor.statDefense == 16 && Close(actor.moveSpeed,1.15f) && Close(state.spiritualEnergyCostMultiplier,0.88f)
                };
                check(bonus && !string.IsNullOrEmpty(actor.setBonus),"native engine equipment and armor-set dispatch apply actual complete set bonuses and tradeoffs: " + sets[family][0]);
                actor.armor[2] = new Item(); actor.ResetEffects(); PlayerLoader.ResetEffects(actor); actor.UpdateEquips(0); actor.UpdateArmorSets(0);
                check(state.spiritualEnergyRegenBonus == 0 && Close(state.spiritualEnergyCostMultiplier,1f) && Close(actor.moveSpeed,1f) && Close(actor.endurance,0f) && string.IsNullOrEmpty(actor.setBonus),"native removing one armor piece clears all set bonuses on next prepared equipment frame: " + sets[family][0]);
            }
        } finally { Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; }
        check(ReferenceEquals(Main.player[0],previous) && Main.netMode == mode && Main.myPlayer == local,"native armor instance audit restores player and engine context");
    }
}
