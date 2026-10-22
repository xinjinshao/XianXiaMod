using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Content.NPCs.Enemies;
namespace XianXia.Common.Systems;
public class SpiritTideEnemies : GlobalNPC
{
    public override bool InstancePerEntity=>true;
    private bool tideEnemy;
    private uint tideRun;
    private static int Slime=>ModContent.NPCType<WanderingSpiritSlime>();
    private static int Worm=>ModContent.NPCType<ShatteredJadeWorm>();
    private static int Bat=>ModContent.NPCType<TalismanBat>();
    public override void EditSpawnPool(IDictionary<int,float> pool,NPCSpawnInfo info)
    {
        if(!SpiritTideSystem.Active||Main.dayTime||!info.Player.ZoneOverworldHeight||info.PlayerInTown)return;
        pool.Clear();pool[Slime]=1f;pool[Worm]=SpiritTideSystem.Wave>=2?1f:.4f;pool[Bat]=SpiritTideSystem.Wave>=3?1.4f:.5f;
    }
    public override void OnSpawn(NPC npc,IEntitySource source)
    {
        if(Main.netMode==NetmodeID.MultiplayerClient||!SpiritTideSystem.Active||Main.dayTime
            ||source is not EntitySource_SpawnNPC||npc.Center.Y>=Main.worldSurface*16||npc.SpawnedFromStatue
            ||(npc.type!=Slime&&npc.type!=Worm&&npc.type!=Bat))return;
        tideEnemy=true; tideRun=SpiritTideSystem.RunId;
        npc.lifeMax*=1+SpiritTideSystem.Wave;npc.life=npc.lifeMax;
        npc.damage=(int)Math.Ceiling(npc.damage*(1f+.25f*SpiritTideSystem.Wave));npc.defense+=SpiritTideSystem.Wave*3;npc.netUpdate=true;
    }
    public override void OnKill(NPC npc)
    {
        if(tideEnemy) { tideEnemy=false;if(tideRun==SpiritTideSystem.RunId)SpiritTideSystem.RegisterKill(npc); }
    }
    public override void SendExtraAI(NPC npc,BitWriter bitWriter,BinaryWriter writer)
    {
        writer.Write(tideEnemy);
        if(!tideEnemy)return;
        writer.Write(tideRun);writer.Write(npc.lifeMax);writer.Write(npc.damage);writer.Write(npc.defense);writer.Write(npc.life);
    }
    public override void ReceiveExtraAI(NPC npc,BitReader bitReader,BinaryReader reader)
    {
        bool incoming = reader.ReadBoolean();
        if (!incoming) { tideEnemy = false; tideRun = 0; return; }
        uint run = reader.ReadUInt32();
        int life = reader.ReadInt32(), damage = reader.ReadInt32(), defense = reader.ReadInt32();
        int current = reader.ReadInt32();
        if (life <= 0 || damage < 0 || defense < 0 || current < 0 || current > life) return;
        tideEnemy = true; tideRun = run;
        npc.lifeMax = life; npc.damage = damage; npc.defense = defense;
        // Native SyncNPC can encode full life relative to the unboosted local maximum.
        npc.life = current;
    }
}
