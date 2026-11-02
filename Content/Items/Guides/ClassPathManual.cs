using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Content.Items.Guides;

public class ClassPathManual : ModItem
{
    private int selectedClass;
    public override string Texture => "XianXia/Content/Items/Guides/SectLedger";
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = Item.height = 32; Item.maxStack = 1; Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(silver: 2); Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 20; Item.UseSound = SoundID.Item4;
    }
    public override bool AltFunctionUse(Player player) => true;
    public override bool? UseItem(Player player)
    {
        if (Main.dedServ || Main.myPlayer != player.whoAmI || !player.active || player.dead) return true;
        if (player.altFunctionUse == 2) selectedClass = (selectedClass + 1) % 4;
        foreach (string line in Guidance(player,CurrentStage(),selectedClass)) Main.NewText(line,120,245,220);
        return true;
    }
    public override void AddRecipes() => CreateRecipe().AddRecipeGroup(RecipeGroupID.Wood,10)
        .AddIngredient(ItemID.FallenStar).AddTile(TileID.WorkBenches).Register();
    internal static int CurrentStage() => !Main.hardMode ? DownedBossSystem.DownedSpiritVeinWyrm ? 1 : 0
        : !NPC.downedPlantBoss ? 2 : !NPC.downedGolemBoss ? 3 : !NPC.downedMoonlord ? 4 : 5;
    private static string Text(string key,params object[] args) => Language.GetTextValue("Mods.XianXia.ClassPaths." + key,args);
    private static string StationName(int tile)
    {
        Item station = ContentSamples.ItemsByType.Values.Where(item => item.createTile == tile).OrderBy(item => item.type).FirstOrDefault();
        return station == null ? Text("SpecialStation") : Lang.GetItemNameValue(station.type);
    }
    internal static int Candidate(int stage,int path) => (stage,path) switch {
        (0,0) => ModContent.ItemType<WoodgrainFlyingSword>(), (0,1) => ModContent.ItemType<SpiritwoodCrossbow>(),
        (0,2) => ModContent.ItemType<CinnabarTalismanFlameItem>(), (0,3) => ModContent.ItemType<SmallArtifactPendant>(),
        (1,0) => ModContent.ItemType<BlackFurnaceWarhammer>(), (1,1) => ModContent.ItemType<TalismanCrossbow>(),
        (1,2) => ModContent.ItemType<GreenwoodArrayPlate>(), (1,3) => ModContent.ItemType<FurnaceAshSpiritContract>(),
        (2,0) => ModContent.ItemType<ThunderPatternSwordCase>(), (2,1) => ModContent.ItemType<StarEclipseArbalest>(),
        (2,2) => ModContent.ItemType<ThunderTalismanArrayPlate>(), (2,3) => ModContent.ItemType<StarAbyssLarvaContract>(),
        (3,0) => ModContent.ItemType<FormlessSwordWheel>(), (3,1) => ModContent.ItemType<SectMechanismCrossbow>(),
        (3,2) => ModContent.ItemType<GreenwoodMedicineCauldron>(), (3,3) => ModContent.ItemType<NascentSoulCloneTalisman>(),
        (4,0) => ModContent.ItemType<HeavenTabletWardSeal>(), (4,1) => ModContent.ItemType<HeavenLawArbalest>(),
        (4,2) => ModContent.ItemType<BrokenHeavenDecree>(), (4,3) => ModContent.ItemType<CelestialPuppetToken>(),
        (5,0) => ModContent.ItemType<MoonboneDharmaSword>(), (5,1) => ModContent.ItemType<StarCalamityMechanismCase>(),
        (5,2) => ModContent.ItemType<ArchiveStarCodex>(), (5,3) => ModContent.ItemType<ArchivedImmortalSoulContract>(),
        _ => 0
    };
    internal static IEnumerable<string> Guidance(Player player,int stage,int path)
    {
        int type = Candidate(stage,path); if (type == 0) yield break;
        var item = new Item(type);
        yield return Text("Header",Text("Stage" + stage),Text("Path" + path),Lang.GetItemNameValue(type));
        foreach (Recipe recipe in Main.recipe.Take(Recipe.numRecipes).Where(r => !r.Disabled && r.createItem.type == type)) {
            var ingredients = recipe.requiredItem.Where(i => !i.IsAir).Select(i => {
                int group = recipe.acceptedGroups.FirstOrDefault(id => RecipeGroup.recipeGroups[id].ValidItems.Contains(i.type),-1);
                return "[i:" + i.type + "] " + (group < 0 ? Lang.GetItemNameValue(i.type) : RecipeGroup.recipeGroups[group].GetText()) + " ×" + i.stack;
            });
            string stations = string.Join(", ",recipe.requiredTile.Select(StationName));
            yield return Text("Recipe",string.Join(", ",ingredients),stations.Length == 0 ? Text("ByHand") : stations);
            if (recipe.Conditions.Count > 0) yield return Text("Conditions",string.Join(", ",recipe.Conditions.Select(c => c.Description.Value)));
        }
        if (item.ModItem is CultivationWeaponItem weapon)
            yield return Text("Spirit",player.GetModPlayer<XianXiaPlayer>().GetSpiritualEnergyCost(weapon.GetSpiritCost(player)));
        else if (item.useAmmo > 0) yield return Text("Arrows");
        else if (item.DamageType == DamageClass.Summon && ContentSamples.ProjectilesByType.TryGetValue(item.shoot,out var projectile))
            yield return Text("Summon",item.mana,projectile.minionSlots);
        yield return Text("Caution");
    }
}
