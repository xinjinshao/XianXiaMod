using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public class PillQualitySystem : GlobalItem
{
    private static readonly HashSet<int> PillTypes = new();

    public override void Load()
    {
        PillTypes.Clear();
    }

    public override void SetDefaults(Item item)
    {
        if (item.ModItem?.Mod == Mod)
        {
            string name = item.ModItem.Name;
            if (name.Contains("Pill") || name.Contains("Talisman") || name == "OldHeavenDaoScroll"
                || name == "StarEclipseCrystal" || name == "Moonbone" || name == "HeavenDaoFragment"
                || name == "DaoSeveringDust")
            {
                PillTypes.Add(item.type);
            }
        }
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (!PillTypes.Contains(item.type))
            return null;

        float roll = Main.rand.NextFloat();
        int quality = roll < 0.05f ? 4 : roll < 0.20f ? 3 : roll < 0.50f ? 2 : 1;

        if (quality >= 3 && Main.myPlayer == player.whoAmI)
        {
            string qualityName = quality switch
            {
                4 => Language.GetTextValue("Mods.XianXia.Progression.PillQualitySpirit"),
                3 => Language.GetTextValue("Mods.XianXia.Progression.PillQualityFine"),
                _ => ""
            };
            if (!string.IsNullOrEmpty(qualityName))
            {
                float bonus = 1f + quality * 0.15f;
                player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().RestoreSpiritualEnergy((int)(10 * bonus));
                player.AddBuff(BuffID.Regeneration, 60 * 5 * quality);
                Main.NewText(qualityName, (byte)(quality == 4 ? 255 : 180), (byte)(quality == 4 ? 215 : 220), (byte)120);
            }
        }

        return null;
    }
}
