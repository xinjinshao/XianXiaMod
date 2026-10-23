using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.NPCs.Town;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditCommissionCapacity(Action<bool,string> check)
    {
        Player[] originals=Main.player.Take(2).ToArray();int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        int slot=Enumerable.Range(0,Main.maxNPCs).First(i=>!Main.npc[i].active);NPC oldNpc=Main.npc[slot];Item[] items=(Item[])Main.item.Clone();int[] reuse=(int[])Main.timeItemSlotCannotBeReusedFor.Clone();
        bool hard=Main.hardMode,plant=NPC.downedPlantBoss,golem=NPC.downedGolemBoss,moon=NPC.downedMoonlord;
        var clients=(RemoteClient[])Netplay.Clients.Clone();var world=ModContent.GetInstance<DownedBossSystem>();var saved=new TagCompound();world.SaveWorldData(saved);var mod=ModContent.GetInstance<global::XianXia.XianXia>();
        var quests=new[]{
            ("HerbSectApprentice","herb_sect_apprentice_garden","garden_warden",8,"GreenwoodRoot",10,"SpringReturnPill",3,"",0),
            ("WanderingArtificer","wandering_artificer_furnace","black_furnace_iron_golem",8,"FurnaceSlagIron",10,"ArtifactBlankShard",3,"",0),
            ("TribulationObserver","tribulation_observer_thunder","thunder_marsh_jiao",12,"TribulationCloudDew",8,"TribulationResistingPill",3,"",0),
            ("ArchiveScrollSpirit","archive_scroll_spirit_trial","formless_sword_soul",16,"SectTrialToken",2,"OldHeavenDaoScroll",1,"",0),
            ("FallenHeavenMessenger","fallen_heaven_messenger_tablet","heaven_tablet_guardian",24,"HeavenDaoFragment",6,"BrokenHeavenDecree",1,"",0),
            ("HerbSectApprentice","herb_sect_apprentice_king","greenwood_medicine_king_echo",12,"QiRecoveryPill",5,"WindStepPill",3,"herb_sect_apprentice_garden",8),
            ("WanderingArtificer","wandering_artificer_sword","formless_sword_soul",16,"ArtifactQuenchingCrystal",2,"ArtifactBlankShard",6,"wandering_artificer_furnace",8),
            ("TribulationObserver","tribulation_observer_inspector","broken_heaven_inspector",18,"ThunderBurstPill",3,"TribulationResistingPill",4,"tribulation_observer_thunder",12),
            ("ArchiveScrollSpirit","archive_scroll_spirit_moon","moonbone_immortal",24,"ArchiveRemnantLight",4,"ColdMoonDust",6,"archive_scroll_spirit_trial",16),
            ("FallenHeavenMessenger","fallen_heaven_messenger_core","old_heaven_dao_core",30,"HeavenDaoFragment",8,"QiRecoveryPill",6,"fallen_heaven_messenger_tablet",24)};
        int Type(string name)=>mod.Find<ModItem>(name).Type;
        int Count(Player actor,int type)=>actor.inventory.Take(58).Where(i=>i.type==type).Sum(i=>i.stack)+Main.item.Take(Main.maxItems).Where(i=>i.active&&i.type==type).Sum(i=>i.stack);
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=new AuditOutputSocket(),State=0};
            Main.hardMode=NPC.downedPlantBoss=NPC.downedGolemBoss=NPC.downedMoonlord=true;
            foreach(int netMode in new[]{NetmodeID.SinglePlayer,NetmodeID.Server})foreach(bool fullInventory in new[]{false,true})foreach(var q in quests)for(int capacity=0;capacity<6;capacity++) {
                Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(329);world.LoadWorldData(new TagCompound());DownedBossSystem.DownedBosses.Add(q.Item3);
                if(q.Item9.Length>0)DownedBossSystem.TryClaimCommission(q.Item9,q.Item10);int reputation=DownedBossSystem.SectReputation;
                var actor=new Player {whoAmI=0,active=true,statLife=100};actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,400);Main.player[0]=actor;actor.GetModPlayer<XianXiaPlayer>().cultivationStage=CultivationStage.DaoSevering;
                if(fullInventory)for(int i=0;i<58;i++)actor.inventory[i]=new Item(ItemID.DirtBlock,9999);
                Main.npc[slot]=new NPC {whoAmI=slot};Main.npc[slot].SetDefaults(mod.Find<ModNPC>(q.Item1).Type);Main.npc[slot].whoAmI=slot;Main.npc[slot].active=true;Main.npc[slot].Center=actor.Center;actor.SetTalkNPC(slot);
                var town=(CultivationTownNPC)Main.npc[slot].ModNPC;
                for(int i=0;i<Main.item.Length;i++){Main.item[i]=new Item(ItemID.DirtBlock);Main.item[i].whoAmI=i;Main.item[i].active=i<Main.maxItems;}Array.Clear(Main.timeItemSlotCannotBeReusedFor);
                int free=capacity==0?0:capacity==1?1:capacity==5?3:2;
                for(int i=0;i<free;i++)Main.item[i].active=false;
                if(capacity>=3)Main.timeItemSlotCannotBeReusedFor[0]=10;if(capacity==4)Main.timeItemSlotCannotBeReusedFor[1]=10;
                bool ready=capacity==2||capacity==5;
                Main.myPlayer=netMode==NetmodeID.SinglePlayer?0:Main.maxPlayers;
                var response=town.ClaimCommissionOnServer(actor);
                int first=Type(q.Item5),second=Type(q.Item7);
                check(DownedBossSystem.ClaimedCommissions.Contains(q.Item2)==ready&&DownedBossSystem.SectReputation==reputation+(ready?q.Item4:0),"native commission claim and reputation commit only when every reward has reusable capacity");
                check(Count(actor,first)==(ready?q.Item6:0)&&Count(actor,second)==(ready?q.Item8:0),"native actual commission rewards are exact across inventory and world drops without partial delivery");
                check(Main.item.Take(Main.maxItems).Count(i=>i.active&&i.type==ItemID.DirtBlock)==Main.maxItems-free,"native commission claim never replaces existing world drops under capacity pressure");
                if(!ready) {
                    check(response.ToString()==NetworkText.FromKey("Mods.XianXia.NPCs.Commission.RewardsBusy").ToString(),"native insufficient reward capacity reports unclaimed retry guidance");
                    for(int i=0;i<2;i++){Main.item[i].active=false;Main.timeItemSlotCannotBeReusedFor[i]=0;}
                    town.ClaimCommissionOnServer(actor);
                    check(DownedBossSystem.ClaimedCommissions.Contains(q.Item2)&&DownedBossSystem.SectReputation==reputation+q.Item4&&Count(actor,first)==q.Item6&&Count(actor,second)==q.Item8,"native freeing item capacity lets the same commission deliver fully exactly once");
                }
                town.ClaimCommissionOnServer(actor);
                Main.myPlayer=Main.maxPlayers;var other=new Player {whoAmI=1,active=true,statLife=100};other.Center=actor.Center;Main.player[1]=other;other.GetModPlayer<XianXiaPlayer>().cultivationStage=CultivationStage.DaoSevering;other.SetTalkNPC(slot);town.ClaimCommissionOnServer(other);
                check(DownedBossSystem.SectReputation==reputation+q.Item4&&Count(actor,first)==q.Item6&&Count(actor,second)==q.Item8&&other.inventory.Take(58).All(i=>i.IsAir),"native repeat and second participant commission claims cannot duplicate rewards or reputation");
            }
        } finally {
            for(int i=0;i<items.Length;i++)Main.item[i]=items[i];reuse.CopyTo(Main.timeItemSlotCannotBeReusedFor,0);Main.npc[slot]=oldNpc;for(int i=0;i<2;i++)Main.player[i]=originals[i];Main.netMode=mode;Main.myPlayer=local;Main.rand=random;clients.CopyTo(Netplay.Clients,0);world.LoadWorldData(saved);
            Main.hardMode=hard;NPC.downedPlantBoss=plant;NPC.downedGolemBoss=golem;NPC.downedMoonlord=moon;
        }
        check(Main.player.Take(2).SequenceEqual(originals)&&Main.item.SequenceEqual(items)&&Main.timeItemSlotCannotBeReusedFor.SequenceEqual(reuse),"native commission audit restores all item slots reuse timers and both participants");
    }
}
