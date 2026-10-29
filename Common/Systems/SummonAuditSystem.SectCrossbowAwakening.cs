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
    private static void AuditSectCrossbowAwakening(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode, local = Main.myPlayer;
        var world = ModContent.GetInstance<DownedBossSystem>(); var progress = new TagCompound(); world.SaveWorldData(progress);
        int tx = Main.maxTilesX / 2, ty = 12; Tile station = Main.tile[tx,ty]; bool oldHas = station.HasTile; ushort oldType = station.TileType;
        bool oldHard = Main.hardMode, oldPlant = NPC.downedPlantBoss;
        var output = new AuditOutputSocket(true);
        var previousShots = Main.projectile.Where(projectile => projectile.active).ToHashSet();
        try {
            Main.hardMode = NPC.downedPlantBoss = true;
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(false), State = i == 0 ? 10 : 0, IsActive = i == 0 };
            station.HasTile = true; station.TileType = (ushort)ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>();
            Player Fixture(int netMode) {
                Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
                var actor = new Player { active = true, whoAmI = 0, selectedItem = 0 }; Main.player[0] = actor; actor.Center = new Vector2(tx * 16 + 8,ty * 16 + 8);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.NascentSoul; state.discoveredSpiritualEnergy = true; state.spiritualEnergy = 0;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native sect crossbow fixture imports valid authority state");
                DownedBossSystem.DownedBosses.Add("greenwood_medicine_king_echo");
                actor.inventory[0].SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ArtifactAwakeningSeal>()); actor.inventory[0].stack = 2;
                actor.inventory[1].SetDefaults(ModContent.ItemType<SectMechanismCrossbow>());
                check(actor.inventory[1].Prefix(PrefixID.Demonic),"native sect crossbow keeps native combat prefix");
                actor.inventory[1].GetGlobalItem<RefinedArtifact>().SetLevel(3); actor.inventory[1].GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
                actor.inventory[2].SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>()); actor.inventory[2].stack = 24;
                return actor;
            }
            void Submit(Player actor,byte level=3,bool awakened=false) => InscriptionTransactions.HandleRequest(actor,0,actor.inventory[0].type,1,actor.inventory[1].type,actor.inventory[1].prefix,(byte)InscribedEquipment.GetKind(actor.inventory[1]),level,awakened);
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) {
                Player actor = Fixture(netMode); Item target = actor.inventory[1]; int prefix = target.prefix, sent = output.Sent.Count;
                Submit(actor);
                check(RefinedArtifact.IsAwakened(target) && actor.inventory[0].stack == 1 && actor.inventory[2].IsAir,"native sect crossbow authority spends one seal and exactly twenty-four stones");
                check(ReferenceEquals(target,actor.inventory[1]) && target.prefix == prefix && RefinedArtifact.GetLevel(target) == 3 && InscribedEquipment.GetKind(target) == InscriptionKind.Greenwood,"native sect crossbow awakening retains actual equipment inscription prefix and refinement");
                check(netMode != NetmodeID.Server || output.Sent.Count > sent,"native sect crossbow server emits authoritative inventory/reply output to captured socket");
                actor.GetModPlayer<InscriptionPlayer>().Initialize(); Submit(actor,3,true);
                check(actor.inventory[0].stack == 1 && actor.inventory[2].IsAir,"native repeated sect crossbow awakening spends nothing");
                foreach (Item copy in new[] { target.Clone(),ItemIO.Load(ItemIO.Save(target)) })
                    check(copy.type == target.type && copy.prefix == prefix && RefinedArtifact.IsAwakened(copy) && RefinedArtifact.GetLevel(copy) == 3 && InscribedEquipment.GetKind(copy) == InscriptionKind.Greenwood && RefinedArtifact.SupportsDaoTransformation(copy) && RefinedArtifact.GetDaoRoute(copy) == DownedBossSystem.EndgameRoute.None,"native sect crossbow full save and clone preserve awakening without automatically granting a Dao route");
                using var bytes = new MemoryStream(); target.GetGlobalItem<RefinedArtifact>().NetSend(target,new BinaryWriter(bytes));
                check(bytes.ToArray().AsSpan().SequenceEqual(new byte[] { 3,1,0 }),"native sect crossbow uses unchanged three-byte growth payload with no Dao route");
                actor.selectedItem = 1; var castState = actor.GetModPlayer<XianXiaPlayer>(); castState.spiritualEnergy = 16; castState.activeSkillCooldown = 1200;
                bool shot = WeaponShotTransactions.FirePrepared(actor,(SectMechanismCrossbow)target.ModItem,
                    new Terraria.DataStructures.EntitySource_ItemUse_WithAmmo(actor,target,0),actor.Center,Vector2.UnitX * 14,target.shoot,100,3,netMode == NetmodeID.Server);
                check(shot && castState.spiritualEnergy == 0 && castState.activeSkillCooldown == 1200 && castState.arrayDeploymentCooldown == 0
                    && Main.projectile.Any(projectile => projectile.active && !previousShots.Contains(projectile) && projectile.type == target.shoot && projectile.owner == 0),
                    "native awakened sect crossbow deploys for sixteen energy without array cooldown");
                var bolt = Main.projectile.Single(projectile => projectile.active && !previousShots.Contains(projectile) && projectile.type == target.shoot);
                check(bolt.penetrate == 3 && bolt.tileCollide && bolt.timeLeft == 150 && bolt.DamageType == DamageClass.Ranged
                    && target.mana == 0 && target.useAmmo == 0,
                    "native awakened sect crossbow retains three-hit ranged bolt terrain collision and resource identity");
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
                    check(!RefinedArtifact.IsAwakened(actor.inventory[1]) && actor.inventory[0].stack == 2 && actor.inventory[2].stack == stones,"native sect crossbow invalid realm boss level inscription stones or station preserves resources");
                }
            }
            foreach (int netMode in new[] { 0,1,2 })
            foreach (CultivationStage stage in new[] { CultivationStage.GoldenCore,CultivationStage.NascentSoul,CultivationStage.SpiritSevering })
            foreach (int worldStage in new[] { 0,1,2 }) foreach (bool awakened in new[] { false,true }) {
                Main.netMode = netMode; Main.myPlayer = 0;
                Main.hardMode = worldStage >= 1; NPC.downedPlantBoss = worldStage >= 2;
                var actor = new Player { active = true, whoAmI = 0 }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = stage;
                var item = new Item(ModContent.ItemType<SectMechanismCrossbow>()); var weapon = (SectMechanismCrossbow)item.ModItem;
                var growth = item.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3);
                if (awakened) check(growth.TryAwaken(), "native sect crossbow explicit state fixture awakened");
                int cost = awakened ? 16 : 20;
                check(weapon.GetSpiritCost(actor) == cost, "native sect crossbow awakening changes only intended base deployment cost");
                var damage = StatModifier.Default; weapon.ModifyWeaponDamage(actor,ref damage); growth.ModifyWeaponDamage(item,actor,ref damage);
                check(Math.Abs(damage.ApplyTo(100) - (112 + (awakened ? 10 : 0))) < .001f,
                    "native sect crossbow refinement and awakening damage hooks remain additive");
                state.spiritualEnergy = cost - 1; check(!weapon.CanUseItem(actor), "native sect crossbow rejects one below current energy cost");
                state.spiritualEnergy = cost;
                check(weapon.CanUseItem(actor) == (stage >= CultivationStage.NascentSoul && worldStage == 2),
                    "native sect crossbow exact-energy use retains realm hardmode and Plantera gates");
                check(weapon.AltFunctionUse(actor) == awakened && ArtifactSkillTransactions.HeldSkill(item) == ArtifactSkill.SectVolley && RefinedArtifact.SupportsDaoTransformation(item),
                    "native sect crossbow awakening exposes its defined active skill only when awakened");
                var lines = new List<TooltipLine>(); growth.ModifyTooltips(item,lines);
                check(lines.Count == 2 && lines.TrueForAll(line => !line.Text.Contains("Mods.XianXia.") && !line.Text.Contains("{0}")),
                    "native sect crossbow crafted-awakening tooltips resolve for each state");
            }
        } finally {
            foreach (var projectile in Main.projectile.Where(projectile => projectile.active && !previousShots.Contains(projectile))) { projectile.damage = 0; projectile.active = false; }
            station.HasTile = oldHas; station.TileType = oldType; Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0);
            Main.netMode = mode; Main.myPlayer = local; world.LoadWorldData(progress);
            Main.hardMode = oldHard; NPC.downedPlantBoss = oldPlant;
        }
    }
}
