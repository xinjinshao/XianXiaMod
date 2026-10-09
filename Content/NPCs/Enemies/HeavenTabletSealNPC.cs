using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.NPCs.Bosses;

namespace XianXia.Content.NPCs.Enemies;

public class HeavenTabletSealNPC : ModNPC
{
    public override string Texture => "XianXia/Content/Projectiles/SpiritBolt";
    public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;
    private short parentSlot = -1, remaining = HeavenTabletGuardian.SealShieldTicks;
    private long parentSession;
    private byte order = 255;
    private HeavenTabletGuardian Parent => parentSlot >= 0 && parentSlot < Main.maxNPCs
        && Main.npc[parentSlot]?.ModNPC is HeavenTabletGuardian parent && parent.SealSession == parentSession
        && parentSession > 0 && BossTargeting.HasLivingTarget(parent.NPC) ? parent : null;
    internal bool MatchesParent(HeavenTabletGuardian parent, int expectedOrder) => order == expectedOrder
        && parentSlot == parent.NPC.whoAmI && parentSession > 0 && parentSession == parent.SealSession;
    public override void SetStaticDefaults()
        => NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
    public override void SetDefaults()
    {
        NPC.width = NPC.height = 40; NPC.lifeMax = 1200; NPC.defense = 12; NPC.damage = 0; NPC.value = 0;
        NPC.noGravity = true; NPC.noTileCollide = true; NPC.knockBackResist = 0; NPC.aiStyle = -1;
        NPC.dontTakeDamage = true; NPC.npcSlots = 0;
    }
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        => NPC.lifeMax = BossStatRules.ScaleLife(NPC.lifeMax, balance, bossAdjustment);
    public override void OnSpawn(IEntitySource source)
    {
        if (source is not EntitySource_Parent { Entity: NPC npc } || npc.ModNPC is not HeavenTabletGuardian parent) return;
        parentSlot = (short)npc.whoAmI; parentSession = parent.SealSession;
        if (float.IsFinite(NPC.ai[1]) && NPC.ai[1] >= 0 && NPC.ai[1] <= 3 && NPC.ai[1] == MathF.Truncate(NPC.ai[1]))
            order = (byte)NPC.ai[1];
    }
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
    public override bool CanHitNPC(NPC target) => false;
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
    private bool Vulnerable => NPC.active && NPC.life > 0 && remaining > 0 && Finite(NPC.Center)
        && Finite(NPC.velocity) && Parent?.IsCurrentSeal(this) == true;
    public override bool? CanBeHitByItem(Player player, Item item) => Vulnerable && player.active && !player.dead
        && Finite(player.Center) && Finite(player.velocity)
        && Collision.CanHitLine(player.Center, 1, 1, NPC.Center, 1, 1);
    public override bool? CanBeHitByProjectile(Projectile projectile) => Vulnerable && projectile.friendly
        && Finite(projectile.Center) && Finite(projectile.velocity)
        && Collision.CanHitLine(projectile.Center, 1, 1, NPC.Center, 1, 1);
    public override bool CheckDead()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient && Parent is HeavenTabletGuardian parent && parent.IsCurrentSeal(this)) {
            parent.BreakSeal(this);
            // Consume the puzzle entity without NPC death/loot/kill-progression hooks.
            if (NPC.active) {
                NPC.active = false; NPC.damage = 0;
                if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
            return false;
        }
        NPC.life = 1; return false;
    }
    public override void AI()
    {
        NPC.damage = 0; NPC.velocity = Vector2.Zero;
        HeavenTabletGuardian parent = Parent;
        if (parent == null || order > 3 || remaining <= 0) {
            NPC.dontTakeDamage = true;
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.active) {
                NPC.active = false;
                if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
            return;
        }
        Vector2 offset = order switch { 0 => new(0, -160), 1 => new(160, 0), 2 => new(0, 160), _ => new(-160, 0) };
        NPC.Center = parent.NPC.Center + offset;
        NPC.dontTakeDamage = !Vulnerable;
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        NPC.target = parent.NPC.target;
        remaining--;
        if (remaining % 60 == 0) NPC.netUpdate = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(parentSlot); writer.Write(parentSession); writer.Write(order); writer.Write(remaining);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        short slot = reader.ReadInt16(); long session = reader.ReadInt64(); byte sequence = reader.ReadByte(); short age = reader.ReadInt16();
        parentSlot = slot >= 0 && slot < Main.maxNPCs ? slot : (short)-1; parentSession = session > 0 ? session : 0;
        order = sequence <= 3 ? sequence : (byte)255; remaining = age >= 0 && age <= HeavenTabletGuardian.SealShieldTicks ? age : (short)0;
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return false;
        Vector2 start = NPC.Center - screenPos - new Vector2(20, 20);
        Color color = Vulnerable ? Color.LightGreen : Color.Cyan * 0.35f;
        var pixel = TextureAssets.MagicPixel.Value;
        spriteBatch.Draw(pixel, start, null, color, 0, Vector2.Zero, new Vector2(40, 3), SpriteEffects.None, 0);
        spriteBatch.Draw(pixel, start + new Vector2(0, 37), null, color, 0, Vector2.Zero, new Vector2(40, 3), SpriteEffects.None, 0);
        spriteBatch.Draw(pixel, start, null, color, 0, Vector2.Zero, new Vector2(3, 40), SpriteEffects.None, 0);
        spriteBatch.Draw(pixel, start + new Vector2(37, 0), null, color, 0, Vector2.Zero, new Vector2(3, 40), SpriteEffects.None, 0);
        for (int i = 0; i <= order && i < 4; i++)
            spriteBatch.Draw(pixel, start + new Vector2(8 + i * 7, 16), null, color, 0, Vector2.Zero, new Vector2(3, 8), SpriteEffects.None, 0);
        return false;
    }
}
