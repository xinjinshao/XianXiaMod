using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Content.Items.Weapons;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditCinnabarAwakening(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode, local = Main.myPlayer;
        var world = ModContent.GetInstance<DownedBossSystem>(); var progress = new TagCompound(); world.SaveWorldData(progress);
        int tx = Main.maxTilesX / 2, ty = 12; Tile station = Main.tile[tx,ty]; bool oldHas = station.HasTile; ushort oldType = station.TileType;
        var output = new AuditOutputSocket(true);
        var previousShots = Main.projectile.Where(projectile => projectile.active).ToHashSet();
        try {
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(false), State = i == 0 ? 10 : 0, IsActive = i == 0 };
            station.HasTile = true; station.TileType = (ushort)ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>();
            Player Fixture(int netMode) {
                Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
                var actor = new Player { active = true, whoAmI = 0, selectedItem = 0 }; Main.player[0] = actor; actor.Center = new Vector2(tx * 16 + 8,ty * 16 + 8);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.NascentSoul; state.discoveredSpiritualEnergy = true; state.spiritualEnergy = 0;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native cinnabar flame fixture imports valid authority state");
                DownedBossSystem.DownedBosses.Add("greenwood_medicine_king_echo");
                actor.inventory[0].SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ArtifactAwakeningSeal>()); actor.inventory[0].stack = 2;
                actor.inventory[1].SetDefaults(ModContent.ItemType<CinnabarTalismanFlameItem>());
                check(actor.inventory[1].Prefix(PrefixID.Godly),"native cinnabar flame keeps native combat prefix");
                actor.inventory[1].GetGlobalItem<RefinedArtifact>().SetLevel(3); actor.inventory[1].GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
                actor.inventory[2].SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>()); actor.inventory[2].stack = 24;
                return actor;
            }
            void Submit(Player actor,byte level=3,bool awakened=false) => InscriptionTransactions.HandleRequest(actor,0,actor.inventory[0].type,1,actor.inventory[1].type,actor.inventory[1].prefix,(byte)InscribedEquipment.GetKind(actor.inventory[1]),level,awakened);
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) {
                Player actor = Fixture(netMode); Item target = actor.inventory[1]; int prefix = target.prefix, sent = output.Sent.Count;
                Submit(actor);
                check(RefinedArtifact.IsAwakened(target) && actor.inventory[0].stack == 1 && actor.inventory[2].IsAir,"native cinnabar flame authority spends one seal and exactly twenty-four stones");
                check(ReferenceEquals(target,actor.inventory[1]) && target.prefix == prefix && RefinedArtifact.GetLevel(target) == 3 && InscribedEquipment.GetKind(target) == InscriptionKind.Greenwood,"native cinnabar flame awakening retains actual equipment inscription prefix and refinement");
                check(netMode != NetmodeID.Server || output.Sent.Count > sent,"native cinnabar flame server emits authoritative inventory/reply output to captured socket");
                actor.GetModPlayer<InscriptionPlayer>().Initialize(); Submit(actor,3,true);
                check(actor.inventory[0].stack == 1 && actor.inventory[2].IsAir,"native repeated cinnabar flame awakening spends nothing");
                foreach (Item copy in new[] { target.Clone(),ItemIO.Load(ItemIO.Save(target)) })
                    check(copy.type == target.type && copy.prefix == prefix && RefinedArtifact.IsAwakened(copy) && RefinedArtifact.GetLevel(copy) == 3 && InscribedEquipment.GetKind(copy) == InscriptionKind.Greenwood && RefinedArtifact.SupportsDaoTransformation(copy),"native cinnabar flame full save and clone preserve awakening without automatically granting a Dao route");
                using var bytes = new MemoryStream(); target.GetGlobalItem<RefinedArtifact>().NetSend(target,new BinaryWriter(bytes));
                check(bytes.ToArray().AsSpan().SequenceEqual(new byte[] { 3,1,0 }),"native cinnabar flame uses unchanged three-byte growth payload with no Dao route");
                actor.selectedItem = 1; var castState = actor.GetModPlayer<XianXiaPlayer>(); castState.spiritualEnergy = 3; castState.activeSkillCooldown = 1200;
                bool shot = WeaponShotTransactions.FirePrepared(actor,(CinnabarTalismanFlameItem)target.ModItem,
                    new Terraria.DataStructures.EntitySource_ItemUse_WithAmmo(actor,target,0),actor.Center,Vector2.UnitX * 13,target.shoot,100,3,netMode == NetmodeID.Server);
                check(shot && castState.spiritualEnergy == 0 && castState.activeSkillCooldown == 1200
                    && Main.projectile.Any(projectile => projectile.active && !previousShots.Contains(projectile) && projectile.type == target.shoot && projectile.owner == 0),
                    "native ordinary awakened thunder cast actually creates its registered projectile for three energy without touching shared skill cooldown");
                foreach (var projectile in Main.projectile.Where(projectile => projectile.active && !previousShots.Contains(projectile))) { projectile.damage = 0; projectile.active = false; }
                for (int invalid = 0; invalid < 6; invalid++) {
                    actor = Fixture(netMode); int stones = 24;
                    if (invalid == 0) actor.GetModPlayer<XianXiaPlayer>().cultivationStage = CultivationStage.GoldenCore;
                    if (invalid == 1) DownedBossSystem.DownedBosses.Remove("greenwood_medicine_king_echo");
                    if (invalid == 2) actor.inventory[1].GetGlobalItem<RefinedArtifact>().SetLevel(2);
                    if (invalid == 3) actor.inventory[1].GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.None);
                    if (invalid == 4) { actor.inventory[2].stack = 23; stones = 23; }
                    if (invalid == 5) station.HasTile = false;
                    try { Submit(actor,(byte)(invalid == 2 ? 2 : 3)); } finally { station.HasTile = true; }
                    check(!RefinedArtifact.IsAwakened(actor.inventory[1]) && actor.inventory[0].stack == 2 && actor.inventory[2].stack == stones,"native cinnabar flame invalid realm boss level inscription stones or station preserves resources");
                }
            }
            foreach (int netMode in new[] { 0,1,2 }) foreach (CultivationStage stage in new[] { CultivationStage.QiAwakening,CultivationStage.Foundation,CultivationStage.GoldenCore,CultivationStage.NascentSoul })
                foreach (int reputation in new[] { 23,24 }) foreach (bool awakened in new[] { false,true }) {
                    Main.netMode = netMode; Main.myPlayer = 0;
                    var actor = new Player { active = true, whoAmI = 0 }; Main.player[0] = actor; var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = stage;
                    typeof(DownedBossSystem).GetProperty(nameof(DownedBossSystem.SectReputation)).SetValue(null,reputation);
                    var item = new Item(); item.SetDefaults(ModContent.ItemType<CinnabarTalismanFlameItem>()); var weapon = (CinnabarTalismanFlameItem)item.ModItem;
                    var growth = item.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); if (awakened) check(growth.TryAwaken(),"native cinnabar flame explicit state fixture awakened");
                    bool heritage = stage >= CultivationStage.Foundation && reputation >= 24; int cost = awakened ? 3 : heritage ? 4 : 5;
                    check(weapon.GetSpiritCost(actor) == cost,"native cinnabar flame crafted three-energy cost independent of preserved four/five-energy heritage");
                    var damage = StatModifier.Default; weapon.ModifyWeaponDamage(actor,ref damage); growth.ModifyWeaponDamage(item,actor,ref damage);
                    check(Math.Abs(damage.ApplyTo(100) - (112 + (heritage ? 10 : 0) + (awakened ? 10 : 0))) < .001f,"native cinnabar flame refinement awakening and heritage damage hooks remain additive");
                    state.spiritualEnergy = cost - 1; check(!weapon.CanUseItem(actor),"native cinnabar flame rejects one below current energy cost"); state.spiritualEnergy = cost; check(weapon.CanUseItem(actor),"native cinnabar flame accepts exact current energy cost");
                    check(weapon.AltFunctionUse(actor) == awakened && ArtifactSkillTransactions.HeldSkill(item) == ArtifactSkill.FlameFan && RefinedArtifact.SupportsDaoTransformation(item),"native cinnabar flame supports Dao eligibility with awakening-gated Flame Fan");
                    var lines = new List<TooltipLine>(); weapon.ModifyTooltips(lines); growth.ModifyTooltips(item,lines);
                    check(lines.Count == 3 && lines.TrueForAll(line => !line.Text.Contains("Mods.XianXia.") && !line.Text.Contains("{0}")),"native cinnabar flame heritage and crafted-awakening tooltips resolve independently");
                }
        } finally {
            foreach (var projectile in Main.projectile.Where(projectile => projectile.active && !previousShots.Contains(projectile))) { projectile.damage = 0; projectile.active = false; }
            station.HasTile = oldHas; station.TileType = oldType; Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0);
            Main.netMode = mode; Main.myPlayer = local; world.LoadWorldData(progress);
        }
    }
}
