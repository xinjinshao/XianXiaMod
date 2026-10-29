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
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditCauldronSkill(Action<bool,string> check)
    {
        var players = Main.player; var clients = (RemoteClient[])Netplay.Clients.Clone();
        int mode = Main.netMode, local = Main.myPlayer; bool hard = Main.hardMode, plant = NPC.downedPlantBoss;
        var text = Main.combatText; var before = Main.projectile.Where(p => p.active).ToHashSet();
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        Player Fixture(int authority,int record,int route,bool core) {
            Clear(); Main.netMode = authority; Main.myPlayer = authority == NetmodeID.Server ? Main.maxPlayers : 0;
            Main.hardMode = NPC.downedPlantBoss = true;
            world.LoadWorldData(new TagCompound { ["chosenRoute"] = route }); if (core) DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
            Main.player = Enumerable.Range(0,players.Length).Select(i => new Player { whoAmI = i, active = false }).ToArray();
            var actor = Main.player[0]; actor.active = true; actor.hostile = true; actor.team = 1; actor.statLife = 100; actor.statLifeMax2 = 200;
            actor.Center = new Vector2(Main.maxTilesX * 8,400);
            var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
            check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native cauldron skill fixture imports authority state"); state.spiritPressure = 5;
            actor.inventory[0] = new Item(ModContent.ItemType<GreenwoodMedicineCauldron>());
            var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); if (record > 0) growth.TryTransform((DownedBossSystem.EndgameRoute)record);
            return actor;
        }
        Projectile Field(Player actor) {
            int index = Projectile.NewProjectile(new Terraria.DataStructures.EntitySource_ItemUse_WithAmmo(actor,actor.HeldItem,0),actor.Center,Vector2.Zero,
                ModContent.ProjectileType<MedicineCauldronField>(),82,2,actor.whoAmI);
            check(index >= 0 && index < Main.maxProjectiles,"native cauldron skill fixture creates registered owner-bound field"); return Main.projectile[index];
        }
        void Cast(Player actor) => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.CauldronBloom,0,actor.HeldItem.type,actor.Center);
        try {
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(), State = 0 };
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server })
            foreach (int record in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                var actor = Fixture(authority,record,route,core); var state = actor.GetModPlayer<XianXiaPlayer>(); var field = Field(actor);
                int active = core && route == record ? record : 0, cost = active == 0 ? 30 : active == 3 ? 34 : 40;
                int heal = active == 1 ? 60 : active == 2 ? 45 : active == 3 ? 40 : 30;
                for (int i = 1; i <= 3; i++) {
                    var peer = Main.player[i]; peer.active = true; peer.hostile = true; peer.team = i == 2 ? 2 : 1;
                    peer.statLife = i == 1 ? 195 : 100; peer.statLifeMax2 = 200;
                    peer.Center = field.Center + Vector2.UnitX * (i == 1 ? 200 : i == 3 ? 201 : 0);
                }
                state.spiritualEnergy = cost - 1; Cast(actor);
                check(state.spiritualEnergy == cost - 1 && actor.statLife == 100 && Main.player[1].statLife == 195 && state.activeSkillCooldown == 0,
                    "native cauldron skill shortage heals nobody and starts no cooldown");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost;
                actor.ownedProjectileCounts[field.type] = 1; state.arrayDeploymentCooldown = 480;
                check(!actor.HeldItem.ModItem.CanUseItem(actor),"native ordinary cauldron cannot redeploy an existing field during deployment cooldown");
                actor.altFunctionUse = 2;
                check(actor.HeldItem.ModItem.CanUseItem(actor),"native cauldron alternative bypasses ordinary field redeployment gate at exact active fee");
                Cast(actor);
                check(actor.statLife == 100 + heal && Main.player[1].statLife == 200 && Main.player[2].statLife == 100 && Main.player[3].statLife == 100,
                    "native cauldron skill heals self and same-team edge target with actual cap but excludes enemy and out-of-range players");
                check(state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200 && state.spiritPressure == (active == 3 ? 13 : 5)
                    && state.arrayDeploymentCooldown == 480 && Main.projectile.Count(p => p.active && !before.Contains(p)) == 1,
                    "native cauldron healing spends once with route effects and preserves the single existing field");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost; Cast(actor);
                check(actor.statLife == 100 + heal && state.spiritualEnergy == cost,"native cauldron duplicate skill cannot repeat healing or cost");
            }
            foreach (int invalid in Enumerable.Range(0,9)) {
                var actor = Fixture(NetmodeID.Server,0,0,false); var state = actor.GetModPlayer<XianXiaPlayer>(); var field = Field(actor); state.spiritualEnergy = 100;
                if (invalid == 0) field.active = false;
                if (invalid == 1) field.owner = 1;
                if (invalid == 2) actor.Center += Vector2.UnitX * 201;
                if (invalid == 3) field.ai[0] = float.NaN;
                if (invalid == 4) field.timeLeft = 301;
                if (invalid == 5) actor.statLife = actor.statLifeMax2;
                if (invalid == 6) state.cultivationStage = CultivationStage.GoldenCore;
                if (invalid == 7) NPC.downedPlantBoss = false;
                if (invalid == 8) actor.HeldItem.GetGlobalItem<RefinedArtifact>().SetLevel(2);
                int life = actor.statLife; Cast(actor);
                check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && actor.statLife == life,
                    "native cauldron skill refuses absent foreign distant malformed expired field, no heal, progression or awakening failure");
            }
        }
        finally { Clear(); Main.player = players; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local;
            Main.hardMode = hard; NPC.downedPlantBoss = plant; Main.combatText = text; world.LoadWorldData(saved); }
    }
}
