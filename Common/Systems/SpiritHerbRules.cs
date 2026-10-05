using System;

namespace XianXia.Common.Systems;

public static class SpiritHerbRules
{
    public const int FrameWidth = 18;
    public const int MatureStage = 2;
    public static int Stage(int frameX) => Math.Clamp(frameX / FrameWidth, 0, MatureStage);
    public static short NextFrame(int frameX) => (short)(Math.Min(MatureStage, Stage(frameX) + 1) * FrameWidth);
    public static int RootYield(int frameX) => Stage(frameX) == MatureStage ? 1 : 0;
    public static int SeedYield(int frameX, int roll) => Stage(frameX) == MatureStage ? 1 + Math.Clamp(roll, 0, 2) : 1;
    public static float DrawScale(int frameX) => Stage(frameX) switch { 0 => 0.4f, 1 => 0.7f, _ => 1f };
}
