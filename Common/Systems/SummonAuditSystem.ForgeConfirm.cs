using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Common.UI;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditForgeConfirm(Action<bool,string> check)
    {
        var previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        bool menu = Main.gameMenu, blocked = Main.blockInput, chat = Main.drawingPlayerChat, inventory = Main.playerInventory;
        Item mouse = Main.mouseItem; var connection = Netplay.Connection;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        int x = Main.maxTilesX / 2, y = 12; Tile tile = Main.tile[x,y]; bool has = tile.HasTile; ushort oldType = tile.TileType;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        try {
            foreach (var prototype in ModLoader.GetMod("XianXia").GetContent<ModItem>()) {
                var probe = new Item(prototype.Type); if (!RefinedArtifact.SupportsAwakening(probe)) continue;
                foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient })
                foreach (int scenario in Enumerable.Range(0,8)) {
                    Main.netMode = netMode; Main.myPlayer = 0;
                    Main.gameMenu = Main.blockInput = Main.drawingPlayerChat = false; Main.playerInventory = true; Main.mouseItem = new Item();
                    var output = new AuditOutputSocket(true); Netplay.Connection = new RemoteServer { Socket = output };
                    world.LoadWorldData(new TagCompound()); DownedBossSystem.DownedBosses.Add("greenwood_medicine_king_echo"); DownedBossSystem.DownedBosses.Add("moonbone_immortal");
                    tile.HasTile = true; tile.TileType = (ushort)ModContent.TileType<ArtifactForgeTile>();
                    var actor = new Player { active = true, whoAmI = 0, selectedItem = 0 }; Main.player[0] = actor;
                    actor.Center = new Vector2(x * 16 + 8,y * 16 + 8);
                    var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
                    state.TryInitializeNetwork(state.CaptureSnapshot());
                    actor.inventory[0] = new Item(ModContent.ItemType<ArtifactAwakeningSeal>(),2);
                    actor.inventory[1] = new Item(prototype.Type); actor.inventory[2] = new Item(ModContent.ItemType<LowGradeSpiritStone>(),24);
                    var target = actor.inventory[1]; target.GetGlobalItem<RefinedArtifact>().SetLevel(3); target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
                    if (scenario == 1) state.cultivationStage = CultivationStage.GoldenCore;
                    if (scenario == 2) DownedBossSystem.DownedBosses.Remove("greenwood_medicine_king_echo");
                    if (scenario == 3) target.GetGlobalItem<RefinedArtifact>().SetLevel(2);
                    if (scenario == 4) target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.None);
                    if (scenario == 5) actor.inventory[2].stack = 23;
                    if (scenario == 6) tile.HasTile = false;
                    if (scenario == 7) target.GetGlobalItem<RefinedArtifact>().TryAwaken();
                    var ui = new InscriptionUIState();
                    void Set(string field,object value) => typeof(InscriptionUIState).GetField(field,flags)!.SetValue(ui,value);
                    Set("toolSlot",0); Set("toolType",actor.inventory[0].type); Set("targetSlot",1); Set("targetType",target.type); Set("targetPrefix",target.prefix);
                    Set("previousKind",(byte)InscribedEquipment.GetKind(target)); Set("previousLevel",RefinedArtifact.GetLevel(target)); Set("previousAwakened",RefinedArtifact.IsAwakened(target));
                    Set("previousDaoRoute",(byte)0); Set("previousWorldRoute",(byte)0);
                    var before = state.CaptureSnapshot(); int stones = actor.inventory[2].stack; bool awake = RefinedArtifact.IsAwakened(target);
                    typeof(InscriptionUIState).GetMethod("ConfirmSelection",flags)!.Invoke(ui,null);
                    if (scenario != 0) {
                        check(output.Sent.Count == 0 && actor.inventory[0].stack == 2 && actor.inventory[2].stack == stones
                            && RefinedArtifact.IsAwakened(target) == awake && state.CaptureSnapshot().Equals(before),
                            "native nineteen-weapon forge confirm refuses changed requirements without request or cost: " + prototype.Name + "/" + netMode + "/" + scenario);
                        check((int)typeof(InscriptionUIState).GetField("refreshTimer",flags)!.GetValue(ui)! == 20
                            && (int)typeof(InscriptionUIState).GetField("targetSlot",flags)!.GetValue(ui)! == 1,
                            "native blocked forge confirm retains selection and schedules next-frame requirement feedback");
                    } else check(netMode == NetmodeID.SinglePlayer
                        ? RefinedArtifact.IsAwakened(target) && actor.inventory[0].stack == 1 && actor.inventory[2].IsAir
                        : output.Sent.Count == 1 && !RefinedArtifact.IsAwakened(target) && actor.inventory[0].stack == 2 && actor.inventory[2].stack == 24,
                        "native valid forge confirmation still applies locally or sends exactly one client request");
                }
            }
        } finally {
            Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; Netplay.Connection = connection;
            Main.gameMenu = menu; Main.blockInput = blocked; Main.drawingPlayerChat = chat; Main.playerInventory = inventory; Main.mouseItem = mouse;
            tile.HasTile = has; tile.TileType = oldType; world.LoadWorldData(saved);
        }
    }
}
