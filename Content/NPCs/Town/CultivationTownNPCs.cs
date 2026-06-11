using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Items.BossSummons;
using XianXia.Content.Items.BossSummons.Generated;
using XianXia.Content.Items.Consumables;
using XianXia.Content.Items.Generated;
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
}

[AutoloadHead]
public class HerbSectApprentice : CultivationTownNPC
{
    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerHasItem<GreenwoodRoot>() || AnyPlayerAtStage(CultivationStage.QiAwakening);
    }

    public override List<string> SetNPCNameList() => new() { "青萝", "木苓", "药篱" };

    public override string GetChat() => "草木有灵，丹火要慢。若你急着突破，先把灵压压住。";

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<AlchemyCauldron>();
        shop.Add<SpringReturnPill>();
        shop.Add<QiCondensingPill>();
        shop.Add<GreenwoodRoot>();
        shop.Add<SpiritwoodCharm>();
        shop.Register();
    }
}

[AutoloadHead]
public class WanderingArtificer : CultivationTownNPC
{
    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerHasItem<FurnaceSlagIron>() || DownedBossSystem.DownedSpiritVeinWyrm;
    }

    public override List<string> SetNPCNameList() => new() { "炉叟", "铁照", "游匠" };

    public override string GetChat() => "好材料不是拿来供着的。剑、匣、阵盘，都要先敢用坏。";

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
}

[AutoloadHead]
public class TribulationObserver : CultivationTownNPC
{
    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerAtStage(CultivationStage.Foundation) || AnyPlayerHasItem<TribulationCloudDew>();
    }

    public override List<string> SetNPCNameList() => new() { "观劫子", "听雷", "云衡" };

    public override string GetChat() => "天雷不是罚，是账。你欠得越明白，挨得越稳。";

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<TribulationResistingPill>();
        shop.Add<LightningWardJade>();
        shop.Add<ThunderTalismanArrayPlate>();
        shop.Add<SummonThunderCallingJade>();
        shop.Register();
    }
}

[AutoloadHead]
public class ArchiveScrollSpirit : CultivationTownNPC
{
    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerAtStage(CultivationStage.GoldenCore) || AnyPlayerHasItem<SectTrialToken>();
    }

    public override List<string> SetNPCNameList() => new() { "卷灵", "残页", "墨守" };

    public override string GetChat() => "宗门毁了，规矩还在。你若想借旧法，就得先付新代价。";

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<SectTrialToken>();
        shop.Add<OldHeavenDaoScroll>();
        shop.Add<FormlessSwordWheel>();
        shop.Add<NascentSoulJadeBox>();
        shop.Register();
    }
}

[AutoloadHead]
public class FallenHeavenMessenger : CultivationTownNPC
{
    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return AnyPlayerAtStage(CultivationStage.NascentSoul) || DownedBossSystem.DownedBosses.Contains("heaven_tablet_guardian");
    }

    public override List<string> SetNPCNameList() => new() { "坠使", "玄告", "天残" };

    public override string GetChat() => "旧天道不会回答你，但它留下的碎片仍会索取答案。";

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<HeavenDaoFragment>();
        shop.Add<BrokenHeavenDecree>();
        shop.Add<BrokenHeavenCrownSeal>();
        shop.Add<DaoSeveringRing>();
        shop.Register();
    }
}
