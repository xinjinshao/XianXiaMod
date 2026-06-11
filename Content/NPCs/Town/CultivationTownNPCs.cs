using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Items.BossSummons;
using XianXia.Content.Items.BossSummons.Generated;
using XianXia.Content.Items.Consumables;
using XianXia.Content.Items.Generated;
using XianXia.Content.Items.Guides;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.Stations;
using XianXia.Content.Items.Weapons;

namespace XianXia.Content.NPCs.Town;

public abstract class CultivationTownNPC : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Guide];
        NPCID.Sets.ExtraFramesCount[Type] = NPCID.Sets.ExtraFramesCount[NPCID.Guide];
        NPCID.Sets.AttackFrameCount[Type] = NPCID.Sets.AttackFrameCount[NPCID.Guide];
        NPCID.Sets.DangerDetectRange[Type] = 650;
        NPCID.Sets.AttackType[Type] = 0;
        NPCID.Sets.AttackTime[Type] = 28;
        NPCID.Sets.AttackAverageChance[Type] = 20;
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 12;
        NPC.defense = 18;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        AIType = NPCID.Guide;
        AnimationType = NPCID.Guide;
    }

    protected static bool AnyPlayerAtStage(CultivationStage stage)
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.GetModPlayer<XianXiaPlayer>().cultivationStage >= stage)
            {
                return true;
            }
        }

        return false;
    }

    protected static bool AnyPlayerHasItem<T>() where T : ModItem
    {
        int itemType = ModContent.ItemType<T>();
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.HasItem(itemType))
            {
                return true;
            }
        }

        return false;
    }

    protected static XianXiaPlayer LocalCultivation => Main.LocalPlayer.GetModPlayer<XianXiaPlayer>();

    protected static bool LocalAtStage(CultivationStage stage)
    {
        return LocalCultivation.cultivationStage >= stage;
    }

    protected static bool Downed(string bossId)
    {
        return DownedBossSystem.DownedBosses.Contains(bossId);
    }

    protected static void HideShopItem<T>(Item[] items) where T : ModItem
    {
        int type = ModContent.ItemType<T>();
        foreach (Item item in items)
        {
            if (item is not null && item.type == type)
            {
                item.TurnToAir();
            }
        }
    }

    protected static void AddTownBestiaryFlavor(BestiaryEntry bestiaryEntry, string key)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement($"Mods.XianXia.Bestiary.{key}"));
    }
}

[AutoloadHead]
public class HerbSectApprentice : CultivationTownNPC
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        NPC.Happiness.SetBiomeAffection<ForestBiome>(AffectionLevel.Like);
        NPC.Happiness.SetBiomeAffection<JungleBiome>(AffectionLevel.Love);
        NPC.Happiness.SetBiomeAffection<DesertBiome>(AffectionLevel.Dislike);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        AddTownBestiaryFlavor(bestiaryEntry, nameof(HerbSectApprentice));
    }

    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerHasItem<GreenwoodRoot>() || AnyPlayerAtStage(CultivationStage.QiAwakening);
    }

    public override List<string> SetNPCNameList() => new() { "青萝", "木苓", "药篱" };

    public override string GetChat()
    {
        XianXiaPlayer cultivation = LocalCultivation;
        if (cultivation.spiritPressure >= 70)
        {
            return "你的灵压浮在皮肉上，先用回春丹和抗劫丹稳住，别急着再破境。";
        }

        if (cultivation.cultivationStage < CultivationStage.QiCondensation)
        {
            return "青木根能养丹，灵石能引气。先凝住第一口真气，再谈筑基。";
        }

        return "草木有灵，丹火要慢。筑基之前，丹药只是助缘，不是替你走路。";
    }

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<AlchemyCauldron>();
        shop.Add<SpringReturnPill>();
        shop.Add<QiCondensingPill>();
        shop.Add<FoundationPill>();
        shop.Add<GreenwoodRoot>();
        shop.Add<SpiritwoodCharm>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!LocalAtStage(CultivationStage.QiCondensation))
        {
            HideShopItem<FoundationPill>(items);
        }
    }
}

[AutoloadHead]
public class WanderingArtificer : CultivationTownNPC
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        NPC.Happiness.SetBiomeAffection<UndergroundBiome>(AffectionLevel.Love);
        NPC.Happiness.SetBiomeAffection<ForestBiome>(AffectionLevel.Like);
        NPC.Happiness.SetBiomeAffection<OceanBiome>(AffectionLevel.Dislike);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        AddTownBestiaryFlavor(bestiaryEntry, nameof(WanderingArtificer));
    }

    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerHasItem<FurnaceSlagIron>() || DownedBossSystem.DownedSpiritVeinWyrm;
    }

    public override List<string> SetNPCNameList() => new() { "炉叟", "铁照", "游匠" };

    public override string GetChat()
    {
        if (!DownedBossSystem.DownedSpiritVeinWyrm)
        {
            return "木剑和短弩够你探灵脉。等灵脉蠕虫伏下，我再教你铸真正的法器。";
        }

        if (!LocalAtStage(CultivationStage.Foundation))
        {
            return "器胚炉已经热了，但你的气还散。筑基后再碰破云剑，别让剑带着你走。";
        }

        return "好材料不是拿来供着的。剑、匣、阵盘，都要先敢用坏。";
    }

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<ArtifactForge>();
        shop.Add<WoodgrainFlyingSword>();
        shop.Add<SpiritwoodCrossbow>();
        shop.Add<CloudpiercerFlyingSword>();
        shop.Add<GreenwoodArrayPlate>();
        shop.Add<FurnaceHeartRing>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!DownedBossSystem.DownedSpiritVeinWyrm)
        {
            HideShopItem<CloudpiercerFlyingSword>(items);
            HideShopItem<GreenwoodArrayPlate>(items);
        }

        if (!LocalAtStage(CultivationStage.Foundation))
        {
            HideShopItem<FurnaceHeartRing>(items);
        }
    }
}

[AutoloadHead]
public class TribulationObserver : CultivationTownNPC
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        NPC.Happiness.SetBiomeAffection<HallowBiome>(AffectionLevel.Like);
        NPC.Happiness.SetBiomeAffection<UndergroundBiome>(AffectionLevel.Dislike);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        AddTownBestiaryFlavor(bestiaryEntry, nameof(TribulationObserver));
    }

    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerAtStage(CultivationStage.Foundation) || AnyPlayerHasItem<TribulationCloudDew>();
    }

    public override List<string> SetNPCNameList() => new() { "观劫子", "听雷", "云衡" };

    public override string GetChat()
    {
        if (LocalCultivation.tribulationTimer > 0)
        {
            return "别躲进屋里数雷。看清落点，留一口灵气，雷过之后才算你自己的境界。";
        }

        if (!LocalAtStage(CultivationStage.Foundation))
        {
            return "天雷不是罚，是账。筑基之后，这账才会真正写上你的名字。";
        }

        return "天雷不是罚，是账。你欠得越明白，挨得越稳。";
    }

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<TribulationResistingPill>();
        shop.Add<LightningWardJade>();
        shop.Add<ThunderTalismanArrayPlate>();
        shop.Add<SummonThunderCallingJade>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!LocalAtStage(CultivationStage.Foundation))
        {
            HideShopItem<ThunderTalismanArrayPlate>(items);
            HideShopItem<SummonThunderCallingJade>(items);
        }
    }
}

[AutoloadHead]
public class ArchiveScrollSpirit : CultivationTownNPC
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        NPC.Happiness.SetBiomeAffection<ForestBiome>(AffectionLevel.Like);
        NPC.Happiness.SetBiomeAffection<UndergroundBiome>(AffectionLevel.Dislike);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        AddTownBestiaryFlavor(bestiaryEntry, nameof(ArchiveScrollSpirit));
    }

    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerAtStage(CultivationStage.GoldenCore) || AnyPlayerHasItem<SectTrialToken>();
    }

    public override List<string> SetNPCNameList() => new() { "卷灵", "残页", "墨守" };

    public override string GetChat()
    {
        if (!LocalAtStage(CultivationStage.GoldenCore))
        {
            return "宗门试炼令不是门票，是债券。等你结成金丹，再来翻旧卷。";
        }

        if (!Downed("formless_sword_soul"))
        {
            return "无相剑魂还守着残碑。你若听见剑鸣，不要先拔剑，先听完。";
        }

        if (!DownedBossSystem.HasSectReputation(80))
        {
            return $"你的宗门声望已有 {DownedBossSystem.SectReputation}。旧卷认可战绩，也认可耐心。";
        }

        return "宗门毁了，规矩还在。你若想借旧法，就得先付新代价。";
    }

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<SectLedger>();
        shop.Add<SectTrialToken>();
        shop.Add<OldHeavenDaoScroll>();
        shop.Add<FormlessSwordWheel>();
        shop.Add<NascentSoulJadeBox>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!LocalAtStage(CultivationStage.GoldenCore))
        {
            HideShopItem<FormlessSwordWheel>(items);
            HideShopItem<NascentSoulJadeBox>(items);
        }

        if (!DownedBossSystem.HasSectReputation(80))
        {
            HideShopItem<NascentSoulJadeBox>(items);
        }
    }
}

[AutoloadHead]
public class FallenHeavenMessenger : CultivationTownNPC
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        NPC.Happiness.SetBiomeAffection<HallowBiome>(AffectionLevel.Love);
        NPC.Happiness.SetBiomeAffection<ForestBiome>(AffectionLevel.Dislike);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        AddTownBestiaryFlavor(bestiaryEntry, nameof(FallenHeavenMessenger));
    }

    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerAtStage(CultivationStage.NascentSoul) || DownedBossSystem.DownedBosses.Contains("heaven_tablet_guardian");
    }

    public override List<string> SetNPCNameList() => new() { "坠使", "玄告", "天残" };

    public override string GetChat()
    {
        if (!LocalAtStage(CultivationStage.NascentSoul))
        {
            return "你还听不见天碑背面的噪音。等元婴成形，再来问旧天道。";
        }

        if (!Downed("heaven_tablet_guardian"))
        {
            return "天碑守卫仍在。它不恨你，只是不承认你。";
        }

        if (!DownedBossSystem.HasSectReputation(160))
        {
            return $"天道碎片认得战绩。你的宗门声望是 {DownedBossSystem.SectReputation}，还不足以换取斩道之物。";
        }

        return "旧天道不会回答你，但它留下的碎片仍会索取答案。";
    }

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<HeavenDaoFragment>();
        shop.Add<BrokenHeavenDecree>();
        shop.Add<BrokenHeavenCrownSeal>();
        shop.Add<DaoSeveringRing>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!Downed("heaven_tablet_guardian"))
        {
            HideShopItem<BrokenHeavenDecree>(items);
            HideShopItem<BrokenHeavenCrownSeal>(items);
        }

        if (!LocalAtStage(CultivationStage.Tribulation))
        {
            HideShopItem<DaoSeveringRing>(items);
        }

        if (!DownedBossSystem.HasSectReputation(160))
        {
            HideShopItem<DaoSeveringRing>(items);
        }
    }
}
