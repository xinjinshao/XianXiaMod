using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditNpcIdentities(Action<bool,string> check)
    {
        int slot = Enumerable.Range(0,Main.maxNPCs).First(i => !Main.npc[i].active);
        NPC previous = Main.npc[slot]; int mode = Main.netMode;
        long[] history = (long[])NpcEncounterIdentitySystem.ClientLastSeen.Clone();
        byte[] Encode(NPC npc) { using var stream = new MemoryStream(); npc.GetGlobalNPC<NpcEncounterIdentity>().SendExtraAI(npc,null,new BinaryWriter(stream)); return stream.ToArray(); }
        void Read(NPC npc,byte[] wire) { using var stream = new MemoryStream(wire); npc.GetGlobalNPC<NpcEncounterIdentity>().ReceiveExtraAI(npc,null,new BinaryReader(stream)); }
        NPC Create() { var npc = new NPC { whoAmI = slot }; Main.npc[slot] = npc; npc.SetDefaults(NPCID.BlueSlime); npc.whoAmI = slot; npc.active = true; return npc; }
        try {
            Main.netMode = NetmodeID.Server;
            NPC actor = Create(); var firstState = actor.GetGlobalNPC<NpcEncounterIdentity>();
            long first = firstState.GetIdentity(actor); check(first > 0 && actor.netUpdate,"native NPC encounter identity allocates and dirties real registered vanilla NPC");
            actor.netUpdate = false; byte[] firstWire = Encode(actor);
            check(firstWire.Length == 8 && BitConverter.ToInt64(firstWire) == first && !actor.netUpdate,"native NPC identity sends exactly eight bytes without perpetual dirty state");
            actor.SetDefaults(NPCID.BlueSlime); actor.whoAmI = slot; actor.active = true;
            long second = actor.GetGlobalNPC<NpcEncounterIdentity>().GetIdentity(actor); byte[] secondWire = Encode(actor);
            check(second > first && ReferenceEquals(Main.npc[slot],actor),"native same-object same-type NPC reset rotates encounter identity");
            actor.SetDefaults(NPCID.BlueSlime); actor.whoAmI = slot; actor.active = true;
            long third = actor.GetGlobalNPC<NpcEncounterIdentity>().GetIdentity(actor); byte[] thirdWire = Encode(actor);
            check(third > second,"native subsequent NPC initialization receives another distinct identity");
            Main.netMode = NetmodeID.MultiplayerClient; Array.Clear(NpcEncounterIdentitySystem.ClientLastSeen);
            NPC client = Create(); var clientState = client.GetGlobalNPC<NpcEncounterIdentity>(); client.netUpdate = false;
            check(clientState.GetIdentity(client) == 0 && !client.netUpdate,"native client NPC waits without allocating or dirtying an identity");
            Read(client,firstWire); check(clientState.GetIdentity(client) == first,"native client global receives authority's complete encounter identity");
            for (int length = 0; length < 8; length++) {
                try { Read(client,secondWire[..length]); throw new InvalidOperationException("native truncated NPC identity accepted"); }
                catch (EndOfStreamException) { }
                check(clientState.GetIdentity(client) == first && NpcEncounterIdentitySystem.ClientLastSeen[slot] == first,"native truncated NPC identity preserves state and replay history");
            }
            Read(client,secondWire); Read(client,firstWire);
            check(clientState.GetIdentity(client) == second,"native old NPC identity cannot overwrite a newer same-type instance");
            Read(client,secondWire); check(clientState.GetIdentity(client) == second,"native duplicate current NPC identity is harmless");
            foreach (long bad in new[] { 0L,-1,long.MinValue }) { Read(client,BitConverter.GetBytes(bad)); check(clientState.GetIdentity(client) == second,"native invalid NPC identity is ignored"); }
            client.SetDefaults(NPCID.BlueSlime); client.whoAmI = slot; client.active = true; clientState = client.GetGlobalNPC<NpcEncounterIdentity>();
            Read(client,secondWire); check(clientState.GetIdentity(client) == 0,"native waiting replacement refuses a replay from the previous occupant");
            Read(client,thirdWire); check(clientState.GetIdentity(client) == third,"native replacement activates only after receiving its newer identity");
            Main.netMode = NetmodeID.Server; Main.npc[slot] = actor;
            Read(actor,BitConverter.GetBytes(long.MaxValue));
            check(actor.GetGlobalNPC<NpcEncounterIdentity>().GetIdentity(actor) == third,"native authority ignores incoming identity changes");
        } finally {
            Main.npc[slot].active = false; Main.npc[slot] = previous; Main.netMode = mode;
            history.CopyTo(NpcEncounterIdentitySystem.ClientLastSeen,0);
        }
        check(ReferenceEquals(Main.npc[slot],previous) && Main.netMode == mode && history.SequenceEqual(NpcEncounterIdentitySystem.ClientLastSeen),
            "native NPC identity audit restores slot mode and replay history");
    }
}
