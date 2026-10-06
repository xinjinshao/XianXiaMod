using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Systems;

public class PillQualitySystem : GlobalItem
{
    public override bool InstancePerEntity => true;
    public PillQuality Quality { get; private set; } = PillQuality.Standard;
    public bool Crafted { get; private set; }

    public static bool IsPill(Item item) => item.ModItem?.Mod is global::XianXia.XianXia
        && item.ModItem.Name is "QiCondensingPill" or "FoundationPill"
            or "QiRecoveryPill" or "SpringReturnPill" or "TribulationResistingPill";

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => IsPill(entity);
    public static PillQuality GetQuality(Item item) => IsPill(item)
        ? item.GetGlobalItem<PillQualitySystem>().Quality : PillQuality.Standard;
    public static float BenefitMultiplier(Item item) => PillQualityRules.Multiplier(GetQuality(item));
    public static int Scale(Item item, int amount) => PillQualityRules.Scale(amount, GetQuality(item));

    public override void OnCreated(Item item, ItemCreationContext context)
    {
        // tModLoader crafts on the owning client, like prefixes. The result's
        // item metadata is synced; consumption never rolls another quality.
        if (!IsPill(item) || context is not RecipeItemCreationContext || Crafted) return;
        Quality = PillQualityRules.Roll(Main.rand.NextFloat());
        Crafted = true;
    }

    public override bool CanStack(Item destination, Item source) => GetQuality(destination) == GetQuality(source);
    public override bool CanStackInWorld(Item destination, Item source) => CanStack(destination, source);
    public override void OnStack(Item destination, Item source, int numToTransfer)
    {
        Crafted |= source.GetGlobalItem<PillQualitySystem>().Crafted;
    }
    public override void SplitStack(Item destination, Item source, int numToTransfer)
    {
        var original = source.GetGlobalItem<PillQualitySystem>();
        Quality = original.Quality;
        Crafted = original.Crafted;
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        if (Quality == PillQuality.Standard && !Crafted) return;
        tag["qualityVersion"] = 1;
        tag["quality"] = (int)Quality;
        tag["crafted"] = Crafted;
    }
    public override void LoadData(Item item, TagCompound tag)
    {
        Quality = PillQualityRules.Normalize(tag.GetInt("quality"));
        Crafted = tag.GetBool("crafted");
    }
    public override void NetSend(Item item, BinaryWriter writer)
    {
        writer.Write((byte)Quality);
        writer.Write(Crafted);
    }
    public override void NetReceive(Item item, BinaryReader reader)
    {
        int quality = reader.ReadByte();
        bool crafted = reader.ReadBoolean();
        Quality = PillQualityRules.Normalize(quality);
        Crafted = crafted;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!IsPill(item)) return;
        string nameKey = Quality switch
        {
            PillQuality.Coarse => "Mods.XianXia.PillQuality.Coarse",
            PillQuality.Fine => "Mods.XianXia.PillQuality.Fine",
            PillQuality.Spirit => "Mods.XianXia.PillQuality.Spirit",
            _ => "Mods.XianXia.PillQuality.Standard"
        };
        Color color = Quality switch
        { PillQuality.Coarse => Color.Gray, PillQuality.Fine => Color.CornflowerBlue, PillQuality.Spirit => Color.Gold, _ => Color.LimeGreen };
        tooltips.Add(new TooltipLine(Mod, "PillQuality", Language.GetTextValue("Mods.XianXia.PillQuality.Tooltip",
            Language.GetTextValue(nameKey), (int)(BenefitMultiplier(item) * 100))) { OverrideColor = color });
        int bonus = PillQualityRules.BonusEnergy(Quality);
        if (bonus > 0)
            tooltips.Add(new TooltipLine(Mod, "PillQualityBonus", Language.GetTextValue("Mods.XianXia.PillQuality.Bonus",
                bonus, PillQualityRules.RegenerationTicks(Quality) / 60)));
        if (item.ModItem.Name is "QiCondensingPill" or "FoundationPill")
            tooltips.Add(new TooltipLine(Mod, "PillQualityBreakthrough", Language.GetTextValue("Mods.XianXia.PillQuality.Breakthrough")));
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (Main.netMode != NetmodeID.SinglePlayer && CultivationItemTransactions.IsProgressionItem(item)) return null;
        ApplyUseBonus(item, player);
        return null;
    }
    public static void ApplyUseBonus(Item item, Player player)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !IsPill(item)) return;
        PillQuality quality = GetQuality(item);
        int bonus = PillQualityRules.BonusEnergy(quality);
        if (bonus == 0) return;
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().RestoreSpiritualEnergy(bonus);
        player.AddBuff(BuffID.Regeneration, PillQualityRules.RegenerationTicks(quality));
        string key = quality == PillQuality.Spirit ? "Mods.XianXia.Progression.PillQualitySpirit" : "Mods.XianXia.Progression.PillQualityFine";
        if (Main.netMode == NetmodeID.Server)
        {
            var packet = item.ModItem.Mod.GetPacket();
            packet.Write((byte)7);
            NetworkText.FromKey(key).Serialize(packet);
            packet.Send(player.whoAmI);
        }
        else Main.NewText(Language.GetTextValue(key), 255, 220, 120);
    }
}
