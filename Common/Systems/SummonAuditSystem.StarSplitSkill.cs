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
    private static void AuditStarSplitSkill(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode,local = Main.myPlayer;
        var text = Main.combatText; var before = Main.projectile.Where(p => p.active).ToHashSet();
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        try {
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i,Socket = new AuditOutputSocket(),State = 0 };
            Player Fixture(int authority,int record,int route,bool core) {
                Clear(); Main.netMode = authority; Main.myPlayer = authority == NetmodeID.Server ? Main.maxPlayers : 0;
                world.LoadWorldData(new TagCompound { ["chosenRoute"] = route }); if (core) DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                var actor = new Player { active = true,whoAmI = 0,selectedItem = 0,statLife = 100,statLifeMax2 = 200 }; Main.player[0] = actor;
                actor.Center = new Vector2(Main.maxTilesX * 8,400);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Star Split Volley fixture imports authority session"); state.spiritPressure = 5;
                actor.inventory[0] = new Item(ModContent.ItemType<StarEclipseArbalest>());
                var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); if (record > 0) growth.TryTransform((DownedBossSystem.EndgameRoute)record);
                return actor;
            }
            void Cast(Player actor,Vector2 aim) => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.StarSplitVolley,0,actor.HeldItem.type,aim);
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server })
            foreach (int record in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                var actor = Fixture(authority,record,route,core); var state = actor.GetModPlayer<XianXiaPlayer>();
                int active = core && route == record ? record : 0;
                int cost = active == 0 ? 30 : active == 1 ? 40 : active == 2 ? 48 : 34;
                Vector2 aim = actor.Center + Vector2.UnitX * 300;
                check(ArtifactSkillTransactions.HeldSkill(actor.HeldItem) == ArtifactSkill.StarSplitVolley && actor.HeldItem.ModItem.AltFunctionUse(actor),
                    "native awakened star-split crossbow exposes Star Split Volley mapping and alternate attack");
                state.spiritualEnergy = cost - 1; actor.altFunctionUse = 2;
                check(!actor.HeldItem.ModItem.CanUseItem(actor),"native star-split alternate input requires full active cost"); Cast(actor,aim);
                check(state.spiritualEnergy == cost - 1 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Star Split Volley rejects one below current route energy without projectile or skill cooldown");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost;
                check(actor.HeldItem.ModItem.CanUseItem(actor),"native star-split alternate input accepts exact route skill cost");
                int damage = Math.Max(1,(int)(actor.GetWeaponDamage(actor.HeldItem) * (active == 0 ? 1.25f : active == 2 ? 2.25f : 1.75f)));
                Cast(actor,aim); var parents = Main.projectile.Where(p => p.active && !before.Contains(p)).OrderBy(p => p.velocity.Y).ToArray();
                check(parents.Length == 3 && parents.All(p => p.type == ModContent.ProjectileType<StarEclipseSplitBolt>() && p.damage == damage
                    && p.owner == actor.whoAmI && p.DamageType == DamageClass.Ranged && p.timeLeft == 160 && p.penetrate == 2 && p.tileCollide),
                    "native Star Split Volley creates exactly three bounded ranged bolts using current Dao or fallback weapon damage");
                check(parents.Length == 3 && Math.Abs(parents[0].velocity.ToRotation() + .2f) < .001f && Math.Abs(parents[1].velocity.ToRotation()) < .001f
                    && Math.Abs(parents[2].velocity.ToRotation() - .2f) < .001f && parents.All(p => Math.Abs(p.velocity.Length() - 14f) < .001f),
                    "native Star Split Volley follows aimed symmetric three-shot fan at fourteen speed");
                check(state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200 && state.spiritPressure == (active == 3 ? 13 : 5)
                    && actor.statLife == (active == 1 ? 120 : 100),"native Star Split Volley consumes route cost once shares twenty-second cooldown and applies only active route effects");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost; Cast(actor,aim);
                check(state.spiritualEnergy == cost && Main.projectile.Count(p => p.active && !before.Contains(p)) == 3,
                    "native Star Split Volley shared cooldown blocks duplicate cast without energy or extra parent bolts");
                foreach (var bolt in parents) bolt.ModProjectile.AI();
                check(parents.All(p => p.active && p.ModProjectile.CanDamage() != false),"native star-split skill bolts preserve normal bounded flight and owner eligibility");
                if (authority == NetmodeID.SinglePlayer) {
                    var target = new NPC(); target.SetDefaults(NPCID.Zombie); target.Center = actor.Center + Vector2.UnitX * 100;
                    foreach (var parent in parents) parent.ModProjectile.OnHitNPC(target, new NPC.HitInfo(), 1);
                    var children = Main.projectile.Where(p => p.active && !before.Contains(p) && p.type == ModContent.ProjectileType<SpiritBolt>()).ToArray();
                    check(children.Length == 6 && children.All(p => p.damage == Math.Max(1,damage / 2) && p.owner == 0 && p.DamageType == DamageClass.Ranged),
                        "native star-split skill owner hits create exactly six ranged half-damage children");
                    int count = Main.projectile.Count(p => p.active && !before.Contains(p));
                    foreach (var child in children) child.ModProjectile.OnHitNPC(target, new NPC.HitInfo(), 1);
                    check(Main.projectile.Count(p => p.active && !before.Contains(p)) == count,"native star-split children cannot recursively split");
                }
                actor.dead = true;
                check(parents.All(p => p.ModProjectile.CanDamage() == false),"native Star Split Volley projectiles lose damage immediately when owner dies");
                foreach (var parent in parents) parent.ModProjectile.AI();
                check(parents.All(p => !p.friendly && p.timeLeft <= 6),"native Star Split Volley uses existing owner-bound sticky cancellation");
            }
            foreach (int invalid in new[] { 0,1,2 }) {
                var actor = Fixture(NetmodeID.Server,0,0,false); var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergy = 100;
                if (invalid == 0) actor.HeldItem.GetGlobalItem<RefinedArtifact>().SetLevel(2);
                if (invalid == 1) actor.inventory[0] = new Item(ModContent.ItemType<WoodgrainFlyingSword>());
                Cast(actor,invalid == 2 ? new Vector2(float.NaN,0) : actor.Center + Vector2.UnitX);
                check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Star Split Volley rejects unawakened wrong held item or nonfinite aim without spending");
            }
        } finally {
            Clear(); Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; world.LoadWorldData(saved);
        }
    }
}
