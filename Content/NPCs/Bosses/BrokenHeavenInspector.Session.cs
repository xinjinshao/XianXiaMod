using System.IO;
using System.Threading;
using Terraria;
using Terraria.ID;

namespace XianXia.Content.NPCs.Bosses;

public partial class BrokenHeavenInspector
{
    private static long nextSummonSession;
    private long summonSession;

    // Allocate on the authority before either a spawn packet or a child captures it.
    internal long SummonSession {
        get {
            if (summonSession == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                summonSession = Interlocked.Increment(ref nextSummonSession);
            return summonSession;
        }
    }

}
