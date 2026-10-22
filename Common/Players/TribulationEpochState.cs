using System;

namespace XianXia.Common.Players;

// Positive epochs identify active trials; newer negative epochs retire them.
public sealed class TribulationEpochState
{
    private long serverEpoch, clientEpoch;
    public long LastBroadcast { get; private set; }
    public long ClientSession(bool timerActive) => timerActive && clientEpoch > 0 ? clientEpoch : 0;
    public void Reset() { serverEpoch = clientEpoch = LastBroadcast = 0; }
    public void InvalidateReplica() => clientEpoch = 0;
    public long Prepare(bool active,long session,Func<long> allocate)
    {
        if (active) serverEpoch = session > 0 ? session : 0;
        else if (serverEpoch >= 0) { long next = allocate(); serverEpoch = next > 0 ? -next : 0; }
        return serverEpoch;
    }
    public void MarkBroadcast(long epoch) => LastBroadcast = epoch;
    public bool TryReceive(long incoming,ref long lastSeen)
    {
        if (incoming == 0 || incoming == long.MinValue) return false;
        long magnitude = Math.Abs(incoming);
        if (magnitude < lastSeen || (magnitude == lastSeen && incoming != clientEpoch)) return false;
        clientEpoch = incoming; lastSeen = magnitude;
        return true;
    }
}
