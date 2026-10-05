using XianXia.Common.Players;

int assertions = 0;
void Check(bool ok, string message)
{
    assertions++;
    if (!ok) throw new Exception(message);
}
string Failure(int current, int target, int ticks = 0, bool cleared = true,
    bool hardmode = true, bool plantera = true, bool golem = true, bool moonLord = true,
    bool boss = true) => CultivationRules.GetFailure((CultivationStage)current, (CultivationStage)target,
        ticks, cleared, hardmode, plantera, golem, moonLord, _ => boss);

for (int current = -1; current <= 9; current++)
for (int target = -1; target <= 9; target++)
{
    bool valid = current >= 0 && current < 8 && target == current + 1;
    Check((Failure(current, target) == "") == valid, "Only valid sequential advancement is allowed.");
}
for (int current = 0; current < 8; current++)
{
    Check(Failure(current, current + 1, ticks: 1) == "TribulationUnfinished", "Active trial cannot be overwritten.");
    Check((Failure(current, current + 1, cleared: false) == "TribulationUnfinished") == (current >= 3),
        "Only realms with trials require the current trial to be cleared.");
}
Check(Failure(2, 3, hardmode: false, plantera: false, golem: false, moonLord: false) == "", "Foundation remains possible before Hardmode.");
Check(Failure(3, 4, hardmode: false) == "BreakthroughRequiresHardmode", "Golden Core requires Hardmode.");
Check(Failure(4, 5, plantera: false) == "BreakthroughRequiresPlantera", "Nascent Soul requires Plantera.");
Check(Failure(5, 6, golem: false) == "BreakthroughRequiresGolem", "Spirit Severing requires Golem.");
Check(Failure(6, 7, moonLord: false) == "BreakthroughRequiresMoonLord", "Tribulation requires Moon Lord.");
Check(Failure(3, 4, ticks: -1) == "InvalidBreakthroughItem", "Malformed timer must not authorize advancement.");
Check(!CultivationRules.CanRetry(CultivationStage.Foundation, -1, false), "Malformed timer must not authorize retry.");
for (int current = 0; current < 8; current++)
    Check((Failure(current, current + 1, boss: false) == "BreakthroughRequiresTrial") == (current >= 2),
        "Boss material alone cannot bypass the trial gate.");

string[] bosses = {"tribulation_cloud_avatar", "abyssal_star_womb", "formless_sword_soul",
    "heaven_tablet_guardian", "broken_heaven_inspector", "old_heaven_dao_core"};
for (int i = 0; i < bosses.Length; i++)
    Check(CultivationRules.GetRequiredBoss((CultivationStage)(i + 3)) == bosses[i], "Required trial must match the designed progression.");
Check(CultivationRules.GetBossWorldStage("old_heaven_dao_core") == CultivationStage.Tribulation,
    "Core is a Moon Lord-era trial, not a requirement that the player has already severed dao.");
for (int stage = -1; stage <= 9; stage++)
{
    bool eligible = stage >= 3 && stage <= 8;
    Check(CultivationRules.CanRetry((CultivationStage)stage, 0, false) == eligible, "Retry requires an unfinished valid realm.");
    Check(!CultivationRules.CanRetry((CultivationStage)stage, 1, false), "Active trial cannot restart.");
    Check(!CultivationRules.CanRetry((CultivationStage)stage, 0, true), "Cleared trial cannot farm permanent rewards.");
}
// Walk an entire legitimate progression, checking each unmet requirement before satisfying it.
var clearedBosses = new HashSet<string>();
for (int target = 1; target <= 8; target++)
{
    string prerequisite = CultivationRules.GetRequiredBoss((CultivationStage)target);
    if (prerequisite.Length > 0)
    {
        Check(CultivationRules.GetFailure((CultivationStage)(target - 1), (CultivationStage)target,
            0, true, true, true, true, true, clearedBosses.Contains) == "BreakthroughRequiresTrial",
            "Full progression must stop at each missing boss.");
        clearedBosses.Add(prerequisite);
    }
    Check(CultivationRules.GetFailure((CultivationStage)(target - 1), (CultivationStage)target,
        0, true, true, true, true, true, clearedBosses.Contains) == "", "Full progression must remain reachable.");
}
Console.WriteLine($"Progression regression passed: {assertions} assertions against actual CultivationRules source.");
var enemies = new[]{
 ("WanderingSpiritSlime",0),("ShatteredJadeWorm",0),("TalismanBat",0),("HerbGardenVineSpirit",0),("MiasmaFlowerMoth",0),("IronShardSpirit",0),("FurnaceAshGolem",0),
 ("ThunderPatternHawk",1),("TribulationCloudling",1),("StarAbyssLarva",1),("StarEclipsedCultivator",1),
 ("ObsessedSwordCultivator",2),("ScriptureArchiveEcho",2),("HeavenTabletGuard",3),("CelestialPuppet",3),("MoonboneCultivator",4),("ArchivedImmortalSoul",4)
};
foreach(var (enemy,tier) in enemies) for(int mask=0;mask<16;mask++) {
 bool hard=(mask&1)!=0,plant=(mask&2)!=0,golem=(mask&4)!=0,moon=(mask&8)!=0;
 bool expected=(tier<1||hard)&&(tier<2||plant)&&(tier<3||golem)&&(tier<4||moon);
 Check(XianXia.Common.Systems.EnemySpawnRules.Allows(enemy,hard,plant,golem,moon)==expected,"Enemy unlocks require all preceding world gates, including inconsistent save flags.");
}
Check(!XianXia.Common.Systems.EnemySpawnRules.Allows("UnknownEnemy",true,true,true,true),"Unconfigured enemies cannot silently bypass progression.");
Console.WriteLine($"Progression/enemy unlock regression passed: {assertions} assertions against actual policy sources.");
