using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Content.Items.Weapons;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditFlameLifecycle(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode,local = Main.myPlayer;
        var text = Main.combatText; var random = Main.rand; var before = Main.projectile.Where(p => p.active).ToHashSet();
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        var tiles = new List<(int X,int Y,AuditTileSnapshot Saved)>(); NPC victim = null;
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        void RestoreTiles() { foreach (var prior in tiles) prior.Saved.Restore(Main.tile[prior.X,prior.Y]); tiles.Clear(); }
        try {
            Main.rand = new UnifiedRandom(376); Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            world.LoadWorldData(new TagCompound());
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i,Socket = new AuditOutputSocket(),State = 0 };
            Vector2 corridor = Vector2.Zero;
            for (int offset = 2000; offset <= 6000; offset += 800) {
                Vector2 candidate = new Vector2(Main.maxTilesX * 8 + offset,800); var bounds = new Rectangle((int)candidate.X - 40,300,2640,1000);
                if (bounds.Right >= Main.maxTilesX * 16 - 100 || Collision.SolidCollision(new Vector2(bounds.X,bounds.Y),bounds.Width,bounds.Height)
                    || Main.npc.Any(npc => npc != null && npc.active && npc.Hitbox.Intersects(bounds))) continue;
                corridor = candidate; break;
            }
            check(corridor != Vector2.Zero,"native Flame Fan lifecycle finds clear real-world corridor for all three complete flights");
            (Player Actor,Projectile[] Flames) Cast(int authority) {
                Clear(); Main.netMode = authority; Main.myPlayer = authority == NetmodeID.Server ? Main.maxPlayers : 0;
                var actor = new Player { active = true,whoAmI = 0,selectedItem = 0,statLife = 100,statLifeMax2 = 100 }; Main.player[0] = actor; actor.Center = corridor;
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Flame Fan flight fixture initializes owner session"); state.spiritualEnergy = 100;
                actor.inventory[0] = new Item(ModContent.ItemType<CinnabarTalismanFlameItem>()); var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken();
                ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.FlameFan,0,actor.HeldItem.type,corridor + Vector2.UnitX * 300);
                var flames = Main.projectile.Where(p => p.active && !before.Contains(p)).OrderBy(p => p.velocity.Y).ToArray();
                check(flames.Length == 3,"native lifecycle fixture uses actual Flame Fan transaction rather than standalone projectile spawn"); return (actor,flames);
            }
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) {
                var cast = Cast(authority); bool flight = true; string firstFailure = "";
                for (int tick = 1; tick <= 180; tick++) foreach (var flame in cast.Flames) {
                    flame.Update(flame.whoAmI);
                    bool expected = flame.active == (tick < 180) && flame.timeLeft == 180 - tick && float.IsFinite(flame.position.X) && float.IsFinite(flame.position.Y);
                    if (!expected && firstFailure.Length == 0) firstFailure = $"mode={authority}, tick={tick}, active={flame.active}, life={flame.timeLeft}, position={flame.position}, owner={cast.Actor.Center}, canDamage={flame.ModProjectile.CanDamage()}";
                    flight &= expected;
                }
                check(flight,"native Flame Fan three projectiles complete 180 real updates to natural expiry in SP and server authority: " + firstFailure);
                cast = Cast(authority); int wx = (int)(corridor.X + 100) / 16, wy = (int)corridor.Y / 16;
                for (int x = wx; x < wx + 2; x++) for (int y = wy - 4; y <= wy + 4; y++) {
                    Tile wall = Main.tile[x,y]; tiles.Add((x,y,AuditTileSnapshot.Capture(wall))); wall.HasTile = true; wall.TileType = TileID.Stone; wall.IsHalfBlock = false; wall.Slope = Terraria.ID.SlopeType.Solid; wall.IsActuated = false;
                }
                for (int tick = 0; tick < 12; tick++) foreach (var flame in cast.Flames) if (flame.active) flame.Update(flame.whoAmI);
                check(cast.Flames.All(p => !p.active),"native Flame Fan three real flight paths collide with solid tiles and stop instead of passing through walls"); RestoreTiles();
                check(!Collision.SolidCollision(new Vector2(wx * 16,(wy - 4) * 16),32,144),"native Flame Fan wall fixture restores clear original terrain after collision test");
                cast = Cast(authority); cast.Actor.dead = true;
                check(cast.Flames.All(p => p.ModProjectile.CanDamage() == false),"native Flame Fan owner death suppresses damage before flight update");
                for (int tick = 0; tick < 6; tick++) foreach (var flame in cast.Flames) if (flame.active) flame.Update(flame.whoAmI);
                check(cast.Flames.All(p => !p.active),"native Flame Fan owner death ends all three projectiles within six real updates");
            }
            int targetSlot = NPC.NewNPC(new Terraria.DataStructures.EntitySource_Misc("FlameHitAudit"),(int)corridor.X,800,NPCID.Zombie);
            if (targetSlot >= Main.maxNPCs) throw new InvalidOperationException("Missing native Flame Fan hit target slot");
            victim = Main.npc[targetSlot]; victim.lifeMax = 10000; victim.damage = victim.defense = 0; victim.noGravity = true;
            foreach (int shot in new[] { 0,1,2 }) foreach (bool immune in new[] { false,true }) {
                var cast = Cast(NetmodeID.SinglePlayer); var flame = cast.Flames[shot]; foreach (var other in cast.Flames) if (other != flame) other.active = false;
                victim.active = true; victim.life = 10000; victim.Center = corridor + Vector2.UnitX * 100; victim.velocity = Vector2.Zero;
                Array.Clear(victim.immune); Array.Clear(victim.buffType); Array.Clear(victim.buffTime); victim.buffImmune[BuffID.OnFire3] = immune;
                flame.Center = victim.Center; flame.velocity = Vector2.Zero; flame.Update(flame.whoAmI); int buff = victim.FindBuffIndex(BuffID.OnFire3);
                check(victim.life < 10000 && !flame.active && (immune ? buff < 0 : buff >= 0 && victim.buffTime[buff] == 180),
                    "native actual Flame Fan NPC collision deals damage consumes sole penetration and applies three-second burn respecting immunity");
            }
        } finally {
            RestoreTiles(); Clear(); if (victim != null) { victim.damage = 0; victim.active = false; }
            Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; Main.rand = random; world.LoadWorldData(saved);
        }
    }
}
