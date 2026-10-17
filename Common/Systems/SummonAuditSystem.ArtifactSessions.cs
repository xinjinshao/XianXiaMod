using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditArtifactSessions(Action<bool,string> check)
    {
        Player original = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        var clients = (RemoteClient[])Netplay.Clients.Clone(); var before = Main.projectile.Where(p => p.active).ToHashSet();
        int wheel = ModContent.ProjectileType<FormlessSwordWheelProjectile>(), hammer = ModContent.ProjectileType<FurnaceHammerProjectile>(), impact = ModContent.ProjectileType<FurnaceImpactBurst>();
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        Projectile Spawn(Player actor,int type) {
            int index = Projectile.NewProjectile(new EntitySource_Misc("ArtifactOwnerSessionAudit"),actor.Center,Vector2.UnitX * 8,type,59,9,0);
            check(index >= 0 && index < Main.maxProjectiles,"native artifact owner-session registered body creation succeeds"); return Main.projectile[index];
        }
        try {
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (int type in new[] { wheel,hammer,impact }) foreach (int lifecycle in Enumerable.Range(0,6)) {
                Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
                var actor = new Player { whoAmI = 0, active = true, statLife = 100, statLifeMax2 = 100 }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.discoveredSpiritualEnergy = true; state.cultivationStage = CultivationStage.NascentSoul;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native melee artifact session fixture imports valid state");
                Projectile p = Spawn(actor,type); var logic = p.ModProjectile; var snapshot = state.CaptureSnapshot(); long session = state.ArtifactOwnerSession;
                check(logic.CanDamage() == null,"native fresh wheel hammer and impact have active owner-session damage");
                check(!state.TryInitializeNetwork(snapshot) && state.ArtifactOwnerSession == session && logic.CanDamage() == null,"native duplicate import does not cancel legitimate melee artifacts");
                switch (lifecycle) {
                    case 0: state.ResetNetworkSession(); check(state.TryInitializeNetwork(snapshot),"native same-object melee artifact rejoin accepts fresh session"); break;
                    case 1: actor.dead = true; state.UpdateDead(); actor.dead = false; break;
                    case 2: state.OnEnterWorld(); break;
                    case 3: var tag = new TagCompound(); state.SaveData(tag); state.LoadData(tag); break;
                    case 4: state.Initialize(); break;
                    case 5: Main.player[0] = new Player { whoAmI = 0, active = true, statLife = 100, statLifeMax2 = 100 }; break;
                }
                check(logic.CanDamage() == false,"native owner lifecycle or replacement suppresses wheel hammer and impact before AI");
                if (type == hammer) {
                    p.timeLeft = 0; logic.OnKill(0); logic.OnKill(0);
                    check(Main.projectile.Count(projectile => projectile.active && !before.Contains(projectile)) == 1,"native stale-session hammer expiry and repeated callback cannot create new valid impact");
                }
                logic.AI(); check(!p.friendly && p.timeLeft <= 6 && p.velocity == Vector2.Zero && logic.CanDamage() == false,"native stale melee artifact locks inert bounded cleanup");
                for (int tick = 0; tick < 6 && p.active; tick++) p.Update(p.whoAmI);
                check(!p.active && !Main.projectile.Any(projectile => projectile.active && !before.Contains(projectile)),"native stale artifact exits within six updates without secondary projectile");
                if (lifecycle == 0) {
                    Projectile fresh = Spawn(actor,type); check(fresh.ModProjectile.CanDamage() == null,"native same Player rejoin permits fresh valid melee artifacts");
                    if (type == hammer) {
                        fresh.Kill(); var children = Main.projectile.Where(projectile => projectile.active && !before.Contains(projectile)).ToArray();
                        check(children.Length == 1 && children[0].type == impact && children[0].damage == 29 && children[0].ModProjectile.CanDamage() == null,"native new-session hammer retains one valid half-damage impact");
                    }
                }
            }
        } finally { Clear(); Main.player[0] = original; Main.netMode = mode; Main.myPlayer = local; clients.CopyTo(Netplay.Clients,0); }
        check(ReferenceEquals(Main.player[0],original) && Main.netMode == mode,"native melee artifact session fixture restores player and authority context");
    }
}
