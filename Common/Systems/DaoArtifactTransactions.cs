using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public static class DaoArtifactTransactions
{
    public static void Request(Player player,int toolSlot,int toolType,int targetSlot,int targetType,int prefix,byte kind,byte level,bool awakened,byte daoRoute,byte worldRoute)
    {
        if (player.whoAmI != Main.myPlayer || toolSlot < 0 || toolSlot >= 58 || targetSlot < 0 || targetSlot >= 58) return;
        if (Main.netMode == NetmodeID.SinglePlayer) { Handle(player,toolSlot,toolType,targetSlot,targetType,prefix,kind,level,awakened,daoRoute,worldRoute); return; }
        if (Main.netMode != NetmodeID.MultiplayerClient) return;
        var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket();
        packet.Write((byte)12); packet.Write((byte)toolSlot); packet.Write(toolType); packet.Write((byte)targetSlot); packet.Write(targetType);
        packet.Write(prefix); packet.Write(kind); packet.Write(level); packet.Write(awakened); packet.Write(daoRoute); packet.Write(worldRoute); packet.Send();
    }
    public static void Handle(Player player,int toolSlot,int toolType,int targetSlot,int targetType,int prefix,byte kind,byte level,bool awakened,byte daoRoute,byte worldRoute)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !player.active || player.dead || player.noItems || player.CCed
            || (Main.netMode == NetmodeID.Server && !player.GetModPlayer<XianXiaPlayer>().NetworkInitialized)
            || toolSlot < 0 || toolSlot >= Math.Min(58,player.inventory.Length) || toolSlot != player.selectedItem
            || targetSlot < 0 || targetSlot >= Math.Min(58,player.inventory.Length) || toolSlot == targetSlot) return;
        if (!player.GetModPlayer<InscriptionPlayer>().BeginTransaction()) return;
        Item tool = player.inventory[toolSlot], target = player.inventory[targetSlot];
        string result = "ChangedInventory";
        if (tool.type == toolType && tool.stack > 0 && tool.ModItem is InscriptionToolItem material
            && material.Mod is global::XianXia.XianXia && material.TransformsArtifact && RefinedArtifact.IsSample(target)
            && target.stack == 1 && target.type == targetType && target.prefix == prefix
            && (byte)InscribedEquipment.GetKind(target) == kind && RefinedArtifact.GetLevel(target) == level
            && RefinedArtifact.IsAwakened(target) == awakened && (byte)RefinedArtifact.GetDaoRoute(target) == daoRoute
            && (byte)DownedBossSystem.ChosenRoute == worldRoute)
        {
            if (!EndgameRouteTransactions.NearAltar(player)) result = "NeedAltar";
            else if (!DaoArtifactRules.CanTransform(level,awakened,kind != 0,daoRoute,player.GetModPlayer<XianXiaPlayer>().cultivationStage,
                worldRoute,DownedBossSystem.DownedBosses.Contains("old_heaven_dao_core"))) result = "Requirements";
            else {
                int stoneType = ModContent.ItemType<LowGradeSpiritStone>(), count = 0;
                for (int slot = 0; slot < Math.Min(58,player.inventory.Length); slot++)
                    if (player.inventory[slot].type == stoneType && player.inventory[slot].stack > 0) count += Math.Min(DaoArtifactRules.StoneCost,player.inventory[slot].stack);
                if (count < DaoArtifactRules.StoneCost) result = "NeedStones";
                else if (target.GetGlobalItem<RefinedArtifact>().TryTransform(DownedBossSystem.ChosenRoute)) {
                    tool.stack--; if (tool.stack == 0) tool.TurnToAir();
                    int remaining = DaoArtifactRules.StoneCost;
                    for (int slot = 0; slot < Math.Min(58,player.inventory.Length) && remaining > 0; slot++) {
                        Item item = player.inventory[slot]; if (item.type != stoneType || item.stack <= 0) continue;
                        int taken = Math.Min(remaining,item.stack); item.stack -= taken; remaining -= taken;
                        if (item.stack == 0) item.TurnToAir(); Sync(player,slot);
                    }
                    Sync(player,toolSlot); Sync(player,targetSlot); result = "Success";
                }
            }
        }
        Reply(player,"Mods.XianXia.DaoArtifacts." + result);
    }
    private static void Sync(Player player,int slot) { if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncEquipment,-1,-1,null,player.whoAmI,slot); }
    private static void Reply(Player player,string key)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(Language.GetTextValue(key),220,210,120);
        else { var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket(); packet.Write((byte)7); NetworkText.FromKey(key).Serialize(packet); packet.Send(player.whoAmI); }
    }
}
