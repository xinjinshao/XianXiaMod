using System;
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
    private static void AuditWardDao(Action<bool,string> check)
    {
        Player previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        bool hard = Main.hardMode, plant = NPC.downedPlantBoss, golem = NPC.downedGolemBoss;
        var clients = (RemoteClient[])Netplay.Clients.Clone(); var text = Main.combatText;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        var before = Main.projectile.Where(p => p.active).ToHashSet();
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        try {
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            Main.hardMode = NPC.downedPlantBoss = NPC.downedGolemBoss = true;
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (int route in new[] { 1,2,3 }) {
                Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
                var tag = new TagCompound(); world.SaveWorldData(tag); tag["chosenRoute"] = route; world.LoadWorldData(tag); DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                var actor = new Player { whoAmI = 0, active = true, statLife = 50, statLifeMax2 = 100 }; Main.player[0] = actor; actor.Center = new Vector2(Main.maxTilesX * 8,400);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native ward Dao fixture imports valid state"); state.spiritualEnergy = 100;
                actor.inventory[0].SetDefaults(ModContent.ItemType<HeavenTabletWardSeal>()); var item = actor.HeldItem; var growth = item.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken();
                check(growth.TryTransform((DownedBossSystem.EndgameRoute)route),"native ward Dao fixture explicitly transforms supported item");
                var damage = StatModifier.Default; growth.ModifyWeaponDamage(item,actor,ref damage);
                check(Math.Abs(damage.Base - 54) < .001f,"native ward Dao grows base damage from 156 to 210");
                var weapon = (HeavenTabletWardSeal)item.ModItem; int cost = route == 1 ? 24 : route == 2 ? 30 : 20;
                actor.altFunctionUse = 2; state.spiritualEnergy = cost - 1; check(!weapon.CanUseItem(actor),"native ward Dao alternative input checks route-specific energy");
                check(ArtifactSkillTransactions.UnavailableReason(actor,ArtifactSkill.WardGuard) == "Mods.XianXia.Skills.NeedEnergy","native panel readiness reports actual Dao ward shortage");
                state.spiritualEnergy = 100;
                var snapshot = state.CaptureSnapshot(); int life = actor.statLife;
                check(ArtifactSkillTransactions.UnavailableReason(actor,ArtifactSkill.WardGuard) == null && state.CaptureSnapshot() == snapshot && actor.statLife == life,"native panel readiness is read-only before Dao ward cast");
                void Cast() => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.WardGuard,0,item.type,actor.Center);
                Cast();
                check(ArtifactSkillTransactions.UnavailableReason(actor,ArtifactSkill.WardGuard) == "Mods.XianXia.Skills.CoolingDown","native panel readiness follows actual Dao ward cooldown");
                check(state.spiritualEnergy == 100 - cost && state.wardGuardTimer == 180 && state.activeSkillCooldown == 1200
                    && actor.statLife == (route == 1 ? 70 : 50) && state.spiritPressure == (route == 3 ? 8 : 0),"native ward Dao authoritative protection route cost healing and pressure");
                actor.statDefense = Player.DefenseStat.Default; int buffIndex = 0; float oldEndurance = actor.endurance; int oldDefense = actor.statDefense;
                ModContent.GetInstance<global::XianXia.Content.Buffs.ArtifactWardBuff>().Update(actor,ref buffIndex);
                check(Math.Abs(actor.endurance - oldEndurance - .35f) < .001f && (int)actor.statDefense == oldDefense + 12,"native ward Dao preserves actual thirty-five-percent and twelve-defense buff: endurance=" + actor.endurance + " before=" + oldEndurance + "; defense=" + (int)actor.statDefense + " before=" + oldDefense + "; timer=" + state.wardGuardTimer);
                state.skillRequestCooldown = 0; Cast(); check(state.spiritualEnergy == 100 - cost && actor.statLife == (route == 1 ? 70 : 50) && state.spiritPressure == (route == 3 ? 8 : 0),"native ward Dao repeated request cannot repeat success effects");
                actor.altFunctionUse = 0; state.spiritualEnergy = 100;
                check(weapon.GetSpiritCost(actor) == (route == 3 ? 24 : 30),"native ward Dao ordinary fee follows active world route");
                check(WeaponShotTransactions.FirePrepared(actor,weapon,new Terraria.DataStructures.EntitySource_ItemUse_WithAmmo(actor,item,0),actor.Center,Vector2.UnitY * 7,item.shoot,156,7,netMode == NetmodeID.Server)
                    && state.spiritualEnergy == 100 - (route == 3 ? 24 : 30) && state.activeSkillCooldown == 1200,"native ward Dao ordinary shot remains separate from shared skill cooldown");
                Clear(); state.skillRequestCooldown = state.activeSkillCooldown = state.wardGuardTimer = 0; state.spiritualEnergy = 100; state.spiritPressure = 0; actor.statLife = 50;
                DownedBossSystem.DownedBosses.Remove("old_heaven_dao_core"); Cast();
                check(state.spiritualEnergy == 82 && state.wardGuardTimer == 180 && actor.statLife == 50 && state.spiritPressure == 0 && weapon.GetSpiritCost(actor) == 24
                    && (int)RefinedArtifact.GetDaoRoute(item) == route,"native ward Dao inactive world restores original protection and awakened ordinary cost without erasing route");
                DownedBossSystem.DownedBosses.Add("old_heaven_dao_core"); actor.inventory[0].SetDefaults(ModContent.ItemType<ThunderPatternSwordCase>());
                var other = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); other.SetLevel(3); other.TryAwaken(); other.TryTransform((DownedBossSystem.EndgameRoute)route);
                actor.armor[3].SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.Accessories.LightningWardJade>());
                state.skillRequestCooldown = state.activeSkillCooldown = state.wardGuardTimer = 0; state.spiritualEnergy = 100;
                ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.WardGuard,0,actor.HeldItem.type,actor.Center);
                check(state.spiritualEnergy == 82 && state.wardGuardTimer == 180 && actor.statLife == 50 && state.spiritPressure == 0,"native jade ward cannot borrow another held artifact Dao route");
            }
        } finally { Clear(); world.LoadWorldData(saved); Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; Main.hardMode = hard; NPC.downedPlantBoss = plant; NPC.downedGolemBoss = golem; clients.CopyTo(Netplay.Clients,0); Main.combatText = text; }
        check(ReferenceEquals(Main.player[0],previous) && Main.netMode == mode && Main.combatText == text,"native ward Dao fixture restores authority and rendering context");
    }
}
