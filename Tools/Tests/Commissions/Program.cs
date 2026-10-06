using System.Text.Json;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using XianXia.Common.Systems;
using XianXia.Common.Players;
using XianXia.Content.NPCs.Town;
int count=0;void Check(bool ok,string msg){count++;if(!ok)throw new Exception(msg);}
var world=new DownedBossSystem();
var entries=new (CultivationTownNPC Town,string Prior,string PrimaryBoss,string Id,string Boss,CultivationStage Stage,int Reputation,string R1,int S1,string R2,int S2)[]{
 (new HerbSectApprentice(),"herb_sect_apprentice_garden","garden_warden","herb_sect_apprentice_king","greenwood_medicine_king_echo",CultivationStage.NascentSoul,12,"QiRecoveryPill",5,"WindStepPill",3),
 (new WanderingArtificer(),"wandering_artificer_furnace","black_furnace_iron_golem","wandering_artificer_sword","formless_sword_soul",CultivationStage.NascentSoul,16,"ArtifactQuenchingCrystal",2,"ArtifactBlankShard",6),
 (new TribulationObserver(),"tribulation_observer_thunder","thunder_marsh_jiao","tribulation_observer_inspector","broken_heaven_inspector",CultivationStage.SpiritSevering,18,"ThunderBurstPill",3,"TribulationResistingPill",4),
 (new ArchiveScrollSpirit(),"archive_scroll_spirit_trial","formless_sword_soul","archive_scroll_spirit_moon","moonbone_immortal",CultivationStage.Tribulation,24,"ArchiveRemnantLight",4,"ColdMoonDust",6),
 (new FallenHeavenMessenger(),"fallen_heaven_messenger_tablet","heaven_tablet_guardian","fallen_heaven_messenger_core","old_heaven_dao_core",CultivationStage.Tribulation,30,"HeavenDaoFragment",8,"QiRecoveryPill",6)};
void Prepare(int index,bool prior=true){world.ClearWorld();Main.netMode=2;Main.hardMode=NPC.downedPlantBoss=NPC.downedGolemBoss=NPC.downedMoonlord=true;Main.LocalPlayer=new();Main.LocalPlayer.State.cultivationStage=entries[index].Stage;var entry=entries[index];entry.Town.NPC.active=true;entry.Town.NPC.Center=new(0,0);entry.Town.NPC.whoAmI=index;Main.LocalPlayer.Center=new(0,0);Main.LocalPlayer.talkNPC=index;DownedBossSystem.MarkDowned(entry.PrimaryBoss);DownedBossSystem.MarkDowned(entry.Boss);if(prior)DownedBossSystem.ClaimedCommissions.Add(entry.Prior);}
foreach(string lang in new[]{"en-US","zh-Hans"}){
 using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(args[0],"Localization/followup-commissions",lang+".hjson")));
 foreach(var npc in json.RootElement.GetProperty("Mods").GetProperty("XianXia").GetProperty("FollowupCommissions").EnumerateObject())foreach(var key in npc.Value.EnumerateObject())Language.Texts["Mods.XianXia.FollowupCommissions."+npc.Name+"."+key.Name]=key.Value.GetString();
 for(int i=0;i<entries.Length;i++){
  var entry=entries[i];Prepare(i,false);Check(!entry.Town.GetChat().Contains("FollowupCommissions"),"No followup hint before first quest");var primary=entry.Town.ClaimCommissionOnServer(Main.LocalPlayer);Check(DownedBossSystem.ClaimedCommissions.Contains(entry.Prior)&&!DownedBossSystem.ClaimedCommissions.Contains(entry.Id)&&Main.LocalPlayer.Rewards.Count==2,"Actual NPC first click preserves legacy reward only");
  Main.LocalPlayer.Rewards.Clear();int before=DownedBossSystem.SectReputation;Check(entry.Town.GetChat().Contains(Language.GetTextValue("Mods.XianXia.FollowupCommissions."+entry.Town.Name+".Ready")),"Actual GetChat includes localized ready hint");
  var result=entry.Town.ClaimCommissionOnServer(Main.LocalPlayer);Check(result.Key.EndsWith(".Claimed")&&result.ToString().Contains((before+entry.Reputation).ToString()),"Actual server NPC uses localized formatted completion");
  Check(DownedBossSystem.SectReputation==before+entry.Reputation&&DownedBossSystem.ClaimedCommissions.Contains(entry.Id),"Authoritative world reward tracked once");
  Check(Main.LocalPlayer.Rewards.Count==2&&Main.LocalPlayer.Rewards[0].Stack==entry.S1&&ModContent.ItemName(Main.LocalPlayer.Rewards[0].Type)==entry.R1&&Main.LocalPlayer.Rewards[1].Stack==entry.S2&&ModContent.ItemName(Main.LocalPlayer.Rewards[1].Type)==entry.R2,"Exact personal rewards from actual NPC hooks");
  var another=new Player{talkNPC=i};another.State.cultivationStage=entry.Stage;Check(entry.Town.ClaimCommissionOnServer(another).Key.EndsWith(".Completed")&&another.Rewards.Count==0,"Another eligible player cannot duplicate world reward");
  Check(entry.Town.GetChat().Contains(Language.GetTextValue("Mods.XianXia.FollowupCommissions."+entry.Town.Name+".Completed")),"Actual GetChat changes to completion state");
  var tag=new TagCompound();world.SaveWorldData(tag);world.ClearWorld();world.LoadWorldData(tag);Check(DownedBossSystem.ClaimedCommissions.Contains(entry.Id)&&DownedBossSystem.SectReputation==before+entry.Reputation,"Save/reload preserves once-only progress and recomputed reputation");
  Check(entry.Town.ClaimCommissionOnServer(Main.LocalPlayer).Key.EndsWith(".Completed")&&Main.LocalPlayer.Rewards.Count==2,"Reload cannot reissue supplies");
 }
}
for(int i=0;i<entries.Length;i++){
 var entry=entries[i];
 foreach(Action blocker in new Action[]{()=>Main.LocalPlayer.State.cultivationStage=(CultivationStage)((int)entry.Stage-1),()=>Main.hardMode=false,()=>NPC.downedPlantBoss=false,()=>DownedBossSystem.DownedBosses.Remove(entry.Boss)}){Prepare(i);blocker();int before=DownedBossSystem.SectReputation;var result=entry.Town.ClaimCommissionOnServer(Main.LocalPlayer);Check(result.Key.EndsWith(".Locked")&&Main.LocalPlayer.Rewards.Count==0&&DownedBossSystem.SectReputation==before&&!DownedBossSystem.ClaimedCommissions.Contains(entry.Id),"Realm/world/boss rejection leaves progress and rewards unchanged");Check(entry.Town.GetChat().Contains(Language.GetTextValue("Mods.XianXia.FollowupCommissions."+entry.Town.Name+".Locked")),"Actual chat explains locked followup");}
 if(entry.Stage>=CultivationStage.SpiritSevering){Prepare(i);NPC.downedGolemBoss=false;Check(entry.Town.ClaimCommissionOnServer(Main.LocalPlayer).Key.EndsWith(".Locked"),"Golem gate retained");}
 if(entry.Stage>=CultivationStage.Tribulation){Prepare(i);NPC.downedMoonlord=false;Check(entry.Town.ClaimCommissionOnServer(Main.LocalPlayer).Key.EndsWith(".Locked"),"Moon Lord gate retained");}
 foreach(Action blocker in new Action[]{()=>Main.netMode=1,()=>Main.LocalPlayer.active=false,()=>Main.LocalPlayer.dead=true,()=>Main.LocalPlayer.noItems=true,()=>Main.LocalPlayer.CCed=true,()=>entry.Town.NPC.active=false,()=>Main.LocalPlayer.talkNPC=-1,()=>Main.LocalPlayer.Center=new(601,0)}){Prepare(i);blocker();int before=DownedBossSystem.SectReputation;var result=entry.Town.ClaimCommissionOnServer(Main.LocalPlayer);Check(result.Key.EndsWith(".Unavailable")&&Main.LocalPlayer.Rewards.Count==0&&DownedBossSystem.SectReputation==before,"Invalid requester/context cannot claim");}
 Prepare(i);Main.netMode=1;ModPacket.Sent.Clear();string shop="";entry.Town.OnChatButtonClicked(false,ref shop);Check(ModPacket.Sent.Count==1&&ModPacket.Sent[0].SequenceEqual(new byte[]{1,(byte)i,0})&&Main.LocalPlayer.Rewards.Count==0&&!DownedBossSystem.ClaimedCommissions.Contains(entry.Id),"Actual client button sends existing request without local reward");
 Prepare(i);Check(!FollowupCommissions.TryClaim(Main.LocalPlayer,entry.Town.NPC,"WrongNPC",out _),"Unknown or mismatched NPC rejected");DownedBossSystem.ClaimedCommissions.Clear();Check(!FollowupCommissions.TryClaim(Main.LocalPlayer,entry.Town.NPC,entry.Town.Name,out _),"Prior completion is mandatory");
 Prepare(i);Main.netMode=0;entry.Town.OnChatButtonClicked(false,ref shop);Check(DownedBossSystem.ClaimedCommissions.Contains(entry.Id)&&Main.LocalPlayer.Rewards.Count==2&&!string.IsNullOrWhiteSpace(Main.npcChatText),"Actual single-player button claims and displays feedback");
}
Console.WriteLine($"Actual five-NPC followup hooks passed: {count} assertions; world save and claims linked, engine inventory/packet/UI boundaries mocked.");
