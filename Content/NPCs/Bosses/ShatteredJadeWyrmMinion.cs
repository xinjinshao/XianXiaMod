using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Bosses;

public class ShatteredJadeWyrmMinion : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.BossFrameCount;
    }

    public override string Texture => "XianXia/Content/NPCs/Enemies/ShatteredJadeWorm";

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ShatteredJadeWyrmMinion"));
    }

    public override void SetDefaults()
    {
        NPC.width = 44;
        NPC.height = 20;
        NPC.damage = 14;
        NPC.defense = 2;
        NPC.lifeMax = 70;
        NPC.knockBackResist = 0.2f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 8);
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        Vector2 desired = target.Center - NPC.Center;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * 4.5f, 0.05f);
        NPC.rotation = NPC.velocity.ToRotation();
    }
}
