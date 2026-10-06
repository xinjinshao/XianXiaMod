using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;

public static class InscriptionTransactions
{
    public static bool NearForge(Player player)
    {
        var center = player.Center.ToTileCoordinates();
        int basic = ModContent.TileType<ArtifactForgeTile>();
        int thunder = ModContent.TileType<ThunderPatternForgeTile>();
        int heaven = ModContent.TileType<HeavenFireFurnaceTile>();
        int dao = ModContent.TileType<DaoSeveringAltarTile>();
        for (int x = Math.Max(0, center.X - 6); x <= Math.Min(Main.maxTilesX - 1, center.X + 6); x++)
            for (int y = Math.Max(0, center.Y - 4); y <= Math.Min(Main.maxTilesY - 1, center.Y + 4); y++) {
                Tile tile = Main.tile[x, y];
                if (tile.HasTile && (tile.TileType == basic || tile.TileType == thunder || tile.TileType == heaven || tile.TileType == dao)) return true;
            }
        return false;
    }
    public static void Request(Player player, int toolSlot, int toolType, int targetSlot, int targetType, int prefix, byte previous, byte previousLevel = 0, bool previousAwakened = false)
    {
        if (toolSlot < 0 || toolSlot >= 58 || targetSlot < 0 || targetSlot >= 58) return;
        Item tool = player.inventory[toolSlot];
        if (Main.netMode == NetmodeID.SinglePlayer) {
            HandleRequest(player, toolSlot, toolType, targetSlot, targetType, prefix, previous, previousLevel, previousAwakened);
            return;
        }
        if (Main.netMode != NetmodeID.MultiplayerClient || player.whoAmI != Main.myPlayer || tool.ModItem is not InscriptionToolItem) return;
        ModPacket packet = tool.ModItem.Mod.GetPacket();
        packet.Write((byte)9); packet.Write((byte)toolSlot); packet.Write(toolType);
        packet.Write((byte)targetSlot); packet.Write(targetType); packet.Write(prefix);
        packet.Write(previous); packet.Write(previousLevel); packet.Write(previousAwakened); packet.Send();
    }
    public static void HandleRequest(Player player, int toolSlot, int toolType, int targetSlot, int targetType, int prefix, byte previous, byte previousLevel = 0, bool previousAwakened = false)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !player.active || player.dead || player.noItems || player.CCed
            || (Main.netMode == NetmodeID.Server && !player.GetModPlayer<XianXiaPlayer>().NetworkInitialized)
            || toolSlot < 0 || toolSlot >= Math.Min(58, player.inventory.Length) || toolSlot != player.selectedItem
            || targetSlot < 0 || targetSlot >= Math.Min(58, player.inventory.Length) || targetSlot == toolSlot) return;
        if (!player.GetModPlayer<InscriptionPlayer>().BeginTransaction()) return;
        Item tool = player.inventory[toolSlot], target = player.inventory[targetSlot];
        string result = "Mods.XianXia.Inscriptions.ChangedInventory";
        if (tool.type == toolType && tool.stack > 0 && tool.ModItem is InscriptionToolItem material
            && material.Mod is global::XianXia.XianXia && !material.TransformsArtifact && target.type == targetType && target.prefix == prefix
            && InscribedEquipment.IsEligible(target) && target.stack == 1 && previous <= 5
            && (byte)InscribedEquipment.GetKind(target) == previous && RefinedArtifact.GetLevel(target) == previousLevel
            && RefinedArtifact.IsAwakened(target) == previousAwakened)
        {
            InscriptionKind next = material.TargetKind;
            int stoneType = ModContent.ItemType<LowGradeSpiritStone>();
            int required = next == InscriptionKind.None ? 0 : InscriptionRules.SpiritStoneCost;
            if (material.RefinesArtifact) required = RefinementRules.StoneCost(previousLevel);
            if (material.AwakensArtifact) required = RefinementRules.AwakeningStoneCost;
            int count = 0;
            for (int slot = 0; slot < Math.Min(58, player.inventory.Length); slot++)
                if (player.inventory[slot].type == stoneType && player.inventory[slot].stack > 0) count += Math.Min(required, player.inventory[slot].stack);
            if (!NearForge(player)) result = "Mods.XianXia.Inscriptions.NeedForge";
            else if (material.AwakensArtifact && (!RefinedArtifact.SupportsAwakening(target)
                || !RefinementRules.CanAwaken(previousLevel, previous != 0, previousAwakened,
                    player.GetModPlayer<XianXiaPlayer>().cultivationStage, DownedBossSystem.DownedBosses.Contains("greenwood_medicine_king_echo"))))
                result = "Mods.XianXia.Refinement.AwakeningRequirements";
            else if (material.RefinesArtifact && (!RefinedArtifact.SupportsRefinement(target)
                || !RefinementRules.CanAdvance(previousLevel, previous != 0, player.GetModPlayer<XianXiaPlayer>().cultivationStage,
                    DownedBossSystem.DownedBosses.Contains(RefinementRules.RequiredBoss(previousLevel)))))
                result = "Mods.XianXia.Refinement.Requirements";
            else if (!material.RefinesArtifact && !material.AwakensArtifact && !InscriptionRules.CanChange(InscribedEquipment.GetKind(target), next)) result = "Mods.XianXia.Inscriptions.NoChange";
            else if (count < required) result = "Mods.XianXia.Inscriptions.NeedStones";
            else {
                if (material.AwakensArtifact) target.GetGlobalItem<RefinedArtifact>().TryAwaken();
                else if (material.RefinesArtifact) target.GetGlobalItem<RefinedArtifact>().SetLevel(previousLevel + 1);
                else target.GetGlobalItem<InscribedEquipment>().SetKind(next);
                tool.stack--;
                if (tool.stack <= 0) tool.TurnToAir();
                for (int slot = 0; slot < Math.Min(58, player.inventory.Length) && required > 0; slot++) {
                    Item item = player.inventory[slot];
                    if (item.type != stoneType || item.stack <= 0) continue;
                    int take = Math.Min(required, item.stack); required -= take; item.stack -= take;
                    if (item.stack <= 0) item.TurnToAir();
                    SyncSlot(player, slot);
                }
                result = material.AwakensArtifact ? "Mods.XianXia.Refinement.AwakeningSuccess" : material.RefinesArtifact ? "Mods.XianXia.Refinement.Success"
                    : next == InscriptionKind.None ? "Mods.XianXia.Inscriptions.Removed" : "Mods.XianXia.Inscriptions.Success";
            }
        }
        SyncSlot(player, toolSlot); SyncSlot(player, targetSlot);
        if (Main.netMode == NetmodeID.Server) {
            ModPacket packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket();
            packet.Write((byte)7); NetworkText.FromKey(result).Serialize(packet); packet.Send(player.whoAmI);
        }
        else Main.NewText(Language.GetTextValue(result), 120, 230, 210);
    }
    private static void SyncSlot(Player player, int slot)
    {
        if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, slot);
    }
}
