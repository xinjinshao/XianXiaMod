using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Buffs;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditPillBenefits(Action<bool,string> check)
    {
        Player previous=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var clients=(RemoteClient[])Netplay.Clients.Clone();var connection=Netplay.Connection;var random=Main.rand;bool broadcast=NetMessage.buffer[0].broadcast;
        var world=ModContent.GetInstance<DownedBossSystem>();var savedWorld=new TagCompound();world.SaveWorldData(savedWorld);
        var mod=ModContent.GetInstance<global::XianXia.XianXia>();using var probe=mod.GetPacket();int offset=(int)probe.BaseStream.Position;var output=new AuditOutputSocket(true);
        void Deliver(byte[] payload) {using var reader=new BinaryReader(new MemoryStream(payload));mod.HandlePacket(reader,256);}
        (Player Actor,Item Pill) Fixture(int netMode,int itemType,int buff,int duration,PillQuality quality,int scenario) {
            Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;
            var actor=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};Main.player[0]=actor;
            var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=CultivationStage.Foundation;state.discoveredSpiritualEnergy=true;state.ResetEffects();state.spiritualEnergy=scenario is 5 or 7 or 10?100:120;state.spiritPressure=scenario is 3 or 4?10:0;state.TryInitializeNetwork(state.CaptureSnapshot());
            var pill=new Item(itemType,2);actor.inventory[0]=pill;pill.GetGlobalItem<PillQualitySystem>().LoadData(pill,new TagCompound {["quality"]=(int)quality,["crafted"]=true});
            actor.buffImmune[buff]=scenario is 2 or 4 or 11;actor.buffImmune[BuffID.Regeneration]=scenario is 2 or 4 or 5;
            if(scenario!=0&&!actor.buffImmune[buff])actor.AddBuff(buff,duration+(scenario==8?-1:scenario==9?600:0));
            if(scenario is not (6 or 7 or 11)&&!actor.buffImmune[BuffID.Regeneration])actor.AddBuff(BuffID.Regeneration,2000);
            return(actor,pill);
        }
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i==0?output:new AuditOutputSocket(),State=i==0?10:0,IsActive=i==0};NetMessage.buffer[0].broadcast=true;
            foreach(bool resistance in new[]{false,true})foreach(PillQuality quality in Enum.GetValues<PillQuality>())foreach(int scenario in Enumerable.Range(0,12)) {
                Main.rand=new Terraria.Utilities.UnifiedRandom(334);
                int type=resistance?ModContent.ItemType<TribulationResistingPill>():ModContent.ItemType<SpringReturnPill>(),buff=resistance?ModContent.BuffType<TribulationResistanceBuff>():ModContent.BuffType<SpringReturnBuff>();
                int duration=PillQualityRules.Scale(resistance?5400:3600,quality);var fixture=Fixture(NetmodeID.Server,type,buff,duration,quality,scenario);Player actor=fixture.Actor;Item pill=fixture.Pill;var state=actor.GetModPlayer<XianXiaPlayer>();
                bool high=quality is PillQuality.Fine or PillQuality.Spirit,ready=scenario is 0 or 3 or 4 or 8||high&&scenario is 5 or 6 or 7 or 10 or 11;
                var before=state.CaptureSnapshot();var beforeBuffs=AuthoritativeBuffSync.Capture(actor);int oldPrimary=actor.FindBuffIndex(buff),oldMain=oldPrimary<0?0:actor.buffTime[oldPrimary],oldRegen=actor.FindBuffIndex(BuffID.Regeneration),oldRegenTime=oldRegen<0?0:actor.buffTime[oldRegen];
                check(pill.ModItem.CanUseItem(actor)==ready&&state.CaptureSnapshot().Equals(before),"native buff pill benefit preview admits exactly primary pressure or quality reward without mutation");
                output.Sent.Clear();CultivationItemTransactions.HandleRequest(actor,0,type);
                int main=actor.FindBuffIndex(buff),regen=actor.FindBuffIndex(BuffID.Regeneration);
                int expectedEnergy=ready?Math.Min(120,before.Energy+PillQualityRules.BonusEnergy(quality)):before.Energy,expectedPressure=ready?Math.Max(0,before.Pressure-PillQualityRules.Scale(resistance?12:4,quality)):before.Pressure;
                int expectedMain=ready&&!actor.buffImmune[buff]?Math.Max(oldMain,duration):oldMain,expectedRegen=ready&&!actor.buffImmune[BuffID.Regeneration]?Math.Max(oldRegenTime,PillQualityRules.RegenerationTicks(quality)):oldRegenTime;
                check(pill.stack==(ready?1:2)&&state.spiritualEnergy==expectedEnergy&&state.spiritPressure==expectedPressure,"native buff pill consumes only for a useful main pressure or quality outcome");
                check((main<0?0:actor.buffTime[main])==expectedMain&&(regen<0?0:actor.buffTime[regen])==expectedRegen,"native buff pill preserves immunity and longer buffs while granting eligible quality regeneration");
                check(state.ResourceRevision==before.Revision+(ready?1:0)&&!state.ApplyingProgressionItem,"native buff pill commits one resource revision and clears transaction reentry");
                var afterBuffs=AuthoritativeBuffSync.Capture(actor);byte[][] packets=output.Sent.Where(p=>p.Length>2&&p[2]==55).ToArray();
                check(packets.Length==afterBuffs.Count(kv=>!beforeBuffs.TryGetValue(kv.Key,out int time)||time!=kv.Value),"native buff pill synchronizes each actually changed primary or quality buff once");
                var sp=Fixture(NetmodeID.SinglePlayer,type,buff,duration,quality,scenario);
                check(sp.Pill.ModItem.CanUseItem(sp.Actor)==ready,"native single-player buff pill uses identical useful-benefit gate");
                bool? used=ready?ItemLoader.UseItem(sp.Pill,sp.Actor):sp.Pill.ModItem.UseItem(sp.Actor);
                var spState=sp.Actor.GetModPlayer<XianXiaPlayer>();var spBuffs=AuthoritativeBuffSync.Capture(sp.Actor);
                check(used==ready&&(spState.CaptureSnapshot() with {Revision=0}).Equals(state.CaptureSnapshot() with {Revision=0})&&spBuffs.Count==afterBuffs.Count&&afterBuffs.All(kv=>spBuffs.TryGetValue(kv.Key,out int time)&&time==kv.Value),"native single-player actual item loader matches authority resources and all buffs");
                Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=0;Netplay.Connection=new RemoteServer();var receiver=new Player {whoAmI=0,active=true,statLife=100};Main.player[0]=receiver;receiver.inventory[0]=new Item(type,2);receiver.GetModPlayer<XianXiaPlayer>().ApplySnapshot(before);Array.Copy(actor.buffImmune,receiver.buffImmune,actor.buffImmune.Length);foreach(var kv in beforeBuffs)receiver.AddBuff(kv.Key,kv.Value);
                byte[][] canonical=output.Sent.Where(p=>p.Length>offset&&(p[offset]==13||p[offset]==4)).ToArray();
                for(int replay=0;replay<2;replay++) {
                    foreach(byte[] packet in canonical)Deliver(packet[offset..]);
                    foreach(byte[] packet in packets){byte[] payload=packet[2..];using var reader=new BinaryReader(new MemoryStream(payload));var buffer=new MessageBuffer {whoAmI=256,readBuffer=payload,reader=reader};buffer.GetData(0,payload.Length,out int kind);check(kind==55,"official buff pill owner decodes actual primary and quality buff changes");}
                }
                var received=AuthoritativeBuffSync.Capture(receiver);
                check(receiver.inventory[0].stack==pill.stack&&receiver.GetModPlayer<XianXiaPlayer>().CaptureSnapshot().Equals(state.CaptureSnapshot())&&received.Count==afterBuffs.Count&&afterBuffs.All(kv=>received.TryGetValue(kv.Key,out int time)&&time==kv.Value),"actual buff pill owner corrections replay to canonical inventory resources and all benefits");
                Main.player[0]=actor;Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;int sent=output.Sent.Count;int stack=pill.stack;CultivationItemTransactions.HandleRequest(actor,0,type);
                check(pill.stack==stack&&output.Sent.Count==sent,"native buff pill duplicate request cannot repeat any main or quality benefit");
            }
            DownedBossSystem.MarkDowned("tribulation_cloud_avatar");
            foreach(int target in new[]{2,3})foreach(PillQuality quality in Enum.GetValues<PillQuality>()) {
                int type=target==2?ModContent.ItemType<QiCondensingPill>():ModContent.ItemType<FoundationPill>();
                Player Progression(int netMode) {
                    Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;var player=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};Main.player[0]=player;
                    var state=player.GetModPlayer<XianXiaPlayer>();state.cultivationStage=(CultivationStage)(target-1);state.discoveredSpiritualEnergy=true;state.ResetEffects();state.TryInitializeNetwork(state.CaptureSnapshot());
                    player.inventory[0]=new Item(type,2);player.inventory[0].GetGlobalItem<PillQualitySystem>().LoadData(player.inventory[0],new TagCompound {["quality"]=(int)quality,["crafted"]=true});return player;
                }
                var authority=Progression(NetmodeID.Server);CultivationItemTransactions.HandleRequest(authority,0,type);var authoritative=authority.GetModPlayer<XianXiaPlayer>();var effects=AuthoritativeBuffSync.Capture(authority);
                check((int)authoritative.cultivationStage==target&&authority.inventory[0].stack==1,"native progression pill authority advances and consumes once with quality reward");
                var single=Progression(NetmodeID.SinglePlayer);bool? used=ItemLoader.UseItem(single.inventory[0],single);var singleState=single.GetModPlayer<XianXiaPlayer>();var singleBuffs=AuthoritativeBuffSync.Capture(single);
                check(used==true&&(singleState.CaptureSnapshot() with {Revision=0}).Equals(authoritative.CaptureSnapshot() with {Revision=0})&&singleBuffs.Count==effects.Count&&effects.All(kv=>singleBuffs.TryGetValue(kv.Key,out int time)&&time==kv.Value),"native two-progression-pills item loader matches authority stage trial resources and quality effects");
                var beforeFailed=singleState.CaptureSnapshot();var beforeFailedBuffs=AuthoritativeBuffSync.Capture(single);bool? failed=ItemLoader.UseItem(single.inventory[0],single);var afterFailedBuffs=AuthoritativeBuffSync.Capture(single);
                check(failed==false&&singleState.CaptureSnapshot().Equals(beforeFailed)&&beforeFailedBuffs.Count==afterFailedBuffs.Count&&beforeFailedBuffs.All(kv=>afterFailedBuffs.TryGetValue(kv.Key,out int time)&&time==kv.Value),"native failed repeated progression pill never grants early quality rewards");
            }
        } finally {Main.player[0]=previous;Main.netMode=mode;Main.myPlayer=local;clients.CopyTo(Netplay.Clients,0);Netplay.Connection=connection;Main.rand=random;NetMessage.buffer[0].broadcast=broadcast;world.LoadWorldData(savedWorld);}
        check(ReferenceEquals(Main.player[0],previous)&&ReferenceEquals(Netplay.Connection,connection),"native buff pill benefit audit restores owner and engine context");
    }
}
