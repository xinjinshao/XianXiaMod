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
    // Shared read-only preview. Confirmation still revalidates the complete item snapshot.
    public static string RequirementFailure(Player player, Item target)
    {
        const string prefix = "Mods.XianXia.DaoArtifacts.";
        if (target == null || !RefinedArtifact.SupportsDaoTransformation(target) || target.stack != 1) return prefix + "ChangedInventory";
        if (!EndgameRouteTransactions.NearAltar(player)) return prefix + "NeedAltar";
        byte level = RefinedArtifact.GetLevel(target);
        bool awakened = RefinedArtifact.IsAwakened(target), inscribed = InscribedEquipment.GetKind(target) != InscriptionKind.None;
        var route = RefinedArtifact.GetDaoRoute(target); var stage = player.GetModPlayer<XianXiaPlayer>().cultivationStage;
        bool core = DownedBossSystem.DownedBosses.Contains("old_heaven_dao_core");
        if (!DaoArtifactRules.CanTransform(level, awakened, inscribed, (int)route, stage, (int)DownedBossSystem.ChosenRoute, core)) {
            if (route != DownedBossSystem.EndgameRoute.None) return prefix + "AlreadyTransformed";
            if (level != 3 || !awakened) return prefix + "NeedAwakening";
            if (!inscribed) return prefix + "NeedInscription";
            if (stage != CultivationStage.DaoSevering) return prefix + "NeedRealm";
            if (!core) return prefix + "NeedCore";
            return prefix + "NeedWorldRoute";
        }
        int stoneType = ModContent.ItemType<LowGradeSpiritStone>(), count = 0;
        for (int slot = 0; slot < Math.Min(58, player.inventory.Length); slot++)
            if (player.inventory[slot].type == stoneType && player.inventory[slot].stack > 0) count += Math.Min(DaoArtifactRules.StoneCost, player.inventory[slot].stack);
        return count < DaoArtifactRules.StoneCost ? prefix + "NeedStones" : "";
    }
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
        string result = "Mods.XianXia.DaoArtifacts.ChangedInventory";
        if (tool.type == toolType && tool.stack > 0 && tool.ModItem is InscriptionToolItem material
            && material.Mod is global::XianXia.XianXia && material.TransformsArtifact && RefinedArtifact.SupportsDaoTransformation(target)
            && target.stack == 1 && target.type == targetType && target.prefix == prefix
            && (byte)InscribedEquipment.GetKind(target) == kind && RefinedArtifact.GetLevel(target) == level
            && RefinedArtifact.IsAwakened(target) == awakened && (byte)RefinedArtifact.GetDaoRoute(target) == daoRoute
            && (byte)DownedBossSystem.ChosenRoute == worldRoute)
        {
            result = RequirementFailure(player, target);
            if (result.Length == 0) {
                result = "Mods.XianXia.DaoArtifacts.ChangedInventory";
                if (target.GetGlobalItem<RefinedArtifact>().TryTransform(DownedBossSystem.ChosenRoute)) {
                    int stoneType = ModContent.ItemType<LowGradeSpiritStone>();
                    tool.stack--; if (tool.stack == 0) tool.TurnToAir();
                    int remaining = DaoArtifactRules.StoneCost;
                    for (int slot = 0; slot < Math.Min(58,player.inventory.Length) && remaining > 0; slot++) {
                        Item item = player.inventory[slot]; if (item.type != stoneType || item.stack <= 0) continue;
                        int taken = Math.Min(remaining,item.stack); item.stack -= taken; remaining -= taken;
                        if (item.stack == 0) item.TurnToAir(); Sync(player,slot);
                    }
                    Sync(player,toolSlot); Sync(player,targetSlot); result = "Mods.XianXia.DaoArtifacts.Success";
                }
            }
        }
        Reply(player,result);
    }
    private static void Sync(Player player,int slot) { if (Main.netMode == NetmodeID.Server) AuthoritativeInventorySync.Send(player,slot); }
    private static void Reply(Player player,string key)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(Language.GetTextValue(key),220,210,120);
        else { var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket(); packet.Write((byte)7); NetworkText.FromKey(key).Serialize(packet); packet.Send(player.whoAmI); }
    }
}
