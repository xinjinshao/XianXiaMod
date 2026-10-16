using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public class StationRecipeGroups : ModSystem
{
    public const string EarlyAnvil = "XianXia:EarlyAnvil";
    public const string HardmodeAnvil = "XianXia:HardmodeAnvil";
    public const string HardmodeForge = "XianXia:HardmodeForge";

    public override void AddRecipeGroups()
    {
        RecipeGroup.RegisterGroup(EarlyAnvil, new RecipeGroup(() => Language.GetTextValue("Mods.XianXia.RecipeGroups.EarlyAnvil"), ItemID.IronAnvil, ItemID.LeadAnvil));
        RecipeGroup.RegisterGroup(HardmodeAnvil, new RecipeGroup(() => Language.GetTextValue("Mods.XianXia.RecipeGroups.HardmodeAnvil"), ItemID.MythrilAnvil, ItemID.OrichalcumAnvil));
        RecipeGroup.RegisterGroup(HardmodeForge, new RecipeGroup(() => Language.GetTextValue("Mods.XianXia.RecipeGroups.HardmodeForge"), ItemID.AdamantiteForge, ItemID.TitaniumForge));
    }
}
