using Terraria;
using Terraria.ID;
using XianXia.Common.Systems;
int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
byte[] Wire(NpcEncounterIdentity state,NPC npc){using var stream=new MemoryStream();state.SendExtraAI(npc,null,new BinaryWriter(stream));return stream.ToArray();}
void Read(NpcEncounterIdentity state,NPC npc,long value){using var stream=new MemoryStream(BitConverter.GetBytes(value));state.ReceiveExtraAI(npc,null,new BinaryReader(stream));}
var lifecycle=new NpcEncounterIdentitySystem();lifecycle.ClearWorld();
var npc=new NPC{whoAmI=3};Main.npc[3]=npc;var state=new NpcEncounterIdentity();state.SetDefaults(npc);Main.netMode=NetmodeID.Server;
long first=state.GetIdentity(npc);Check(first>0&&npc.netUpdate&&state.GetIdentity(npc)==first,"Authority lazily assigns one stable identity and requests initial sync");
npc.netUpdate=false;Check(Wire(state,npc).Length==8&&!npc.netUpdate,"Sending the identity does not mark another dirty update");
state.SetDefaults(npc);long second=state.GetIdentity(npc);Check(second>first,"Same NPC object reinitialization gets a new authority identity");
var another=new NPC{whoAmI=4};Main.npc[4]=another;var otherState=new NpcEncounterIdentity();otherState.SetDefaults(another);Check(otherState.GetIdentity(another)>second,"Different entity gets a unique identity");
Main.netMode=NetmodeID.MultiplayerClient;var client=new NpcEncounterIdentity();client.SetDefaults(npc);npc.netUpdate=false;
Check(client.GetIdentity(npc)==0&&!npc.netUpdate,"Client cannot invent an identity while waiting for NPC synchronization");
Read(client,npc,first);Check(client.GetIdentity(npc)==first,"Client accepts first complete server identity");
Read(client,npc,first);Check(client.GetIdentity(npc)==first,"Duplicate identity updates for a live instance remain valid");
for(int length=0;length<8;length++){byte[] before=Wire(client,npc);try{using var stream=new MemoryStream(BitConverter.GetBytes(second)[..length]);client.ReceiveExtraAI(npc,null,new BinaryReader(stream));throw new Exception("Truncated identity accepted");}catch(EndOfStreamException){}Check(before.SequenceEqual(Wire(client,npc))&&NpcEncounterIdentitySystem.ClientLastSeen[3]==first,"Every truncated identity leaves live state and slot history unchanged");}
Read(client,npc,second);Read(client,npc,first);Check(client.GetIdentity(npc)==second,"Old identity cannot overwrite the replacement instance");
foreach(long bad in new[]{0L,-1,long.MinValue}){Read(client,npc,bad);Check(client.GetIdentity(npc)==second,"Nonpositive server identities rejected");}
client.SetDefaults(npc);Check(client.GetIdentity(npc)==0,"Client defaults clear the previous occupant identity");Read(client,npc,second);Check(client.GetIdentity(npc)==0,"Previous occupant packet cannot bind a new waiting instance");Read(client,npc,second+100);Check(client.GetIdentity(npc)==second+100,"New occupant waits for its own newer identity");
var replacement=new NPC{whoAmI=3};Main.npc[3]=replacement;Check(client.GetIdentity(npc)==0,"Detached object cannot provide an identity for its former slot");Check(client.GetIdentity(replacement)==0,"Copied global state cannot carry the old owner's identity onto a client replacement");
foreach(int mode in new[]{NetmodeID.SinglePlayer,NetmodeID.Server}){Main.netMode=mode;var own=new NpcEncounterIdentity();own.SetDefaults(replacement);long id=own.GetIdentity(replacement);Read(own,replacement,long.MaxValue);Check(own.GetIdentity(replacement)==id,"Authority ignores incoming client identity fields");}
foreach(string reset in new[]{"clear","load","unload"}){Main.netMode=NetmodeID.MultiplayerClient;var fresh=new NpcEncounterIdentity();fresh.SetDefaults(replacement);switch(reset){case "clear":lifecycle.ClearWorld();break;case "load":lifecycle.OnWorldLoad();break;case "unload":lifecycle.OnWorldUnload();break;}Read(fresh,replacement,1);Check(fresh.GetIdentity(replacement)==1,"World/connection lifecycle accepts a new server's independent identity sequence");}
replacement.whoAmI=-1;Check(state.GetIdentity(replacement)==0,"Negative NPC slot fails closed");replacement.whoAmI=Main.maxNPCs;Check(state.GetIdentity(replacement)==0,"Out-of-range NPC slot fails closed");
Console.WriteLine($"NPC encounter identities: {checks} checks passed against actual global/system source; NPC transport mocked.");
