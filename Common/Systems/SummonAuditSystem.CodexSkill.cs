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
    private static void AuditCodexSkill(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode,local = Main.myPlayer;
        bool hard = Main.hardMode, plant = NPC.downedPlantBoss, golem = NPC.downedGolemBoss, moon = NPC.downedMoonlord;
        var text = Main.combatText; var before = Main.projectile.Where(p => p.active).ToHashSet();
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        try {
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i,Socket = new AuditOutputSocket(),State = 0 };
            Player Fixture(int authority,int record,int route,bool core) {
                Clear(); Main.hardMode = NPC.downedPlantBoss = NPC.downedGolemBoss = NPC.downedMoonlord = true; Main.netMode = authority; Main.myPlayer = authority == NetmodeID.Server ? Main.maxPlayers : 0;
                world.LoadWorldData(new TagCompound { ["chosenRoute"] = route }); if (core) DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                var actor = new Player { active = true,whoAmI = 0,selectedItem = 0,statLife = 100,statLifeMax2 = 200 }; Main.player[0] = actor;
                actor.Center = new Vector2(Main.maxTilesX * 8,400);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Codex Constellation fixture imports authority session"); state.spiritPressure = 5;
                actor.inventory[0] = new Item(ModContent.ItemType<ArchiveStarCodex>());
                var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); if (record > 0) growth.TryTransform((DownedBossSystem.EndgameRoute)record);
                return actor;
            }
            void Cast(Player actor,Vector2 aim) => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.CodexConstellation,0,actor.HeldItem.type,aim);
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server })
            foreach (int record in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                var actor = Fixture(authority,record,route,core); var state = actor.GetModPlayer<XianXiaPlayer>();
                int active = core && route == record ? record : 0;
                int cost = active == 0 ? 60 : active == 1 ? 76 : active == 2 ? 90 : 68;
                Vector2 aim = actor.Center + Vector2.UnitX * 300;
                check(ArtifactSkillTransactions.HeldSkill(actor.HeldItem) == ArtifactSkill.CodexConstellation && actor.HeldItem.ModItem.AltFunctionUse(actor),
                    "native awakened codex crossbow exposes Codex Constellation mapping and alternate attack");
                state.spiritualEnergy = cost - 1; actor.altFunctionUse = 2;
                check(!actor.HeldItem.ModItem.CanUseItem(actor),"native codex alternate input requires full active cost"); Cast(actor,aim);
                check(state.spiritualEnergy == cost - 1 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Codex Constellation rejects one below current route energy without projectile or skill cooldown");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost;
                check(actor.HeldItem.ModItem.CanUseItem(actor),"native codex alternate input accepts exact route skill cost");
                int damage = Math.Max(1,(int)(actor.GetWeaponDamage(actor.HeldItem) * (active == 0 ? 1.75f : active == 2 ? 2.75f : 2.25f)));
                Cast(actor,aim); var flames = Main.projectile.Where(p => p.active && !before.Contains(p)).OrderBy(p => p.velocity.Y).ToArray();
                check(flames.Length == 3 && flames.All(p => p.type == ModContent.ProjectileType<ArchiveStarOrb>() && p.damage == damage
                    && p.owner == actor.whoAmI && p.DamageType == DamageClass.Magic && p.timeLeft == 150 && p.penetrate == 4 && p.usesLocalNPCImmunity && p.localNPCHitCooldown == 20 && p.tileCollide),
                    "native Codex Constellation creates exactly three bounded ranged bolts using current Dao or fallback weapon damage");
                check(flames.Length == 3 && Math.Abs(flames[0].velocity.ToRotation() + .24f) < .001f && Math.Abs(flames[1].velocity.ToRotation()) < .001f
                    && Math.Abs(flames[2].velocity.ToRotation() - .24f) < .001f && flames.All(p => Math.Abs(p.velocity.Length() - 10f) < .001f),
                    "native Codex Constellation follows aimed symmetric three-shot fan at ten speed");
                check(state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200 && state.spiritPressure == (active == 3 ? 13 : 5)
                    && actor.statLife == (active == 1 ? 120 : 100),"native Codex Constellation consumes route cost once shares twenty-second cooldown and applies only active route effects");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost; Cast(actor,aim);
                check(state.spiritualEnergy == cost && Main.projectile.Count(p => p.active && !before.Contains(p)) == 3,
                    "native Codex Constellation shared cooldown blocks duplicate cast without energy or extra swords");
                bool charge = true;
                for (int age = 0; age <= 18; age++) {
                    if (age > 0) foreach (var orb in flames) { orb.ModProjectile.AI(); orb.timeLeft--; }
                    charge &= flames.All(p => p.ai[0] == age && (p.ModProjectile.CanDamage() != false) == (age == 18) && p.damage == damage);
                }
                check(charge,"native codex skill retains exactly eighteen harmless charge updates before enabling magic damage");
                actor.dead = true;
                check(flames.All(p => p.ModProjectile.CanDamage() == false),"native Codex Constellation projectiles lose damage immediately when owner dies");
                foreach (var flame in flames) flame.ModProjectile.AI();
                check(flames.All(p => !p.friendly && p.timeLeft <= 6),"native Codex Constellation uses existing owner-bound sticky cancellation");
            }
            foreach (int invalid in new[] { 0,1,2,3,4,5,6,7 }) {
                var actor = Fixture(NetmodeID.Server,0,0,false); var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergy = 100;
                if (invalid == 0) actor.HeldItem.GetGlobalItem<RefinedArtifact>().SetLevel(2);
                if (invalid == 1) actor.inventory[0] = new Item(ModContent.ItemType<WoodgrainFlyingSword>());
                if (invalid == 3) state.cultivationStage = CultivationStage.SpiritSevering;
                if (invalid == 4) Main.hardMode = false;
                if (invalid == 5) NPC.downedPlantBoss = false;
                if (invalid == 6) NPC.downedGolemBoss = false;
                if (invalid == 7) NPC.downedMoonlord = false;
                Cast(actor,invalid == 2 ? new Vector2(float.NaN,0) : actor.Center + Vector2.UnitX);
                check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Codex Constellation rejects unawakened wrong item nonfinite aim or insufficient realm/world progression without spending");
            }
        } finally {
            Clear(); Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; world.LoadWorldData(saved); Main.hardMode = hard; NPC.downedPlantBoss = plant; NPC.downedGolemBoss = golem; NPC.downedMoonlord = moon;
        }
    }
}
