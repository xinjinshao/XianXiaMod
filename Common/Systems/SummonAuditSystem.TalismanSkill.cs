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
    private static void AuditTalismanSkill(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone();
        int mode = Main.netMode, local = Main.myPlayer; var random = Main.rand; var text = Main.combatText;
        var before = Main.projectile.Where(p => p.active).ToHashSet();
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        try {
            Main.rand = new Terraria.Utilities.UnifiedRandom(403);
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(), State = 0 };
            foreach (int authority in new[] { NetmodeID.SinglePlayer, NetmodeID.Server })
            foreach (int record in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true })
            foreach (int ammoType in new[] { ItemID.WoodenArrow, ModContent.ItemType<CinnabarTalismanArrow>(), ItemID.EndlessQuiver }) {
                Clear(); Main.netMode = authority; Main.myPlayer = authority == NetmodeID.Server ? Main.maxPlayers : 0;
                world.LoadWorldData(new TagCompound { ["chosenRoute"] = route }); if (core) DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                var actor = new Player { active = true, whoAmI = 0, selectedItem = 0, statLife = 100, statLifeMax2 = 200 }; Main.player[0] = actor;
                actor.Center = new Vector2(Main.maxTilesX * 8,400);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Rune Shot initializes authority session"); state.spiritPressure = 5;
                actor.inventory[0] = new Item(ModContent.ItemType<TalismanCrossbow>());
                var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); if (record > 0) growth.TryTransform((DownedBossSystem.EndgameRoute)record);
                actor.inventory[54] = new Item(ammoType,ammoType == ItemID.EndlessQuiver ? 1 : 5);
                Item ammo = actor.inventory[54]; int initial = ammo.stack;
                check(actor.PickAmmo(actor.HeldItem,out int type,out float speed,out int damage,out float knockback,out int used,true) && used == ammoType && ammo.stack == initial,
                    "native Rune Shot preview uses native selected arrow without consumption");
                actor.altFunctionUse = 2;
                check(!actor.HeldItem.ModItem.CanConsumeAmmo(ammo,actor),"native alternate animation cannot consume a second arrow");
                int active = core && route == record ? record : 0, cost = active == 0 ? 18 : active == 1 ? 24 : active == 2 ? 30 : 20;
                Vector2 aim = actor.Center + Vector2.UnitX * 300;
                void Cast() => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.TalismanShot,0,actor.HeldItem.type,aim);
                state.spiritualEnergy = cost - 1; Cast();
                check(state.spiritualEnergy == cost - 1 && ammo.stack == initial && !Main.projectile.Any(p => p.active && !before.Contains(p)),"native Rune Shot insufficient energy preserves arrows");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost; Cast();
                var shots = Main.projectile.Where(p => p.active && !before.Contains(p)).ToArray();
                int expected = Math.Max(1,(int)(damage * (active == 0 ? 1.75f : active == 2 ? 2.75f : 2.25f)));
                check(shots.Length == 1 && shots[0].type == type && shots[0].damage == expected && shots[0].owner == 0
                    && Math.Abs(shots[0].velocity.Length() - speed * 1.25f) < .001f,"native Rune Shot preserves selected projectile and applies route damage and speed");
                check(ammo.stack == initial - (ammo.consumable ? 1 : 0) && !state.ApplyingArtifactAmmo,"native Rune Shot consumes native ammunition exactly once and retains infinite quiver");
                check(state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200 && state.spiritPressure == (active == 3 ? 13 : 5) && actor.statLife == (active == 1 ? 120 : 100),
                    "native Rune Shot applies exact route fee shared cooldown healing and pressure");
                state.skillRequestCooldown = 0; state.spiritualEnergy = cost; int remaining = ammo.stack; Cast();
                check(ammo.stack == remaining && state.spiritualEnergy == cost && Main.projectile.Count(p => p.active && !before.Contains(p)) == 1,"native Rune Shot cooldown rejects duplicate without ammo consumption");
                Clear(); state.activeSkillCooldown = 0; state.skillRequestCooldown = 0; actor.inventory[54].TurnToAir(); Cast();
                check(state.spiritualEnergy == cost && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),"native Rune Shot requires compatible arrows");
                actor.altFunctionUse = 0;
                check(actor.HeldItem.ModItem.CanConsumeAmmo(ammo,actor),"native ordinary shooting retains native ammo consumption");
            }
            check(ModLoader.GetMod("XianXia").GetContent<ModItem>().Select(sample => new Item(sample.Type)).Where(RefinedArtifact.SupportsRefinement).All(item => ArtifactSkillTransactions.HeldSkill(item).HasValue),
                "native all nineteen refinable weapons have a registered active skill");
        } finally {
            Clear(); Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local;
            Main.rand = random; Main.combatText = text; world.LoadWorldData(saved);
        }
    }
}
