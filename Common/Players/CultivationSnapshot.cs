using System.IO;
using System.Numerics;

namespace XianXia.Common.Players;

// Fixed wire format. Read the whole value before validating or applying it.
public readonly record struct CultivationSnapshot(int Energy, byte Stage, bool Discovered,
    byte Pressure, ushort Timer, byte Intensity, byte TrialStage, byte Kind,
    byte Attempts, ushort ClearedStages, ushort ArrayCooldown, ushort Weakness, uint Revision = 0,
    ushort SkillCooldown = 0, ushort WardTimer = 0)
{
    public int Comprehension => BitOperations.PopCount((uint)ClearedStages);
    public bool IsValid()
    {
        if (Stage > 8 || Energy < 0 || Energy > MaxEnergy(Stage) + Comprehension * 5
            || Pressure > 100 || Timer > 6000 || Attempts > 10 || ArrayCooldown > 480
            || Weakness > 10800 || SkillCooldown > 1200 || WardTimer > 180
            || WardTimer > SkillCooldown || (Stage > 0 && !Discovered)) return false;
        int validMask = Stage < 3 ? 0 : ((1 << (Stage + 1)) - 1) & ~7;
        if ((ClearedStages & ~validMask) != 0) return false;
        if (Timer == 0) return Intensity == 0 && TrialStage == 0 && Kind == 0;
        return Stage >= 3 && TrialStage == Stage && Intensity == Stage - 1
            && Kind == KindFor(Stage) && (ClearedStages & (1 << Stage)) == 0;
    }

    public static int MaxEnergy(int stage) => stage switch
    { 0 or 1 => 40, 2 => 80, 3 => 120, 4 => 180, 5 => 240, 6 => 320, 7 => 420, 8 => 500, _ => 40 };
    public static byte KindFor(int stage) => stage switch { 3 or 4 => 1, 5 => 2, 6 => 3, _ => 4 };

    public void Write(BinaryWriter writer)
    {
        writer.Write(Energy); writer.Write(Stage); writer.Write(Discovered);
        writer.Write(Pressure); writer.Write(Timer); writer.Write(Intensity);
        writer.Write(TrialStage); writer.Write(Kind); writer.Write(Attempts);
        writer.Write(ClearedStages); writer.Write(ArrayCooldown); writer.Write(Weakness);
        writer.Write(Revision);
        writer.Write(SkillCooldown); writer.Write(WardTimer);
    }

    public static CultivationSnapshot Read(BinaryReader reader) => new(reader.ReadInt32(),
        reader.ReadByte(), reader.ReadBoolean(), reader.ReadByte(), reader.ReadUInt16(),
        reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
        reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt32(), reader.ReadUInt16(), reader.ReadUInt16());
}
