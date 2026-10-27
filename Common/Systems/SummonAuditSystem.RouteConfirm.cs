using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Common.UI;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditRouteConfirm(Action<bool,string> check)
    {
        var previous = Main.player[0]; var connection = Netplay.Connection; int mode = Main.netMode,local = Main.myPlayer;
        bool menu = Main.gameMenu,blocked = Main.blockInput,chat = Main.drawingPlayerChat,inventory = Main.playerInventory; var mouse = Main.mouseItem;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        int x = Main.maxTilesX / 2,y = 12; Tile tile = Main.tile[x,y]; var original = AuditTileSnapshot.Capture(tile);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        try {
            Main.gameMenu = Main.blockInput = Main.drawingPlayerChat = false; Main.playerInventory = true; Main.mouseItem = new Item(); Main.myPlayer = 0;
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient }) foreach (int route in new[] { 1,2,3 }) foreach (int failure in new[] { 0,1,2 }) {
                Main.netMode = netMode; world.LoadWorldData(new TagCompound()); DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                tile.HasTile = true; tile.TileType = (ushort)ModContent.TileType<DaoSeveringAltarTile>();
                var actor = new Player { active = true,whoAmI = 0,selectedItem = 0 }; Main.player[0] = actor; actor.Center = new Microsoft.Xna.Framework.Vector2(x * 16 + 8,y * 16 + 8);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.Tribulation;
                actor.inventory[0] = new Item(ModContent.ItemType<RouteMaterial>());
                if (failure == 0) state.cultivationStage = CultivationStage.NascentSoul;
                if (failure == 1) DownedBossSystem.DownedBosses.Remove("old_heaven_dao_core");
                if (failure == 2) tile.HasTile = false;
                var output = new AuditOutputSocket(true); Netplay.Connection = new RemoteServer { Socket = output };
                var ui = new EndgameRouteUIState();
                typeof(EndgameRouteUIState).GetField("slot",flags)!.SetValue(ui,0);
                typeof(EndgameRouteUIState).GetField("selectedItem",flags)!.SetValue(ui,actor.inventory[0]);
                typeof(EndgameRouteUIState).GetField("selected",flags)!.SetValue(ui,(DownedBossSystem.EndgameRoute)route);
                var resources = state.CaptureSnapshot(); var item = actor.inventory[0];
                typeof(EndgameRouteUIState).GetMethod("ConfirmSelection",flags)!.Invoke(ui,null);
                check(output.Sent.Count == 0 && ReferenceEquals(item,actor.inventory[0]) && item.stack == 1 && state.CaptureSnapshot().Equals(resources)
                    && DownedBossSystem.ChosenRoute == DownedBossSystem.EndgameRoute.None,"native route UI confirm rejects missing realm core or altar without packet material or world changes");
                check((int)typeof(EndgameRouteUIState).GetField("requirementRefreshTimer",flags)!.GetValue(ui)! == 30
                    && (DownedBossSystem.EndgameRoute)typeof(EndgameRouteUIState).GetField("selected",flags)!.GetValue(ui)! == (DownedBossSystem.EndgameRoute)route,
                    "native blocked route UI confirm retains preview selection and schedules requirement feedback");
            }
        } finally {
            original.Restore(tile); Main.player[0] = previous; Netplay.Connection = connection; Main.netMode = mode; Main.myPlayer = local;
            Main.gameMenu = menu; Main.blockInput = blocked; Main.drawingPlayerChat = chat; Main.playerInventory = inventory; Main.mouseItem = mouse; world.LoadWorldData(saved);
        }
    }
}
