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
    private static void AuditDischargeSkill(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode,local = Main.myPlayer;
        var random = Main.rand;
        var tiles = new System.Collections.Generic.List<(int X,int Y,bool HasTile,ushort Type)>();
        var text = Main.combatText; var before = Main.projectile.Where(p => p.active).ToHashSet();
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        try {
            Main.rand = new Terraria.Utilities.UnifiedRandom(402);
            for (int x = Main.maxTilesX / 2 - 6; x <= Main.maxTilesX / 2 + 6; x++) for (int y = 8; y <= 30; y++) {
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
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Array Discharge fixture imports authority session"); state.spiritPressure = 5;
                actor.inventory[0] = new Item(ModContent.ItemType<ThunderTalismanArrayPlate>());
                var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); if (record > 0) growth.TryTransform((DownedBossSystem.EndgameRoute)record);
                int index = Projectile.NewProjectile(new Terraria.DataStructures.EntitySource_ItemUse_WithAmmo(actor,actor.HeldItem,0),actor.Center,Vector2.Zero,
                    ModContent.ProjectileType<ThunderTalismanArray>(),46,0,actor.whoAmI);
                check(index >= 0 && index < Main.maxProjectiles,"native discharge fixture creates registered owner-bound thunder field");
                actor.ownedProjectileCounts[Main.projectile[index].type] = 1; state.arrayDeploymentCooldown = 480;
                return actor;
            }
            void Cast(Player actor,Vector2 aim) => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.ArrayDischarge,0,actor.HeldItem.type,aim);
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server })
            foreach (int record in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                var actor = Fixture(authority,record,route,core); var state = actor.GetModPlayer<XianXiaPlayer>();
                int active = core && route == record ? record : 0;
                int cost = active == 0 ? 30 : active == 1 ? 40 : active == 2 ? 48 : 34;
                Vector2 aim = actor.Center + Vector2.UnitX * 300;
                check(ArtifactSkillTransactions.HeldSkill(actor.HeldItem) == ArtifactSkill.ArrayDischarge && actor.HeldItem.ModItem.AltFunctionUse(actor),
                    "native awakened thunder array crossbow exposes Array Discharge mapping and alternate attack");
                state.spiritualEnergy = cost - 1; actor.altFunctionUse = 2;
                check(!actor.HeldItem.ModItem.CanUseItem(actor),"native thunder array alternate input requires full active cost"); Cast(actor,aim);
                check(state.spiritualEnergy == cost - 1 && state.activeSkillCooldown == 0 && Main.projectile.Count(p => p.active && !before.Contains(p)) <= 1,
                    "native Array Discharge rejects one below current route energy without projectile or skill cooldown");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost;
                check(actor.HeldItem.ModItem.CanUseItem(actor),"native thunder array alternate input accepts exact route skill cost");
                int damage = Math.Max(1,(int)(actor.GetWeaponDamage(actor.HeldItem) * (active == 0 ? 1.75f : active == 2 ? 2.75f : 2.25f)));
                Cast(actor,aim); var flames = Main.projectile.Where(p => p.active && !before.Contains(p) && p.type == ModContent.ProjectileType<MinorThunderboltProjectile>()).OrderBy(p => p.Center.X).ToArray();
                check(flames.Length == 3 && flames.All(p => p.type == ModContent.ProjectileType<MinorThunderboltProjectile>() && p.damage == damage
                    && p.owner == actor.whoAmI && p.DamageType == DamageClass.Magic && p.timeLeft == 180 && p.penetrate == 1 && p.tileCollide),
                    "native Array Discharge creates exactly three bounded ranged bolts using current Dao or fallback weapon damage");
                check(flames.Length == 3 && flames.Select((p,index) => Vector2.DistanceSquared(p.Center,actor.Center + new Vector2((index - 1) * 64,-220)) < .01f && p.velocity == Vector2.UnitY * 13).All(ok => ok),
                    "native array discharge creates three magic strikes over the existing field with canonical spacing height and fall speed");
                check(state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200 && state.spiritPressure == (active == 3 ? 13 : 5)
                    && actor.statLife == (active == 1 ? 120 : 100),"native Array Discharge consumes route cost once shares twenty-second cooldown and applies only active route effects");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost; Cast(actor,aim);
                check(state.spiritualEnergy == cost && Main.projectile.Count(p => p.active && !before.Contains(p)) == 4,
                    "native Array Discharge shared cooldown blocks duplicate cast without energy or extra swords");
                foreach (var bolt in flames) bolt.ModProjectile.AI();
                check(flames.All(p => p.active && p.ModProjectile.CanDamage() != false),"native thunder array skill bolts preserve normal bounded flight and owner eligibility");
                var field = Main.projectile.Single(p => p.active && !before.Contains(p) && p.type == ModContent.ProjectileType<ThunderTalismanArray>());
                check(field.timeLeft == 240 && state.arrayDeploymentCooldown == 480,"native discharge does not reset field lifetime or ordinary deployment cooldown");
                field.timeLeft = 225; field.ModProjectile.AI();
                check(Main.projectile.Count(p => p.active && !before.Contains(p)) == 5,"native discharge preserves the next normal single-bolt array wave");
                field.ModProjectile.AI();
                check(Main.projectile.Count(p => p.active && !before.Contains(p)) == 5,"native repeated array boundary after discharge cannot duplicate normal wave");
                actor.dead = true;
                check(flames.All(p => p.ModProjectile.CanDamage() == false),"native Array Discharge projectiles lose damage immediately when owner dies");
                foreach (var flame in flames) flame.ModProjectile.AI();
                check(flames.All(p => !p.friendly && p.timeLeft <= 6),"native Array Discharge uses existing owner-bound sticky cancellation");
            }
            foreach (int invalid in new[] { 0,1,2,3,4,5 }) {
                var actor = Fixture(NetmodeID.Server,0,0,false); var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergy = 100;
                if (invalid == 0) actor.HeldItem.GetGlobalItem<RefinedArtifact>().SetLevel(2);
                if (invalid == 1) actor.inventory[0] = new Item(ModContent.ItemType<WoodgrainFlyingSword>());
                var field = Main.projectile.Single(p => p.active && !before.Contains(p));
                if (invalid == 3) field.active = false;
                if (invalid == 4) field.owner = 1;
                if (invalid == 5) field.timeLeft = 241;
                Cast(actor,invalid == 2 ? new Vector2(float.NaN,0) : actor.Center + Vector2.UnitX);
                check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && Main.projectile.Count(p => p.active && !before.Contains(p)) <= 1,
                    "native Array Discharge rejects unawakened wrong held item or nonfinite aim without spending");
            }
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) {
                var actor = Fixture(authority,0,0,false); var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergy = 100;
                int x = (int)(actor.Center.X / 16), y = (int)((actor.Center.Y - 220) / 16);
                Tile tile = Main.tile[x,y]; tile.HasTile = true; tile.TileType = TileID.Stone;
                Cast(actor,actor.Center);
                check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && Main.projectile.Count(p => p.active && !before.Contains(p)) == 1,
                    "native array discharge rejects blocked bolt headroom and preserves field and energy");
                tile.HasTile = false;
            }
        } finally {
            foreach (var old in tiles) { Tile tile = Main.tile[old.X,old.Y]; tile.HasTile = old.HasTile; tile.TileType = old.Type; }
            Main.rand = random; Clear(); Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; world.LoadWorldData(saved);
        }
    }
}
