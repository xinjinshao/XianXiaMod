using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem {
    private void AuditMalformedProjectileOwners(Action<bool,string> check) {
        var previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer; var clients = (RemoteClient[])Netplay.Clients.Clone(); var connection = Netplay.Connection;
        var before = Main.projectile.Where(p => p.active).ToHashSet(); var text = Main.combatText; var random = Main.rand;
        string[] names = { "CloudpiercerSwordProjectile","CloudWispProjectile","MoonboneShardProjectile","ThunderSwordProjectile","MinorThunderboltProjectile","ThunderCaseStrikeProjectile",
            "SpiritBolt","SpiritBoltProjectile","ContractSpiritBolt","StarEclipseSplitBolt","CinnabarTalismanFlame","SectMechanismBolt","HeavenLawBolt","StarCalamityMechanismBolt","CinnabarArrowProjectile","HeavenTabletWardProjectile","ArchiveStarOrb","DecreeJudgementBeam",
            "FormlessSwordWheelProjectile","FurnaceHammerProjectile","FurnaceImpactBurst","GreenwoodArrayField","ThunderTalismanArray","MedicineCauldronField","MedicineSpiritBolt",
            "SmallArtifactSpirit","FurnaceAshSpirit","StarAbyssSpirit","NascentSoulSpirit","CelestialPuppetSpirit","ArchivedSoulSpirit","SmallTabletPetProjectile","BlackFurnaceIronGolemPetProjectile" };
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        try {
            Main.rand = new Terraria.Utilities.UnifiedRandom(288); Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            Netplay.Connection = new RemoteServer { Socket = new AuditOutputSocket(false) };
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            foreach (string name in names) foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient,NetmodeID.Server }) foreach (int corruption in Enumerable.Range(0,4)) {
                Clear(); Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0; var actor = new Player { whoAmI = 0, active = true }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
                int index = Projectile.NewProjectile(actor.GetSource_FromThis(),actor.Center,Vector2.Zero,Mod.Find<ModProjectile>(name).Type,60,1,0);
                check(index >= 0 && index < Main.maxProjectiles,"native malformed owner fixture creates registered " + name); var p = Main.projectile[index];
                Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
                if (corruption == 0) p.owner = -1; else if (corruption == 1) p.owner = 999; else if (corruption == 2) p.owner = Main.maxPlayers; else Main.player[0] = null;
                p.timeLeft = corruption % 2; new PlayerProjectileOwnerCleanupSystem().PreUpdateProjectiles(); if (p.active) p.Update(p.whoAmI);
                check(!p.active && !p.friendly && !p.hostile && p.hide && p.velocity == Vector2.Zero,"native malformed owner pre-update guard retires without unsafe expiry: " + name);
                var guard = p.GetGlobalProjectile<ServerPlayerProjectileSync>(); check(guard.CanDamage(p) == false,"native malformed owner global damage guard remains latched: " + name);
                Main.player[0] = actor; p.owner = 0; p.active = true; p.friendly = true; p.timeLeft = 1; new PlayerProjectileOwnerCleanupSystem().PreUpdateProjectiles(); if (p.active) p.Update(p.whoAmI);
                check(!p.active && guard.CanDamage(p) == false,"native repaired slot and forced reactivation cannot resume retired entity: " + name);
                check(!Main.projectile.Any(child => child.active && !before.Contains(child)),"native malformed owner never releases children during expiry: " + name);
            }
            Clear(); Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0; var owner = new Player { whoAmI = 0, active = true }; owner.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = owner;
            int parent = Projectile.NewProjectile(owner.GetSource_FromThis(),owner.Center,Vector2.UnitX,Mod.Find<ModProjectile>("CloudpiercerSwordProjectile").Type,60,1,0); Main.projectile[parent].timeLeft = 1; new PlayerProjectileOwnerCleanupSystem().PreUpdateProjectiles(); Main.projectile[parent].Update(parent);
            check(!Main.projectile[parent].active && Main.projectile.Count(p => p.active && !before.Contains(p) && p.type == Mod.Find<ModProjectile>("CloudWispProjectile").Type) == 1,"native healthy expiry still runs ordinary OnKill and releases one wisp");
        } finally { Clear(); Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; Main.rand = random; Netplay.Connection = connection; clients.CopyTo(Netplay.Clients,0); }
    }
}
