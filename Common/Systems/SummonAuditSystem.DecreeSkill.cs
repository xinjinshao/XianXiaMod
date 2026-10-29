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
    private static void AuditDecreeSkill(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode,local = Main.myPlayer;
        var text = Main.combatText; var before = Main.projectile.Where(p => p.active).ToHashSet();
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        var tiles = new System.Collections.Generic.List<(int X,int Y,bool HasTile,ushort Type)>();
        try {
            for (int x = Main.maxTilesX / 2 - 5; x <= Main.maxTilesX / 2 + 42; x++) for (int y = 19; y <= 32; y++) {
                Tile tile = Main.tile[x,y]; tiles.Add((x,y,tile.HasTile,tile.TileType)); tile.HasTile = false;
            }
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i,Socket = new AuditOutputSocket(),State = 0 };
            Player Fixture(int authority,int record,int route,bool core) {
                Clear(); Main.netMode = authority; Main.myPlayer = authority == NetmodeID.Server ? Main.maxPlayers : 0;
                world.LoadWorldData(new TagCompound { ["chosenRoute"] = route }); if (core) DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                var actor = new Player { active = true,whoAmI = 0,selectedItem = 0,statLife = 100,statLifeMax2 = 200 }; Main.player[0] = actor;
                actor.Center = new Vector2(Main.maxTilesX * 8,400);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Decree Triad fixture imports authority session"); state.spiritPressure = 5;
                actor.inventory[0] = new Item(ModContent.ItemType<BrokenHeavenDecree>());
                var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); if (record > 0) growth.TryTransform((DownedBossSystem.EndgameRoute)record);
                return actor;
            }
            void Cast(Player actor,Vector2 aim) => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.DecreeTriad,0,actor.HeldItem.type,aim);
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server })
            foreach (int record in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                var actor = Fixture(authority,record,route,core); var state = actor.GetModPlayer<XianXiaPlayer>();
                int active = core && route == record ? record : 0;
                int cost = active == 0 ? 36 : active == 1 ? 48 : active == 2 ? 60 : 40;
                Vector2 aim = actor.Center + Vector2.UnitX * 300;
                check(ArtifactSkillTransactions.HeldSkill(actor.HeldItem) == ArtifactSkill.DecreeTriad && actor.HeldItem.ModItem.AltFunctionUse(actor),
                    "native awakened decree crossbow exposes Decree Triad mapping and alternate attack");
                state.spiritualEnergy = cost - 1; actor.altFunctionUse = 2;
                check(!actor.HeldItem.ModItem.CanUseItem(actor),"native decree alternate input requires full active cost"); Cast(actor,aim);
                check(state.spiritualEnergy == cost - 1 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Decree Triad rejects one below current route energy without projectile or skill cooldown");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost;
                check(actor.HeldItem.ModItem.CanUseItem(actor),"native decree alternate input accepts exact route skill cost");
                int damage = Math.Max(1,(int)(actor.GetWeaponDamage(actor.HeldItem) * (active == 0 ? 1.5f : active == 2 ? 2.5f : 2f)));
                Cast(actor,aim); var flames = Main.projectile.Where(p => p.active && !before.Contains(p)).OrderBy(p => p.Center.X).ToArray();
                check(flames.Length == 3 && flames.All(p => p.type == ModContent.ProjectileType<DecreeJudgementBeam>() && p.damage == damage
                    && p.owner == actor.whoAmI && p.DamageType == DamageClass.Magic && p.timeLeft == 36 && p.penetrate == 8 && !p.tileCollide && p.usesLocalNPCImmunity && p.localNPCHitCooldown == 8),
                    "native Decree Triad creates exactly three bounded ranged bolts using current Dao or fallback weapon damage");
                check(flames.Length == 3 && flames.Select((p,index) => Vector2.DistanceSquared(p.Center,aim + new Vector2((index - 1) * 80,0)) < .01f && p.velocity == Vector2.Zero).All(ok => ok),
                    "native Decree Triad places three stationary visible pillars eighty pixels apart at the aimed focus");
                check(state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200 && state.spiritPressure == (active == 3 ? 13 : 5)
                    && actor.statLife == (active == 1 ? 120 : 100),"native Decree Triad consumes route cost once shares twenty-second cooldown and applies only active route effects");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost; Cast(actor,aim);
                check(state.spiritualEnergy == cost && Main.projectile.Count(p => p.active && !before.Contains(p)) == 3,
                    "native Decree Triad shared cooldown blocks duplicate cast without energy or extra swords");
                bool timing = true;
                for (int remaining = 36; remaining >= 1; remaining--) foreach (var beam in flames) {
                    beam.timeLeft = remaining; beam.ModProjectile.AI(); timing &= beam.ModProjectile.CanDamage() == (remaining < 18) && beam.damage == damage;
                }
                check(timing,"native Decree Triad preserves full warning delay and last-seventeen-tick damage window");
                actor.dead = true;
                check(flames.All(p => p.ModProjectile.CanDamage() == false),"native Decree Triad projectiles lose damage immediately when owner dies");
                foreach (var flame in flames) flame.ModProjectile.AI();
                check(flames.All(p => !p.friendly && p.timeLeft <= 6),"native Decree Triad uses existing owner-bound sticky cancellation");
            }
            foreach (int invalid in new[] { 0,1,2 }) {
                var actor = Fixture(NetmodeID.Server,0,0,false); var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergy = 100;
                if (invalid == 0) actor.HeldItem.GetGlobalItem<RefinedArtifact>().SetLevel(2);
                if (invalid == 1) actor.inventory[0] = new Item(ModContent.ItemType<WoodgrainFlyingSword>());
                Cast(actor,invalid == 2 ? new Vector2(float.NaN,0) : actor.Center + Vector2.UnitX);
                check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Decree Triad rejects unawakened wrong held item or nonfinite aim without spending");
            }
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) {
                var actor = Fixture(authority,0,0,false); var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergy = 100;
                Vector2 aim = actor.Center + Vector2.UnitX * 300; int x = (int)(aim.X / 16), y = (int)(aim.Y / 16);
                Tile tile = Main.tile[x,y]; tile.HasTile = true; tile.TileType = TileID.Stone;
                Cast(actor,aim);
                check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),
                    "native Decree Triad refuses a solid pillar footprint without cost healing or cooldown");
                tile.HasTile = false;
                actor = Fixture(authority,0,0,false); state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergy = 100;
                Cast(actor,actor.Center + Vector2.UnitX * 10000);
                var pillars = Main.projectile.Where(p => p.active && !before.Contains(p)).OrderBy(p => p.Center.X).ToArray();
                check(pillars.Length == 3 && pillars.All(p => Vector2.DistanceSquared(actor.Center,p.Center) <= 600 * 600)
                    && Math.Abs(pillars[1].Center.X - actor.Center.X - 520) < .01f,"native far Decree aim is clamped while every pillar remains within six hundred pixels");
            }
        } finally {
            foreach (var old in tiles) { Tile tile = Main.tile[old.X,old.Y]; tile.HasTile = old.HasTile; tile.TileType = old.Type; }
            Clear(); Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; world.LoadWorldData(saved);
        }
    }
}
