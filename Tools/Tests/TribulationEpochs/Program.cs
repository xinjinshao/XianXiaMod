using XianXia.Common.Players;
int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
long sequence=100;var state=new TribulationEpochState();long Next()=>++sequence;
long idle=state.Prepare(false,0,Next);Check(idle<0&&state.Prepare(false,0,Next)==idle,"Inactive baseline stable without allocating every update");
state.MarkBroadcast(idle);Check(state.LastBroadcast==idle,"Broadcast marker tracks exactly the emitted epoch");
long first=Next();Check(state.Prepare(true,first,Next)==first&&state.Prepare(true,first,Next)==first,"Active trial uses its authority session unchanged");
long ended=state.Prepare(false,0,Next);Check(-ended>first,"Ending a trial emits a newer retired epoch");
long second=Next();Check(state.Prepare(true,second,Next)==second&&second>-ended,"Retry creates a newer active epoch");
long seen=0;var client=new TribulationEpochState();Check(client.ClientSession(true)==0,"Client waits for identity despite an active resource snapshot");
Check(client.TryReceive(first,ref seen)&&client.ClientSession(false)==0&&client.ClientSession(true)==first,"Identity-first ordering waits for active timer");
Check(client.TryReceive(first,ref seen),"Duplicate current metadata accepted");Check(client.TryReceive(ended,ref seen)&&client.ClientSession(true)==0,"Retirement blocks old hazards even if an old active timer arrives");
Check(!client.TryReceive(first,ref seen)&&client.ClientSession(true)==0,"Old active packet cannot revive retired trial");
Check(!client.TryReceive(-ended,ref seen),"Same epoch cannot switch retirement sign");
Check(client.TryReceive(second,ref seen)&&client.ClientSession(true)==second,"New trial gets its own identity");
foreach(long bad in new[]{0L,long.MinValue,first,ended}){long before=seen;Check(!client.TryReceive(bad,ref seen)&&seen==before&&client.ClientSession(true)==second,"Invalid or stale epochs cannot mutate current identity or history");}
client.InvalidateReplica();Check(client.ClientSession(true)==0&&!client.TryReceive(second,ref seen),"Death clears local identity and refuses the previous active packet");
var reused=new TribulationEpochState();Check(!reused.TryReceive(second,ref seen),"A replacement player in the same slot refuses previous occupant replay");
long next=Next();Check(reused.TryReceive(next,ref seen)&&reused.ClientSession(true)==next,"Replacement waits for a fresh server identity");
reused.Reset();seen=0;Check(reused.TryReceive(1,ref seen),"New world/server may start an independent authority sequence");
state.Reset();Check(state.LastBroadcast==0&&state.ClientSession(true)==0,"Initialization reset clears all local epoch state");
Console.WriteLine($"Tribulation epochs: {checks} checks passed against actual state source; transport separate.");
