using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.NPCs.Enemies;

namespace XianXia.Common.Systems;

public class HeartDemonStats : GlobalNPC
{
    public const string SpawnContext = "XianXiaHeartDemon";
    private bool heartDemon;
    public bool IsHeartDemon => heartDemon;
    public override bool InstancePerEntity => true;
    private static bool Supports(NPC npc) => npc.type == ModContent.NPCType<TribulationCloudling>() || npc.type == ModContent.NPCType<StarEclipsedCultivator>();
    public override bool AppliesToEntity(NPC entity,bool lateInstantiation) => Supports(entity);
    public override void SetDefaults(NPC entity) => heartDemon = false;
    public override void OnSpawn(NPC npc,IEntitySource source)
    {
        if (heartDemon || Main.netMode == NetmodeID.MultiplayerClient || !Supports(npc)
            || source is not EntitySource_Parent { Entity: Player player, Context: SpawnContext } || !player.active || player.dead
            || !player.TryGetModPlayer<XianXiaPlayer>(out var state) || state.tribulationTimer <= 0
            || state.tribulationKind != XianXiaPlayer.TribulationKind.HeartDemon) return;
        heartDemon = true;
        npc.lifeMax = Math.Max(1,(int)(npc.lifeMax * 0.5f)); npc.life = npc.lifeMax;
        npc.damage = Math.Max(1,npc.damage * 2 / 3); npc.defDamage = npc.damage; npc.netUpdate = true;
    }
    public override void SendExtraAI(NPC npc,BitWriter bitWriter,BinaryWriter writer)
    {
        writer.Write(heartDemon);
        if (!heartDemon) return;
        writer.Write(npc.lifeMax); writer.Write(npc.life); writer.Write(npc.damage);
    }
    public override void ReceiveExtraAI(NPC npc,BitReader bitReader,BinaryReader reader)
    {
        bool tagged = reader.ReadBoolean();
        if (!tagged) { if (Main.netMode == NetmodeID.MultiplayerClient) heartDemon = false; return; }
        int maximum = reader.ReadInt32(), current = reader.ReadInt32(), damage = reader.ReadInt32();
        if (Main.netMode != NetmodeID.MultiplayerClient || maximum <= 0 || current < 0 || current > maximum || damage < 0) return;
        heartDemon = true; npc.lifeMax = maximum; npc.life = current; npc.damage = damage; npc.defDamage = damage;
    }
}
