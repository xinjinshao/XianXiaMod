using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditRareEquipment(Action<bool,string> check)
    {
        Player previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        try {
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
            var actor = new Player { whoAmI = 0, active = true, statLife = 100 }; Main.player[0] = actor;
            Item jiao = new Item(ModContent.ItemType<ThunderMarshJiaoWing>()), moon = new Item(ModContent.ItemType<MoonboneImmortalWingAccessory>());
            check(jiao.wingSlot >= ArmorIDs.Wing.Count && moon.wingSlot >= ArmorIDs.Wing.Count && jiao.wingSlot != moon.wingSlot,"native rare wings own distinct modded equip slots without overwriting vanilla slots");
            foreach (var spec in new[] { (jiao,120,6f,1.5f), (moon,180,9f,2.5f) }) {
                actor.ResetEffects(); actor.armor[3] = spec.Item1; actor.UpdateEquips(0);
                var stats = actor.GetWingStats(spec.Item1.wingSlot);
                check(actor.wingsLogic == spec.Item1.wingSlot,"native registered rare wings enter functional equipment slots: " + spec.Item1.ModItem.Name);
                check(stats.FlyTime == spec.Item2 && stats.AccRunSpeedOverride == spec.Item3 && stats.AccRunAccelerationMult == spec.Item4,"native registered rare wings expose intended flight time speed and acceleration: " + spec.Item1.ModItem.Name);
                float falling = 0, rising = 0, ascend = 0, maximum = 0, constant = 0;
                spec.Item1.ModItem.VerticalWingSpeeds(actor,ref falling,ref rising,ref ascend,ref maximum,ref constant);
                check(falling == 0.85f && rising == 0.15f && ascend == 1f && maximum == 3f && constant == 0.135f,"native registered rare wings provide vertical flight parameters: " + spec.Item1.ModItem.Name);
                actor.armor[3] = new Item(); actor.ResetEffects(); actor.UpdateEquips(0);
                check(actor.wingsLogic == 0,"native removing rare wings clears functional flight slot: " + spec.Item1.ModItem.Name);
            }
            actor.ResetEffects(); actor.armor[3] = new Item(ModContent.ItemType<TribulationCloudBottle>()); actor.UpdateEquips(0);
            check(actor.GetJumpState(ExtraJump.CloudInABottle).Enabled && actor.buffImmune[BuffID.Electrified],"native actual cloud bottle equipment enables cloud jump and electrified immunity");
            actor.armor[3] = new Item(); actor.ResetEffects(); actor.buffImmune[BuffID.Electrified] = false; typeof(ExtraJumpLoader).GetMethod("ResetEnableFlags",System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.Invoke(null,new object[] { actor }); actor.UpdateEquips(0);
            check(!actor.GetJumpState(ExtraJump.CloudInABottle).Enabled && !actor.buffImmune[BuffID.Electrified],"native unequipped cloud bottle does not regrant jump or immunity after prepared frame resets");
            foreach (int type in new[] { ModContent.ItemType<GardenWardenMask>(),ModContent.ItemType<InspectorMask>() }) {
                var item = new Item(type); actor.ApplyEquipVanity(item);
                check(item.vanity && item.headSlot >= ArmorIDs.Head.Count && EquipLoader.GetEquipSlot(item.ModItem.Mod,item.ModItem.Name,EquipType.Head) == item.headSlot,"native rare masks retain registered independent head slots on new items: " + item.ModItem.Name);
            }
            var robe = new Item(ModContent.ItemType<FormlessSwordSoulCostume>()); actor.ApplyEquipVanity(robe);
            check(robe.vanity && robe.bodySlot >= ArmorIDs.Body.Count && EquipLoader.GetEquipSlot(robe.ModItem.Mod,robe.ModItem.Name,EquipType.Body) == robe.bodySlot,"native new sword costume retains registered independent body slot");
            foreach (var original in new[] { jiao,moon,robe,new Item(ModContent.ItemType<GardenWardenMask>()),new Item(ModContent.ItemType<InspectorMask>()) }) {
                foreach (var copy in new[] { original.Clone(),Terraria.ModLoader.IO.ItemIO.Load(Terraria.ModLoader.IO.ItemIO.Save(original)) }) {
                    check(!ReferenceEquals(copy.ModItem,original.ModItem) && copy.wingSlot == original.wingSlot && copy.headSlot == original.headSlot && copy.bodySlot == original.bodySlot,"native rare equipment cloning and saved item reload preserve independent equip slots: " + original.ModItem.Name);
                    if (copy.ModItem is FormlessSwordSoulCostume) {
                        int legs = -1; bool matched = false; copy.ModItem.SetMatch(true,ref legs,ref matched);
                        check(matched && legs >= ArmorIDs.Legs.Count,"native copied and reloaded costume preserves loaded robe leg slot");
                    }
                }
            }
            foreach (bool male in new[] { false,true }) {
                int slot = -1; bool robes = false; robe.ModItem.SetMatch(male,ref slot,ref robes);
                check(robes && slot >= ArmorIDs.Legs.Count,"native registered sword costume supplies robe leg match for both genders");
            }
        } finally { Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; }
        check(ReferenceEquals(Main.player[0],previous) && Main.netMode == mode && Main.myPlayer == local,"native rare equipment audit restores player and engine mode");
    }
}
