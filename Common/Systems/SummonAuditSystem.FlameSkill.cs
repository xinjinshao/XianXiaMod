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
    private static void AuditFlameSkill(Action<bool,string> check)
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
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Flame Fan fixture imports authority session"); state.spiritPressure = 5;
                actor.inventory[0] = new Item(ModContent.ItemType<CinnabarTalismanFlameItem>());
                var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); if (record > 0) growth.TryTransform((DownedBossSystem.EndgameRoute)record);
                return actor;
            }
            void Cast(Player actor,Vector2 aim) => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.FlameFan,0,actor.HeldItem.type,aim);
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server })
            foreach (int record in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                var actor = Fixture(authority,record,route,core); var state = actor.GetModPlayer<XianXiaPlayer>();
                int active = core && route == record ? record : 0;
                int cost = active == 0 ? 18 : active == 1 ? 24 : active == 2 ? 30 : 20;
                Vector2 aim = actor.Center + Vector2.UnitX * 300;
                check(ArtifactSkillTransactions.HeldSkill(actor.HeldItem) == ArtifactSkill.FlameFan && actor.HeldItem.ModItem.AltFunctionUse(actor),
                    "native awakened cinnabar exposes Flame Fan mapping and alternate attack");
                state.spiritualEnergy = cost - 1; Cast(actor,aim);
                check(state.spiritualEnergy == cost - 1 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Flame Fan rejects one below current route energy without projectile or skill cooldown");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost;
                int damage = Math.Max(1,(int)(actor.GetWeaponDamage(actor.HeldItem) * (active == 0 ? 1.5f : active == 2 ? 2.5f : 2f)));
                Cast(actor,aim); var flames = Main.projectile.Where(p => p.active && !before.Contains(p)).OrderBy(p => p.velocity.Y).ToArray();
                check(flames.Length == 3 && flames.All(p => p.type == ModContent.ProjectileType<CinnabarTalismanFlame>() && p.damage == damage
                    && p.owner == actor.whoAmI && p.DamageType == DamageClass.Magic && p.timeLeft == 180 && p.penetrate == 1 && p.tileCollide),
                    "native Flame Fan creates exactly three bounded magic flames using current Dao or fallback weapon damage");
                check(flames.Length == 3 && Math.Abs(flames[0].velocity.ToRotation() + .18f) < .001f && Math.Abs(flames[1].velocity.ToRotation()) < .001f
                    && Math.Abs(flames[2].velocity.ToRotation() - .18f) < .001f && flames.All(p => Math.Abs(p.velocity.Length() - 14f) < .001f),
                    "native Flame Fan follows aimed symmetric three-shot fan at fourteen speed");
                check(state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200 && state.spiritPressure == (active == 3 ? 13 : 5)
                    && actor.statLife == (active == 1 ? 120 : 100),"native Flame Fan consumes route cost once shares twenty-second cooldown and applies only active route effects");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost; Cast(actor,aim);
                check(state.spiritualEnergy == cost && Main.projectile.Count(p => p.active && !before.Contains(p)) == 3,
                    "native Flame Fan shared cooldown blocks duplicate cast without energy or extra flames");
                actor.dead = true;
                check(flames.All(p => p.ModProjectile.CanDamage() == false),"native Flame Fan projectiles lose damage immediately when owner dies");
                foreach (var flame in flames) flame.ModProjectile.AI();
                check(flames.All(p => !p.friendly && p.timeLeft <= 6),"native Flame Fan uses existing owner-bound sticky cancellation");
            }
            foreach (int invalid in new[] { 0,1,2 }) {
                var actor = Fixture(NetmodeID.Server,0,0,false); var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergy = 100;
                if (invalid == 0) actor.HeldItem.GetGlobalItem<RefinedArtifact>().SetLevel(2);
                if (invalid == 1) actor.inventory[0] = new Item(ModContent.ItemType<WoodgrainFlyingSword>());
                Cast(actor,invalid == 2 ? new Vector2(float.NaN,0) : actor.Center + Vector2.UnitX);
                check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Flame Fan rejects unawakened wrong held item or nonfinite aim without spending");
            }
        } finally {
            Clear(); Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; world.LoadWorldData(saved);
        }
    }
}
