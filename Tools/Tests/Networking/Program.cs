using System.Text;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.NPCs.Town;
using Game = Terraria.Main;

int assertions = 0;
void Check(bool condition, string description)
{
    assertions++;
    if (!condition) throw new Exception(description);
}

var world = new DownedBossSystem();
Game.netMode = NetmodeID.Server;
world.ClearWorld();
DownedBossSystem.MarkDowned("spirit_vein_wyrm");
DownedBossSystem.MarkDowned("spirit_vein_wyrm");
Check(DownedBossSystem.SectReputation == 5 && Terraria.NetMessage.Broadcasts == 1,
    "Repeated kills must not grant reputation or broadcast unchanged state.");
Check(!DownedBossSystem.TryClaimCommission("herb_sect_apprentice_garden", 999), "Reject mismatched reward value.");
Check(!DownedBossSystem.TryClaimCommission("unknown", 8), "Reject unknown commission.");
Check(DownedBossSystem.TryClaimCommission("herb_sect_apprentice_garden", 8), "Allow first authoritative claim.");
Check(!DownedBossSystem.TryClaimCommission("herb_sect_apprentice_garden", 8), "Reject repeated claim.");
Check(DownedBossSystem.SectReputation == 13 && Terraria.NetMessage.Broadcasts == 2, "Claim updates reputation and broadcasts once.");
Game.netMode = NetmodeID.MultiplayerClient;
DownedBossSystem.MarkDowned("garden_warden");
Check(!DownedBossSystem.TryClaimCommission("wandering_artificer_furnace", 8), "Client cannot claim authoritative rewards.");
Check(DownedBossSystem.SectReputation == 13, "Client cannot mutate world progress.");

// Wire-format coverage: every boss subset and every commission subset, with all routes.
int[] bossReputation = {18,10,36,24,10,24,36,60,80,5,18,12}; // Ordinal wire order.
int[] commissionReputation = {16,24,8,12,8};
int Expected(ushort mask, int[] values) => values.Select((value, bit) => (mask & (1 << bit)) != 0 ? value : 0).Sum();
byte[] Snapshot(ushort bosses, ushort commissions, byte route)
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    writer.Write(bosses); writer.Write(commissions); writer.Write(route);
    return stream.ToArray();
}
void RoundTrip(ushort bosses, ushort commissions, byte route)
{
    byte[] bytes = Snapshot(bosses, commissions, route);
    using var reader = new BinaryReader(new MemoryStream(bytes));
    world.NetReceive(reader);
    Check(DownedBossSystem.SectReputation == Expected(bosses, bossReputation) + Expected(commissions, commissionReputation), "Reputation must derive from received flags.");
    Check(DownedBossSystem.DownedSpiritVeinWyrm == ((bosses & (1 << 9)) != 0), "Legacy wyrm flag must follow snapshot.");
    using var output = new MemoryStream();
    using var writer = new BinaryWriter(output);
    world.NetSend(writer);
    Check(output.ToArray().SequenceEqual(bytes), "Snapshot must round-trip exactly, replacing stale flags.");
}
for (ushort mask = 0; mask < 4096; mask++) RoundTrip(mask, 0, (byte)(mask % 4));
for (ushort mask = 0; mask < 32; mask++) RoundTrip(4095, mask, (byte)(mask % 4));
using (var reader = new BinaryReader(new MemoryStream(Snapshot(0, 0, 255)))) world.NetReceive(reader);
Check(DownedBossSystem.ChosenRoute == DownedBossSystem.EndgameRoute.None, "Invalid route must be sanitized.");

// Validate every legal realm/completed-mask combination and exact wire roundtrip.
for (byte stage = 0; stage <= 8; stage++)
{
    int validMask = stage < 3 ? 0 : ((1 << (stage + 1)) - 1) & ~7;
    for (ushort mask = 0; mask < 512; mask++)
    {
        var value = new CultivationSnapshot(0, stage, stage != 0, 0, 0, 0, 0, 0, 0, mask, 0, 0);
        Check(value.IsValid() == ((mask & ~validMask) == 0), "Completed mask must contain only reached trial stages.");
        using var bytes = new MemoryStream();
        using var writer = new BinaryWriter(bytes);
        value.Write(writer); bytes.Position = 0;
        using var reader = new BinaryReader(bytes);
        Check(CultivationSnapshot.Read(reader) == value, "Full cultivation wire state must roundtrip exactly.");
    }
    if (stage >= 3)
    {
        var active = new CultivationSnapshot(0, stage, true, 100, 100, (byte)(stage - 1), stage,
            CultivationSnapshot.KindFor(stage), 10, 0, 480, 10800);
        Check(active.IsValid(), "Every realm has a valid active trial snapshot.");
        Check(!(active with { ClearedStages = (ushort)(1 << stage) }).IsValid(), "Completed trial cannot remain active.");
        Check(!(active with { Kind = 0 }).IsValid(), "Active trial cannot omit its kind.");
        Check(!(active with { Intensity = 0 }).IsValid(), "Active trial strength must match realm.");
        Check(!(active with { TrialStage = 0 }).IsValid(), "Active trial must match current realm.");
        Check(!(active with { Energy = 1000 }).IsValid(), "Energy cap derives from actual realm and completed trials.");
    }
}
var mod = new XianXia.XianXia();
void Packet(int sender, Action<BinaryWriter> write)
{
    using var stream = new MemoryStream();
    using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) write(writer);
    stream.Position = 0;
    using var reader = new BinaryReader(stream);
    mod.HandlePacket(reader, sender);
}
void State(int sender, byte player, int energy, int stage) => Packet(sender, writer =>
{ writer.Write((byte)0); writer.Write(player); writer.Write(energy); writer.Write(stage); writer.Write((ushort)0); writer.Write((uint)0); });
Game.netMode = NetmodeID.Server;
State(0, 1, 40, 1);
Check(Game.player[1].State.spiritualEnergy == 0 && ModPacket.Sent.Count == 0, "Spoofed player index must not mutate or relay.");
State(0, 0, -1, 1); State(0, 0, 1001, 1); State(0, 0, 40, 9);
Check(Game.player[0].State.spiritualEnergy == 0 && ModPacket.Sent.Count == 0, "Reject invalid ranges before mutation.");
for (int length = 0; length < 16; length++) Packet(0, writer => writer.Write(new byte[length]));
Check(Game.player[0].State.spiritualEnergy == 0 && ModPacket.Sent.Count == 0, "Truncated packets must be harmless.");
CultivationSnapshot initial = new(40, 1, true, 0, 0, 0, 0, 0, 0, 0, 0, 0);
void FullState(int sender, byte message, byte index, CultivationSnapshot state) => Packet(sender, writer =>
{ writer.Write(message); writer.Write(index); state.Write(writer); });
State(0, 0, 40, 1);
Check(Game.player[0].State.spiritualEnergy == 0, "Runtime reports cannot initialize a realm.");
FullState(0, 3, 1, initial);
Check(!Game.player[1].State.NetworkInitialized, "Joining snapshot cannot target another player.");
FullState(0, 3, 0, initial with { Stage = 9 });
Check(!Game.player[0].State.NetworkInitialized, "Invalid initial realm is rejected.");
byte[] initialBytes;
using (var stream = new MemoryStream())
{
    using var writer = new BinaryWriter(stream);
    writer.Write((byte)3); writer.Write((byte)0); initial.Write(writer);
    initialBytes = stream.ToArray();
}
for (int length = 0; length < initialBytes.Length; length++)
{
    Packet(0, writer => writer.Write(initialBytes.AsSpan(0, length)));
    Check(!Game.player[0].State.NetworkInitialized, "Partial snapshot must not initialize or partially apply state.");
}
FullState(0, 3, 0, initial);
Check(Game.player[0].State.spiritualEnergy == 40 && ModPacket.Sent.Count == 1, "Initial snapshot is accepted and echoed to owner.");
FullState(0, 3, 0, initial with { Stage = 8 });
FullState(0, 4, 0, initial with { Stage = 8 });
State(0, 0, 40, 8);
Check(Game.player[0].State.cultivationStage == CultivationStage.QiAwakening, "Runtime reports, repeated imports and forged server snapshots cannot advance realm.");
State(0, 0, 41, 1);
Check(Game.player[0].State.spiritualEnergy == 40, "Energy is limited to actual realm maximum.");
State(0, 0, 20, 1);
Check(Game.player[0].State.spiritualEnergy == 40 && ModPacket.Sent.Count == 1, "Even legal-range owner balance reports are retired and cannot mutate resource state.");
Game.netMode = NetmodeID.MultiplayerClient;
FullState(0, 4, 0, initial with { Stage = 3, Timer = 100, TrialStage = 3, Intensity = 2, Kind = 1 });
Check(Game.player[0].State.Snapshot.Timer == 100, "Client receives complete canonical trial state.");
Game.netMode = NetmodeID.Server;

// Actual inventory transaction code; only the Terraria item effect is stubbed.
var held = new Terraria.Item { type = 123, stack = 2, consumable = true,
    ModItem = new ModItem { Mod = mod, Name = "FoundationPill" } };
held.ModItem.Item = held;
Game.player[0].inventory[0] = held;
held.ModItem.Effect = p => p.State.spiritualEnergy--;
void Use(byte slot = 0, int type = 123) => Packet(0, w => { w.Write((byte)5); w.Write(slot); w.Write(type); });
Use(59); Use(0, 999);
Check(held.stack == 2 && held.ModItem.Uses == 0, "Wrong slot/type cannot execute or consume items.");
held.ModItem.Allowed = false;
Use();
Check(held.stack == 2 && held.ModItem.Uses == 0, "Failed server eligibility never consumes item.");
Game.player[0].State.ProgressionItemCooldown = 0;
held.ModItem.Allowed = true;
Use(); Use();
Check(held.stack == 1 && held.ModItem.Uses == 1, "Accepted request executes/consumes once; immediate replay is rate limited.");
Game.player[0].State.ProgressionItemCooldown = 0;
int awardedEnergy = Game.player[0].State.spiritualEnergy;
State(0, 0, 1, 3);
Check(Game.player[0].State.spiritualEnergy == awardedEnergy, "Old energy reports cannot undo authoritative item rewards.");
held.ModItem.Effect = _ => { };
Use();
Check(held.stack == 1, "No-op item effect cannot consume progression materials.");
Game.player[0].State.ProgressionItemCooldown = 0;
held.ModItem.Name = "OtherItem";
Use();
Check(held.ModItem.Uses == 2, "Unregistered item cannot use progression transaction.");
// New recovery pill runs through the same production inventory transaction.
held.ModItem.Name = "QiRecoveryPill"; held.stack = 3; held.ModItem.Allowed = true;
Check(CultivationItemTransactions.IsProgressionItem(held) && PillQualitySystem.IsPill(held), "Recovery pill registered in authority and stored-quality paths");
Game.player[0].State.spiritualEnergy = 0; Game.player[0].State.ProgressionItemCooldown = 0;
held.ModItem.Effect = p => { p.State.RestoreSpiritualEnergy(40); p.AddBuff(2,1800); };
Use(); Check(held.stack == 2 && Game.player[0].State.spiritualEnergy == Math.Min(40, Game.player[0].State.maxSpiritualEnergy) && Game.player[0].buffTime[2] == 1800,"Recovery transaction applies effect and consumes one");
int recoveryUses=held.ModItem.Uses;Use();Check(held.stack==2&&held.ModItem.Uses==recoveryUses,"Immediate recovery replay does not consume twice");
Game.player[0].State.ProgressionItemCooldown=0;held.ModItem.Allowed=false;Use();Check(held.stack==2&&held.ModItem.Uses==recoveryUses,"Cooldown/full-energy eligibility rejection preserves inventory");
held.ModItem.Name="FoundationPill";held.ModItem.Allowed=true;held.ModItem.Effect=_=>{};Game.player[0].buffTime[2]=0;Game.player[0].State.ProgressionItemCooldown=0;
var consumption = new CultivationItemTransactions();
Game.netMode = NetmodeID.MultiplayerClient;
held.ModItem.Name = "FoundationPill";
Check(!consumption.ConsumeItem(held, Game.player[0]), "Client must not independently consume transaction materials.");
Game.netMode = NetmodeID.SinglePlayer;
Check(consumption.ConsumeItem(held, Game.player[0]), "Single player retains normal engine consumption.");
Game.netMode = NetmodeID.Server;

var town = new CultivationTownNPC();
Game.npc[0].ModNPC = town;
void Claim(short index) => Packet(0, writer => { writer.Write((byte)1); writer.Write(index); });
Claim(-1); Claim(4); Claim(0);
Check(town.Claims == 0, "Reject invalid index or absent conversation.");
Game.player[0].talkNPC = 0;
Game.player[0].Center = new() { X = 601 };
Claim(0);
Game.player[0].Center = new(); Game.player[0].dead = true;
Claim(0);
Check(town.Claims == 0, "Reject distant or dead players.");
Game.player[0].dead = false;
Claim(0);
Check(town.Claims == 1 && ModPacket.Sent.Last().Recipient == 0, "Valid commission runs on server and responds only to requester.");
Game.netMode = NetmodeID.MultiplayerClient;
Claim(0);
Check(town.Claims == 1, "Client cannot execute server request locally.");
Game.netMode = NetmodeID.Server;
var sessions = new CultivationNetworkingSystem();
Game.player[0].active = false;
Game.player[0].State.NetworkWasActive = false;
sessions.PostUpdatePlayers();
Check(Game.player[0].State.NetworkInitialized, "Pre-active joining handshake must not discard its imported snapshot.");
Game.player[0].active = true;
sessions.PostUpdatePlayers();
Game.player[0].active = false;
sessions.PostUpdatePlayers();
Check(!Game.player[0].State.NetworkInitialized, "Disconnect releases the one-time import guard for the next character in that slot.");
Game.player[0].active = true;
FullState(0, 3, 0, initial);
Check(Game.player[0].State.NetworkInitialized && Game.player[0].State.cultivationStage == CultivationStage.QiAwakening,
    "A reconnect can import its saved character after the previous active session ended.");
// Actual boss transaction + base item code, with Terraria NPC creation stubbed.
foreach (var npc in Game.npc) npc.active = false;
var boss = new TestBossSummon { Mod = mod, Name = "TestSummon" };
var summonItem = new Terraria.Item { ModItem = boss, type = 456, stack = 3, consumable = true, useTime = 45 };
boss.Item = summonItem;
Game.player[0].inventory[0] = summonItem;
Game.player[0].State.BossSummonCooldown = 0;
Game.player[1].State.NetworkInitialized = true;
var otherBoss = new TestBossSummon { Mod = mod, Name = "TestSummon" };
var otherItem = new Terraria.Item { ModItem = otherBoss, type = 456, stack = 3, consumable = true };
otherBoss.Item = otherItem;
Game.player[1].inventory[0] = otherItem;
void Summon(int sender = 0, byte slot = 0, int type = 456) => Packet(sender, w =>
{ w.Write((byte)6); w.Write(slot); w.Write(type); });
byte[] requestBytes;
using (var bytes = new MemoryStream())
{
    using var writer = new BinaryWriter(bytes);
    writer.Write((byte)6); writer.Write((byte)0); writer.Write(456);
    requestBytes = bytes.ToArray();
}
for (int length = 0; length < requestBytes.Length; length++)
{
    Packet(0, writer => writer.Write(requestBytes.AsSpan(0, length)));
    Check(Terraria.NPC.SpawnCalls == 0 && summonItem.stack == 3, "Truncated summon requests never spawn or consume.");
}
Packet(0, writer => { writer.Write((byte)7); writer.Write("forged feedback"); });
Check(Game.Chat.Count == 0, "Server ignores client-forged feedback packets.");
Summon(0, 59); Summon(0, 0, 999); Summon(-1); Summon(4);
Check(Terraria.NPC.SpawnCalls == 0 && summonItem.stack == 3, "Invalid summon ownership, slot and item type cannot generate bosses or consume material.");
Game.player[0].dead = true; Summon(); Game.player[0].dead = false;
Game.player[0].active = false; Summon(); Game.player[0].active = true;
Check(Terraria.NPC.SpawnCalls == 0, "Dead or inactive summoners are rejected.");
boss.Allowed = false; Summon();
Check(summonItem.stack == 3 && Terraria.NPC.SpawnCalls == 0 && ModPacket.Sent.Last().Data[0] == 7,
    "Server eligibility failure preserves inventory and sends localized feedback.");
Game.player[0].State.BossSummonCooldown = 0;
boss.Allowed = true;
Summon(); Summon(); Summon(1);
Check(Terraria.NPC.SpawnCalls == 1 && Game.npc.Count(n => n.active && n.type == 100) == 1,
    "Repeated requests and two competing players produce only one boss.");
Check(summonItem.stack == 2 && otherItem.stack == 3, "Only the successful summoner spends one material.");
int calls = Terraria.NPC.SpawnCalls;
boss.UseItem(Game.player[0]);
Check(Terraria.NPC.SpawnCalls == calls, "Replicated server item-use hook cannot bypass the transaction.");
Check(!boss.ConsumeItem(Game.player[0]), "Server replicated hook never consumes a second material.");
foreach (var npc in Game.npc) npc.active = true;
Game.npc[0].type = 0;
Game.player[0].State.BossSummonCooldown = 0;
Summon();
Check(summonItem.stack == 2 && !Terraria.NPC.AnyNPCs(100), "NPC capacity exhaustion does not consume material.");
foreach (var npc in Game.npc) npc.active = false;
Game.player[0].State.BossSummonCooldown = 0;
summonItem.stack = 0; Summon();
Check(Terraria.NPC.SpawnCalls == calls + 1, "Empty summon stack is rejected.");
summonItem.stack = 1;
// Standard cursor inventory slot is supported when it is the actual selected slot.
Game.player[0].selectedItem = 58; Game.player[0].inventory[58] = summonItem;
Summon(0, 58);
Check(summonItem.stack == 0 && summonItem.type == 0 && Terraria.NPC.AnyNPCs(100),
    "Successful cursor-slot transaction consumes the last item and turns it to air.");
Game.player[0].selectedItem = 0;
summonItem.type = 456; summonItem.stack = 1;
Game.netMode = NetmodeID.MultiplayerClient;
foreach (var npc in Game.npc) npc.active = false;
calls = Terraria.NPC.SpawnCalls;
int beforeRemote = ModPacket.Sent.Count;
boss.UseItem(Game.player[1]);
Check(ModPacket.Sent.Count == beforeRemote, "Remote clients cannot emit a summon request for another player's animation.");
boss.UseItem(Game.player[0]);
Check(Terraria.NPC.SpawnCalls == calls && ModPacket.Sent.Last().Data[0] == 6 && !boss.ConsumeItem(Game.player[0]),
    "Client requests summoning and never independently generates or consumes.");
Game.netMode = NetmodeID.SinglePlayer;
Check(boss.UseItem(Game.player[0]) == true && boss.ConsumeItem(Game.player[0]) && !boss.ConsumeItem(Game.player[0]),
    "Single-player success generates once and permits exactly one engine consumption.");
Check(boss.UseItem(Game.player[0]) == false && !boss.ConsumeItem(Game.player[0]),
    "Single-player existing boss failure does not permit consumption.");
foreach (var npc in Game.npc) npc.active = true;
Game.npc[0].type = 0;
Check(boss.UseItem(Game.player[0]) == false && !boss.ConsumeItem(Game.player[0]),
    "Single-player capacity failure does not permit consumption.");

// Actual shared world/site/night rules, using the engine boundary's biome flags.
Game.netMode = NetmodeID.Server;
Game.myPlayer = 255;
Game.hardMode = false;
Check(!BossSummonRules.CanUseGeneratedBossSummon(Game.player[0], "old_heaven_dao_core"), "Core summoning requires hardmode.");
Game.hardMode = true;
Terraria.NPC.downedPlantBoss = Terraria.NPC.downedGolemBoss = true;
Check(!BossSummonRules.CanUseGeneratedBossSummon(Game.player[0], "old_heaven_dao_core"), "Core summoning requires Moon Lord.");
Terraria.NPC.downedMoonlord = true;
Check(!BossSummonRules.CanUseGeneratedBossSummon(Game.player[0], "old_heaven_dao_core"), "Core summoning requires its actual server biome.");
Game.player[0].Biomes.Add(typeof(XianXia.Content.Biomes.MoonboneAbyssBiome));
Game.dayTime = true;
Check(!BossSummonRules.CanUseGeneratedBossSummon(Game.player[0], "old_heaven_dao_core"), "Core summoning requires night.");
Game.dayTime = false;
Check(BossSummonRules.CanUseGeneratedBossSummon(Game.player[0], "old_heaven_dao_core"), "Correct server world/site/night conditions allow core summoning.");
// Actual weapon transaction and base shoot path; only engine hooks/spawning are stubbed.
Game.myPlayer = 0;
Game.netMode = NetmodeID.Server;
Game.player[0].State.WeaponShotCooldown = 0;
Game.player[0].State.spiritualEnergy = 20;
Game.player[0].State.arrayDeploymentCooldown = 0;
foreach (var proj in Game.projectile) proj.active = false;
Array.Clear(Game.player[0].ownedProjectileCounts);
var weapon = new TestWeapon { Mod = mod, Name = "TestWeapon" };
var weaponItem = new Terraria.Item { ModItem = weapon, type = 789, stack = 1, shoot = 2, damage = 20 };
weapon.Item = weaponItem;
Game.player[0].inventory[0] = weaponItem;
void Shot(byte slot = 0, int itemType = 789, float x = 100, float y = 0) => Packet(0, w =>
{ w.Write((byte)8); w.Write(slot); w.Write(itemType); w.Write(x); w.Write(y); });
Shot(59); Shot(0, 999); Shot(0, 789, float.NaN); Shot(0, 789, float.PositiveInfinity);
Check(Game.projectile.All(p => !p.active) && Game.player[0].State.spiritualEnergy == 20,
    "Wrong slot/type and nonfinite aim cannot spend energy or spawn projectiles.");
Game.player[0].CCed = true; Shot(); Game.player[0].CCed = false;
Game.player[0].noItems = true; Shot(); Game.player[0].noItems = false;
Check(Game.projectile.All(p => !p.active) && Game.player[0].State.spiritualEnergy == 20,
    "Crowd-controlled or item-disabled players cannot submit firing requests.");
Shot(); Shot();
Check(Game.projectile.Count(p => p.active) == 1 && Game.player[0].State.spiritualEnergy == 16,
    "Accepted attack spends once; replay is rate limited across held weapons.");
Check(Game.projectile.Single(p => p.active).damage == 25 && Game.projectile.Single(p => p.active).type == 2,
    "Shot uses canonical weapon damage and projectile type, never client-supplied values.");
Check(!Game.player[0].State.ApplyingWeaponShot, "Server shoot-hook authorization is cleared after use.");
Game.player[0].State.WeaponShotCooldown = 0;
Game.player[0].State.spiritualEnergy = 3; Shot();
Check(Game.player[0].State.spiritualEnergy == 3 && Game.projectile.Count(p => p.active) == 1,
    "Insufficient energy cannot produce a projectile or partial spend.");
Game.player[0].State.WeaponShotCooldown = 0;
Game.player[0].State.spiritualEnergy = 20;
CombinedHooks.SuppressDefault = true; Shot();
Check(Game.player[0].State.spiritualEnergy == 20 && Game.projectile.Count(p => p.active) == 1,
    "A suppressed shot with no generated projectile refunds the resource transaction.");
CombinedHooks.SuppressDefault = false;
Game.player[0].State.WeaponShotCooldown = 0;
Terraria.Projectile.AllowSpawn = false; Shot();
Check(Game.player[0].State.spiritualEnergy == 20, "Spawn failure restores the pre-shot snapshot.");
Terraria.Projectile.AllowSpawn = true;
foreach (var proj in Game.projectile) proj.active = true;
Game.player[0].State.WeaponShotCooldown = 0; Shot();
Check(Game.player[0].State.spiritualEnergy == 20, "Full projectile capacity does not spend energy.");
foreach (var proj in Game.projectile) proj.active = false;
Array.Clear(Game.player[0].ownedProjectileCounts);
Game.player[0].State.WeaponShotCooldown = 0;
weapon.Array = true; Shot();
Check(Game.player[0].State.arrayDeploymentCooldown == 480 && Game.player[0].State.spiritualEnergy == 16,
    "Array deployment is charged and shares a server cooldown.");
Game.player[0].State.WeaponShotCooldown = 0; Shot();
Check(Game.projectile.Count(p => p.active) == 1 && Game.player[0].State.spiritualEnergy == 16,
    "Server array cooldown blocks a repeated deployment.");
Game.player[0].State.arrayDeploymentCooldown = 0;
Game.player[0].State.WeaponShotCooldown = 0; Shot();
Check(Game.projectile.Count(p => p.active) == 1 && Game.player[0].State.spiritualEnergy == 16,
    "Existing same-type array blocks a new deployment independently of cooldown.");
Game.netMode = NetmodeID.MultiplayerClient;
int packets = ModPacket.Sent.Count;
weapon.Shoot(Game.player[0], null, new(), new(), 999, 9999, 999);
Check(ModPacket.Sent.Count == packets + 1 && ModPacket.Sent.Last().Data[0] == 8 && Game.player[0].State.spiritualEnergy == 16,
    "Client shoot hook only sends an aim request; it neither spends nor creates projectiles.");
Game.netMode = NetmodeID.Server;
Check(!weapon.Shoot(Game.player[0], null, new(), new(), 2, 20, 2), "Replicated server Shoot cannot authorize a default projectile.");
// Buff-only potion transactions and the actual authority-side legacy pill bonus.
Game.netMode = NetmodeID.Server;
Game.rand.Roll = 0.1f;
var recovery = new Terraria.Item { type = 321, stack = 2, consumable = true,
    ModItem = new ModItem { Mod = mod, Name = "SpringReturnPill" } };
recovery.ModItem.Item = recovery;
Game.player[0].inventory[0] = recovery;
recovery.GetGlobalItem<PillQualitySystem>().OnCreated(recovery, new Terraria.DataStructures.RecipeItemCreationContext());
Game.player[0].State.ProgressionItemCooldown = 0;
Game.player[0].State.spiritualEnergy = 10;
Array.Clear(Game.player[0].buffTime);
recovery.ModItem.Effect = p => p.buffTime[2] = 600;
Packet(0, w => { w.Write((byte)5); w.Write((byte)0); w.Write(321); });
Check(recovery.stack == 1 && Game.player[0].buffTime[2] == 600,
    "A buff-only recovery potion consumes exactly once, even when the pre-bonus resource snapshot is unchanged.");
Check(Game.player[0].State.spiritualEnergy == 24 && Game.player[0].buffTime[1] == 900,
    "Stored fine pill bonus is applied once on the server and included in canonical state.");
int bonusEnergy = Game.player[0].State.spiritualEnergy;
Game.netMode = NetmodeID.MultiplayerClient;
PillQualitySystem.ApplyUseBonus(recovery, Game.player[0]);
Check(Game.player[0].State.spiritualEnergy == bonusEnergy, "Client cannot independently roll and apply a quality resource bonus.");
Game.netMode = NetmodeID.Server;
recovery.ModItem.Name = "StarAbyssForbiddenTalisman";
PillQualitySystem.ApplyUseBonus(recovery, Game.player[0]);
Check(Game.player[0].State.spiritualEnergy == bonusEnergy, "Non-pill talismans do not receive a pill bonus.");
// Actual quality hooks: per-batch creation, save/network validation and stacking.
var pill = new Terraria.Item { type = 321, stack = 3, ModItem = new ModItem { Mod = mod, Name = "SpringReturnPill" } };
var quality = pill.GetGlobalItem<PillQualitySystem>();
Game.rand.Roll = 0.01f;
quality.OnCreated(pill, new Terraria.DataStructures.ItemCreationContext());
Check(quality.Quality == PillQuality.Standard && !quality.Crafted, "Non-recipe sources default to a standard pill without a random roll.");
quality.OnCreated(pill, new Terraria.DataStructures.RecipeItemCreationContext());
Check(quality.Quality == PillQuality.Spirit && quality.Crafted, "Recipe output rolls and stores one batch quality.");
Game.rand.Roll = 0.99f;
quality.OnCreated(pill, new Terraria.DataStructures.RecipeItemCreationContext());
Check(quality.Quality == PillQuality.Spirit, "Repeated creation notification cannot reroll an existing crafted batch.");
var savedQuality = new Terraria.ModLoader.IO.TagCompound();
quality.SaveData(pill, savedQuality);
var split = new Terraria.Item { type = 321, ModItem = new ModItem { Mod = mod, Name = "SpringReturnPill" } };
var splitQuality = split.GetGlobalItem<PillQualitySystem>();
splitQuality.SplitStack(split, pill, 1);
Check(splitQuality.Quality == PillQuality.Spirit && splitQuality.Crafted, "Splitting preserves quality and creation provenance.");
Check(quality.CanStack(pill, split) && quality.CanStackInWorld(pill, split), "Same-quality batches may merge in inventory and world.");
var standard = new Terraria.Item { type = 321, ModItem = new ModItem { Mod = mod, Name = "SpringReturnPill" } };
Check(!quality.CanStack(pill, standard) && !quality.CanStackInWorld(pill, standard), "Different quality batches cannot silently merge or overwrite quality.");
var loaded = new PillQualitySystem();
loaded.LoadData(pill, savedQuality);
Check(loaded.Quality == PillQuality.Spirit && loaded.Crafted, "Quality and batch provenance survive save/load.");
loaded.LoadData(pill, new Terraria.ModLoader.IO.TagCompound());
Check(loaded.Quality == PillQuality.Standard && !loaded.Crafted, "Legacy saves safely migrate to standard pills.");
loaded.LoadData(pill, new Terraria.ModLoader.IO.TagCompound { ["quality"] = 999 });
Check(loaded.Quality == PillQuality.Standard, "Invalid save quality never grants the highest grade.");
using (var stream = new MemoryStream())
{
    using var writer = new BinaryWriter(stream);
    quality.NetSend(pill, writer);
    stream.Position = 0;
    using var reader = new BinaryReader(stream);
    loaded.NetReceive(pill, reader);
    Check(loaded.Quality == PillQuality.Spirit && loaded.Crafted, "Item network state preserves quality and provenance.");
}
try { loaded.NetReceive(pill, new BinaryReader(new MemoryStream(new byte[] { 1 }))); }
catch (EndOfStreamException) { }
Check(loaded.Quality == PillQuality.Spirit && loaded.Crafted, "Truncated quality state cannot partially overwrite an item.");
loaded.NetReceive(pill, new BinaryReader(new MemoryStream(new byte[] { 255, 0 })));
Check(loaded.Quality == PillQuality.Standard && !loaded.Crafted, "Invalid network grade is normalized safely.");
Check(PillQualityRules.Scale(60, PillQuality.Coarse) == 45 && PillQualityRules.Scale(60, PillQuality.Fine) == 75
    && PillQualityRules.Scale(60, PillQuality.Spirit) == 90, "All grades have distinct, deterministic benefit strengths.");
Check(PillQualityRules.Roll(0.05f) == PillQuality.Fine && PillQualityRules.Roll(0.2f) == PillQuality.Standard
    && PillQualityRules.Roll(0.5f) == PillQuality.Coarse && PillQualityRules.Roll(float.NaN) == PillQuality.Standard,
    "Creation probability boundaries and invalid random values are well defined.");
var oldStandard = new Terraria.Item { ModItem = new ModItem { Mod = mod, Name = "SpringReturnPill" } };
var newStandard = new Terraria.Item { ModItem = new ModItem { Mod = mod, Name = "SpringReturnPill" } };
Game.rand.Roll = 0.3f;
newStandard.GetGlobalItem<PillQualitySystem>().OnCreated(newStandard, new Terraria.DataStructures.RecipeItemCreationContext());
oldStandard.GetGlobalItem<PillQualitySystem>().OnStack(oldStandard, newStandard, 1);
Check(oldStandard.GetGlobalItem<PillQualitySystem>().Crafted, "Merging standard-grade crafted and legacy stacks retains creation provenance.");
var tips = new List<TooltipLine>();
quality.ModifyTooltips(pill, tips);
Check(tips.Count == 2 && tips[0].OverrideColor.HasValue, "Tooltip exposes colored grade and fixed additive bonus.");
Game.netMode = NetmodeID.Server;
Game.player[0].State.spiritualEnergy = 0;
Game.rand.Roll = 0.99f;
PillQualitySystem.ApplyUseBonus(pill, Game.player[0]);
Check(Game.player[0].State.spiritualEnergy == 16 && quality.Quality == PillQuality.Spirit,
    "Using the pill applies its stored grade even when the current random result would be coarse.");
// The real inscription request parser, inventory transaction, metadata and effects.
void SetupInscription(InscriptionKind next = InscriptionKind.Greenwood, InscriptionKind current = InscriptionKind.None, int stones = 3)
{
    Game.netMode = NetmodeID.Server;
    Game.player[0] = new Terraria.Player { whoAmI = 0, Center = new Microsoft.Xna.Framework.Vector2(16,16) };
    Game.player[0].State.NetworkInitialized = true;
    var material = new TestInscriptionTool { Mod=mod, Next=next };
    var needle = new Terraria.Item { type=600,stack=2,damage=0,maxStack=99,ModItem=material };
    material.Item=needle; Game.player[0].inventory[0]=needle;
    var equipment = new Terraria.Item { type=601,stack=1,damage=20,prefix=300,ModItem=new ModItem {Mod=mod} };
    equipment.GetGlobalItem<XianXia.Common.Items.InscribedEquipment>().SetKind(current);
    Game.player[0].inventory[1]=equipment;
    Game.player[0].inventory[2]=new Terraria.Item {type=200,stack=stones};
    for(int x=0;x<Game.maxTilesX;x++) for(int y=0;y<Game.maxTilesY;y++) Game.tile[x,y].HasTile=false;
    Game.tile[0,0].HasTile=true;Game.tile[0,0].TileType=1;
}
InscriptionKind CurrentInscription() => XianXia.Common.Items.InscribedEquipment.GetKind(Game.player[0].inventory[1]);
byte[] InscriptionBytes(byte targetSlot=1,int targetType=601,int prefix=300,byte previous=0,byte toolSlot=0,int toolType=600)
{
    using var bytes=new MemoryStream();using var writer=new BinaryWriter(bytes);
    writer.Write((byte)9);writer.Write(toolSlot);writer.Write(toolType);writer.Write(targetSlot);
    writer.Write(targetType);writer.Write(prefix);writer.Write(previous);writer.Write((byte)0);writer.Write(false);return bytes.ToArray();
}
void Inscribe(byte previous=0,int sender=0,byte targetSlot=1,int targetType=601,int prefix=300,byte toolSlot=0,int toolType=600)
    => Packet(sender,w=>w.Write(InscriptionBytes(targetSlot,targetType,prefix,previous,toolSlot,toolType)));
SetupInscription();
byte[] inscriptionBytes=InscriptionBytes();
for(int size=0;size<inscriptionBytes.Length;size++) {
    Packet(0,w=>w.Write(inscriptionBytes.AsSpan(0,size)));
    Check(CurrentInscription()==InscriptionKind.None && Game.player[0].inventory[0].stack==2 && Game.player[0].inventory[2].stack==3,
        "Every truncated inscription request leaves metadata and both material stacks unchanged.");
}
Inscribe();
Check(CurrentInscription()==InscriptionKind.Greenwood && Game.player[0].inventory[0].stack==1 && Game.player[0].inventory[2].IsAir,
    "Successful inscription consumes exactly one needle and three stones.");
Check(Game.player[0].inventory[1].prefix==300 && Game.player[0].inventory[1].type==601,"Inscription preserves equipment and even a prefix above byte range.");
Inscribe();
Check(Game.player[0].inventory[0].stack==1,"Duplicate requests cannot consume materials during the shared cooldown.");
foreach(var kind in Enum.GetValues<InscriptionKind>().Where(k=>k!=InscriptionKind.None)) {
    SetupInscription(kind, InscriptionKind.None);Inscribe();
    Check(CurrentInscription()==kind,"Each of the five actual inscriptions can be applied.");
    SetupInscription(kind,kind);Inscribe((byte)kind);
    Check(Game.player[0].inventory[0].stack==2 && Game.player[0].inventory[2].stack==3,"Applying the same inscription does not spend materials.");
}
SetupInscription(InscriptionKind.Thunder,InscriptionKind.Greenwood);Inscribe(1);
Check(CurrentInscription()==InscriptionKind.Thunder,"Replacement overwrites the previous inscription.");
SetupInscription(InscriptionKind.None,InscriptionKind.Thunder,0);Inscribe(3);
Check(CurrentInscription()==InscriptionKind.None && Game.player[0].inventory[0].stack==1,"Removal costs one stone tool and no spirit stones.");
SetupInscription(InscriptionKind.None);Inscribe();
Check(Game.player[0].inventory[0].stack==2,"Clearing an uninscribed item costs nothing.");
SetupInscription(stones:2);Inscribe();
Check(CurrentInscription()==InscriptionKind.None && Game.player[0].inventory[0].stack==2,"Insufficient stones leave all materials and metadata unchanged.");
SetupInscription();Game.player[0].inventory[2].stack=1;Game.player[0].inventory[3]=new Terraria.Item {type=200,stack=2};Inscribe();
Check(CurrentInscription()==InscriptionKind.Greenwood && Game.player[0].inventory[2].IsAir && Game.player[0].inventory[3].IsAir,"Material consumption works across split stacks.");
SetupInscription();Game.tile[0,0].HasTile=false;Inscribe();
Check(CurrentInscription()==InscriptionKind.None && Game.player[0].inventory[0].stack==2,"Removing the nearby forge before confirmation aborts without consumption.");
foreach(Action invalidate in new Action[] {
    ()=>Game.player[0].dead=true, ()=>Game.player[0].active=false, ()=>Game.player[0].noItems=true,
    ()=>Game.player[0].CCed=true, ()=>Game.player[0].State.NetworkInitialized=false,
    ()=>Game.player[0].selectedItem=1, ()=>Game.player[0].inventory[1].consumable=true,
    ()=>Game.player[0].inventory[1].vanity=true, ()=>Game.player[0].inventory[1].ModItem.Mod=new Mod(),
    ()=>Game.player[0].inventory[1].maxStack=2, ()=>Game.player[0].inventory[1].stack=2
}) {
    SetupInscription();invalidate();Inscribe();
    Check(Game.player[0].inventory[0].stack==2 && Game.player[0].inventory[2].stack==3,"Invalid owner, state, or equipment never consumes materials.");
}
foreach(Action request in new Action[] {
    ()=>Inscribe(previous:1),()=>Inscribe(previous:255),()=>Inscribe(targetSlot:58),()=>Inscribe(targetSlot:0),
    ()=>Inscribe(targetType:999),()=>Inscribe(prefix:301),()=>Inscribe(toolSlot:1),()=>Inscribe(toolType:999),
    ()=>Inscribe(sender:1),()=>Inscribe(sender:-1)
}) {
    SetupInscription();request();
    Check(CurrentInscription()==InscriptionKind.None && Game.player[0].inventory[0].stack==2,"Stale, foreign, or invalid inscription requests cannot modify the target.");
}
SetupInscription();Game.netMode=NetmodeID.MultiplayerClient;Inscribe();
Check(CurrentInscription()==InscriptionKind.None && Game.player[0].inventory[0].stack==2,"A client cannot process its own authoritative inscription transaction.");
SetupInscription();Game.netMode=NetmodeID.SinglePlayer;
InscriptionTransactions.Request(Game.player[0],0,600,1,601,300,0);
Check(CurrentInscription()==InscriptionKind.Greenwood,"Single player uses the same checked transaction.");

var engraved=Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.InscribedEquipment>();
var inscriptionSave=new Terraria.ModLoader.IO.TagCompound();engraved.SaveData(null,inscriptionSave);
var engravedCopy=new XianXia.Common.Items.InscribedEquipment();engravedCopy.LoadData(null,inscriptionSave);
Check(engravedCopy.Kind==InscriptionKind.Greenwood,"Equipment inscription saves and loads.");
using(var data=new MemoryStream()) {
 using(var writer=new BinaryWriter(data,Encoding.UTF8,true)) engraved.NetSend(null,writer);
 data.Position=0;using var reader=new BinaryReader(data);engravedCopy.NetReceive(null,reader);
 Check(engravedCopy.Kind==InscriptionKind.Greenwood,"Equipment inscription metadata round trips over item networking.");
}
engravedCopy.LoadData(null,new Terraria.ModLoader.IO.TagCompound { ["inscription"]=999 });
Check(engravedCopy.Kind==InscriptionKind.None,"Invalid saved inscription safely becomes uninscribed.");
engravedCopy.LoadData(null,new Terraria.ModLoader.IO.TagCompound());
Check(engravedCopy.Kind==InscriptionKind.None,"Legacy equipment safely remains uninscribed.");

SetupInscription(InscriptionKind.StarAbyss);Inscribe();
var passive=Game.player[0].GetModPlayer<InscriptionPlayer>();
Game.player[0].selectedItem=1;
passive.ResetEffects();passive.RegisterAccessory(InscriptionKind.Greenwood);passive.RegisterAccessory(InscriptionKind.Greenwood);
passive.RegisterAccessory(InscriptionKind.Furnace);passive.RegisterAccessory(InscriptionKind.Thunder);
passive.RegisterAccessory(InscriptionKind.BrokenHeaven);passive.PostUpdateEquips();passive.UpdateLifeRegen();
Check(Game.player[0].State.spiritualEnergyRegenBonus==1 && Game.player[0].lifeRegen==2,"Repeated identical accessory passives apply recovery once.");
Check(Game.player[0].armorPenetration==6 && Game.player[0].statDefense==3,"Furnace gives actual armor penetration and accessory defense.");
Check(Math.Abs(Game.player[0].moveSpeed-0.08f)<0.001f && Math.Abs(Game.player[0].attackSpeed-0.08f)<0.001f,"Thunder gives actual movement and attack speed.");
Check(Game.player[0].critChance==4 && Math.Abs(Game.player[0].State.spiritualEnergyCostMultiplier-1.15f)<0.001f,"Broken Heaven accessory gives crit and costs actual spiritual energy.");
Game.netMode=NetmodeID.MultiplayerClient;
for(int tick=0;tick<240;tick++) passive.PostUpdate();
Check(Game.player[0].State.spiritPressure==0,"Client cannot generate Star Abyss pollution.");
Game.netMode=NetmodeID.Server;
for(int tick=0;tick<60;tick++) passive.PostUpdate();
Game.player[0].selectedItem=0;passive.ResetEffects();passive.PostUpdateEquips();
for(int tick=0;tick<200;tick++) passive.PostUpdate();
Game.player[0].selectedItem=1;passive.ResetEffects();passive.PostUpdateEquips();
for(int tick=0;tick<60;tick++) passive.PostUpdate();
Check(Game.player[0].State.spiritPressure==4,"Cumulative Star Abyss use pays pressure even after switching equipment.");
Game.player[0].State.spiritPressure=99;
for(int tick=0;tick<120;tick++) passive.PostUpdate();
Check(Game.player[0].State.spiritPressure==100,"Star Abyss pollution respects the canonical pressure maximum.");
var modifier=new StatModifier();float weaponCrit=0;var kb=new StatModifier();
var effects=Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.InscribedEquipment>();
effects.SetKind(InscriptionKind.Furnace);effects.ModifyWeaponDamage(Game.player[0].inventory[1],Game.player[0],ref modifier);effects.ModifyWeaponKnockback(Game.player[0].inventory[1],Game.player[0],ref kb);
Check(Math.Abs(modifier.Bonus-0.08f)<0.001f && Math.Abs(kb.Bonus-0.2f)<0.001f,"Furnace weapon modifies actual damage and knockback hooks.");
effects.SetKind(InscriptionKind.StarAbyss);modifier=new();effects.ModifyWeaponDamage(Game.player[0].inventory[1],Game.player[0],ref modifier);
Check(Math.Abs(modifier.Bonus-0.15f)<0.001f,"Star Abyss weapon modifies actual damage hook.");
effects.SetKind(InscriptionKind.BrokenHeaven);kb=new();effects.ModifyWeaponCrit(Game.player[0].inventory[1],Game.player[0],ref weaponCrit);effects.ModifyWeaponKnockback(Game.player[0].inventory[1],Game.player[0],ref kb);
Check(weaponCrit==8 && Math.Abs(kb.Bonus-0.3f)<0.001f,"Broken Heaven weapon modifies actual crit and knockback hooks.");
Check(InscriptionRules.Normalize(-1)==InscriptionKind.None && !InscriptionRules.CanChange(InscriptionKind.Greenwood,(InscriptionKind)255),"Invalid inscription choices cannot pass the policy.");
// Refinement reuses the same atomic material/target transaction.
void SetupRefinement(int level=0, int stones=6, string name="CloudpiercerFlyingSword")
{
 SetupInscription(InscriptionKind.None,InscriptionKind.Greenwood,stones);
 ((TestInscriptionTool)Game.player[0].inventory[0].ModItem).Refining=true;
 Game.player[0].inventory[1].ModItem.Name=name;
 if(XianXia.Common.Items.RefinedArtifact.IsSample(Game.player[0].inventory[1]))
  Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.RefinedArtifact>().SetLevel(level);
 Game.player[0].State.cultivationStage=CultivationStage.NascentSoul;
 DownedBossSystem.DownedBosses.Clear();
 DownedBossSystem.DownedBosses.UnionWith(new[]{"black_furnace_iron_golem","thunder_marsh_jiao","formless_sword_soul"});
}
void Refine(byte oldLevel=0) => InscriptionTransactions.HandleRequest(Game.player[0],0,600,1,601,300,1,oldLevel);
foreach(string sample in new[]{"CloudpiercerFlyingSword","GreenwoodArrayPlate"}) for(byte level=0;level<3;level++) {
 SetupRefinement(level,RefinementRules.StoneCost(level),sample);Refine(level);
 Check(XianXia.Common.Items.RefinedArtifact.GetLevel(Game.player[0].inventory[1])==level+1,"Both vertical samples advance exactly one refinement level.");
 Check(CurrentInscription()==InscriptionKind.Greenwood && Game.player[0].inventory[1].prefix==300,"Refinement retains inscription and prefix.");
 Check(Game.player[0].inventory[0].stack==1 && Game.player[0].inventory[2].IsAir,"Refinement consumes one crystal and the exact level-dependent stones.");
}
foreach(Action invalidate in new Action[] {
 ()=>Game.player[0].State.cultivationStage=CultivationStage.QiAwakening,
 ()=>DownedBossSystem.DownedBosses.Clear(),
 ()=>Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.InscribedEquipment>().SetKind(InscriptionKind.None),
 ()=>Game.player[0].inventory[2].stack=5,
 ()=>Game.tile[0,0].HasTile=false
}) {
 SetupRefinement();invalidate();Refine();
 Check(XianXia.Common.Items.RefinedArtifact.GetLevel(Game.player[0].inventory[1])==0 && Game.player[0].inventory[0].stack==2,"Unmet stage, boss, inscription, stones or forge never consumes a crystal or advances refinement.");
}
SetupRefinement(3);Refine(3);
Check(Game.player[0].inventory[0].stack==2 && XianXia.Common.Items.RefinedArtifact.GetLevel(Game.player[0].inventory[1])==3,"Maximum refinement cannot spend materials or exceed level three.");
SetupRefinement(1,12);Refine(0);
Check(Game.player[0].inventory[0].stack==2,"Stale refinement selection is rejected before any material mutation.");
SetupRefinement(name:"OtherWeapon");Refine();
Check(Game.player[0].inventory[0].stack==2,"Non-sample weapons cannot be refined.");
SetupRefinement(2,18);Game.player[0].State.cultivationStage=CultivationStage.GoldenCore;Refine(2);
Check(Game.player[0].inventory[0].stack==2,"Level three requires Nascent Soul despite earlier-level qualification.");
SetupRefinement(2,18);DownedBossSystem.DownedBosses.Remove("formless_sword_soul");Refine(2);
Check(Game.player[0].inventory[0].stack==2,"Level three requires its own boss, not merely earlier defeated bosses.");
SetupRefinement(2,3);((TestInscriptionTool)Game.player[0].inventory[0].ModItem).Refining=false;
InscriptionTransactions.HandleRequest(Game.player[0],0,600,1,601,300,1,2);
Check(CurrentInscription()==InscriptionKind.None && XianXia.Common.Items.RefinedArtifact.GetLevel(Game.player[0].inventory[1])==2,"Removing an inscription retains the equipment's existing refinement level.");
var refinedData=new XianXia.Common.Items.RefinedArtifact();refinedData.SetLevel(3);
var refinedSave=new Terraria.ModLoader.IO.TagCompound();refinedData.SaveData(null,refinedSave);
var refinedCopy=new XianXia.Common.Items.RefinedArtifact();refinedCopy.LoadData(null,refinedSave);
Check(refinedCopy.Level==3,"Refinement level persists independently of inscriptions.");
refinedCopy.LoadData(null,new Terraria.ModLoader.IO.TagCompound { ["refinement"]=999 });
Check(refinedCopy.Level==0,"Invalid saved refinement safely resets to unrefined.");
var refinedDamage=new StatModifier();refinedData.ModifyWeaponDamage(null,null,ref refinedDamage);
Check(Math.Abs(refinedDamage.Bonus-0.12f)<0.001f,"Maximum refinement adds the actual 12% weapon damage modifier.");
Check(!RefinementRules.CanAdvance(-1,true,CultivationStage.DaoSevering,true) && !RefinementRules.CanAdvance(3,true,CultivationStage.DaoSevering,true),"Refinement bounds prohibit invalid or maximum-level transitions.");
void SetupAwakening(string name="CloudpiercerFlyingSword") {
 SetupRefinement(3,24,name);
 var material=(TestInscriptionTool)Game.player[0].inventory[0].ModItem;
 material.Refining=false;material.Awakening=true;
 DownedBossSystem.DownedBosses.Add("greenwood_medicine_king_echo");
}
void Awaken(bool oldAwake=false)=>InscriptionTransactions.HandleRequest(Game.player[0],0,600,1,601,300,1,3,oldAwake);
foreach(string name in new[]{"CloudpiercerFlyingSword","GreenwoodArrayPlate"}) {
 SetupAwakening(name);Awaken();
 Check(XianXia.Common.Items.RefinedArtifact.IsAwakened(Game.player[0].inventory[1]),"Both samples awaken only through the confirmed server transaction.");
 Check(Game.player[0].inventory[0].stack==1 && Game.player[0].inventory[2].IsAir,"Awakening consumes exactly one seal and 24 stones.");
 Check(CurrentInscription()==InscriptionKind.Greenwood && XianXia.Common.Items.RefinedArtifact.GetLevel(Game.player[0].inventory[1])==3 && Game.player[0].inventory[1].prefix==300,"Awakening retains refinement, inscription and prefix.");
}
foreach(Action invalidate in new Action[] {
 ()=>Game.player[0].State.cultivationStage=CultivationStage.GoldenCore,
 ()=>DownedBossSystem.DownedBosses.Remove("greenwood_medicine_king_echo"),
 ()=>Game.player[0].inventory[2].stack=23,
 ()=>Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.RefinedArtifact>().SetLevel(2),
 ()=>Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.InscribedEquipment>().SetKind(InscriptionKind.None),
 ()=>Game.tile[0,0].HasTile=false,
 ()=>Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.RefinedArtifact>().TryAwaken()
}) {
 SetupAwakening();invalidate();Awaken();
 Check(Game.player[0].inventory[0].stack==2,"Unmet awakening requirements and stale state never consume a seal.");
}
SetupAwakening();Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.RefinedArtifact>().TryAwaken();Awaken(true);
Check(Game.player[0].inventory[0].stack==2,"Already awakened equipment cannot spend another seal.");
var awakeMeta=new XianXia.Common.Items.RefinedArtifact();
Check(!awakeMeta.TryAwaken(),"An unrefined artifact cannot directly gain crafted awakening.");
awakeMeta.SetLevel(3);Check(awakeMeta.TryAwaken() && !awakeMeta.TryAwaken(),"Awakening is a one-way transition at maximum refinement.");
var awakeSave=new Terraria.ModLoader.IO.TagCompound();awakeMeta.SaveData(null,awakeSave);
var awakeCopy=new XianXia.Common.Items.RefinedArtifact();awakeCopy.LoadData(null,awakeSave);
Check(awakeCopy.Awakened && awakeCopy.Level==3,"Item save retains the permanent crafted awakening.");
using(var bytes=new MemoryStream()) {
 using(var writer=new BinaryWriter(bytes,Encoding.UTF8,true)) awakeMeta.NetSend(null,writer);
 bytes.Position=0;using var reader=new BinaryReader(bytes);awakeCopy.NetReceive(null,reader);
 Check(awakeCopy.Awakened && bytes.Length==3,"Item networking retains level, awakening and route together.");
}
try { using var reader=new BinaryReader(new MemoryStream(new byte[]{0}));awakeCopy.NetReceive(null,reader);throw new Exception("Expected short metadata"); }
catch(EndOfStreamException) {Check(awakeCopy.Level==3 && awakeCopy.Awakened,"Short metadata cannot partially clear existing advancement.");}
awakeCopy.LoadData(null,new Terraria.ModLoader.IO.TagCompound { ["refinement"]=1,["awakened"]=true });
Check(!awakeCopy.Awakened,"Malformed low-level save cannot grant awakening.");
awakeCopy.LoadData(null,new Terraria.ModLoader.IO.TagCompound { ["refinement"]=3 });
Check(!awakeCopy.Awakened,"Legacy level-three equipment still needs the explicit awakening process.");
void SetupSkill(string name="CloudpiercerFlyingSword") {
 SetupAwakening(name);Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.RefinedArtifact>().TryAwaken();
 Game.player[0].selectedItem=1;Game.player[0].State.spiritualEnergy=100;
 foreach(var projectile in Game.projectile) projectile.active=false;
 Terraria.Projectile.AllowSpawn=true;
}
void Skill(ArtifactSkill skill=ArtifactSkill.SwordBurst,int sender=0,int type=601,float x=100,float y=100,byte slot=1) { Game.player[0].State.skillRequestCooldown=0; Packet(sender,w=>{
 w.Write((byte)10);w.Write((byte)skill);w.Write(slot);w.Write(type);w.Write(x);w.Write(y);
}); }
SetupSkill();Skill();
Check(Game.projectile.Count(p=>p.active && p.owner==0)==3 && Game.projectile.Where(p=>p.active).All(p=>p.damage==50),"Sword burst creates three authoritative projectiles with server-computed double weapon damage.");
Check(Game.player[0].State.spiritualEnergy==76 && Game.player[0].State.activeSkillCooldown==900,"Burst spends its actual energy cost and starts the per-player cooldown.");
Skill();Check(Game.player[0].State.spiritualEnergy==76,"Repeated burst is blocked by shared cooldown.");
SetupSkill();Game.player[0].State.spiritualEnergy=23;Skill();
Check(!Game.projectile.Any(p=>p.active) && Game.player[0].State.activeSkillCooldown==0,"Insufficient energy cannot create a burst or cooldown.");
SetupSkill();Terraria.Projectile.AllowSpawn=false;Skill();
Check(Game.player[0].State.spiritualEnergy==100 && Game.player[0].State.activeSkillCooldown==0,"Projectile creation failure rolls back spent energy.");
foreach(Action bad in new Action[]{()=>Skill(x:float.NaN),()=>Skill(y:float.PositiveInfinity),()=>Skill(sender:1),()=>Skill(sender:-1),()=>Skill(type:999),()=>Skill(slot:0),()=>Skill((ArtifactSkill)255)}) {
 SetupSkill();bad();Check(Game.player[0].State.spiritualEnergy==100 && !Game.projectile.Any(p=>p.active),"Invalid aim, owner, item or skill cannot spend or spawn.");
}
SetupSkill();Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.RefinedArtifact>().SetLevel(2);Skill();
Check(Game.player[0].State.spiritualEnergy==100,"Unawakened equipment cannot cast its active skill.");
SetupSkill();Game.player[0].armor[13]=new Terraria.Item {type=200,stack=1};Skill(ArtifactSkill.WardGuard);
Check(Game.player[0].State.wardGuardTimer==0,"Vanity accessories cannot grant the protective active.");
SetupSkill();Game.player[0].armor[8]=new Terraria.Item {type=200,stack=1};Skill(ArtifactSkill.WardGuard);
Check(Game.player[0].State.wardGuardTimer==0,"A locked functional slot cannot grant the protective active.");
SetupSkill();Game.player[0].armor[3]=new Terraria.Item {type=200,stack=1};Skill(ArtifactSkill.WardGuard);
Check(Game.player[0].State.wardGuardTimer==180 && Game.player[0].State.activeSkillCooldown==1200 && Game.player[0].State.spiritualEnergy==82,"Equipped ward grants a bounded shield and spends its actual cost.");
Game.player[0].armor[3].TurnToAir();Skill();
Check(Game.player[0].State.spiritualEnergy==82,"Switching gear cannot bypass the shared skill cooldown.");
SetupSkill("GreenwoodArrayPlate");Skill(ArtifactSkill.ArrayPulse);
Check(Game.player[0].State.spiritualEnergy==100,"Pulse without a real owned array does not spend energy.");
Game.projectile[0]=new Terraria.Projectile {active=true,owner=0,type=3,Center=Game.player[0].Center};
Skill(ArtifactSkill.ArrayPulse);Check(Game.player[0].State.activeSkillCooldown==0,"Pulse with no missing health has no cost or cooldown.");
Game.player[0].statLife=60;Skill(ArtifactSkill.ArrayPulse);
Check(Game.player[0].statLife==80 && Game.player[0].State.spiritualEnergy==82 && Game.player[0].State.activeSkillCooldown==1200,"Owned array pulse actually heals and starts the shared cooldown.");
Check(!(initial with {SkillCooldown=0,WardTimer=1}).IsValid() && !(initial with {SkillCooldown=1201}).IsValid(),"Snapshot rejects illegal skill timer relationships and excessive cooldown.");
SetupSkill("GreenwoodArrayPlate");
ArtifactSkillTransactions.HandleRequest(Game.player[0],ArtifactSkill.ArrayPulse,1,601,new Microsoft.Xna.Framework.Vector2(100,100));
int replies=ModPacket.Sent.Count;
ArtifactSkillTransactions.HandleRequest(Game.player[0],ArtifactSkill.ArrayPulse,1,601,new Microsoft.Xna.Framework.Vector2(100,100));
Check(ModPacket.Sent.Count==replies,"Failed requests share a short server rate gate instead of emitting repeated replies.");
void SetupRoute() {
 SetupInscription();world.ClearWorld();DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
 Game.player[0].inventory[0]=new Terraria.Item { type=200,stack=1 };
 Game.player[0].State.cultivationStage=CultivationStage.Tribulation;
 Game.tile[0,0].TileType=4;
}
void ChooseRoute(byte route=1,int sender=0,byte slot=0,int type=200) => Packet(sender,w=>{
 w.Write((byte)11);w.Write(slot);w.Write(type);w.Write(route);
});
foreach(byte route in new byte[]{1,2,3}) {
 SetupRoute();int syncs=Terraria.NetMessage.WorldSends;ChooseRoute(route);
 Check((byte)DownedBossSystem.ChosenRoute==route && Game.player[0].inventory[0].IsAir,"Every valid route can be selected and spends exactly one real material.");
 Check(Terraria.NetMessage.WorldSends==syncs+1,"A successful route broadcasts authoritative world progress.");
 var routeSave=new Terraria.ModLoader.IO.TagCompound();world.SaveWorldData(routeSave);world.ClearWorld();world.LoadWorldData(routeSave);
 Check((byte)DownedBossSystem.ChosenRoute==route,"The chosen world route survives saving and loading.");
 using(var bytes=new MemoryStream()) {
  using(var writer=new BinaryWriter(bytes,Encoding.UTF8,true))world.NetSend(writer);
  world.ClearWorld();bytes.Position=0;using var reader=new BinaryReader(bytes);world.NetReceive(reader);
  Check((byte)DownedBossSystem.ChosenRoute==route,"Joining clients receive the selected world route.");
 }
 Game.player[0].GetModPlayer<XianXia.Common.Players.InscriptionPlayer>().Initialize();
 Game.player[0].inventory[0]=new Terraria.Item {type=200,stack=1};ChooseRoute((byte)(route%3+1));
 Check((byte)DownedBossSystem.ChosenRoute==route && Game.player[0].inventory[0].stack==1,"Conflicting subsequent selections neither overwrite the world nor consume another material.");
}
foreach(Action invalid in new Action[]{()=>ChooseRoute(0),()=>ChooseRoute(4),()=>ChooseRoute(255),()=>ChooseRoute(sender:1),()=>ChooseRoute(sender:-1),()=>ChooseRoute(slot:1),()=>ChooseRoute(type:201),()=>{Game.player[0].dead=true;ChooseRoute();},()=>{Game.player[0].CCed=true;ChooseRoute();},()=>{Game.player[0].State.NetworkInitialized=false;ChooseRoute();},()=>{Game.player[0].State.cultivationStage=CultivationStage.NascentSoul;ChooseRoute();},()=>{DownedBossSystem.DownedBosses.Clear();ChooseRoute();},()=>{Game.tile[0,0].TileType=1;ChooseRoute();},()=>{Game.player[0].inventory[0].type=201;ChooseRoute();}}) {
 SetupRoute();invalid();Check(DownedBossSystem.ChosenRoute==DownedBossSystem.EndgameRoute.None && Game.player[0].inventory[0].stack==1,"Invalid route requests and unmet gates cannot change the world or spend material.");
}
using(var bytes=new MemoryStream()) {
 using(var writer=new BinaryWriter(bytes,Encoding.UTF8,true)){writer.Write((byte)11);writer.Write((byte)0);writer.Write(200);writer.Write((byte)1);}
 for(int length=0;length<bytes.Length;length++) {
  SetupRoute();using var reader=new BinaryReader(new MemoryStream(bytes.ToArray()[..length]));mod.HandlePacket(reader,0);
  Check(DownedBossSystem.ChosenRoute==DownedBossSystem.EndgameRoute.None && Game.player[0].inventory[0].stack==1,"Truncated route packets cannot partially commit a world choice.");
 }
}
SetupRoute();Game.netMode=NetmodeID.MultiplayerClient;ChooseRoute();
Check(DownedBossSystem.ChosenRoute==DownedBossSystem.EndgameRoute.None && !DownedBossSystem.TryChooseRoute(DownedBossSystem.EndgameRoute.RebuildHeaven),"Clients cannot directly select or import a world route through requests.");
SetupRoute();ChooseRoute(1);
Game.player[1]=new Terraria.Player {whoAmI=1,active=true,selectedItem=0};
Game.player[1].State.NetworkInitialized=true;Game.player[1].State.cultivationStage=CultivationStage.Tribulation;
Game.player[1].inventory[0]=new Terraria.Item {type=200,stack=1};ChooseRoute(2,sender:1);
Check(DownedBossSystem.ChosenRoute==DownedBossSystem.EndgameRoute.RebuildHeaven && Game.player[1].inventory[0].stack==1,"Two players racing to choose different routes resolve to the first server commit without consuming the loser's item.");
SetupRoute();Game.player[0].State.cultivationStage=(CultivationStage)9;ChooseRoute();
Check(DownedBossSystem.ChosenRoute==DownedBossSystem.EndgameRoute.None,"Out-of-range realm cannot choose an ending.");
void SetupDao(byte route=1,string name="CloudpiercerFlyingSword") {
 SetupSkill(name);world.ClearWorld();DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
 DownedBossSystem.TryChooseRoute((DownedBossSystem.EndgameRoute)route);
 var material=(TestInscriptionTool)Game.player[0].inventory[0].ModItem;material.Awakening=false;material.Transforming=true;
 Game.player[0].selectedItem=0;Game.player[0].State.cultivationStage=CultivationStage.DaoSevering;
 Game.player[0].inventory[2]=new Terraria.Item {type=200,stack=36};Game.tile[0,0].TileType=4;
}
void Dao(byte route=1,byte previous=0,int sender=0,int prefix=300,byte kind=1,byte level=3,bool awakened=true) => Packet(sender,w=>{
 w.Write((byte)12);w.Write((byte)0);w.Write(600);w.Write((byte)1);w.Write(601);w.Write(prefix);w.Write(kind);w.Write(level);w.Write(awakened);w.Write(previous);w.Write(route);
});
foreach(byte route in new byte[]{1,2,3}) foreach(string name in new[]{"CloudpiercerFlyingSword","GreenwoodArrayPlate"}) {
 SetupDao(route,name);var original=Game.player[0].inventory[1];Dao(route);
 Check((byte)XianXia.Common.Items.RefinedArtifact.GetDaoRoute(original)==route && Game.player[0].inventory[0].stack==1 && Game.player[0].inventory[2].IsAir,"Both sample artifacts transform into every world route for exactly one seal and 36 stones.");
 Check(ReferenceEquals(original,Game.player[0].inventory[1]) && original.prefix==300 && XianXia.Common.Items.RefinedArtifact.GetLevel(original)==3 && XianXia.Common.Items.RefinedArtifact.IsAwakened(original) && CurrentInscription()==InscriptionKind.Greenwood,"Dao transformation preserves the actual item, prefix, inscription, refinement and awakening.");
 Game.player[0].GetModPlayer<XianXia.Common.Players.InscriptionPlayer>().Initialize();Dao(route,previous:route);
 Check(Game.player[0].inventory[0].stack==1,"A completed transformation cannot consume another seal.");
 var daoMeta=original.GetGlobalItem<XianXia.Common.Items.RefinedArtifact>();var daoSave=new Terraria.ModLoader.IO.TagCompound();daoMeta.SaveData(original,daoSave);
 var daoLoaded=new XianXia.Common.Items.RefinedArtifact();daoLoaded.LoadData(original,daoSave);
 Check((byte)daoLoaded.DaoRoute==route,"Dao route persists in item saves.");
 using(var bytes=new MemoryStream()) {
  using(var writer=new BinaryWriter(bytes,Encoding.UTF8,true))daoMeta.NetSend(original,writer);
  bytes.Position=0;using var reader=new BinaryReader(bytes);daoLoaded.NetReceive(original,reader);
  Check((byte)daoLoaded.DaoRoute==route && bytes.Length==3,"Dao route travels with complete item metadata.");
 }
 var damage=new StatModifier();daoMeta.ModifyWeaponDamage(original,Game.player[0],ref damage);
 Check(Math.Abs(damage.Base-original.damage*(DaoArtifactRules.DamageScale(name)-1))<0.01f,"Matching-world transformation grows weapon base damage through the native modifier.");
 Game.player[0].selectedItem=1;
 if(name=="CloudpiercerFlyingSword") {
  Skill();Check(Game.player[0].State.spiritualEnergy==100-DaoArtifactRules.SkillCost(ArtifactSkill.SwordBurst,(DownedBossSystem.EndgameRoute)route) && Game.projectile.Where(p=>p.active).All(p=>p.damage==25*DaoArtifactRules.BurstMultiplier((DownedBossSystem.EndgameRoute)route)),"Dao sword burst uses authoritative route-specific cost and damage.");
 } else {
  Game.projectile[0]=new Terraria.Projectile {active=true,owner=0,type=3,Center=Game.player[0].Center};Game.player[0].statLife=50;
  Skill(ArtifactSkill.ArrayPulse);Check(Game.player[0].statLife==50+DaoArtifactRules.PulseHeal((DownedBossSystem.EndgameRoute)route),"Rebuild Dao pulse has its actual healing upgrade.");
 }
 if(route==3) Check(Game.player[0].State.spiritPressure==8,"Star Abyss active skills apply the real pressure cost.");
 world.ClearWorld();Check(XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(original)==DownedBossSystem.EndgameRoute.None && (byte)XianXia.Common.Items.RefinedArtifact.GetDaoRoute(original)==route,"Moving to another world disables bonuses without destroying the item's route metadata.");
}
foreach(Action invalid in new Action[]{()=>Dao(sender:-1),()=>Dao(sender:1),()=>Dao(route:2),()=>Dao(prefix:0),()=>Dao(kind:2),()=>Dao(level:2),()=>Dao(awakened:false),()=>{Game.player[0].State.cultivationStage=CultivationStage.Tribulation;Dao();},()=>{Game.tile[0,0].TileType=1;Dao();},()=>{DownedBossSystem.DownedBosses.Clear();Dao();},()=>{Game.player[0].inventory[2].stack=35;Dao();},()=>{Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.RefinedArtifact>().SetLevel(2);Dao();},()=>{((TestInscriptionTool)Game.player[0].inventory[0].ModItem).Transforming=false;Dao();}}) {
 SetupDao();invalid();Check(XianXia.Common.Items.RefinedArtifact.GetDaoRoute(Game.player[0].inventory[1])==DownedBossSystem.EndgameRoute.None && Game.player[0].inventory[0].stack==2,"Illegal or stale transformation requests cannot upgrade or consume a seal.");
}
SetupDao();InscriptionTransactions.HandleRequest(Game.player[0],0,600,1,601,300,1,3,true);
Check(CurrentInscription()==InscriptionKind.Greenwood && Game.player[0].inventory[0].stack==2,"A Dao seal cannot be abused as a removal stone through the old packet.");
using(var bytes=new MemoryStream()) {
 using(var writer=new BinaryWriter(bytes,Encoding.UTF8,true)){writer.Write((byte)12);writer.Write((byte)0);writer.Write(600);writer.Write((byte)1);writer.Write(601);writer.Write(300);writer.Write((byte)1);writer.Write((byte)3);writer.Write(true);writer.Write((byte)0);writer.Write((byte)1);}
 for(int length=0;length<bytes.Length;length++) {
  SetupDao();using var reader=new BinaryReader(new MemoryStream(bytes.ToArray()[..length]));mod.HandlePacket(reader,0);
  Check(XianXia.Common.Items.RefinedArtifact.GetDaoRoute(Game.player[0].inventory[1])==DownedBossSystem.EndgameRoute.None && Game.player[0].inventory[0].stack==2,"Truncated Dao packets cannot partially change equipment or consume materials.");
 }
}
SetupDao();Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.InscribedEquipment>().SetKind(InscriptionKind.None);Dao(kind:0);
Check(Game.player[0].inventory[0].stack==2,"An accurately described but uninscribed sample still cannot transform.");
SetupDao();Game.player[0].inventory[1].GetGlobalItem<XianXia.Common.Items.RefinedArtifact>().LoadData(null,new Terraria.ModLoader.IO.TagCompound { ["refinement"]=3 });Dao(awakened:false);
Check(Game.player[0].inventory[0].stack==2,"An accurately described but unawakened sample still cannot transform.");
SetupDao();Dao();Game.player[0].selectedItem=1;Game.player[0].statLife=85;Skill();
Check(Game.player[0].statLife==100,"Rebuild sword burst actually heals the owner without exceeding maximum health.");
SetupDao();Dao();Game.player[0].selectedItem=1;Game.player[0].statLife=50;Terraria.Projectile.AllowSpawn=false;Skill();
Check(Game.player[0].statLife==50 && Game.player[0].State.spiritualEnergy==100,"A failed Rebuild burst neither grants free healing nor spends energy.");
// Exercise the new actual weapon through the existing server packet path.
var codex=new XianXia.Content.Items.Weapons.ArchiveStarCodex {Mod=mod,Name="ArchiveStarCodex"};codex.SetDefaults();
var caster=Game.player[0];caster.active=true;caster.dead=caster.noItems=caster.CCed=false;caster.selectedItem=0;caster.altFunctionUse=0;
codex.Item.type=900;codex.Item.stack=1;codex.Item.ModItem=codex;caster.inventory[0]=codex.Item;
caster.State.NetworkInitialized=true;caster.State.spiritualEnergy=100;
for(int stage=0;stage<=8;stage++)for(int flags=0;flags<16;flags++){
 caster.State.cultivationStage=(CultivationStage)stage;
 Game.hardMode=(flags&1)!=0;Terraria.NPC.downedPlantBoss=(flags&2)!=0;Terraria.NPC.downedGolemBoss=(flags&4)!=0;Terraria.NPC.downedMoonlord=(flags&8)!=0;
 Check(codex.CanUseItem(caster)==(stage>=7&&flags==15),"Codex requires final realm and all native world stage gates");
}
Game.hardMode=Terraria.NPC.downedPlantBoss=Terraria.NPC.downedGolemBoss=Terraria.NPC.downedMoonlord=true;
caster.State.cultivationStage=CultivationStage.Tribulation;caster.State.spiritualEnergy=35;Check(!codex.CanUseItem(caster),"Codex refuses insufficient energy before firing");
void CodexShot(float x=100)=>Packet(0,w=>{w.Write((byte)8);w.Write((byte)0);w.Write(900);w.Write(x);w.Write(0f);});
Game.netMode=NetmodeID.Server;caster.State.spiritualEnergy=100;caster.State.WeaponShotCooldown=0;
foreach(var shot in Game.projectile)shot.active=false;Array.Clear(caster.ownedProjectileCounts);Terraria.Projectile.AllowSpawn=true;Terraria.ModLoader.CombinedHooks.SuppressDefault=false;Terraria.ModLoader.CombinedHooks.AllowShoot=true;
CodexShot();CodexShot();Check(Game.projectile.Count(p=>p.active)==1&&caster.State.spiritualEnergy==64,"Actual codex packet spends 36 energy once and throttles duplicate requests");
Check(Game.projectile.Single(p=>p.active).damage==caster.GetWeaponDamage(codex.Item),"Actual codex server packet uses canonical weapon damage");
foreach(var shot in Game.projectile)shot.active=false;caster.State.WeaponShotCooldown=0;caster.State.spiritualEnergy=100;Terraria.Projectile.AllowSpawn=false;CodexShot();Check(caster.State.spiritualEnergy==100&&!Game.projectile.Any(p=>p.active),"Actual codex spawn failure restores energy");
Terraria.Projectile.AllowSpawn=true;caster.State.WeaponShotCooldown=0;CodexShot(float.NaN);Check(caster.State.spiritualEnergy==100&&!Game.projectile.Any(p=>p.active),"Actual codex rejects nonfinite aim");
Game.netMode=NetmodeID.MultiplayerClient;codex.Shoot(caster,null,default,default,2,999,99);Check(caster.State.spiritualEnergy==100&&!Game.projectile.Any(p=>p.active),"Actual codex client cannot spend energy or create its own cast");
var orb=new XianXia.Content.Projectiles.ArchiveStarOrb();orb.SetDefaults();Check(orb.Projectile.DamageType==Terraria.ModLoader.DamageClass.Magic&&orb.Projectile.friendly&&!orb.Projectile.hostile,"Codex orb belongs to magic and damages enemies");
Check(orb.Projectile.tileCollide&&orb.Projectile.penetrate==4&&orb.Projectile.localNPCHitCooldown==20,"Codex orb preserves bounded penetration and terrain collision");
Game.dedServ=true;int lightBefore=Terraria.Lighting.Calls;Check(orb.CanDamage()==false,"Codex orb is harmless at cast start");
for(int i=0;i<17;i++)orb.AI();Check(orb.CanDamage()==false,"Codex orb cannot damage during its 18-tick charge");
orb.AI();Check(orb.CanDamage()==null,"Codex orb activates native damage after charging");
for(int i=0;i<10000;i++)orb.AI();Check(orb.Projectile.ai[0]==18&&Terraria.Lighting.Calls==lightBefore,"Codex age counter is bounded and server renders no lighting");
Game.dedServ=false;orb.AI();Check(Terraria.Lighting.Calls==lightBefore+1,"Codex client retains lighting");

// Real ranged implementations exercise the shared authority transaction.
var rangedWeapons=new XianXia.Common.Items.CultivationWeaponItem[]{new XianXia.Content.Items.Weapons.SectMechanismCrossbow(),new XianXia.Content.Items.Weapons.HeavenLawArbalest(),new XianXia.Content.Items.Weapons.StarCalamityMechanismCase()};
for(int n=0;n<rangedWeapons.Length;n++){
 var rangedWeapon=rangedWeapons[n];rangedWeapon.Mod=mod;rangedWeapon.SetDefaults();rangedWeapon.Item.type=901+n;rangedWeapon.Item.stack=1;rangedWeapon.Item.ModItem=rangedWeapon;caster.inventory[0]=rangedWeapon.Item;
 int cost=rangedWeapon.GetSpiritCost(caster),required=5+n,mask=n==0?3:n==1?7:15;
 caster.State.spiritualEnergy=100;
 for(int stage=0;stage<=8;stage++)for(int flags=0;flags<16;flags++){
  caster.State.cultivationStage=(CultivationStage)stage;Game.hardMode=(flags&1)!=0;Terraria.NPC.downedPlantBoss=(flags&2)!=0;Terraria.NPC.downedGolemBoss=(flags&4)!=0;Terraria.NPC.downedMoonlord=(flags&8)!=0;
  Check(rangedWeapon.CanUseItem(caster)==(stage>=required&&(flags&mask)==mask),"Ranged realm and world gates");
 }
 Game.hardMode=Terraria.NPC.downedPlantBoss=Terraria.NPC.downedGolemBoss=Terraria.NPC.downedMoonlord=true;caster.State.cultivationStage=(CultivationStage)required;
 caster.State.spiritualEnergy=cost-1;Check(!rangedWeapon.CanUseItem(caster),"Ranged energy gate");
 void RangedShot()=>Packet(0,w=>{w.Write((byte)8);w.Write((byte)0);w.Write(rangedWeapon.Item.type);w.Write(100f);w.Write(0f);});
 Game.netMode=NetmodeID.Server;caster.State.spiritualEnergy=100;caster.State.WeaponShotCooldown=0;foreach(var shot in Game.projectile)shot.active=false;
 RangedShot();RangedShot();Check(Game.projectile.Count(p=>p.active)==1&&caster.State.spiritualEnergy==100-cost,"Ranged server spends once and throttles duplicates");
 Check(Game.projectile.Single(p=>p.active).damage==caster.GetWeaponDamage(rangedWeapon.Item),"Ranged canonical damage");
 foreach(var shot in Game.projectile)shot.active=false;caster.State.WeaponShotCooldown=0;caster.State.spiritualEnergy=100;Terraria.Projectile.AllowSpawn=false;RangedShot();Check(caster.State.spiritualEnergy==100,"Ranged failed spawn rollback");Terraria.Projectile.AllowSpawn=true;
 Game.netMode=NetmodeID.MultiplayerClient;rangedWeapon.Shoot(caster,null,default,default,2,999,99);Check(caster.State.spiritualEnergy==100&&!Game.projectile.Any(p=>p.active),"Ranged client only requests shot");
}
foreach(var bolt in new XianXia.Content.Projectiles.MechanismBolt[]{new XianXia.Content.Projectiles.SectMechanismBolt(),new XianXia.Content.Projectiles.HeavenLawBolt(),new XianXia.Content.Projectiles.StarCalamityMechanismBolt()}){
 bolt.SetDefaults();Check(bolt.Projectile.DamageType==Terraria.ModLoader.DamageClass.Ranged&&bolt.Projectile.friendly&&!bolt.Projectile.hostile&&bolt.Projectile.tileCollide&&bolt.Projectile.timeLeft==150&&bolt.Projectile.localNPCHitCooldown==20,"Mechanism projectile native collision and ranged defaults");
 Game.dedServ=true;int lights=Terraria.Lighting.Calls;bolt.AI();Check(Terraria.Lighting.Calls==lights,"Mechanism server skips lighting");
}
var lawBolt=new XianXia.Content.Projectiles.HeavenLawBolt();var victim=new Terraria.NPC();lawBolt.OnHitNPC(victim,default,10);Check(victim.LastBuff==Terraria.ID.BuffID.Ichor&&victim.BuffDuration==120,"Heaven law applies bounded native Ichor");
var bouncing=new XianXia.Content.Projectiles.StarCalamityMechanismBolt();bouncing.SetDefaults();
bouncing.Projectile.velocity=new Microsoft.Xna.Framework.Vector2(0,3);Check(!bouncing.OnTileCollide(new Microsoft.Xna.Framework.Vector2(4,3))&&bouncing.Projectile.velocity.X==-4&&bouncing.Projectile.velocity.Y==3&&bouncing.Projectile.netUpdate,"First wall bounce reflects only blocked axis and syncs");
bouncing.Projectile.velocity=new Microsoft.Xna.Framework.Vector2(-4,0);Check(!bouncing.OnTileCollide(new Microsoft.Xna.Framework.Vector2(-4,3))&&bouncing.Projectile.velocity.Y==-3,"Second floor bounce");
Check(bouncing.OnTileCollide(new Microsoft.Xna.Framework.Vector2(-4,-3))&&bouncing.Projectile.ai[0]==2,"Third collision destroys through native hook without increasing bounce counter");
bouncing.Projectile.ai[0]=0;bouncing.Projectile.velocity=default;Check(!bouncing.OnTileCollide(new Microsoft.Xna.Framework.Vector2(4,3))&&bouncing.Projectile.velocity.X==-4&&bouncing.Projectile.velocity.Y==-3,"Corner reflects both axes");

var cauldron=new XianXia.Content.Items.Weapons.GreenwoodMedicineCauldron {Mod=mod,Name="GreenwoodMedicineCauldron"};cauldron.SetDefaults();cauldron.Item.type=910;cauldron.Item.stack=1;cauldron.Item.ModItem=cauldron;caster.inventory[0]=cauldron.Item;
caster.State.spiritualEnergy=100;caster.State.arrayDeploymentCooldown=0;Array.Clear(caster.ownedProjectileCounts);
for(int stage=0;stage<=8;stage++)for(int flags=0;flags<16;flags++){
 caster.State.cultivationStage=(CultivationStage)stage;Game.hardMode=(flags&1)!=0;Terraria.NPC.downedPlantBoss=(flags&2)!=0;Terraria.NPC.downedGolemBoss=(flags&4)!=0;Terraria.NPC.downedMoonlord=(flags&8)!=0;
 Check(cauldron.CanUseItem(caster)==(stage>=5&&(flags&3)==3),"Medicine cauldron realm and native world gates");
}
Game.hardMode=Terraria.NPC.downedPlantBoss=true;caster.State.cultivationStage=CultivationStage.NascentSoul;
caster.State.spiritualEnergy=31;Check(!cauldron.CanUseItem(caster),"Cauldron insufficient energy");caster.State.spiritualEnergy=100;
caster.State.arrayDeploymentCooldown=1;Check(!cauldron.CanUseItem(caster),"Cauldron obeys shared deployment cooldown");caster.State.arrayDeploymentCooldown=0;
caster.ownedProjectileCounts[cauldron.Item.shoot]=1;Check(!cauldron.CanUseItem(caster),"Cauldron cannot duplicate existing array");Array.Clear(caster.ownedProjectileCounts);
void CauldronShot()=>Packet(0,w=>{w.Write((byte)8);w.Write((byte)0);w.Write(910);w.Write(100f);w.Write(0f);});
Game.netMode=NetmodeID.Server;caster.State.WeaponShotCooldown=0;foreach(var shot in Game.projectile)shot.active=false;
CauldronShot();CauldronShot();Check(Game.projectile.Count(p=>p.active)==1&&caster.State.spiritualEnergy==68&&caster.State.arrayDeploymentCooldown==480,"Cauldron server charges once and starts shared cooldown");
Check(Game.projectile.Single(p=>p.active).damage==caster.GetWeaponDamage(cauldron.Item)&&Game.projectile.Single(p=>p.active).velocity.LengthSquared()==0,"Cauldron server uses canonical damage and stationary deployment");
foreach(var shot in Game.projectile)shot.active=false;Array.Clear(caster.ownedProjectileCounts);caster.State.WeaponShotCooldown=0;caster.State.arrayDeploymentCooldown=0;caster.State.spiritualEnergy=100;Terraria.Projectile.AllowSpawn=false;CauldronShot();Check(caster.State.spiritualEnergy==100&&caster.State.arrayDeploymentCooldown==0,"Cauldron failed spawn rolls back energy and array cooldown");Terraria.Projectile.AllowSpawn=true;
Game.netMode=NetmodeID.MultiplayerClient;cauldron.Shoot(caster,null,default,default,4,999,99);Check(caster.State.spiritualEnergy==100&&!Game.projectile.Any(p=>p.active),"Cauldron client only requests deployment");
var medicineField=new XianXia.Content.Projectiles.MedicineCauldronField();medicineField.SetDefaults();medicineField.Projectile.owner=0;medicineField.Projectile.active=true;medicineField.Projectile.damage=87;medicineField.Projectile.knockBack=3;
Check(medicineField.CanDamage()==false&&medicineField.Projectile.timeLeft==300&&!medicineField.Projectile.tileCollide&&medicineField.Projectile.DamageType==Terraria.ModLoader.DamageClass.Magic,"Cauldron field is bounded magic deployment without contact damage");
foreach(var enemy in Game.npc)enemy.active=false;Game.npc[0].active=true;Game.npc[0].Center=new Microsoft.Xna.Framework.Vector2(100,0);caster.Center=medicineField.Projectile.Center=default;
Game.dedServ=true;int medicineLights=Terraria.Lighting.Calls;medicineField.Projectile.ai[0]=59;medicineField.AI();Check(!Game.projectile.Any(p=>p.active)&&medicineField.Projectile.ai[0]==59,"Remote client cannot advance wave clock or spawn spirits");
Game.netMode=NetmodeID.Server;medicineField.AI();Check(Game.projectile.Count(p=>p.active)==2&&Game.projectile.All(p=>!p.active||p.owner==0&&p.damage==87&&p.type==5),"Server wave emits exactly two canonical magic spirits");
Check(Terraria.Lighting.Calls==medicineLights&&medicineField.Projectile.ai[0]==0,"Server wave resets clock without graphical work");
foreach(var shot in Game.projectile)shot.active=false;medicineField.Projectile.ai[0]=0;for(int i=0;i<59;i++)medicineField.AI();Check(!Game.projectile.Any(p=>p.active),"No wave before 60 ticks");medicineField.AI();Check(Game.projectile.Count(p=>p.active)==2,"Wave at 60 ticks");
foreach(var shot in Game.projectile)shot.active=false;Terraria.Collision.Visible=false;medicineField.Projectile.ai[0]=59;medicineField.AI();Check(!Game.projectile.Any(p=>p.active)&&medicineField.Projectile.ai[0]==0,"Blocked line of sight discards wave");Terraria.Collision.Visible=true;
Game.npc[0].Center=new Microsoft.Xna.Framework.Vector2(601,0);medicineField.Projectile.ai[0]=59;medicineField.AI();Check(!Game.projectile.Any(p=>p.active),"Out-of-range target is ignored");
Game.npc[0].Center=new Microsoft.Xna.Framework.Vector2(100,0);Game.npc[0].friendly=true;medicineField.Projectile.ai[0]=59;medicineField.AI();Check(!Game.projectile.Any(p=>p.active),"Friendly NPC cannot be targeted");Game.npc[0].friendly=false;
Game.npc[0].dontTakeDamage=true;medicineField.Projectile.ai[0]=59;medicineField.AI();Check(!Game.projectile.Any(p=>p.active),"Invulnerable NPC cannot be targeted");Game.npc[0].dontTakeDamage=false;
foreach(var enemy in Game.npc)enemy.active=false;medicineField.Projectile.ai[0]=59;medicineField.AI();Game.npc[0].active=true;medicineField.AI();Check(!Game.projectile.Any(p=>p.active)&&medicineField.Projectile.ai[0]==1,"Empty wave cannot stockpile attacks for acquired target");
medicineField.Projectile.owner=-1;medicineField.Projectile.active=true;medicineField.AI();Check(!medicineField.Projectile.active,"Invalid cauldron owner cleanup");medicineField.Projectile.owner=Game.maxPlayers;medicineField.Projectile.active=true;medicineField.AI();Check(!medicineField.Projectile.active,"Upper invalid owner cleanup");
medicineField.Projectile.owner=0;medicineField.Projectile.active=true;caster.dead=true;medicineField.AI();Check(!medicineField.Projectile.active,"Dead owner ends cauldron");caster.dead=false;
medicineField.Projectile.active=true;caster.active=false;medicineField.AI();Check(!medicineField.Projectile.active,"Disconnected owner ends cauldron");caster.active=true;
medicineField.Projectile.active=true;caster.Center=new Microsoft.Xna.Framework.Vector2(1601,0);medicineField.AI();Check(!medicineField.Projectile.active,"Distant owner ends cauldron");caster.Center=default;
medicineField.Projectile.active=true;medicineField.Projectile.ai[0]=59;Game.npc[1].active=true;Game.npc[1].Center=new Microsoft.Xna.Framework.Vector2(-50,0);medicineField.AI();Check(Game.projectile.Count(p=>p.active)==2&&Game.projectile.Where(p=>p.active).All(p=>p.velocity.X<0),"Cauldron prefers nearest visible enemy");
foreach(var shot in Game.projectile)shot.active=false;Game.netMode=NetmodeID.SinglePlayer;medicineField.Projectile.ai[0]=59;medicineField.AI();Check(Game.projectile.Count(p=>p.active)==2,"Single-player authority emits one wave");
foreach(var shot in Game.projectile)shot.active=false;Game.netMode=NetmodeID.Server;Terraria.Projectile.AllowSpawn=false;medicineField.Projectile.ai[0]=59;medicineField.AI();Terraria.Projectile.AllowSpawn=true;medicineField.AI();Check(!Game.projectile.Any(p=>p.active)&&medicineField.Projectile.ai[0]==1,"Failed wave is discarded without immediate catch-up spawn");
var medicineBolt=new XianXia.Content.Projectiles.MedicineSpiritBolt();medicineBolt.SetDefaults();Check(medicineBolt.Projectile.DamageType==Terraria.ModLoader.DamageClass.Magic&&medicineBolt.Projectile.tileCollide&&medicineBolt.Projectile.penetrate==1&&medicineBolt.Projectile.timeLeft==90,"Medicine spirit remains magic with bounded lifetime and terrain collision");medicineBolt.AI();Check(Terraria.Lighting.Calls==medicineLights,"Medicine spirit server skips lighting");Game.dedServ=false;medicineBolt.AI();Check(Terraria.Lighting.Calls==medicineLights+1,"Medicine spirit client retains lighting");

var hammer=new XianXia.Content.Items.Weapons.BlackFurnaceWarhammer {Mod=mod,Name="BlackFurnaceWarhammer"};hammer.SetDefaults();hammer.Item.type=920;hammer.Item.stack=1;hammer.Item.ModItem=hammer;caster.inventory[0]=hammer.Item;caster.State.spiritualEnergy=100;
for(int stage=0;stage<=8;stage++){caster.State.cultivationStage=(CultivationStage)stage;Check(hammer.CanUseItem(caster)==(stage>=2),"Hammer requires Qi Condensation without Hardmode restriction");}
caster.State.cultivationStage=CultivationStage.QiCondensation;caster.State.spiritualEnergy=11;Check(!hammer.CanUseItem(caster),"Hammer rejects insufficient energy");caster.State.spiritualEnergy=100;
void HammerShot()=>Packet(0,w=>{w.Write((byte)8);w.Write((byte)0);w.Write(920);w.Write(100f);w.Write(0f);});
Game.netMode=NetmodeID.Server;caster.State.WeaponShotCooldown=0;caster.State.arrayDeploymentCooldown=0;foreach(var shot in Game.projectile)shot.active=false;Array.Clear(caster.ownedProjectileCounts);
HammerShot();HammerShot();Check(Game.projectile.Count(p=>p.active)==1&&caster.State.spiritualEnergy==88,"Hammer request spends once and throttles repeats");Check(Game.projectile.Single(p=>p.active).damage==caster.GetWeaponDamage(hammer.Item)&&Game.projectile.Single(p=>p.active).type==6,"Hammer uses canonical melee damage and actual projectile type");
foreach(var shot in Game.projectile)shot.active=false;caster.State.spiritualEnergy=100;caster.State.WeaponShotCooldown=0;Terraria.Projectile.AllowSpawn=false;HammerShot();Check(caster.State.spiritualEnergy==100,"Hammer failed creation refunds resource");Terraria.Projectile.AllowSpawn=true;
Game.netMode=NetmodeID.MultiplayerClient;hammer.Shoot(caster,null,default,default,6,999,99);Check(caster.State.spiritualEnergy==100&&!Game.projectile.Any(p=>p.active),"Hammer client cannot fire or spend independently");
var thrownHammer=new XianXia.Content.Projectiles.FurnaceHammerProjectile();thrownHammer.SetDefaults();thrownHammer.Projectile.owner=0;thrownHammer.Projectile.damage=59;thrownHammer.Projectile.knockBack=9;
Check(thrownHammer.Projectile.DamageType==Terraria.ModLoader.DamageClass.Melee&&thrownHammer.Projectile.penetrate==1&&thrownHammer.Projectile.tileCollide&&thrownHammer.Projectile.timeLeft==90,"Hammer native melee collision and bounded lifetime");
Game.dedServ=true;int hammerLights=Terraria.Lighting.Calls;thrownHammer.Projectile.velocity=new Microsoft.Xna.Framework.Vector2(9,0);thrownHammer.AI();Check(thrownHammer.Projectile.velocity.Y==0.25f&&thrownHammer.Projectile.rotation>0,"Hammer has gravity and spins along travel direction");for(int i=0;i<1000;i++)thrownHammer.AI();Check(thrownHammer.Projectile.velocity.Y==12&&Terraria.Lighting.Calls==hammerLights,"Hammer gravity caps and dedicated server renders no lighting");
thrownHammer.Projectile.rotation=0;thrownHammer.Projectile.velocity=new Microsoft.Xna.Framework.Vector2(-9,0);thrownHammer.AI();Check(thrownHammer.Projectile.rotation<0,"Hammer reverse-direction spin");
thrownHammer.OnKill(10);Check(!Game.projectile.Any(p=>p.active),"Client destruction cannot duplicate impact");Game.netMode=NetmodeID.Server;thrownHammer.OnKill(10);Check(Game.projectile.Count(p=>p.active)==1&&Game.projectile.Single(p=>p.active).type==7&&Game.projectile.Single(p=>p.active).damage==29&&Game.projectile.Single(p=>p.active).owner==0,"Server creates one impact with half canonical parent damage");
foreach(var shot in Game.projectile)shot.active=false;Game.netMode=NetmodeID.SinglePlayer;thrownHammer.OnKill(0);Check(Game.projectile.Count(p=>p.active)==1,"Single-player destruction creates one impact");foreach(var shot in Game.projectile)shot.active=false;Game.netMode=NetmodeID.Server;
thrownHammer.Projectile.owner=-1;thrownHammer.OnKill(0);thrownHammer.Projectile.owner=Game.maxPlayers;thrownHammer.OnKill(0);Check(!Game.projectile.Any(p=>p.active),"Impact rejects invalid owner bounds");thrownHammer.Projectile.owner=0;caster.dead=true;thrownHammer.OnKill(0);caster.dead=false;caster.active=false;thrownHammer.OnKill(0);caster.active=true;Check(!Game.projectile.Any(p=>p.active),"No free impact after owner death or disconnect");
Terraria.Projectile.AllowSpawn=false;thrownHammer.OnKill(0);Terraria.Projectile.AllowSpawn=true;Check(!Game.projectile.Any(p=>p.active),"Impact spawn failure does not create fallback duplicates");
var impact=new XianXia.Content.Projectiles.FurnaceImpactBurst();impact.SetDefaults();Check(impact.Projectile.DamageType==Terraria.ModLoader.DamageClass.Melee&&impact.Projectile.width==96&&impact.Projectile.height==96&&impact.Projectile.usesLocalNPCImmunity&&impact.Projectile.localNPCHitCooldown==-1,"Melee impact uses bounded area and per-projectile single-hit immunity");
Check(impact.CanDamage()==null,"Impact starts damaging");impact.Projectile.timeLeft=9;Check(impact.CanDamage()==null,"Impact second tick remains damaging");impact.Projectile.timeLeft=8;Check(impact.CanDamage()==false,"Impact later visual frames cannot damage");
Terraria.Collision.Visible=false;Check(impact.CanHitNPC(Game.npc[0])==false,"Impact rejects wall-separated targets");Terraria.Collision.Visible=true;Check(impact.CanHitNPC(Game.npc[0])==null,"Impact leaves visible targets to native hitbox checks");impact.Projectile.velocity=new Microsoft.Xna.Framework.Vector2(3,3);impact.AI();Check(impact.Projectile.velocity.LengthSquared()==0&&Terraria.Lighting.Calls==hammerLights,"Impact stays stationary and server skips lighting");Game.dedServ=false;impact.AI();Check(Terraria.Lighting.Calls==hammerLights+1,"Impact retains client lighting");

var talismanBow=new XianXia.Content.Items.Weapons.TalismanCrossbow {Mod=mod,Name="TalismanCrossbow"};talismanBow.SetDefaults();talismanBow.Item.type=930;talismanBow.Item.stack=1;talismanBow.Item.ModItem=talismanBow;
var talismanArrow=new XianXia.Content.Items.Weapons.CinnabarTalismanArrow();talismanArrow.SetDefaults();
Check(talismanBow.Item.useAmmo==Terraria.ID.AmmoID.Arrow&&talismanBow.Item.DamageType==Terraria.ModLoader.DamageClass.Ranged&&talismanBow.Item.shoot==Terraria.ID.ProjectileID.WoodenArrowFriendly,"Talisman crossbow delegates native arrow selection and damage");
Check(talismanArrow.Item.ammo==talismanBow.Item.useAmmo&&talismanArrow.Item.consumable&&talismanArrow.Item.maxStack==9999&&talismanArrow.Item.DamageType==Terraria.ModLoader.DamageClass.Ranged,"Cinnabar ammunition matches native bow ammo category");
caster.inventory[0]=talismanBow.Item;caster.State.spiritualEnergy=100;caster.State.WeaponShotCooldown=0;Game.netMode=NetmodeID.Server;foreach(var shot in Game.projectile)shot.active=false;
Packet(0,w=>{w.Write((byte)8);w.Write((byte)0);w.Write(930);w.Write(100f);w.Write(0f);});Check(caster.State.spiritualEnergy==100&&!Game.projectile.Any(p=>p.active)&&caster.State.WeaponShotCooldown==0,"Native ammo bow cannot bypass arrow consumption using spirit shot packet");
var cinnabarArrow=new XianXia.Content.Projectiles.CinnabarArrowProjectile();cinnabarArrow.SetDefaults();
Check(cinnabarArrow.Projectile.arrow&&cinnabarArrow.Projectile.DamageType==Terraria.ModLoader.DamageClass.Ranged&&cinnabarArrow.Projectile.penetrate==2&&cinnabarArrow.Projectile.tileCollide&&cinnabarArrow.Projectile.timeLeft==180,"Cinnabar shot has arrow compatibility and bounded penetration/lifetime");
Check(cinnabarArrow.Projectile.usesLocalNPCImmunity&&cinnabarArrow.Projectile.localNPCHitCooldown==-1,"Cinnabar arrow cannot repeatedly hit one NPC");
Game.dedServ=true;int arrowLights=Terraria.Lighting.Calls;cinnabarArrow.Projectile.velocity=new Microsoft.Xna.Framework.Vector2(10,0);for(int i=0;i<19;i++)cinnabarArrow.AI();Check(cinnabarArrow.Projectile.velocity.Y==0&&cinnabarArrow.Projectile.ai[0]==19,"Arrow initial straight flight before gravity");cinnabarArrow.AI();Check(cinnabarArrow.Projectile.velocity.Y==0.1f&&cinnabarArrow.Projectile.ai[0]==20,"Arrow gravity starts at twenty ticks");
for(int i=0;i<10000;i++)cinnabarArrow.AI();Check(cinnabarArrow.Projectile.ai[0]==20&&cinnabarArrow.Projectile.velocity.Y==12&&Terraria.Lighting.Calls==arrowLights,"Arrow age/gravity bounded and server avoids lighting");
var burningTarget=new Terraria.NPC();cinnabarArrow.OnHitNPC(burningTarget,default,40);Check(burningTarget.LastBuff==Terraria.ID.BuffID.OnFire3&&burningTarget.BuffDuration==120,"Cinnabar arrow applies two-second native burning");Game.dedServ=false;cinnabarArrow.AI();Check(Terraria.Lighting.Calls==arrowLights+1,"Arrow client retains lighting");

var wardSeal=new XianXia.Content.Items.Weapons.HeavenTabletWardSeal {Mod=mod,Name="HeavenTabletWardSeal"};wardSeal.SetDefaults();wardSeal.Item.type=940;wardSeal.Item.stack=1;wardSeal.Item.ModItem=wardSeal;caster.inventory[0]=wardSeal.Item;caster.State.spiritualEnergy=100;
for(int stage=0;stage<=8;stage++)for(int flags=0;flags<16;flags++){
 caster.State.cultivationStage=(CultivationStage)stage;Game.hardMode=(flags&1)!=0;Terraria.NPC.downedPlantBoss=(flags&2)!=0;Terraria.NPC.downedGolemBoss=(flags&4)!=0;Terraria.NPC.downedMoonlord=(flags&8)!=0;
 Check(wardSeal.CanUseItem(caster)==(stage>=6&&(flags&7)==7),"Ward seal requires realm and Golem world progression");
}
Game.hardMode=Terraria.NPC.downedPlantBoss=Terraria.NPC.downedGolemBoss=true;caster.State.cultivationStage=CultivationStage.SpiritSevering;caster.State.spiritualEnergy=27;Check(!wardSeal.CanUseItem(caster),"Ward seal energy gate");
void WardShot()=>Packet(0,w=>{w.Write((byte)8);w.Write((byte)0);w.Write(940);w.Write(100f);w.Write(0f);});
Game.netMode=NetmodeID.Server;caster.State.spiritualEnergy=100;caster.State.WeaponShotCooldown=0;foreach(var shot in Game.projectile)shot.active=false;Array.Clear(caster.ownedProjectileCounts);
WardShot();WardShot();Check(Game.projectile.Count(p=>p.active)==1&&caster.State.spiritualEnergy==72,"Ward seal charges once and throttles duplicates");Check(Game.projectile.Single(p=>p.active).damage==caster.GetWeaponDamage(wardSeal.Item)&&Game.projectile.Single(p=>p.active).type==8,"Ward seal uses canonical melee damage");
foreach(var shot in Game.projectile)shot.active=false;caster.State.spiritualEnergy=100;caster.State.WeaponShotCooldown=0;Terraria.Projectile.AllowSpawn=false;WardShot();Check(caster.State.spiritualEnergy==100,"Ward seal failed spawn rollback");Terraria.Projectile.AllowSpawn=true;Game.netMode=NetmodeID.MultiplayerClient;wardSeal.Shoot(caster,null,default,default,8,999,99);Check(caster.State.spiritualEnergy==100&&!Game.projectile.Any(p=>p.active),"Ward client only requests casting");
var flyingWard=new XianXia.Content.Projectiles.HeavenTabletWardProjectile {Mod=mod};flyingWard.SetDefaults();flyingWard.Projectile.owner=0;flyingWard.Projectile.active=true;flyingWard.Projectile.Center=new Microsoft.Xna.Framework.Vector2(100,0);flyingWard.Projectile.velocity=new Microsoft.Xna.Framework.Vector2(7,0);caster.Center=default;Game.netMode=NetmodeID.Server;Game.dedServ=true;int wardLights=Terraria.Lighting.Calls;
Check(flyingWard.Projectile.DamageType==Terraria.ModLoader.DamageClass.Melee&&flyingWard.Projectile.penetrate==3&&flyingWard.Projectile.timeLeft==120&&flyingWard.Projectile.localNPCHitCooldown==20,"Ward bounded native melee defaults");
for(int i=0;i<29;i++)flyingWard.AI();Check(flyingWard.Projectile.tileCollide&&flyingWard.Projectile.velocity.X==7&&flyingWard.Projectile.ai[0]==29,"Ward outgoing phase lasts twenty-nine ticks");flyingWard.AI();Check(!flyingWard.Projectile.tileCollide&&flyingWard.Projectile.velocity.X==-12&&flyingWard.Projectile.ai[0]==30,"Ward returns at thirty ticks with bounded speed");
for(int i=0;i<10000;i++)flyingWard.AI();Check(flyingWard.Projectile.ai[0]==30&&Terraria.Lighting.Calls==wardLights,"Ward phase counter is bounded and server skips lighting");
flyingWard.Projectile.ai[0]=0;flyingWard.Projectile.tileCollide=true;Check(!flyingWard.OnTileCollide(new Microsoft.Xna.Framework.Vector2(7,0))&&flyingWard.Projectile.ai[0]==30&&!flyingWard.Projectile.tileCollide&&flyingWard.Projectile.netUpdate,"Terrain collision starts synchronized return");
Terraria.Collision.Visible=false;Check(flyingWard.CanHitNPC(Game.npc[0])==false,"Ward rejects wall-separated hits");Terraria.Collision.Visible=true;Check(flyingWard.CanHitNPC(Game.npc[0])==null,"Ward visible hit defers to native collision");
flyingWard.Projectile.Center=default;Game.netMode=NetmodeID.MultiplayerClient;flyingWard.AI();Check(flyingWard.Projectile.active,"Client waits for authoritative return cleanup");Game.netMode=NetmodeID.Server;Game.myPlayer=255;int wardKills=Terraria.NetMessage.Broadcasts;flyingWard.AI();Check(!flyingWard.Projectile.active,"Server ends ward when it reaches owner");Check(Terraria.NetMessage.Broadcasts==wardKills+1,"Server explicitly broadcasts native player-owned ward destruction");
flyingWard.Projectile.Center=new Microsoft.Xna.Framework.Vector2(100,0);
foreach(int invalidOwner in new[]{-1,Game.maxPlayers}){flyingWard.Projectile.owner=invalidOwner;flyingWard.Projectile.active=true;flyingWard.AI();Check(!flyingWard.Projectile.active&&flyingWard.CanHitNPC(Game.npc[0])==false,"Ward invalid owner cleanup and hit guard");}
flyingWard.Projectile.owner=0;flyingWard.Projectile.active=true;caster.dead=true;flyingWard.AI();Check(!flyingWard.Projectile.active,"Dead owner ends ward");caster.dead=false;caster.active=false;flyingWard.Projectile.active=true;flyingWard.AI();Check(!flyingWard.Projectile.active,"Disconnected owner ends ward");caster.active=true;caster.Center=new Microsoft.Xna.Framework.Vector2(1800,0);flyingWard.Projectile.active=true;flyingWard.AI();Check(!flyingWard.Projectile.active,"Distant owner ends ward");caster.Center=default;
var wardPlayer=new XianXia.Common.Players.HeavenTabletWardPlayer {Player=caster};caster.ownedProjectileCounts[8]=2;
Game.projectile[0]=new Terraria.Projectile {active=true,type=8,owner=0,Center=new Microsoft.Xna.Framework.Vector2(160,0)};Game.projectile[1]=new Terraria.Projectile {active=true,type=8,owner=0,Center=new Microsoft.Xna.Framework.Vector2(20,0)};
for(int mode=0;mode<=2;mode++){Game.netMode=mode;caster.statDefense=10;wardPlayer.PostUpdateEquips();Check(caster.statDefense==16,"Ward equipment bonus recomputes without stacking on every peer");}
Game.projectile[1].active=false;Game.projectile[0].Center=new Microsoft.Xna.Framework.Vector2(161,0);caster.statDefense=10;wardPlayer.PostUpdateEquips();Check(caster.statDefense==10,"Ward defense ends beyond guard radius");Game.projectile[0].Center=new Microsoft.Xna.Framework.Vector2(160,0);
Terraria.Collision.Visible=false;wardPlayer.PostUpdateEquips();Check(caster.statDefense==10,"Wall-separated ward cannot guard owner");Terraria.Collision.Visible=true;
Game.projectile[0].owner=1;wardPlayer.PostUpdateEquips();Check(caster.statDefense==10,"Other player's ward cannot grant defense");Game.projectile[0].owner=0;
caster.inventory[0]=talismanBow.Item;wardPlayer.PostUpdateEquips();Check(caster.statDefense==10,"Switching held weapon removes ward benefit");caster.inventory[0]=wardSeal.Item;
caster.dead=true;wardPlayer.PostUpdateEquips();caster.dead=false;Check(caster.statDefense==10,"Dead player gains no ward defense");caster.active=false;wardPlayer.PostUpdateEquips();caster.active=true;Check(caster.statDefense==10,"Inactive player gains no ward defense");
Game.projectile[0].active=false;wardPlayer.PostUpdateEquips();Check(caster.statDefense==10,"Expired ward gains no defense");

// Exercise native hook boundaries with the actual server sentinel and real mod type.
var relay=new XianXia.Common.Systems.ServerPlayerProjectileSync();Game.netMode=NetmodeID.Server;Game.myPlayer=255;
var relayProjectile=new Terraria.Projectile {active=true,owner=0,identity=321,whoAmI=3,ModProjectile=new XianXia.Content.Projectiles.MedicineCauldronField {Mod=mod},netUpdate=true};
int relayMessages=Terraria.NetMessage.Sent.Count;relay.OnSpawn(relayProjectile,null);Check(Terraria.NetMessage.Sent.Count==relayMessages+1&&Terraria.NetMessage.Sent.Last()==(Terraria.ID.MessageID.SyncProjectile,3,0f)&&!relayProjectile.netUpdate,"Server-created player-owned mod projectile broadcasts exact native slot once");
relay.PostAI(relayProjectile);Check(Terraria.NetMessage.Sent.Count==relayMessages+1,"Unchanged projectile is not broadcast every tick");relayProjectile.netUpdate=true;relay.PostAI(relayProjectile);Check(Terraria.NetMessage.Sent.Count==relayMessages+2&&!relayProjectile.netUpdate,"Server sends dirty state even though projectile belongs to a player");
relay.OnKill(relayProjectile,100);Check(Terraria.NetMessage.Sent.Last()==(Terraria.ID.MessageID.KillProjectile,321,0f),"Relay destruction uses identity plus owner rather than native array slot");
for(int mode=0;mode<=1;mode++){Game.netMode=mode;relayProjectile.netUpdate=true;int count=Terraria.NetMessage.Sent.Count;relay.OnSpawn(relayProjectile,null);relay.PostAI(relayProjectile);relay.OnKill(relayProjectile,100);Check(Terraria.NetMessage.Sent.Count==count&&relayProjectile.netUpdate,"Single-player and clients retain their native owner sync path");}
Game.netMode=NetmodeID.Server;
foreach(int excludedOwner in new[]{-1,Game.maxPlayers,255}){relayProjectile.owner=excludedOwner;relayProjectile.netUpdate=true;int count=Terraria.NetMessage.Sent.Count;relay.OnSpawn(relayProjectile,null);relay.PostAI(relayProjectile);relay.OnKill(relayProjectile,100);Check(Terraria.NetMessage.Sent.Count==count&&relayProjectile.netUpdate,"Invalid and server-owned projectiles keep native behavior");}
relayProjectile.owner=0;relayProjectile.ModProjectile=null;int excludedMessages=Terraria.NetMessage.Sent.Count;relay.OnSpawn(relayProjectile,null);relay.PostAI(relayProjectile);relay.OnKill(relayProjectile,100);Check(Terraria.NetMessage.Sent.Count==excludedMessages,"Vanilla projectile traffic is not intercepted");
relayProjectile.ModProjectile=new XianXia.Content.Projectiles.MedicineCauldronField {Mod=new Terraria.ModLoader.Mod()};relay.OnSpawn(relayProjectile,null);relay.PostAI(relayProjectile);relay.OnKill(relayProjectile,100);Check(Terraria.NetMessage.Sent.Count==excludedMessages,"Other mods keep their own networking behavior");
relayProjectile.ModProjectile=new XianXia.Content.Projectiles.MedicineCauldronField {Mod=mod};relayProjectile.active=false;relay.OnSpawn(relayProjectile,null);relay.PostAI(relayProjectile);Check(Terraria.NetMessage.Sent.Count==excludedMessages,"Inactive projectiles cannot send creation or dirty-state packets");
relayProjectile.active=true;relayProjectile.Kill();Check(!relayProjectile.active&&Terraria.NetMessage.Sent.Count==excludedMessages+1,"Native destruction hook broadcasts once");relayProjectile.Kill();Check(Terraria.NetMessage.Sent.Count==excludedMessages+1,"Repeated cleanup of inactive projectile sends no second death packet");
medicineField.Mod=mod;medicineField.Projectile.active=true;medicineField.Projectile.identity=555;medicineField.Projectile.owner=0;caster.dead=true;medicineField.AI();caster.dead=false;Check(!medicineField.Projectile.active&&Terraria.NetMessage.Sent.Last()==(Terraria.ID.MessageID.KillProjectile,555,0f),"Medicine field owner-death cleanup broadcasts through global native hook");
var joinField=new XianXia.Content.Projectiles.MedicineCauldronField();joinField.SetDefaults();Check(joinField.Projectile.netImportant,"Medicine field is included in native late-join projectile sync");Game.myPlayer=0;

Game.myPlayer=255;Game.netMode=NetmodeID.Server;Game.dedServ=true;caster.Center=default;caster.active=true;caster.dead=false;
foreach(var legacyArray in new Terraria.ModLoader.ModProjectile[]{new XianXia.Content.Projectiles.GreenwoodArrayField {Mod=mod},new XianXia.Content.Projectiles.ThunderTalismanArray {Mod=mod}}){
 legacyArray.SetDefaults();legacyArray.Projectile.owner=0;legacyArray.Projectile.Center=default;legacyArray.Projectile.active=true;
 Check(legacyArray.CanDamage()==null&&legacyArray.Projectile.netImportant&&legacyArray.Projectile.DamageType==Terraria.ModLoader.DamageClass.Magic,"Living legacy array retains native magic damage and late-join defaults");
 foreach(int badOwner in new[]{-1,Game.maxPlayers}){legacyArray.Projectile.owner=badOwner;legacyArray.Projectile.active=true;legacyArray.AI();Check(!legacyArray.Projectile.active&&legacyArray.CanDamage()==false,"Legacy array rejects invalid owner bounds without indexing player");}
 legacyArray.Projectile.owner=0;legacyArray.Projectile.active=true;caster.dead=true;int killedBefore=Terraria.NetMessage.Sent.Count;legacyArray.AI();Check(!legacyArray.Projectile.active&&legacyArray.CanDamage()==false&&Terraria.NetMessage.Sent.Count==killedBefore+1,"Server kills and broadcasts dead-owner legacy array");caster.dead=false;
 legacyArray.Projectile.active=true;caster.active=false;legacyArray.AI();Check(!legacyArray.Projectile.active,"Disconnected owner ends legacy array");caster.active=true;
 legacyArray.Projectile.active=true;legacyArray.Projectile.Center=new Microsoft.Xna.Framework.Vector2(1601,0);legacyArray.AI();Check(!legacyArray.Projectile.active&&legacyArray.CanDamage()==false,"Legacy array ends beyond owner leash");
 legacyArray.Projectile.active=true;legacyArray.Projectile.Center=new Microsoft.Xna.Framework.Vector2(1600,0);Check(legacyArray.CanDamage()==null,"Legacy array leash includes exact boundary");
 Game.netMode=NetmodeID.MultiplayerClient;caster.dead=true;legacyArray.AI();Check(legacyArray.Projectile.active&&legacyArray.CanDamage()==false,"Client disables orphan damage and awaits native server death message");caster.dead=false;Game.netMode=NetmodeID.Server;
}
var recoveryField=new XianXia.Content.Projectiles.GreenwoodArrayField {Mod=mod};recoveryField.SetDefaults();recoveryField.Projectile.owner=0;recoveryField.Projectile.active=true;recoveryField.Projectile.Center=default;
caster.State=new XianXia.Common.Players.XianXiaPlayer {spiritualEnergy=10,maxSpiritualEnergy=40};caster.statLife=50;caster.statLifeMax2=100;Game.player[1].statLife=50;
int recoveryLights=Terraria.Lighting.Calls;Game.GameUpdateCount=59;recoveryField.AI();Check(caster.statLife==50&&caster.State.spiritualEnergy==10,"Greenwood recovery waits for sixty-tick boundary");Game.GameUpdateCount=60;recoveryField.AI();Check(caster.statLife==51&&caster.State.spiritualEnergy==11&&Game.player[1].statLife==50,"Server recovery restores one life and energy only to intersecting owner");
var overlapField=new XianXia.Content.Projectiles.GreenwoodArrayField {Mod=mod};overlapField.SetDefaults();overlapField.Projectile.owner=0;overlapField.AI();Check(caster.statLife==51&&caster.State.spiritualEnergy==11,"Overlapping greenwood fields share per-player recovery limit");
Game.GameUpdateCount=120;caster.Center=new Microsoft.Xna.Framework.Vector2(200,0);recoveryField.AI();Check(caster.statLife==51&&caster.State.spiritualEnergy==11,"Outside field cannot recover resources");caster.Center=default;
Game.netMode=NetmodeID.MultiplayerClient;recoveryField.AI();Check(caster.statLife==51&&caster.State.spiritualEnergy==11,"Client cannot grant independent recovery");Game.netMode=NetmodeID.SinglePlayer;recoveryField.AI();Check(caster.statLife==52&&caster.State.spiritualEnergy==12,"Single-player recovery retains native heal behavior");
Game.GameUpdateCount=180;caster.statLife=100;caster.State.spiritualEnergy=40;recoveryField.AI();Check(caster.statLife==100&&caster.State.spiritualEnergy==40,"Recovery cannot overflow life or energy limits");Check(Terraria.Lighting.Calls==recoveryLights,"Dedicated server greenwood field never renders lighting");
Game.netMode=NetmodeID.Server;foreach(var shot in Game.projectile)shot.active=false;Array.Clear(caster.ownedProjectileCounts);
var lightningField=new XianXia.Content.Projectiles.ThunderTalismanArray {Mod=mod};lightningField.SetDefaults();lightningField.Projectile.owner=0;lightningField.Projectile.active=true;lightningField.Projectile.damage=101;int lightningLights=Terraria.Lighting.Calls;
for(int life=240;life>0;life--){lightningField.Projectile.timeLeft=life;lightningField.AI();}
Check(Game.projectile.Count(p=>p.active)==5&&Game.projectile.Where(p=>p.active).All(p=>p.owner==0&&p.damage==50&&p.type==10&&p.velocity.Y==13),"Thunder field retains five authoritative half-damage lightning waves over its lifetime");
Check(Terraria.Lighting.Calls==lightningLights,"Dedicated thunder field renders no lighting");foreach(var shot in Game.projectile)shot.active=false;Game.netMode=NetmodeID.MultiplayerClient;lightningField.Projectile.timeLeft=225;lightningField.AI();Check(!Game.projectile.Any(p=>p.active),"Client cannot duplicate lightning waves");
Game.netMode=NetmodeID.Server;caster.dead=true;lightningField.Projectile.active=true;lightningField.AI();Check(!Game.projectile.Any(p=>p.active)&&!lightningField.Projectile.active,"Dead owner cannot release a final free lightning wave");caster.dead=false;
var inheritedBolt=new XianXia.Content.Projectiles.MinorThunderboltProjectile();inheritedBolt.SetDefaults();inheritedBolt.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(new Terraria.Projectile {DamageType=Terraria.ModLoader.DamageClass.Melee}));Check(inheritedBolt.Projectile.DamageType==Terraria.ModLoader.DamageClass.Melee,"Shared bolt inherits melee from sword case");
inheritedBolt.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(lightningField.Projectile));Check(inheritedBolt.Projectile.DamageType==Terraria.ModLoader.DamageClass.Magic,"Shared bolt inherits magic from thunder field");inheritedBolt.AI();Check(Terraria.Lighting.Calls==lightningLights,"Shared bolt avoids dedicated-server lighting");Game.dedServ=false;inheritedBolt.AI();Check(Terraria.Lighting.Calls==lightningLights+1,"Shared bolt retains client lighting");Game.myPlayer=0;

Console.WriteLine($"Networking/artificing/skills/routes/Dao regression passed: {assertions} assertions. Engine boundaries are stubbed; live multiplayer remains required.");

// Actual recovery ModItem hooks plus production server inventory transaction.
Game.netMode=NetmodeID.Server;Game.myPlayer=255;
var recoveryPlayer=Game.player[0];recoveryPlayer.active=true;recoveryPlayer.dead=false;recoveryPlayer.noItems=recoveryPlayer.CCed=false;recoveryPlayer.selectedItem=0;
recoveryPlayer.State.NetworkInitialized=true;recoveryPlayer.State.discoveredSpiritualEnergy=true;recoveryPlayer.State.maxSpiritualEnergy=200;
var realPill=new XianXia.Content.Items.Materials.QiRecoveryPill{Mod=mod,Name="QiRecoveryPill"};realPill.SetDefaults();realPill.Item.type=951;realPill.Item.stack=10;realPill.Item.ModItem=realPill;recoveryPlayer.inventory[0]=realPill.Item;
foreach(var recoveryQuality in new[]{PillQuality.Coarse,PillQuality.Standard,PillQuality.Fine,PillQuality.Spirit}){
 recoveryPlayer.buffTime= new int[22];recoveryPlayer.State.spiritualEnergy=0;recoveryPlayer.State.spiritPressure=95;recoveryPlayer.State.ProgressionItemCooldown=0;
 realPill.Item.GetGlobalItem<PillQualitySystem>().LoadData(realPill.Item,new Terraria.ModLoader.IO.TagCompound{{"quality",(int)recoveryQuality},{"crafted",true}});
 int stack=realPill.Item.stack;
 Check(realPill.CanUseItem(recoveryPlayer),"Actual pill usable with missing energy");
 CultivationItemTransactions.HandleRequest(recoveryPlayer,0,951);
 Check(realPill.Item.stack==stack-1&&recoveryPlayer.State.spiritualEnergy==PillQualityRules.Scale(40,recoveryQuality)+PillQualityRules.BonusEnergy(recoveryQuality),"Actual graded recovery and quality bonus execute once");
 Check(recoveryPlayer.State.spiritPressure==100&&recoveryPlayer.buffTime[2]==1800,"Actual fixed pressure clamp and cooldown across qualities");
 recoveryPlayer.State.ProgressionItemCooldown=0;CultivationItemTransactions.HandleRequest(recoveryPlayer,0,951);
 Check(realPill.Item.stack==stack-1,"Actual cooldown rejects without consuming");
}
recoveryPlayer.buffTime=new int[22];recoveryPlayer.State.spiritualEnergy=198;recoveryPlayer.State.ProgressionItemCooldown=0;CultivationItemTransactions.HandleRequest(recoveryPlayer,0,951);
Check(recoveryPlayer.State.spiritualEnergy==200,"Actual recovery and extra bonus clamp to energy maximum");
recoveryPlayer.buffTime=new int[22];Check(!realPill.CanUseItem(recoveryPlayer),"Actual full-energy rejection");
recoveryPlayer.State.spiritualEnergy=0;recoveryPlayer.State.discoveredSpiritualEnergy=false;Check(!realPill.CanUseItem(recoveryPlayer),"Actual unawakened rejection");recoveryPlayer.State.discoveredSpiritualEnergy=true;
int noEffectEnergy=recoveryPlayer.State.spiritualEnergy;realPill.UseItem(recoveryPlayer);Check(recoveryPlayer.State.spiritualEnergy==noEffectEnergy,"Replicated server use hook does not bypass transaction");
Game.netMode=NetmodeID.MultiplayerClient;Game.myPlayer=0;int oldStack=realPill.Item.stack;int beforeRecoveryPackets=ModPacket.Sent.Count;realPill.UseItem(recoveryPlayer);
Check(recoveryPlayer.State.spiritualEnergy==noEffectEnergy&&realPill.Item.stack==oldStack&&!new CultivationItemTransactions().ConsumeItem(realPill.Item,recoveryPlayer),"Client requests without restoring or consuming");
Check(ModPacket.Sent.Count==beforeRecoveryPackets+1&&ModPacket.Sent.Last().Data[0]==5,"Actual client pill sends canonical inventory request");
recoveryPlayer.dead=true;Check(!realPill.CanUseItem(recoveryPlayer),"Dead owner cannot use recovery pill");recoveryPlayer.dead=false;recoveryPlayer.active=false;Check(!realPill.CanUseItem(recoveryPlayer),"Inactive owner cannot use recovery pill");recoveryPlayer.active=true;
Game.netMode=NetmodeID.SinglePlayer;recoveryPlayer.State.spiritPressure=0;recoveryPlayer.buffTime=new int[22];realPill.UseItem(recoveryPlayer);
Check(recoveryPlayer.State.spiritualEnergy==60&&recoveryPlayer.State.spiritPressure==10&&recoveryPlayer.buffTime[2]==1800,"Actual singleplayer ModItem applies stored spirit quality, pressure and cooldown");
// Actual defensive pill and effect hooks, sharing the recovery fatigue.
var guardPill=new XianXia.Content.Items.Materials.FurnaceGuardPill{Mod=mod,Name="FurnaceGuardPill"};guardPill.SetDefaults();guardPill.Item.type=952;guardPill.Item.stack=10;guardPill.Item.ModItem=guardPill;recoveryPlayer.inventory[0]=guardPill.Item;
Game.netMode=NetmodeID.Server;Game.myPlayer=255;recoveryPlayer.buffTime=new int[22];
Check(CultivationItemTransactions.IsProgressionItem(guardPill.Item)&&PillQualitySystem.IsPill(guardPill.Item),"Guard pill joins transaction and stored-quality paths");
recoveryPlayer.State.cultivationStage=CultivationStage.QiAwakening;Check(!guardPill.CanUseItem(recoveryPlayer),"Guard pill requires Qi Condensation");recoveryPlayer.State.cultivationStage=CultivationStage.QiCondensation;
foreach(var guardQuality in new[]{PillQuality.Coarse,PillQuality.Standard,PillQuality.Fine,PillQuality.Spirit}){
 recoveryPlayer.buffTime=new int[22];recoveryPlayer.State.ProgressionItemCooldown=0;
 guardPill.Item.GetGlobalItem<PillQualitySystem>().LoadData(guardPill.Item,new Terraria.ModLoader.IO.TagCompound{{"quality",(int)guardQuality},{"crafted",true}});
 int stack=guardPill.Item.stack;Check(guardPill.CanUseItem(recoveryPlayer),"Guard pill eligible before dosing");CultivationItemTransactions.HandleRequest(recoveryPlayer,0,952);
 Check(guardPill.Item.stack==stack-1&&recoveryPlayer.buffTime[3]==PillQualityRules.Scale(3600,guardQuality)&&recoveryPlayer.buffTime[2]==1800,"Guard duration is graded, fatigue fixed and one consumed");
 recoveryPlayer.State.ProgressionItemCooldown=0;CultivationItemTransactions.HandleRequest(recoveryPlayer,0,952);Check(guardPill.Item.stack==stack-1,"Active guard pill cannot be refreshed or double consumed");
 recoveryPlayer.buffTime[3]=0;Check(!guardPill.CanUseItem(recoveryPlayer),"Removing guard does not clear shared fatigue");
 recoveryPlayer.State.spiritualEnergy=0;Check(!realPill.CanUseItem(recoveryPlayer),"Guard fatigue blocks recovery pill");
}
recoveryPlayer.buffTime=new int[22];realPill.Item.GetGlobalItem<PillQualitySystem>().LoadData(realPill.Item,new Terraria.ModLoader.IO.TagCompound{{"quality",2},{"crafted",true}});recoveryPlayer.inventory[0]=realPill.Item;recoveryPlayer.State.ProgressionItemCooldown=0;CultivationItemTransactions.HandleRequest(recoveryPlayer,0,951);
Check(!guardPill.CanUseItem(recoveryPlayer),"Recovery fatigue blocks guard pill");
var guardEffect=new XianXia.Content.Buffs.FurnaceGuardBuff();guardEffect.SetStaticDefaults();int guardIndex=3;
Check(!Game.buffNoSave[3],"Guard effect participates in native buff saving");
recoveryPlayer.statDefense=20;recoveryPlayer.moveSpeed=1;guardEffect.Update(recoveryPlayer,ref guardIndex);Check(recoveryPlayer.statDefense==28&&Math.Abs(recoveryPlayer.moveSpeed-.9f)<.001f,"Actual guard buff adds defense and movement tradeoff");
recoveryPlayer.statDefense=20;recoveryPlayer.moveSpeed=1;guardEffect.Update(recoveryPlayer,ref guardIndex);Check(recoveryPlayer.statDefense==28&&Math.Abs(recoveryPlayer.moveSpeed-.9f)<.001f,"Effect reapplies from reset player stats without permanent accumulation");
Console.WriteLine($"Networking including actual recovery pill hooks: {assertions} assertions; buff/inventory engine boundaries mocked.");
sealed class TestBossSummon : XianXia.Common.Items.CultivationBossSummonItem
{
    public override int BossType => 100;
}

sealed class TestWeapon : XianXia.Common.Items.CultivationWeaponItem
{
    public bool Array;
    public override int GetSpiritCost(Terraria.Player player) => 4;
    public override bool DeploysArray => Array;
}
sealed class TestInscriptionTool : XianXia.Common.Items.InscriptionToolItem
{
    public InscriptionKind Next;
    public bool Refining;
    public bool Awakening;
    public bool Transforming;
    public override bool TransformsArtifact=>Transforming;
    public override bool AwakensArtifact=>Awakening;
    public override bool RefinesArtifact=>Refining;
    public override InscriptionKind TargetKind=>Next;
}
