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
    private static void AuditCombatPillBenefits(Action<bool,string> check)
    {
        Player previous=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var clients=(RemoteClient[])Netplay.Clients.Clone();var connection=Netplay.Connection;var random=Main.rand;bool broadcast=NetMessage.buffer[0].broadcast;
        var mod=ModContent.GetInstance<global::XianXia.XianXia>();using var probe=mod.GetPacket();int offset=(int)probe.BaseStream.Position;var output=new AuditOutputSocket(true);
        int fatigue=ModContent.BuffType<QiRecoveryCooldownBuff>();
        var cases=new[]{(ModContent.ItemType<FurnaceGuardPill>(),ModContent.BuffType<FurnaceGuardBuff>(),ModContent.BuffType<WindStepBuff>(),3600),(ModContent.ItemType<WindStepPill>(),ModContent.BuffType<WindStepBuff>(),ModContent.BuffType<ThunderBurstBuff>(),2700),(ModContent.ItemType<ThunderBurstPill>(),ModContent.BuffType<ThunderBurstBuff>(),ModContent.BuffType<FurnaceGuardBuff>(),1200)};
        void Deliver(byte[] payload){using var reader=new BinaryReader(new MemoryStream(payload));mod.HandlePacket(reader,256);}
        (Player Actor,Item Pill) Fixture(int netMode,int type,int main,int other,PillQuality grade,int scenario) {
            Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;var actor=new Player {whoAmI=0,active=true,statLife=100,selectedItem=0};Main.player[0]=actor;
            var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=scenario==7?CultivationStage.QiAwakening:CultivationStage.Foundation;state.discoveredSpiritualEnergy=true;state.ResetEffects();state.spiritualEnergy=state.maxSpiritualEnergy-(scenario is 2 or 5 or 6 or 7 or 8?20:0);state.spiritPressure=50;state.TryInitializeNetwork(state.CaptureSnapshot());
            Item pill=new Item(type,2);actor.inventory[0]=pill;pill.GetGlobalItem<PillQualitySystem>().LoadData(pill,new TagCompound {["quality"]=(int)grade,["crafted"]=true});actor.buffImmune[main]=scenario is 1 or 2 or 3 or 4;actor.buffImmune[BuffID.Regeneration]=scenario is 1 or 2;
            if(scenario is 0 or 4)actor.AddBuff(BuffID.Regeneration,2000);if(scenario==5)actor.AddBuff(other,100);if(scenario==6)actor.AddBuff(fatigue,1800);if(scenario==8)actor.AddBuff(main,5);return(actor,pill);
        }
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=i==0?output:new AuditOutputSocket(),State=i==0?10:0,IsActive=i==0};NetMessage.buffer[0].broadcast=true;
            foreach(var entry in cases)foreach(PillQuality grade in Enum.GetValues<PillQuality>())foreach(int scenario in Enumerable.Range(0,9)) {
                Main.rand=new Terraria.Utilities.UnifiedRandom(335);var fixture=Fixture(NetmodeID.Server,entry.Item1,entry.Item2,entry.Item3,grade,scenario);Player actor=fixture.Actor;Item pill=fixture.Pill;var state=actor.GetModPlayer<XianXiaPlayer>();var before=state.CaptureSnapshot();var beforeBuffs=AuthoritativeBuffSync.Capture(actor);
                bool high=grade is PillQuality.Fine or PillQuality.Spirit,ready=scenario==0||high&&(scenario is 2 or 3);
                check(pill.ModItem.CanUseItem(actor)==ready&&state.CaptureSnapshot().Equals(before),"native combat pill gate excludes useless immunity and preserves realm mutual effect and fatigue restrictions");
                output.Sent.Clear();CultivationItemTransactions.HandleRequest(actor,0,entry.Item1);
                int expectedEnergy=ready?Math.Min(state.maxSpiritualEnergy,before.Energy+PillQualityRules.BonusEnergy(grade)):before.Energy;
                check(pill.stack==(ready?1:2)&&state.spiritualEnergy==expectedEnergy&&state.spiritPressure==50&&state.ResourceRevision==before.Revision+(ready?1:0),"native combat pills consume only for main or quality benefits and never lower pressure");
                int main=actor.FindBuffIndex(entry.Item2),cooldown=actor.FindBuffIndex(fatigue),regen=actor.FindBuffIndex(BuffID.Regeneration);
                int expectedMain=scenario==0?PillQualityRules.Scale(entry.Item4,grade):scenario==8?5:0;
                int expectedFatigue=ready||scenario==6?1800:0,expectedRegen=scenario is 0 or 4?2000:ready&&scenario==3?PillQualityRules.RegenerationTicks(grade):0;
                check((main<0?0:actor.buffTime[main])==expectedMain&&(cooldown<0?0:actor.buffTime[cooldown])==expectedFatigue&&(regen<0?0:actor.buffTime[regen])==expectedRegen,"native combat pill main immunity quality rewards and shared fatigue resolve exactly once");
                var afterBuffs=AuthoritativeBuffSync.Capture(actor);byte[][] effects=output.Sent.Where(p=>p.Length>2&&p[2]==55).ToArray();byte[][] canonical=output.Sent.Where(p=>p.Length>offset&&(p[offset]==4||p[offset]==13)).ToArray();
                check(effects.Length==afterBuffs.Count(kv=>!beforeBuffs.TryGetValue(kv.Key,out int time)||time!=kv.Value)&&canonical.Count(p=>p[offset]==4)==1&&canonical.Count(p=>p[offset]==13)==1,"native combat pill sends each changed effect once and returns canonical owner state on refusal");
                var single=Fixture(NetmodeID.SinglePlayer,entry.Item1,entry.Item2,entry.Item3,grade,scenario);check(single.Pill.ModItem.CanUseItem(single.Actor)==ready,"native combat pill single-player gate matches authority eligibility");
                bool? used=ready?ItemLoader.UseItem(single.Pill,single.Actor):single.Pill.ModItem.UseItem(single.Actor);var singleState=single.Actor.GetModPlayer<XianXiaPlayer>();var singleBuffs=AuthoritativeBuffSync.Capture(single.Actor);
                check(used==ready&&(singleState.CaptureSnapshot() with {Revision=0}).Equals(state.CaptureSnapshot() with {Revision=0})&&singleBuffs.Count==afterBuffs.Count&&afterBuffs.All(kv=>singleBuffs.TryGetValue(kv.Key,out int time)&&time==kv.Value),"native combat pill item loader matches authority main quality and fatigue outcomes");
                Main.netMode=NetmodeID.MultiplayerClient;Main.myPlayer=0;Netplay.Connection=new RemoteServer();var receiver=new Player {whoAmI=0,active=true,statLife=100};Main.player[0]=receiver;receiver.inventory[0]=new Item(entry.Item1,2);receiver.GetModPlayer<XianXiaPlayer>().ApplySnapshot(before);Array.Copy(actor.buffImmune,receiver.buffImmune,actor.buffImmune.Length);foreach(var kv in beforeBuffs)receiver.AddBuff(kv.Key,kv.Value);
                for(int replay=0;replay<2;replay++) {
                    foreach(byte[] packet in canonical)Deliver(packet[offset..]);
                    foreach(byte[] packet in effects){byte[] payload=packet[2..];using var reader=new BinaryReader(new MemoryStream(payload));var buffer=new MessageBuffer {whoAmI=256,readBuffer=payload,reader=reader};buffer.GetData(0,payload.Length,out int kind);check(kind==55,"official combat pill owner decodes actual primary fatigue and quality packets");}
                }
                var received=AuthoritativeBuffSync.Capture(receiver);check(receiver.inventory[0].stack==pill.stack&&receiver.GetModPlayer<XianXiaPlayer>().CaptureSnapshot().Equals(state.CaptureSnapshot())&&received.Count==afterBuffs.Count&&afterBuffs.All(kv=>received.TryGetValue(kv.Key,out int time)&&time==kv.Value),"actual combat pill owner replay converges every useful outcome without duplicate material cost");
                Main.player[0]=actor;Main.netMode=NetmodeID.Server;Main.myPlayer=Main.maxPlayers;int sent=output.Sent.Count;int stack=pill.stack;CultivationItemTransactions.HandleRequest(actor,0,entry.Item1);
                check(pill.stack==stack&&output.Sent.Count==sent,"native combat pill duplicate request cannot repeat fatigue or quality bonuses");
            }
        } finally {Main.player[0]=previous;Main.netMode=mode;Main.myPlayer=local;clients.CopyTo(Netplay.Clients,0);Netplay.Connection=connection;Main.rand=random;NetMessage.buffer[0].broadcast=broadcast;}
        check(ReferenceEquals(Main.player[0],previous)&&ReferenceEquals(Netplay.Connection,connection),"native combat pill audit restores owner connection and engine context");
    }
}
