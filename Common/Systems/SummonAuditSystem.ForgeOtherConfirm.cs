using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Common.UI;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditForgeOtherConfirm(Action<bool,string> check)
    {
        var previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        bool menu = Main.gameMenu, blocked = Main.blockInput, chat = Main.drawingPlayerChat, inventory = Main.playerInventory;
        Item mouse = Main.mouseItem; var connection = Netplay.Connection;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        int x = Main.maxTilesX / 2, y = 12; Tile tile = Main.tile[x,y]; bool has = tile.HasTile; ushort oldType = tile.TileType;
        var mod = ModLoader.GetMod("XianXia"); int Type(string name) => mod.Find<ModItem>(name).Type;
        byte[] Inventory(Player actor) { using var stream = new MemoryStream(); TagIO.ToStream(new TagCompound { ["items"] = actor.inventory.Take(58).Select(ItemIO.Save).ToList() },stream,false); return stream.ToArray(); }
        var ops = new List<(string Tool,string Weapon,byte Level,InscriptionKind Kind,int Cost,int Route,int States)>();
        string[] needles = { "GreenwoodInscriptionNeedle","FurnaceInscriptionNeedle","ThunderInscriptionNeedle","StarAbyssInscriptionNeedle","BrokenHeavenInscriptionNeedle" };
        foreach (string needle in needles) ops.Add((needle,"CloudpiercerFlyingSword",0,InscriptionKind.None,3,0,3));
        ops.Add(("InscriptionRemovalStone","CloudpiercerFlyingSword",2,InscriptionKind.Greenwood,0,0,3));
        for (byte level = 0; level < 3; level++) ops.Add(("ArtifactQuenchingCrystal","CloudpiercerFlyingSword",level,InscriptionKind.Greenwood,6 + level * 6,0,7));
        foreach (string weapon in new[] { "CloudpiercerFlyingSword","GreenwoodArrayPlate","HeavenTabletWardSeal","MoonboneDharmaSword","FormlessSwordWheel","BlackFurnaceWarhammer","ThunderPatternSwordCase","CinnabarTalismanFlameItem","WoodgrainFlyingSword" })
            for (int route = 1; route <= 3; route++) ops.Add(("DaoTransformationSeal",weapon,3,InscriptionKind.Greenwood,36,route,10));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        try {
            foreach (var op in ops) foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient }) for (int scenario = 0; scenario < op.States; scenario++) {
                Main.netMode = netMode; Main.myPlayer = 0; Main.gameMenu = Main.blockInput = Main.drawingPlayerChat = false; Main.playerInventory = true; Main.mouseItem = new Item();
                var output = new AuditOutputSocket(true); Netplay.Connection = new RemoteServer { Socket = output };
                world.LoadWorldData(new TagCompound { ["chosenRoute"] = op.Route });
                foreach (string boss in new[] { "black_furnace_iron_golem","thunder_marsh_jiao","formless_sword_soul","old_heaven_dao_core" }) DownedBossSystem.DownedBosses.Add(boss);
                tile.HasTile = true; tile.TileType = (ushort)(op.Route == 0 ? ModContent.TileType<ArtifactForgeTile>() : ModContent.TileType<DaoSeveringAltarTile>());
                var actor = new Player { active = true, whoAmI = 0, selectedItem = 0 }; Main.player[0] = actor; actor.Center = new Vector2(x * 16 + 8,y * 16 + 8);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true; state.TryInitializeNetwork(state.CaptureSnapshot());
                actor.inventory[0] = new Item(Type(op.Tool),2); actor.inventory[1] = new Item(Type(op.Weapon)); actor.inventory[2] = new Item(Type("LowGradeSpiritStone"),op.Cost + 2);
                var target = actor.inventory[1]; var tool = (InscriptionToolItem)actor.inventory[0].ModItem;
                var growth = target.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(op.Level); target.GetGlobalItem<InscribedEquipment>().SetKind(op.Kind);
                if (op.Route > 0) growth.TryAwaken();
                if (scenario == 1) tile.HasTile = false;
                if (op.Route > 0) {
                    switch (scenario) {
                        case 2: actor.inventory[2].stack = 35; break;
                        case 3: target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.None); break;
                        case 4: growth.SetLevel(2); break;
                        case 5: growth.SetLevel(2); growth.SetLevel(3); break;
                        case 6: growth.TryTransform((DownedBossSystem.EndgameRoute)op.Route); break;
                        case 7: state.cultivationStage = CultivationStage.Tribulation; break;
                        case 8: DownedBossSystem.DownedBosses.Remove("old_heaven_dao_core"); break;
                        case 9: world.LoadWorldData(new TagCompound()); DownedBossSystem.DownedBosses.Add("old_heaven_dao_core"); break;
                    }
                } else if (tool.RefinesArtifact) {
                    switch (scenario) {
                        case 2: actor.inventory[2].stack = op.Cost - 1; break;
                        case 3: state.cultivationStage = CultivationStage.None; break;
                        case 4: DownedBossSystem.DownedBosses.Remove(RefinementRules.RequiredBoss(op.Level)); break;
                        case 5: target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.None); break;
                        case 6: growth.SetLevel(3); break;
                    }
                } else if (scenario == 2) target.GetGlobalItem<InscribedEquipment>().SetKind(tool.TargetKind);
                var ui = new InscriptionUIState();
                void Set(string field,object value) => typeof(InscriptionUIState).GetField(field,flags)!.SetValue(ui,value);
                Set("toolSlot",0); Set("toolType",actor.inventory[0].type); Set("targetSlot",1); Set("targetType",target.type); Set("targetPrefix",target.prefix);
                Set("previousKind",(byte)InscribedEquipment.GetKind(target)); Set("previousLevel",RefinedArtifact.GetLevel(target)); Set("previousAwakened",RefinedArtifact.IsAwakened(target));
                Set("previousDaoRoute",(byte)RefinedArtifact.GetDaoRoute(target)); Set("previousWorldRoute",(byte)DownedBossSystem.ChosenRoute);
                byte[] before = Inventory(actor); var resources = state.CaptureSnapshot();
                typeof(InscriptionUIState).GetMethod("ConfirmSelection",flags)!.Invoke(ui,null);
                string context = op.Tool + "/" + op.Weapon + "/" + op.Level + "/" + op.Route + "/" + netMode + "/" + scenario;
                if (scenario > 0) {
                    check(output.Sent.Count == 0 && Inventory(actor).SequenceEqual(before) && state.CaptureSnapshot().Equals(resources),
                        "native inscription refinement Dao UI confirm refuses unmet requirements without packet or inventory changes: " + context);
                    check((int)typeof(InscriptionUIState).GetField("refreshTimer",flags)!.GetValue(ui)! == 20
                        && (int)typeof(InscriptionUIState).GetField("targetSlot",flags)!.GetValue(ui)! == 1,
                        "native blocked non-awakening forge confirm retains selection and schedules feedback");
                } else if (netMode == NetmodeID.MultiplayerClient) check(output.Sent.Count == 1 && Inventory(actor).SequenceEqual(before),
                    "native ready non-awakening forge client confirm sends one request without speculative item changes");
                else check(actor.inventory[0].stack == 1 && actor.inventory[2].stack == 2
                    && (op.Route > 0 ? RefinedArtifact.GetDaoRoute(target) == (DownedBossSystem.EndgameRoute)op.Route
                        : tool.RefinesArtifact ? RefinedArtifact.GetLevel(target) == op.Level + 1 : InscribedEquipment.GetKind(target) == tool.TargetKind),
                    "native ready inscription refinement Dao UI confirm applies exact local materials and intended result");
            }
        } finally {
            Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; Netplay.Connection = connection;
            Main.gameMenu = menu; Main.blockInput = blocked; Main.drawingPlayerChat = chat; Main.playerInventory = inventory; Main.mouseItem = mouse;
            tile.HasTile = has; tile.TileType = oldType; world.LoadWorldData(saved);
        }
    }
}
