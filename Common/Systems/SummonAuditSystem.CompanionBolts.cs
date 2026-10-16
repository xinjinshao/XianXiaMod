using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditCompanionBolts(Action<bool, string> check, List<NPC> owned, List<Projectile> shots)
    {
        Player oldOwner = Main.player[0]; var oldText = Main.combatText;
        int oldMode = Main.netMode, oldLocal = Main.myPlayer;
        try {
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
            Main.combatText = Enumerable.Range(0, 100).Select(_ => new CombatText { active = true }).ToArray();
            Vector2 center = new Vector2(Main.maxTilesX * 8 + 1400, 160);
            Main.player[0] = new Player { whoAmI = 0, active = true, position = center };
            var source = new EntitySource_Misc("XianXiaCompanionFanAudit");
            int index = NPC.NewNPC(source, (int)center.X, (int)center.Y, NPCID.BlueSlime);
            check(index >= 0 && index < Main.maxNPCs, "native companion fan target allocated");
            NPC target = Main.npc[index]; owned.Add(target);
            target.damage = 0; target.defense = 0; target.life = target.lifeMax = 100000; target.Center = center;
            foreach (int count in new[] { 2, 3 }) {
                Array.Clear(target.immune);
                for (int shot = 0; shot < count; shot++) {
                    int slot = Projectile.NewProjectile(source, center, Vector2.Zero, ModContent.ProjectileType<ContractSpiritBolt>(), 100, 0, 0);
                    check(slot >= 0 && slot < Main.maxProjectiles, "native companion fan shot allocated");
                    Projectile bolt = Main.projectile[slot]; shots.Add(bolt);
                    int before = target.life;
                    bolt.Update(bolt.whoAmI);
                    check(target.life < before, "native each simultaneous contract fan bolt damages the same living target without clearing immunity between shots");
                    check(!bolt.active && bolt.penetrate == 0, "native full contract update consumes the single-hit shot");
                }
            }
            check(target.active && target.life > 0, "native contract fan target survives without kill rewards");
            int expirySlot = Projectile.NewProjectile(source, center + new Vector2(500, -80), Vector2.Zero,
                ModContent.ProjectileType<ContractSpiritBolt>(), 100, 0, 0);
            check(expirySlot >= 0 && expirySlot < Main.maxProjectiles, "native contract expiry shot allocated");
            Projectile expiry = Main.projectile[expirySlot]; shots.Add(expiry);
            for (int frame = 1; frame <= 180; frame++) {
                expiry.Update(expiry.whoAmI);
                check(expiry.active == (frame < 180) && expiry.timeLeft == 180 - frame,
                    "native emitted contract bolt has exactly 180 updates of bounded flight without a maintained summon buff");
            }
            int wallX = (int)(center.X + 300) / 16, wallY = (int)center.Y / 16;
            Tile tile = Main.tile[wallX, wallY]; bool oldTile = tile.HasTile; ushort oldType = tile.TileType;
            try {
                tile.HasTile = true; tile.TileType = TileID.Stone;
                int wallSlot = Projectile.NewProjectile(source, new Vector2(wallX * 16 - 24, wallY * 16 + 8), new Vector2(8, 0),
                    ModContent.ProjectileType<ContractSpiritBolt>(), 100, 0, 0);
                check(wallSlot >= 0 && wallSlot < Main.maxProjectiles, "native terrain test contract shot allocated");
                Projectile blocked = Main.projectile[wallSlot]; shots.Add(blocked);
                for (int frame = 0; frame < 6 && blocked.active; frame++) blocked.Update(blocked.whoAmI);
                check(!blocked.active, "native contract shot collides with actual solid terrain during flight");
            } finally { tile.HasTile = oldTile; tile.TileType = oldType; }
        } finally { Main.player[0] = oldOwner; Main.combatText = oldText; Main.netMode = oldMode; Main.myPlayer = oldLocal; }
        check(ReferenceEquals(Main.player[0], oldOwner) && Main.combatText == oldText && Main.netMode == oldMode && Main.myPlayer == oldLocal,
            "native contract fan audit restores owner damage-text and engine contexts");
    }
}
