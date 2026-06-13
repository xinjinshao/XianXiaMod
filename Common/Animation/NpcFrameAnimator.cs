using Terraria;

namespace XianXia.Common.Animation;

public static class NpcFrameAnimator
{
    public const int EnemyFrameCount = 6;
    public const int BossFrameCount = 6;
    public const int TownFrameCount = 4;

    public static void Animate(NPC npc, int frameHeight, int frameCount, int ticksPerFrame)
    {
        if (frameCount <= 1 || frameHeight <= 0)
        {
            npc.frame.Y = 0;
            return;
        }

        npc.frameCounter++;
        if (npc.frameCounter >= ticksPerFrame)
        {
            npc.frameCounter = 0;
            int nextFrame = npc.frame.Y / frameHeight + 1;
            if (nextFrame >= frameCount)
            {
                nextFrame = 0;
            }

            npc.frame.Y = nextFrame * frameHeight;
        }
    }
}
