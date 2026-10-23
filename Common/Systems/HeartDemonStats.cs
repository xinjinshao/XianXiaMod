using System;
using System.IO;
using Microsoft.Xna.Framework;
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
    public const float MaximumOwnerDistance = 4000f;
    private bool heartDemon, rejected;
    private short ownerSlot = -1;
    private long ownerSession;
    private Player owner;
    public bool IsHeartDemon => heartDemon;
    public bool IsRejected => rejected;
    public override bool InstancePerEntity => true;
    private static bool Supports(NPC npc) => npc.type == ModContent.NPCType<TribulationCloudling>() || npc.type == ModContent.NPCType<StarEclipsedCultivator>();
    public override bool AppliesToEntity(NPC entity,bool lateInstantiation) => Supports(entity);
    public override void SetDefaults(NPC entity) { heartDemon = rejected = false; ownerSlot = -1; ownerSession = 0; owner = null; }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
    private bool Valid(NPC npc,bool dying = false)
    {
        if (rejected || !npc.active || (!dying && npc.life <= 0) || ownerSlot < 0 || ownerSlot >= Main.maxPlayers || ownerSession <= 0
            || Main.player == null || ownerSlot >= Main.player.Length || Main.player[ownerSlot] is not Player player
            || !player.active || player.dead || !Finite(player.Center) || !Finite(player.velocity) || !Finite(npc.Center) || !Finite(npc.velocity)
            || Vector2.DistanceSquared(npc.Center,player.Center) > MaximumOwnerDistance * MaximumOwnerDistance
            || (Main.netMode != NetmodeID.MultiplayerClient && !ReferenceEquals(owner,player))
            || !player.TryGetModPlayer<XianXiaPlayer>(out var state)) return false;
        return state.tribulationTimer > 0 && state.tribulationKind == XianXiaPlayer.TribulationKind.HeartDemon && state.TribulationSession == ownerSession;
    }
    public bool IsSourceValid(NPC npc) => !heartDemon || Valid(npc);
    private void Cancel(NPC npc)
    {
        rejected = true; npc.damage = npc.defDamage = 0; npc.velocity = Vector2.Zero;
        if (!npc.active) return;
        npc.active = false;
        if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC,number:npc.whoAmI);
    }
    public override void OnSpawn(NPC npc,IEntitySource source)
    {
        if (heartDemon || Main.netMode == NetmodeID.MultiplayerClient || !Supports(npc)
            || source is not EntitySource_Parent { Entity: Player player, Context: SpawnContext }) return;
        heartDemon = true; owner = player;
        if (player.whoAmI >= 0 && player.whoAmI < Main.maxPlayers && Main.player != null && player.whoAmI < Main.player.Length
            && ReferenceEquals(Main.player[player.whoAmI],player) && player.TryGetModPlayer<XianXiaPlayer>(out var state)) {
            ownerSlot = (short)player.whoAmI; ownerSession = state.TribulationSession;
        }
        if (!Valid(npc)) { Cancel(npc); return; }
        npc.lifeMax = Math.Max(1,(int)(npc.lifeMax * 0.5f)); npc.life = npc.lifeMax;
        npc.damage = Math.Max(1,npc.damage * 2 / 3); npc.defDamage = npc.damage; npc.netUpdate = true;
    }
    public override bool PreAI(NPC npc)
    {
        if (!heartDemon || Valid(npc)) return true;
        npc.velocity = Vector2.Zero;
        if (Main.netMode != NetmodeID.MultiplayerClient) Cancel(npc);
        else if (rejected) npc.damage = npc.defDamage = 0;
        return false;
    }
    public override bool CanHitPlayer(NPC npc,Player target,ref int cooldownSlot) => !heartDemon || Valid(npc);
    public override bool CanHitNPC(NPC npc,NPC target) => !heartDemon || Valid(npc);
    public override bool? CanBeHitByItem(NPC npc,Player player,Item item) => !heartDemon || Valid(npc) ? null : false;
    public override bool? CanBeHitByProjectile(NPC npc,Projectile projectile) => !heartDemon || Valid(npc) ? null : false;
    public override bool CheckDead(NPC npc)
    {
        if (!heartDemon) return true;
        if (Main.netMode == NetmodeID.MultiplayerClient) { npc.life = 1; return false; }
        if (Valid(npc,dying:true)) return true;
        Cancel(npc); return false;
    }
    public override void SendExtraAI(NPC npc,BitWriter bitWriter,BinaryWriter writer)
    {
        writer.Write(heartDemon); if (!heartDemon) return;
        writer.Write(npc.lifeMax); writer.Write(Math.Clamp(npc.life,0,Math.Max(0,npc.lifeMax))); writer.Write(npc.damage);
        writer.Write(ownerSlot); writer.Write(ownerSession); writer.Write(rejected);
    }
    public override void ReceiveExtraAI(NPC npc,BitReader bitReader,BinaryReader reader)
    {
        bool tagged = reader.ReadBoolean();
        if (!tagged) { if (Main.netMode == NetmodeID.MultiplayerClient && heartDemon) rejected = true; return; }
        int maximum = reader.ReadInt32(), current = reader.ReadInt32(), damage = reader.ReadInt32();
        short slot = reader.ReadInt16(); long session = reader.ReadInt64(); bool cancelled = reader.ReadBoolean();
        if (Main.netMode != NetmodeID.MultiplayerClient || maximum <= 0 || current < 0 || current > maximum || damage < 0
            || (!cancelled && (slot < 0 || slot >= Main.maxPlayers || session <= 0))) return;
        heartDemon = true; rejected |= cancelled; ownerSlot = slot; ownerSession = session;
        npc.lifeMax = maximum; npc.life = current; npc.damage = npc.defDamage = rejected ? 0 : damage;
    }
}
