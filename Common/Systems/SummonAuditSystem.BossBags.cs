using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditBossBagOpening(Action<bool,string> check)
    {
        var specifications = new[] {
            ("SpiritVeinWyrm","VeinCoilCharm","LowGradeSpiritStone",12,18,"SpiritGel",20,35),
            ("GardenWarden","GardenLifeCharm","GreenwoodRoot",24,44,"LowGradeSpiritStone",8,16),
            ("BlackFurnaceIronGolem","FurnaceHeatGuard","FurnaceSlagIron",16,28,"LowGradeSpiritStone",8,16),
            ("TribulationCloudAvatar","TribulationConductor","TribulationCloudDew",24,44,"LowGradeSpiritStone",8,16),
            ("ThunderMarshJiao","JiaoStormStride","TribulationCloudDew",16,28,"ThunderPatternFeather",8,16),
            ("AbyssalStarWomb","AbyssSpiritLens","StarEclipseCrystal",24,44,"LowGradeSpiritStone",8,16),
            ("FormlessSwordSoul","FormlessSwordEmblem","SectTrialToken",16,28,"LowGradeSpiritStone",8,16),
            ("MoonboneImmortal","MoonboneSpiritBond","Moonbone",16,28,"DaoSeveringDust",8,16),
            ("GreenwoodMedicineKingEcho","MedicineKingLifeSeal","GreenwoodRoot",16,28,"SpringReturnPill",8,16),
            ("HeavenTabletGuardian","HeavenTabletBulwark","HeavenDaoFragment",24,44,"LowGradeSpiritStone",8,16),
            ("BrokenHeavenInspector","InspectorQuiverSeal","HeavenDaoFragment",24,44,"LowGradeSpiritStone",8,16),
            ("OldHeavenDaoCore","OldHeavenResonator","DaoSeveringDust",24,44,"LowGradeSpiritStone",8,16),
        };
        var mod = ModContent.GetInstance<global::XianXia.XianXia>();
        Player previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer, difficulty = Main.GameMode;
        var random = Main.rand; var existing = Main.item.Where(i => i.active).ToHashSet();
        int Find(string name) => mod.Find<ModItem>(name).Type;
        try {
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0; Main.rand = new UnifiedRandom(307);
            var actor = new Player { whoAmI = 0, active = true, statLife = 100 }; actor.Center = new Microsoft.Xna.Framework.Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
            foreach (int gameMode in new[] { 1,2 }) {
                Main.GameMode = gameMode;
                foreach (var spec in specifications) {
                    int bag = Find(spec.Item1 + "Bag"), expert = Find(spec.Item2);
                    var item = new Item(bag);
                    check(ItemID.Sets.BossBag[bag] && item.expert && item.consumable && item.ModItem.CanRightClick(),"native registered boss bag is expert and openable: " + spec.Item1);
                    for (int sample = 0; sample < 8; sample++) {
                        actor.OpenBossBag(bag);
                        Item[] drops = Main.item.Where(i => i.active && !existing.Contains(i)).ToArray();
                        int Count(int type) => drops.Where(i => i.type == type).Sum(i => i.stack);
                        check(Count(expert) == 1,"native actual bag opening emits exactly one matching expert accessory: " + spec.Item1);
                        check(Count(Find(spec.Item3)) >= spec.Item4 && Count(Find(spec.Item3)) <= spec.Item5,"native actual bag opening core material within independent range: " + spec.Item1);
                        check(Count(Find(spec.Item6)) >= spec.Item7 && Count(Find(spec.Item6)) <= spec.Item8,"native actual bag opening secondary material within independent range: " + spec.Item1);
                        check(!drops.Any(i => ItemID.Sets.BossBag[i.type]),"native actual bag opening never emits another boss bag: " + spec.Item1);
                        if (spec.Item1 == "SpiritVeinWyrm") check(Count(Find("LowGradeSpiritCore")) == 1 && Count(Find("SpiritVeinScale")) is >= 12 and <= 18,"native wyrm bag retains guaranteed core and scales");
                        if (spec.Item1 == "OldHeavenDaoCore") check(Count(Find("RouteMaterial")) == 1 && Count(Find("SilentTabletDecoration")) == 1,"native final boss bag retains route material and decoration");
                        foreach (Item drop in drops) drop.active = false;
                    }
                }
            }
        } finally {
            foreach (Item item in Main.item.Where(i => i.active && !existing.Contains(i))) item.active = false;
            Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; Main.GameMode = difficulty; Main.rand = random;
        }
        check(ReferenceEquals(Main.player[0],previous) && ReferenceEquals(Main.rand,random) && Main.item.Where(i => i.active).ToHashSet().SetEquals(existing),"native boss bag opening restores player random difficulty and active item pool");
    }
    private static void AuditBossBagDistribution(Action<bool,string> check)
    {
        string[] bosses = { "SpiritVeinWyrm","GardenWarden","BlackFurnaceIronGolem","TribulationCloudAvatar","ThunderMarshJiao","AbyssalStarWomb","FormlessSwordSoul","MoonboneImmortal","GreenwoodMedicineKingEcho","HeavenTabletGuardian","BrokenHeavenInspector","OldHeavenDaoCore" };
        var mod = ModContent.GetInstance<global::XianXia.XianXia>();
        Player[] players = Main.player.Take(2).ToArray(); var clients = (RemoteClient[])Netplay.Clients.Clone();
        bool[] broadcast = NetMessage.buffer.Take(2).Select(b => b.broadcast).ToArray();
        int mode = Main.netMode, local = Main.myPlayer, difficulty = Main.GameMode; var random = Main.rand;
        var items = Main.item.Where(i => i.active).ToHashSet(); var npcs = Main.npc.Where(n => n.active).ToHashSet();
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Main.rand = new UnifiedRandom(1307);
            var outputs = new[] { new AuditOutputSocket(true), new AuditOutputSocket(true) };
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i < 2 ? outputs[i] : new AuditOutputSocket(), State = i < 2 ? 10 : 0, IsActive = i < 2 };
            for (int i = 0; i < 2; i++) { Main.player[i] = new Player { whoAmI = i, active = true, statLife = 100 }; Main.player[i].Center = new Microsoft.Xna.Framework.Vector2(Main.maxTilesX * 8,400); NetMessage.buffer[i].broadcast = true; }
            foreach (int gameMode in new[] { 0,1,2 }) {
                Main.GameMode = gameMode;
                foreach (string name in bosses) {
                    int type = mod.Find<ModNPC>(name).Type, bag = mod.Find<ModItem>(name + "Bag").Type;
                    int slot = NPC.NewNPC(new Terraria.DataStructures.EntitySource_Misc("BagDistributionAudit"),Main.maxTilesX * 8,400,type);
                    if (slot >= Main.maxNPCs) throw new InvalidOperationException("Missing boss distribution fixture slot");
                    NPC npc = Main.npc[slot]; npc.playerInteraction[0] = npc.playerInteraction[1] = true;
                    foreach (var output in outputs) output.Sent.Clear();
                    Main.ItemDropSolver.TryDropping(new Terraria.GameContent.ItemDropRules.DropAttemptInfo { npc = npc, player = Main.player[0], rng = Main.rand, IsExpertMode = gameMode > 0, IsMasterMode = gameMode == 2 });
                    foreach (var output in outputs) {
                        var bagPackets = output.Sent.Where(p => p.Length >= 5 && p[2] == MessageID.InstancedItem && Main.item[BitConverter.ToInt16(p,3)].type == bag).ToArray();
                        check(bagPackets.Length == (gameMode == 0 ? 0 : 1),"native registered boss loot gives each participating peer one instanced bag only in expert/master: " + name + "/" + gameMode);
                    }
                    check(!Main.item.Any(i => i.active && !items.Contains(i) && i.type == bag),"native boss bags are not duplicated as shared active world loot: " + name + "/" + gameMode);
                    string materialName = name switch {
                        "SpiritVeinWyrm" => "SpiritGel", "GardenWarden" or "GreenwoodMedicineKingEcho" => "GreenwoodRoot",
                        "BlackFurnaceIronGolem" => "FurnaceSlagIron", "TribulationCloudAvatar" or "ThunderMarshJiao" => "TribulationCloudDew",
                        "AbyssalStarWomb" => "StarEclipseCrystal", "FormlessSwordSoul" => "SectTrialToken", "MoonboneImmortal" => "Moonbone",
                        "HeavenTabletGuardian" or "BrokenHeavenInspector" => "HeavenDaoFragment", _ => "DaoSeveringDust"
                    };
                    int material = mod.Find<ModItem>(materialName).Type;
                    bool hasDirectMaterials = Main.item.Any(i => i.active && !items.Contains(i) && i.type == material);
                    check(hasDirectMaterials == (gameMode == 0),"native normal core material drops directly and expert/master does not duplicate bag materials: " + name + "/" + gameMode);
                    foreach (Item item in Main.item.Where(i => i.active && !items.Contains(i))) item.active = false;
                    npc.active = false;
                }
            }
        } finally {
            foreach (Item item in Main.item.Where(i => i.active && !items.Contains(i))) item.active = false;
            foreach (NPC npc in Main.npc.Where(n => n.active && !npcs.Contains(n))) npc.active = false;
            for (int i = 0; i < 2; i++) { Main.player[i] = players[i]; NetMessage.buffer[i].broadcast = broadcast[i]; }
            clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local; Main.GameMode = difficulty; Main.rand = random;
        }
        check(Main.player.Take(2).SequenceEqual(players) && Main.item.Where(i => i.active).ToHashSet().SetEquals(items) && Main.npc.Where(n => n.active).ToHashSet().SetEquals(npcs),"native bag distribution restores player item NPC and networking context");
    }

}
