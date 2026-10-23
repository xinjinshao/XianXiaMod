using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditTownShopBoundaries(Action<bool,string> check)
    {
        var rules=new (string Seller,string Item,CultivationStage Stage,int Reputation,string Boss)[] {
            ("HerbSectApprentice","FoundationPill",CultivationStage.QiCondensation,0,null),
            ("WanderingArtificer","CloudpiercerFlyingSword",CultivationStage.None,0,"spirit_vein_wyrm"),
            ("WanderingArtificer","GreenwoodArrayPlate",CultivationStage.None,0,"spirit_vein_wyrm"),
            ("WanderingArtificer","FurnaceHeartRing",CultivationStage.Foundation,0,null),
            ("TribulationObserver","ThunderTalismanArrayPlate",CultivationStage.Foundation,0,null),
            ("TribulationObserver","SummonThunderCallingJade",CultivationStage.Foundation,0,null),
            ("TribulationObserver","TribulationResistingPill",CultivationStage.None,0,"tribulation_cloud_avatar"),
            ("TribulationObserver","ThunderInscriptionNeedle",CultivationStage.None,0,"thunder_marsh_jiao"),
            ("ArchiveScrollSpirit","FormlessSwordWheel",CultivationStage.GoldenCore,0,null),
            ("ArchiveScrollSpirit","NascentSoulJadeBox",CultivationStage.GoldenCore,80,null),
            ("ArchiveScrollSpirit","StarAbyssInscriptionNeedle",CultivationStage.None,0,"abyssal_star_womb"),
            ("FallenHeavenMessenger","BrokenHeavenDecree",CultivationStage.None,0,"heaven_tablet_guardian"),
            ("FallenHeavenMessenger","BrokenHeavenCrownSeal",CultivationStage.None,0,"heaven_tablet_guardian"),
            ("FallenHeavenMessenger","BrokenHeavenInscriptionNeedle",CultivationStage.SpiritSevering,0,"heaven_tablet_guardian"),
            ("FallenHeavenMessenger","DaoSeveringRing",CultivationStage.Tribulation,160,null),
            ("FallenHeavenMessenger","EndgameRouteFrame",CultivationStage.None,0,"moonbone_immortal"),
        };
        Player[] previous=Main.player.Take(2).ToArray();int mode=Main.netMode,local=Main.myPlayer;
        var bosses=DownedBossSystem.DownedBosses.ToArray();bool wyrm=DownedBossSystem.DownedSpiritVeinWyrm;
        var reputation=typeof(DownedBossSystem).GetField("<SectReputation>k__BackingField",BindingFlags.Static|BindingFlags.NonPublic)!;object oldRep=reputation.GetValue(null);
        var mod=ModContent.GetInstance<global::XianXia.XianXia>();int common=mod.Find<ModItem>("LowGradeSpiritStone").Type;
        try {
            Main.netMode=NetmodeID.MultiplayerClient;
            for(int i=0;i<2;i++)Main.player[i]=new Player { whoAmI=i,active=true,statLife=100 };
            foreach(var rule in rules) {
                var cases=new List<(CultivationStage Stage,int Rep,bool Boss,bool Allowed,string Name)> { (rule.Stage,rule.Reputation,true,true,"exact-unlock") };
                if(rule.Stage>CultivationStage.None)cases.Add(((CultivationStage)((int)rule.Stage-1),rule.Reputation,true,false,"below-stage"));
                if(rule.Reputation>0)cases.Add((rule.Stage,rule.Reputation-1,true,false,"below-reputation"));
                if(rule.Boss!=null)cases.Add((rule.Stage,rule.Reputation,false,false,"unrelated-boss"));
                foreach(var scenario in cases)foreach(int viewer in new[]{0,1}) {
                    Main.myPlayer=viewer;Main.player[viewer].GetModPlayer<XianXiaPlayer>().cultivationStage=scenario.Stage;Main.player[1-viewer].GetModPlayer<XianXiaPlayer>().cultivationStage=CultivationStage.DaoSevering;
                    DownedBossSystem.DownedBosses.Clear();DownedBossSystem.DownedBosses.Add("garden_warden");
                    if(scenario.Boss&&rule.Boss!=null)DownedBossSystem.DownedBosses.Add(rule.Boss);
                    DownedBossSystem.DownedSpiritVeinWyrm=scenario.Boss&&rule.Boss=="spirit_vein_wyrm";reputation.SetValue(null,scenario.Rep);
                    var npc=new NPC();npc.SetDefaults(mod.Find<ModNPC>(rule.Seller).Type);
                    NPCShop shop=NPCShopDatabase.AllShops.OfType<NPCShop>().Single(s=>s.FullName=="XianXia/"+rule.Seller+"/Shop");
                    var filled=new List<Item>();shop.FillShop(filled,npc);Item[] slots=Enumerable.Range(0,40).Select(_=>new Item()).ToArray();filled.CopyTo(slots,0);NPCLoader.ModifyActiveShop(npc,"Shop",slots);
                    int item=mod.Find<ModItem>(rule.Item).Type;
                    check(slots.Count(i=>i!=null&&i.type==item)==(scenario.Allowed?1:0),"native town shop boundary respects local realm world reputation and required boss: "+rule.Seller+"/"+rule.Item+"/"+scenario.Name+"/"+viewer);
                    check(slots.Any(i=>i!=null&&i.type==common),"native town shop boundary keeps basic spirit stone service available");
                    check(shop.Entries.Count(e=>e.Item.type==item)==1,"native town shop boundary leaves one unchanged registered product template");
                }
            }
        } finally {
            for(int i=0;i<2;i++)Main.player[i]=previous[i];Main.netMode=mode;Main.myPlayer=local;DownedBossSystem.DownedBosses.Clear();foreach(var boss in bosses)DownedBossSystem.DownedBosses.Add(boss);DownedBossSystem.DownedSpiritVeinWyrm=wyrm;reputation.SetValue(null,oldRep);
        }
        check(Main.player.Take(2).SequenceEqual(previous)&&DownedBossSystem.DownedBosses.SetEquals(bosses)&&DownedBossSystem.DownedSpiritVeinWyrm==wyrm&&Equals(reputation.GetValue(null),oldRep),"native town shop boundary audit restores players boss flags and exact world reputation");
    }
}
