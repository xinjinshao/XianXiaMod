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
    private static void AuditFieldSessions(Action<bool,string> check)
    {
        Player original = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer; ulong tick = Main.GameUpdateCount;
        var clients = (RemoteClient[])Netplay.Clients.Clone(); var text = Main.combatText; var random = Main.rand;
        var before = Main.projectile.Where(p => p.active).ToHashSet(); NPC target = null;
        int green = ModContent.ProjectileType<GreenwoodArrayField>(), thunder = ModContent.ProjectileType<ThunderTalismanArray>(), medicine = ModContent.ProjectileType<MedicineCauldronField>();
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        Projectile Spawn(Player actor,int type) {
            int index = Projectile.NewProjectile(new EntitySource_Misc("FieldSessionAudit"),actor.Center,Vector2.Zero,type,60,3,0);
            check(index >= 0 && index < Main.maxProjectiles,"native field-session registered creation succeeds"); var p = Main.projectile[index];
            p.timeLeft = type == thunder ? 225 : 300; p.ai[0] = type == medicine ? 59 : 0; return p;
        }
        try {
            Main.rand = new Terraria.Utilities.UnifiedRandom(270); Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
            int npc = NPC.NewNPC(new EntitySource_Misc("FieldSessionTarget"),Main.maxTilesX * 8 + 100,400,NPCID.BlueSlime);
            check(npc >= 0 && npc < Main.maxNPCs,"native medicine field session fixture creates visible vanilla target"); target = Main.npc[npc]; target.Center = new Vector2(Main.maxTilesX * 8 + 100,400); target.life = target.lifeMax = 1000; target.damage = 0; target.noGravity = true;
            check(tick % 60 == 0,"native field-session audit uses a real recovery boundary without changing engine tick");
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (int type in new[] { green,thunder,medicine }) foreach (int lifecycle in Enumerable.Range(0,6)) {
                Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
                var actor = new Player { whoAmI = 0, active = true, statLife = 50, statLifeMax2 = 100 }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.discoveredSpiritualEnergy = true; state.cultivationStage = CultivationStage.NascentSoul;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native field owner session imports valid state"); var snapshot = state.CaptureSnapshot();
                Projectile p = Spawn(actor,type);
                if (type == green) check(((GreenwoodArrayField)p.ModProjectile).CanPulse(actor),"native fresh Greenwood field supplies a pulse source");
                switch (lifecycle) {
                    case 0: state.ResetNetworkSession(); check(state.TryInitializeNetwork(snapshot),"native same-object field reconnect imports fresh session"); break;
                    case 1: actor.dead = true; state.UpdateDead(); actor.dead = false; break;
                    case 2: state.OnEnterWorld(); break;
                    case 3: var tag = new TagCompound(); state.SaveData(tag); state.LoadData(tag); break;
                    case 4: state.Initialize(); break;
                    case 5: Main.player[0] = new Player { whoAmI = 0, active = true, statLife = 50, statLifeMax2 = 100 }; Main.player[0].Center = actor.Center; break;
                }
                var receiver = Main.player[0]; int life = receiver.statLife, energy = receiver.GetModPlayer<XianXiaPlayer>().spiritualEnergy;
                check(p.ModProjectile.CanDamage() == false && (type != green || !((GreenwoodArrayField)p.ModProjectile).CanPulse(receiver)),"native stale field cannot damage or authorize healing pulse before AI");
                p.ModProjectile.AI();
                check(!p.active && receiver.statLife == life && receiver.GetModPlayer<XianXiaPlayer>().spiritualEnergy == energy
                    && !Main.projectile.Any(projectile => projectile.active && !before.Contains(projectile)),"native stale field is removed at recovery/wave boundary without effects");
                Main.player[0] = actor; p.active = true; p.ModProjectile.AI();
                check(!p.active && !Main.projectile.Any(projectile => projectile.active && !before.Contains(projectile)),"native forced old field reactivation cannot restore recovery or attacks");
                if (lifecycle == 0) {
                    actor.statLife = 50; state.spiritualEnergy = 0; Projectile fresh = Spawn(actor,type); fresh.ModProjectile.AI();
                    check(fresh.active && (type == green ? actor.statLife == 51 && state.spiritualEnergy == 1
                        : Main.projectile.Count(projectile => projectile.active && !before.Contains(projectile) && projectile != fresh) == (type == thunder ? 1 : 2)),
                        "native rejoined owner retains fresh Greenwood recovery thunder strike and medicine wave");
                }
            }
        } finally { Clear(); if (target != null) { target.damage = 0; target.active = false; } Main.player[0] = original; Main.netMode = mode; Main.myPlayer = local; Main.rand = random; Main.combatText = text; clients.CopyTo(Netplay.Clients,0); }
        check(ReferenceEquals(Main.player[0],original) && Main.GameUpdateCount == tick && Main.rand == random,"native field-session fixture restores player tick random and authority context");
    }
}
