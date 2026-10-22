using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Content.NPCs.Enemies;
using XianXia.Content.Items.Accessories;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditSpiritTideCompletion(Action<bool,string> check)
    {
        int slot = Enumerable.Range(0,Main.maxNPCs).First(i => !Main.npc[i].active);
        NPC original = Main.npc[slot]; int mode=Main.netMode,local=Main.myPlayer; bool day=Main.dayTime, hard=Main.hardMode;
        int heldSlot=Enumerable.Range(0,Main.maxNPCs).First(i => i!=slot && !Main.npc[i].active); NPC heldOriginal=Main.npc[heldSlot];
        Player originalPlayer=Main.player[0]; bool blood=Main.bloodMoon,pumpkin=Main.pumpkinMoon,snow=Main.snowMoon; int invasion=Main.invasionType;
        var bosses=Main.npc.Where(n => n.active && n.boss).ToArray();
        var random=Main.rand; var clients=(RemoteClient[])Netplay.Clients.Clone(); var existing=Main.item.Where(i => i.active).ToHashSet();
        var fields=typeof(SpiritTideSystem).GetFields(BindingFlags.Static | BindingFlags.NonPublic).Where(f => !f.IsLiteral && !f.IsInitOnly).Select(f => (Field:f,Value:f.GetValue(null))).ToArray();
        var system=ModContent.GetInstance<SpiritTideSystem>();
        var instanceFields=typeof(SpiritTideSystem).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Select(f => (Field:f,Value:f.GetValue(system))).ToArray();
        int[] types={ModContent.NPCType<WanderingSpiritSlime>(),ModContent.NPCType<ShatteredJadeWorm>(),ModContent.NPCType<TalismanBat>()};
        NPC Spawn(int type,IEntitySource source,int y=100) {
            Main.npc[slot]=new NPC { whoAmI=slot };
            int actual=NPC.NewNPC(source,Main.maxTilesX * 8,y,type);
            if(actual!=slot) throw new InvalidOperationException("Tide completion fixture used unexpected NPC slot");
            return Main.npc[slot];
        }
        int RewardCount(int type) => Main.item.Where(i => i.active && !existing.Contains(i) && i.type==type).Sum(i => i.stack);
        try {
            Main.netMode=NetmodeID.Server; Main.myPlayer=Main.maxPlayers; Main.dayTime=false; Main.hardMode=true; Main.rand=new Terraria.Utilities.UnifiedRandom(313);
            for(int i=0;i<Netplay.Clients.Length;i++) Netplay.Clients[i]=new RemoteClient { Id=i,Socket=new AuditOutputSocket(),State=0,IsActive=false };
            system.LoadWorldData(new TagCompound { ["spiritTideActive"]=true,["spiritTideWave"]=1,["spiritTideKills"]=0 });
            foreach(bool underground in new[]{false,true}) {
                NPC excluded=Spawn(types[0],underground ? new EntitySource_SpawnNPC() : new EntitySource_Misc("TideArtificialAudit"),underground ? (int)(Main.worldSurface * 16 + 100) : 100);
                excluded.GetGlobalNPC<SpiritTideEnemies>().OnKill(excluded); excluded.active=false;
                check(SpiritTideSystem.Kills==0,"native actual artificial and underground spawns do not advance tide progress");
            }
            Main.npc[heldSlot]=new NPC { whoAmI=heldSlot };
            int heldIndex=NPC.NewNPC(new EntitySource_SpawnNPC(),Main.maxTilesX * 8,100,types[0],Start:heldSlot);
            if(heldIndex!=heldSlot) throw new InvalidOperationException("Old-run tide fixture used unexpected slot");
            NPC held=Main.npc[heldSlot]; uint firstRun=SpiritTideSystem.RunId;
            for(int kill=1;kill<=54;kill++) {
                int wave=SpiritTideSystem.Wave, before=SpiritTideSystem.Kills, target=SpiritTideSystem.RequiredKills;
                int type=types[(kill-1)%types.Length]; var baseline=new NPC(); baseline.SetDefaults(type);
                NPC npc=Spawn(type,new EntitySource_SpawnNPC());
                check(npc.lifeMax==baseline.lifeMax * (wave+1) && npc.life==npc.lifeMax && npc.damage==(int)Math.Ceiling(baseline.damage*(1f+0.25f*wave)) && npc.defense==baseline.defense+3*wave,"native natural spawn factory applies actual tide wave combat enhancement");
                var state=npc.GetGlobalNPC<SpiritTideEnemies>(); state.OnKill(npc);
                bool finished=kill==54, advanced=before+1==target;
                check(SpiritTideSystem.Active==!finished && SpiritTideSystem.Wave==(finished?0:advanced?wave+1:wave) && SpiritTideSystem.Kills==(advanced?0:before+1),"native registered tide deaths follow exact 12 18 24 wave boundaries");
                int credited=SpiritTideSystem.Kills, creditedWave=SpiritTideSystem.Wave; bool active=SpiritTideSystem.Active;
                state.OnKill(npc);
                check(SpiritTideSystem.Kills==credited && SpiritTideSystem.Wave==creditedWave && SpiritTideSystem.Active==active,"native duplicate registered tide death callback cannot credit twice");
                if(!finished) check(!Main.item.Any(i => i.active && !existing.Contains(i)),"native tide does not emit completion loot before final kill");
                npc.active=false;
            }
            check(RewardCount(ModContent.ItemType<SpiritTidePearl>())==1 && RewardCount(ModContent.ItemType<LowGradeSpiritStone>())==20 && RewardCount(ModContent.ItemType<SpiritHerbSeeds>())==5 && RewardCount(ModContent.ItemType<QiRecoveryPill>())==2,"native final tide death creates exact pearl stones seeds and pill completion reward");
            check(Main.item.Count(i => i.active && !existing.Contains(i))==4,"native completion produces only four expected item entities");
            int rewards=Main.item.Count(i => i.active && !existing.Contains(i));
            NPC after=Spawn(types[0],new EntitySource_SpawnNPC()); after.GetGlobalNPC<SpiritTideEnemies>().OnKill(after); after.active=false;
            check(!SpiritTideSystem.Active && SpiritTideSystem.Kills==0 && Main.item.Count(i => i.active && !existing.Contains(i))==rewards,"native post-completion spawns cannot advance or duplicate event reward");
            Main.player[0]=new Player { whoAmI=0,active=true,statLife=100 };
            Main.player[0].GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage=global::XianXia.Common.Players.CultivationStage.Foundation;
            Main.bloodMoon=Main.pumpkinMoon=Main.snowMoon=false; Main.invasionType=0;
            foreach(var boss in bosses) boss.active=false;
            Main.dayTime=true; system.PostUpdateWorld();
            int duskSeed=Enumerable.Range(0,1000).First(seed => new Terraria.Utilities.UnifiedRandom(seed).NextBool(SpiritTideSystem.StartChance));
            Main.rand=new Terraria.Utilities.UnifiedRandom(duskSeed); Main.dayTime=false; system.PostUpdateWorld();
            check(SpiritTideSystem.Active && SpiritTideSystem.Wave==1 && SpiritTideSystem.Kills==0 && SpiritTideSystem.RunId>firstRun,"native actual eligible next dusk starts fresh tide with newer encounter identity");
            held.GetGlobalNPC<SpiritTideEnemies>().OnKill(held); held.active=false;
            check(SpiritTideSystem.Kills==0 && Main.item.Count(i => i.active && !existing.Contains(i))==rewards,"native previous-run living enemy cannot credit next event or duplicate reward");
        } finally {
            foreach(var item in Main.item.Where(i => i.active && !existing.Contains(i))) item.active=false;
            Main.npc[slot]=original; Main.npc[heldSlot]=heldOriginal; Main.player[0]=originalPlayer; foreach(var boss in bosses) boss.active=true;
            Main.bloodMoon=blood;Main.pumpkinMoon=pumpkin;Main.snowMoon=snow;Main.invasionType=invasion;
            clients.CopyTo(Netplay.Clients,0); Main.netMode=mode;Main.myPlayer=local;Main.dayTime=day;Main.hardMode=hard;Main.rand=random;
            foreach(var field in fields) field.Field.SetValue(null,field.Value);
            foreach(var field in instanceFields) field.Field.SetValue(system,field.Value);
        }
        check(ReferenceEquals(Main.npc[slot],original) && Main.item.Where(i => i.active).ToHashSet().SetEquals(existing),"native tide completion restores NPC slot item pool event and engine context");
    }
}
