using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Items.BossSummons;
using XianXia.Content.Items.Consumables;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Items.Accessories;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Guides;
using XianXia.Content.Items.Stations;

namespace XianXia.Content.NPCs.Town;

public abstract class CultivationTownNPC : ModNPC
{
    protected NetworkText commissionResponse = NetworkText.FromKey("Mods.XianXia.NPCs.Commission.Unavailable");

    protected string CommissionText(string key, params object[] args)
    {
        commissionResponse = NetworkText.FromKey(key, args);
        return commissionResponse.ToString();
    }

    public NetworkText ClaimCommissionOnServer(Player player)
    {
        TryClaimCommission(player, out _);
        return commissionResponse;
    }

    protected static string ServiceChat(string npc, string dialogue, params object[] args) =>
        Language.GetTextValue($"Mods.XianXia.TownGuidance.Dialogue.{npc}.{dialogue}", args)
        + "\n\n" + Language.GetTextValue($"Mods.XianXia.TownGuidance.Services.{npc}");

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.TownFrameCount;
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
    }


    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 10);
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

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = Language.GetTextValue("LegacyInterface.28");
        button2 = Language.GetTextValue("Mods.XianXia.NPCs.Commission.Button");
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
        {
            shopName = "Shop";
            return;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)1);
            packet.Write((short)NPC.whoAmI);
            packet.Send();
            return;
        }

        TryClaimCommission(Main.LocalPlayer, out string text);
        Main.npcChatText = text;
    }

    protected virtual bool TryClaimCommission(Player player, out string text)
    {
        text = CommissionText("Mods.XianXia.NPCs.Commission.Unavailable");
        return false;
    }

    protected bool ClaimCommission(Player player, string key, int reputation, string textKey, params (int Type, int Stack)[] rewards)
    {
        if (!DownedBossSystem.TryClaimCommission(key, reputation))
        {
            return false;
        }

        foreach ((int type, int stack) in rewards)
        {
            player.QuickSpawnItem(NPC.GetSource_FromThis(), type, stack);
        }

        CommissionText(textKey, reputation, DownedBossSystem.SectReputation);
        return true;
    }
}

[AutoloadHead]
public class HerbSectApprentice : CultivationTownNPC
{
    public override void SetDefaults() { base.SetDefaults(); NPC.defense = 15; }

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
            return ServiceChat(nameof(HerbSectApprentice), "Pressure");
        }

        if (cultivation.cultivationStage < CultivationStage.QiCondensation)
        {
            return ServiceChat(nameof(HerbSectApprentice), "Early");
        }

        return ServiceChat(nameof(HerbSectApprentice), "Ready");
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
        shop.Add<QiGatheringPendant>();
        shop.Add<LowGradeSpiritStone>();
        shop.Add<SpiritHerbSeeds>();
        shop.Add<BlankSectScroll>();
        shop.Add<LightningAvoidanceRune>();
        shop.Add<SectLedger>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!LocalAtStage(CultivationStage.QiCondensation))
        {
            HideShopItem<FoundationPill>(items);
        }
    }

    protected override bool TryClaimCommission(Player player, out string text)
    {
        if (!Downed("garden_warden"))
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.HerbSectApprentice.Locked");
            return false;
        }

        bool claimed = ClaimCommission(
            player,
            "herb_sect_apprentice_garden",
            8,
            "Mods.XianXia.NPCs.Commission.HerbSectApprentice.Claimed",
            (ModContent.ItemType<GreenwoodRoot>(), 10),
            (ModContent.ItemType<SpringReturnPill>(), 3));
        text = commissionResponse.ToString();
        if (!claimed)
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.AlreadyClaimed");
        }
        return claimed;
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
            return ServiceChat(nameof(WanderingArtificer), "Early");
        }

        if (!LocalAtStage(CultivationStage.Foundation))
        {
            return ServiceChat(nameof(WanderingArtificer), "Foundation");
        }

        return ServiceChat(nameof(WanderingArtificer), "Ready");
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
        shop.Add<ArtifactBlankShard>();
        shop.Add<FurnaceSlagIron>();
        shop.Add<LowGradeSpiritStone>();
        shop.Add<GreenwoodInscriptionNeedle>();
        shop.Add<FurnaceInscriptionNeedle>();
        shop.Add<InscriptionRemovalStone>();
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
        if (!Downed("thunder_marsh_jiao"))
        {
            HideShopItem<ThunderInscriptionNeedle>(items);
        }
        if (!Downed("heaven_tablet_guardian"))
        {
            HideShopItem<BrokenHeavenInscriptionNeedle>(items);
        }
    }

    protected override bool TryClaimCommission(Player player, out string text)
    {
        if (!Downed("black_furnace_iron_golem"))
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.WanderingArtificer.Locked");
            return false;
        }

        bool claimed = ClaimCommission(
            player,
            "wandering_artificer_furnace",
            8,
            "Mods.XianXia.NPCs.Commission.WanderingArtificer.Claimed",
            (ModContent.ItemType<FurnaceSlagIron>(), 10),
            (ModContent.ItemType<ArtifactBlankShard>(), 3));
        text = commissionResponse.ToString();
        if (!claimed)
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.AlreadyClaimed");
        }
        return claimed;
    }
}

[AutoloadHead]
public class TribulationObserver : CultivationTownNPC
{
    public override void SetDefaults() { base.SetDefaults(); NPC.defense = 16; }

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
            return ServiceChat(nameof(TribulationObserver), "Active");
        }

        if (!LocalAtStage(CultivationStage.Foundation))
        {
            return ServiceChat(nameof(TribulationObserver), "Early");
        }

        return ServiceChat(nameof(TribulationObserver), "Ready");
    }

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<TribulationGauge>();
        shop.Add<TribulationResistingPill>();
        shop.Add<LightningWardJade>();
        shop.Add<ThunderTalismanArrayPlate>();
        shop.Add<SummonThunderCallingJade>();
        shop.Add<TribulationCloudDew>();
        shop.Add<LowGradeSpiritStone>();
        shop.Add<LightningAvoidanceRune>();
        shop.Add<TribulationTrainingToken>();
        shop.Add<ThunderInscriptionNeedle>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!LocalAtStage(CultivationStage.Foundation))
        {
            HideShopItem<ThunderTalismanArrayPlate>(items);
            HideShopItem<SummonThunderCallingJade>(items);
        }
        if (!Downed("tribulation_cloud_avatar"))
        {
            HideShopItem<TribulationResistingPill>(items);
        }
    }

    protected override bool TryClaimCommission(Player player, out string text)
    {
        if (!Downed("thunder_marsh_jiao"))
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.TribulationObserver.Locked");
            return false;
        }

        bool claimed = ClaimCommission(
            player,
            "tribulation_observer_thunder",
            12,
            "Mods.XianXia.NPCs.Commission.TribulationObserver.Claimed",
            (ModContent.ItemType<TribulationCloudDew>(), 8),
            (ModContent.ItemType<TribulationResistingPill>(), 3));
        text = commissionResponse.ToString();
        if (!claimed)
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.AlreadyClaimed");
        }
        return claimed;
    }
}

[AutoloadHead]
public class ArchiveScrollSpirit : CultivationTownNPC
{
    public override void SetDefaults() { base.SetDefaults(); NPC.defense = 12; NPC.knockBackResist = 0.75f; NPC.lifeMax = 200; }

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
            return ServiceChat(nameof(ArchiveScrollSpirit), "Early");
        }

        if (!Downed("formless_sword_soul"))
        {
            return ServiceChat(nameof(ArchiveScrollSpirit), "SwordSoul");
        }

        if (!DownedBossSystem.HasSectReputation(80))
        {
            return ServiceChat(nameof(ArchiveScrollSpirit), "Reputation", DownedBossSystem.SectReputation);
        }

        return ServiceChat(nameof(ArchiveScrollSpirit), "Ready");
    }

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<SectLedger>();
        shop.Add<SectTrialToken>();
        shop.Add<OldHeavenDaoScroll>();
        shop.Add<FormlessSwordWheel>();
        shop.Add<NascentSoulJadeBox>();
        shop.Add<ArtifactBlankShard>();
        shop.Add<LowGradeSpiritStone>();
        shop.Add<BlankSectScroll>();
        shop.Add<StarAbyssInscriptionNeedle>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!LocalAtStage(CultivationStage.GoldenCore))
        {
            HideShopItem<FormlessSwordWheel>(items);
            HideShopItem<NascentSoulJadeBox>(items);
        }
        if (!Downed("abyssal_star_womb"))
        {
            HideShopItem<StarAbyssInscriptionNeedle>(items);
        }
        if (!DownedBossSystem.HasSectReputation(80))
        {
            HideShopItem<NascentSoulJadeBox>(items);
        }
    }

    protected override bool TryClaimCommission(Player player, out string text)
    {
        if (!Downed("formless_sword_soul"))
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.ArchiveScrollSpirit.Locked");
            return false;
        }

        bool claimed = ClaimCommission(
            player,
            "archive_scroll_spirit_trial",
            16,
            "Mods.XianXia.NPCs.Commission.ArchiveScrollSpirit.Claimed",
            (ModContent.ItemType<SectTrialToken>(), 2),
            (ModContent.ItemType<OldHeavenDaoScroll>(), 1));
        text = commissionResponse.ToString();
        if (!claimed)
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.AlreadyClaimed");
        }
        return claimed;
    }
}

[AutoloadHead]
public class FallenHeavenMessenger : CultivationTownNPC
{
    public override void SetDefaults() { base.SetDefaults(); NPC.defense = 24; NPC.lifeMax = 300; NPC.knockBackResist = 0.6f; }

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
            return ServiceChat(nameof(FallenHeavenMessenger), "Early");
        }

        if (!Downed("heaven_tablet_guardian"))
        {
            return ServiceChat(nameof(FallenHeavenMessenger), "Guardian");
        }

        if (!DownedBossSystem.HasSectReputation(160))
        {
            return ServiceChat(nameof(FallenHeavenMessenger), "Reputation", DownedBossSystem.SectReputation);
        }

        return ServiceChat(nameof(FallenHeavenMessenger), "Ready");
    }

    public override void AddShops()
    {
        NPCShop shop = new(Type);
        shop.Add<HeavenDaoFragment>();
        shop.Add<BrokenHeavenDecree>();
        shop.Add<BrokenHeavenCrownSeal>();
        shop.Add<DaoSeveringRing>();
        shop.Add<LowGradeSpiritStone>();
        shop.Add<BrokenHeavenInscriptionNeedle>();
        shop.Add<HeavenDaoRouteHint>();
        shop.Add<EndgameRouteFrame>();
        shop.Register();
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        if (!Downed("heaven_tablet_guardian"))
        {
            HideShopItem<BrokenHeavenDecree>(items);
            HideShopItem<BrokenHeavenCrownSeal>(items);
        }
        if (!LocalAtStage(CultivationStage.SpiritSevering))
        {
            HideShopItem<BrokenHeavenInscriptionNeedle>(items);
        }
        if (!LocalAtStage(CultivationStage.Tribulation))
        {
            HideShopItem<DaoSeveringRing>(items);
        }
        if (!DownedBossSystem.HasSectReputation(160))
        {
            HideShopItem<DaoSeveringRing>(items);
        }
        if (!Downed("moonbone_immortal"))
        {
            HideShopItem<EndgameRouteFrame>(items);
        }
    }

    protected override bool TryClaimCommission(Player player, out string text)
    {
        if (!Downed("heaven_tablet_guardian"))
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.FallenHeavenMessenger.Locked");
            return false;
        }

        bool claimed = ClaimCommission(
            player,
            "fallen_heaven_messenger_tablet",
            24,
            "Mods.XianXia.NPCs.Commission.FallenHeavenMessenger.Claimed",
            (ModContent.ItemType<HeavenDaoFragment>(), 6),
            (ModContent.ItemType<BrokenHeavenDecree>(), 1));
        text = commissionResponse.ToString();
        if (!claimed)
        {
            text = CommissionText("Mods.XianXia.NPCs.Commission.AlreadyClaimed");
        }
        return claimed;
    }
}
