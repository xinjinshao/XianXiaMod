using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditNeedleShops(Action<bool,string> check)
    {
        Player[] previous=Main.player.Take(2).ToArray();int mode=Main.netMode,local=Main.myPlayer;
        var downed=DownedBossSystem.DownedBosses.ToArray();var mod=ModContent.GetInstance<global::XianXia.XianXia>();
        int control=mod.Find<ModItem>("LowGradeSpiritStone").Type;
        try {
            Main.netMode=NetmodeID.MultiplayerClient;
            for(int i=0;i<2;i++)Main.player[i]=new Player { whoAmI=i,active=true,statLife=100 };
            foreach(int flags in new[]{0,1,2,3}) foreach(bool swap in new[]{false,true}) foreach(int viewer in new[]{0,1}) {
                DownedBossSystem.DownedBosses.Clear();if((flags&1)!=0)DownedBossSystem.DownedBosses.Add("thunder_marsh_jiao");if((flags&2)!=0)DownedBossSystem.DownedBosses.Add("heaven_tablet_guardian");
                Main.myPlayer=viewer;
                for(int i=0;i<2;i++)Main.player[i].GetModPlayer<XianXiaPlayer>().cultivationStage=(i==0)!=swap?CultivationStage.SpiritSevering:CultivationStage.QiAwakening;
                foreach(var spec in new[]{("TribulationObserver","ThunderInscriptionNeedle",1),("FallenHeavenMessenger","BrokenHeavenInscriptionNeedle",2)}) {
                    int item=mod.Find<ModItem>(spec.Item2).Type;var npc=new NPC();npc.SetDefaults(mod.Find<ModNPC>(spec.Item1).Type);
                    NPCShop shop=NPCShopDatabase.AllShops.OfType<NPCShop>().Single(s => s.FullName=="XianXia/"+spec.Item1+"/Shop");
                    var filled=new List<Item>();shop.FillShop(filled,npc);Item[] slots=Enumerable.Range(0,40).Select(_ => new Item()).ToArray();filled.CopyTo(slots,0);
                    NPCLoader.ModifyActiveShop(npc,"Shop",slots);
                    bool expected=(flags&spec.Item3)!=0&&(spec.Item3==1||Main.LocalPlayer.GetModPlayer<XianXiaPlayer>().cultivationStage>=CultivationStage.SpiritSevering);
                    check(slots.Count(i => i!=null&&i.type==item)==(expected?1:0),"native actual seller needle visibility follows own boss and local stage: "+spec.Item1+"/"+flags+"/"+swap+"/"+viewer);
                    check(slots.Any(i => i!=null&&i.type==control),"native needle shop filtering preserves ordinary spirit stone service: "+spec.Item1);
                    check(shop.Entries.Count(e => e.Item.type==item)==1,"native active shop filtering does not mutate registered needle template: "+spec.Item1);
                }
            }
            NPCShop artificer=NPCShopDatabase.AllShops.OfType<NPCShop>().Single(s => s.FullName=="XianXia/WanderingArtificer/Shop");
            check(!artificer.Entries.Any(e => e.Item.type==mod.Find<ModItem>("ThunderInscriptionNeedle").Type||e.Item.type==mod.Find<ModItem>("BrokenHeavenInscriptionNeedle").Type),"native needles remain sold by intended NPCs without adding duplicate artificer entries");
        } finally {
            for(int i=0;i<2;i++)Main.player[i]=previous[i];Main.netMode=mode;Main.myPlayer=local;DownedBossSystem.DownedBosses.Clear();foreach(string boss in downed)DownedBossSystem.DownedBosses.Add(boss);
        }
        check(Main.player.Take(2).SequenceEqual(previous)&&DownedBossSystem.DownedBosses.SetEquals(downed),"native needle shop audit restores viewers and world boss progress");
    }
}
