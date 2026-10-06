using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Chat;

namespace XianXia.Common.Systems;
public class SpiritTideSystem : ModSystem
{
    public static bool Active { get; private set; }
    public static int Wave { get; private set; }
    public static int Kills { get; private set; }
    public static uint RunId { get; private set; }
    private bool wasDay, dirty;
    private int syncTimer;
    public static int RequiredKills => Wave switch { 1 => 12, 2 => 18, 3 => 24, _ => 0 };
    public override void OnWorldLoad() { RunId=0; Active=false; Wave=Kills=0; wasDay=Main.dayTime; dirty=false; syncTimer=0; }
    public override void OnWorldUnload() { Active=false; Wave=Kills=0; dirty=false; }
    public override void SaveWorldData(TagCompound tag)
    {
        tag["spiritTideActive"]=Active; tag["spiritTideWave"]=Wave; tag["spiritTideKills"]=Kills;
    }
    public override void LoadWorldData(TagCompound tag) { Apply(tag.GetBool("spiritTideActive"),tag.GetInt("spiritTideWave"),tag.GetInt("spiritTideKills")); if(Active)RunId=1; }
    private static void Apply(bool active,int wave,int kills)
    {
        Active=active&&wave>=1&&wave<=3; Wave=Active?wave:0;
        Kills=Active?Math.Clamp(kills,0,RequiredKills-1):0;
    }
    public override void NetSend(BinaryWriter writer) { writer.Write(Active);writer.Write((byte)Wave);writer.Write((byte)Kills); }
    public override void NetReceive(BinaryReader reader) => Apply(reader.ReadBoolean(),reader.ReadByte(),reader.ReadByte());
    private static void Announce(string key)
    {
        if(Main.netMode==NetmodeID.Server) ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Mods.XianXia.SpiritTide."+key),new Color(120,245,220));
        else Main.NewText(Language.GetTextValue("Mods.XianXia.SpiritTide."+key),120,245,220);
    }
    private void Sync()
    {
        if(Main.netMode==NetmodeID.Server) NetMessage.SendData(MessageID.WorldData);
        dirty=false;syncTimer=0;
    }
    public override void PostUpdateWorld()
    {
        if(Main.netMode==NetmodeID.MultiplayerClient)return;
        if(wasDay&&!Main.dayTime&&!Active&&Main.hardMode&&!Main.bloodMoon&&!Main.pumpkinMoon&&!Main.snowMoon&&Main.invasionType==0)
        {
            bool eligible=false,boss=false;
            foreach(var player in Main.ActivePlayers) if(!player.dead&&player.GetModPlayer<Players.XianXiaPlayer>().cultivationStage>=Players.CultivationStage.Foundation) eligible=true;
            foreach(var npc in Main.ActiveNPCs) if(npc.boss) boss=true;
            if(eligible&&!boss&&Main.rand.NextBool(8)) { RunId++;Active=true;Wave=1;Kills=0;Announce("Started");Sync(); }
        }
        wasDay=Main.dayTime;
        if(Active&&Main.dayTime) { Active=false;Wave=Kills=0;Announce("DawnEnded");Sync(); }
        if(dirty&&++syncTimer>=30)Sync();
    }
    public static void RegisterKill(NPC npc)
    {
        if(!Active||Main.netMode==NetmodeID.MultiplayerClient||Main.dayTime)return;
        var system=ModContent.GetInstance<SpiritTideSystem>();
        if(++Kills>=RequiredKills)
        {
            Kills=0;
            if(Wave<3) { Wave++;Announce("WaveAdvanced"); }
            else
            {
                Active=false;Wave=0;Announce("Completed");
                Item.NewItem(npc.GetSource_Loot(),npc.Hitbox,ModContent.ItemType<global::XianXia.Content.Items.Accessories.SpiritTidePearl>());
                Item.NewItem(npc.GetSource_Loot(),npc.Hitbox,ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(),20);
                Item.NewItem(npc.GetSource_Loot(),npc.Hitbox,ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SpiritHerbSeeds>(),5);
                Item.NewItem(npc.GetSource_Loot(),npc.Hitbox,ModContent.ItemType<global::XianXia.Content.Items.Materials.QiRecoveryPill>(),2);
            }
            system.Sync();
        }
        else system.dirty=true;
    }
}
