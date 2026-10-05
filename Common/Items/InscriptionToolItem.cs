using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Common.Items;

public abstract class InscriptionToolItem : ModItem
{
    public abstract InscriptionKind TargetKind { get; }
    public virtual bool RefinesArtifact => false;
    public virtual bool AwakensArtifact => false;
    public virtual bool TransformsArtifact => false;
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    protected void Configure(int value, int rarity)
    {
        Item.width = Item.height = 32;
        Item.maxStack = 99;
        Item.value = value;
        Item.rare = rarity;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 20;
        Item.UseSound = SoundID.Item4;
        Item.consumable = false; // The confirmed transaction consumes the material.
    }
    public override bool CanUseItem(Player player) => InscriptionTransactions.NearForge(player)
        && (Main.netMode == NetmodeID.Server || Main.mouseItem.IsAir);
    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.Server && player.whoAmI == Main.myPlayer)
            ModContent.GetInstance<InscriptionUISystem>().Open(player.selectedItem);
        return true;
    }
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "InscriptionHelp", Terraria.Localization.Language.GetTextValue("Mods.XianXia.Inscriptions.ToolHelp")));
        tooltips.Add(new TooltipLine(Mod, "InscriptionCost", Terraria.Localization.Language.GetTextValue(
            TransformsArtifact ? "Mods.XianXia.DaoArtifacts.Cost" : AwakensArtifact ? "Mods.XianXia.Refinement.AwakeningCost" : RefinesArtifact ? "Mods.XianXia.Refinement.ToolCost" : TargetKind == InscriptionKind.None ? "Mods.XianXia.Inscriptions.RemovalCost" : "Mods.XianXia.Inscriptions.NeedleCost", InscriptionRules.SpiritStoneCost)));
    }
}
