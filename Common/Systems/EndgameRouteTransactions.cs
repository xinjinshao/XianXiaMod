using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;

public static class EndgameRouteTransactions
{
    public static string NameKey(DownedBossSystem.EndgameRoute route) => "Mods.XianXia.Routes." + route;
    public static bool NearAltar(Player player)
    {
        var center = player.Center.ToTileCoordinates();
        int type = ModContent.TileType<DaoSeveringAltarTile>();
        for (int x = Math.Max(0, center.X - 6); x <= Math.Min(Main.maxTilesX - 1, center.X + 6); x++)
            for (int y = Math.Max(0, center.Y - 4); y <= Math.Min(Main.maxTilesY - 1, center.Y + 4); y++)
                if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == type) return true;
        return false;
    }
    public static void Request(Player player, int slot, DownedBossSystem.EndgameRoute route)
    {
        if (slot < 0 || slot >= 58 || player.whoAmI != Main.myPlayer) return;
        if (Main.netMode == NetmodeID.SinglePlayer) { Handle(player, slot, player.inventory[slot].type, (byte)route); return; }
        if (Main.netMode != NetmodeID.MultiplayerClient) return;
        var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket();
        packet.Write((byte)11); packet.Write((byte)slot); packet.Write(player.inventory[slot].type); packet.Write((byte)route); packet.Send();
    }
    public static void Handle(Player player, int slot, int type, byte route)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !player.active || player.dead || player.noItems || player.CCed
            || slot < 0 || slot >= Math.Min(58, player.inventory.Length) || slot != player.selectedItem
            || (Main.netMode == NetmodeID.Server && !player.GetModPlayer<XianXiaPlayer>().NetworkInitialized)
            || route < 1 || route > 3) return;
        if (!player.GetModPlayer<InscriptionPlayer>().BeginTransaction()) return;
        Item item = player.inventory[slot];
        if (item.type != type || type != ModContent.ItemType<RouteMaterial>() || item.stack <= 0) return;
        string result;
        if (DownedBossSystem.ChosenRoute != DownedBossSystem.EndgameRoute.None) result = "AlreadyChosen";
        else if (!DownedBossSystem.DownedBosses.Contains("old_heaven_dao_core")
            || player.GetModPlayer<XianXiaPlayer>().cultivationStage < CultivationStage.Tribulation
            || player.GetModPlayer<XianXiaPlayer>().cultivationStage > CultivationStage.DaoSevering) result = "Requirements";
        else if (!NearAltar(player)) result = "NeedAltar";
        else if (DownedBossSystem.TryChooseRoute((DownedBossSystem.EndgameRoute)route)) {
            item.stack--; if (item.stack == 0) item.TurnToAir();
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, slot);
            BroadcastChoice((DownedBossSystem.EndgameRoute)route);
            return;
        }
        else result = "AlreadyChosen";
        Reply(player.whoAmI, NetworkText.FromKey("Mods.XianXia.Routes." + result));
    }
    private static void BroadcastChoice(DownedBossSystem.EndgameRoute route)
    {
        NetworkText text = NetworkText.FromKey("Mods.XianXia.Routes.Completed", NetworkText.FromKey(NameKey(route)));
        if (Main.netMode == NetmodeID.SinglePlayer) { Reply(Main.myPlayer, text); return; }
        foreach (Player player in Main.ActivePlayers) Reply(player.whoAmI, text);
    }
    private static void Reply(int player, NetworkText text)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(text.ToString(), 220, 210, 120);
        else { var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket(); packet.Write((byte)7); text.Serialize(packet); packet.Send(player); }
    }
}
