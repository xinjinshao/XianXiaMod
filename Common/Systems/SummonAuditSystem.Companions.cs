using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private void AuditCompanionContact(Action<bool, string> check)
    {
        Player previous = Main.player[0];
        int local = Main.myPlayer;
        try {
            Main.myPlayer = Main.maxPlayers;
            foreach (string name in new[] { "SmallArtifactSpirit", "FurnaceAshSpirit", "StarAbyssSpirit",
                "NascentSoulSpirit", "CelestialPuppetSpirit", "ArchivedSoulSpirit" }) {
                var owner = new Player { whoAmI = 0, active = true, position = new Vector2(Main.maxTilesX * 8, 160) };
                Main.player[0] = owner;
                string buffName = name switch { "StarAbyssSpirit" => "StarAbyssSpiritBuff", "NascentSoulSpirit" => "NascentSoulSpiritBuff",
                    "CelestialPuppetSpirit" => "CelestialPuppetSpiritBuff", "ArchivedSoulSpirit" => "ArchivedSoulSpiritBuff",
                    "FurnaceAshSpirit" => "FurnaceAshSpiritBuff", _ => "SmallArtifactSpiritBuff" };
                int buff = Mod.Find<ModBuff>(buffName).Type;
                owner.AddBuff(buff, 18000);
                var projectile = new Projectile(); projectile.SetDefaults(Mod.Find<ModProjectile>(name).Type);
                projectile.owner = 0; projectile.Center = owner.Center; projectile.ai[1] = 18;
                var spirit = (SmallArtifactSpirit)projectile.ModProjectile;
                var target = new NPC(); target.SetDefaults(NPCID.Zombie); target.active = true; target.Center = owner.Center + new Vector2(80, 0);
                bool contact = name is "SmallArtifactSpirit" or "FurnaceAshSpirit" or "CelestialPuppetSpirit";
                check(spirit.CanHitNPC(target) == (contact ? (bool?)null : false), "native registered spirit preserves contact versus ranged role");
                int wallX = (int)(owner.Center.X + 40) / 16, wallY = (int)owner.Center.Y / 16 - 2;
                var oldWall = new (bool HasTile, ushort Type)[5];
                try {
                    for (int i = 0; i < oldWall.Length; i++) {
                        Tile tile = Main.tile[wallX, wallY + i]; oldWall[i] = (tile.HasTile, tile.TileType);
                        tile.HasTile = true; tile.TileType = TileID.Stone;
                    }
                    check(spirit.CanHitNPC(target) is false, "native terrain obstruction immediately blocks companion contact");
                } finally {
                    for (int i = 0; i < oldWall.Length; i++) {
                        Tile tile = Main.tile[wallX, wallY + i]; tile.HasTile = oldWall[i].HasTile; tile.TileType = oldWall[i].Type;
                    }
                }
                check(spirit.CanHitNPC(target) == (contact ? (bool?)null : false), "native terrain restoration preserves original companion eligibility");
                owner.ClearBuff(buff);
                check(spirit.CanHitNPC(target) is false, "native canceled buff blocks hit before next companion AI");
                owner.AddBuff(buff, 18000); owner.dead = true;
                check(spirit.CanHitNPC(target) is false, "native owner death blocks stale companion contact");
                owner.dead = false; target.Center = owner.Center + new Vector2(1200, 0);
                check(spirit.CanHitNPC(target) is false, "native companion contact respects owner attack range");
                target.Center = new Vector2(float.NaN, 0);
                check(spirit.CanHitNPC(target) is false, "native companion refuses nonfinite target geometry");
                Main.player[0] = null;
                check(spirit.CanHitNPC(target) is false, "native companion safely refuses missing owner slot");
            }
        } finally { Main.player[0] = previous; Main.myPlayer = local; }
        check(ReferenceEquals(Main.player[0], previous) && Main.myPlayer == local, "native companion audit restores owner and local-player context");
    }
}
