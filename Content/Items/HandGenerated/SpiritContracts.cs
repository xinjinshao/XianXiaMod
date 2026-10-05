using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Buffs;
using XianXia.Content.Projectiles;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.HandGenerated;

public abstract class SpiritContractItem : ModItem
{
    protected virtual bool Ready => true;
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.StaffMinionSlotsRequired[Type] = 1;
    }
    protected void Configure(int damage, int mana, int projectile, int buff, int price, int rarity)
    {
        Item.width = Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: price); Item.rare = rarity;
        Item.accessory = true; Item.damage = damage; Item.DamageType = DamageClass.Summon;
        Item.mana = mana; Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.HoldUp; Item.useTime = Item.useAnimation = 30;
        Item.shoot = projectile; Item.buffType = buff;
    }
    public override bool CanUseItem(Player player)
    {
        if (!Ready || !player.active || player.dead || player.slotsMinions + 1 > player.maxMinions) return false;
        bool capacity = false;
        for (int i = 0; i < Main.maxProjectiles; i++) if (!Main.projectile[i].active) { capacity = true; break; }
        if (!capacity) return false;
        foreach (Projectile projectile in Main.ActiveProjectiles)
            if (projectile.owner == player.whoAmI && projectile.type == Item.shoot) return false;
        return true;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer || !CanUseItem(player)) return false;
        int index = Projectile.NewProjectile(source, player.Center, Vector2.Zero, Item.shoot, damage, knockback, player.whoAmI);
        if (index >= 0 && index < Main.maxProjectiles)
        {
            Main.projectile[index].originalDamage = Item.damage;
            player.AddBuff(Item.buffType, 2);
        }
        return false;
    }
}

public class FurnaceAshSpiritContract : SpiritContractItem
{
    public override void SetDefaults() => Configure(28, 12, ModContent.ProjectileType<FurnaceAshSpirit>(),
        ModContent.BuffType<FurnaceAshSpiritBuff>(), 2, ItemRarityID.Orange);
    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetDamage(DamageClass.Summon) += 0.08f;
    public override void AddRecipes() => CreateRecipe().AddIngredient<OldFurnaceEmber>(2)
        .AddIngredient<FurnaceSlagIron>(8).AddIngredient<LowGradeSpiritStone>(12)
        .AddTile(ModContent.TileType<ArtifactForgeTile>()).Register();
}

public class StarAbyssLarvaContract : SpiritContractItem
{
    protected override bool Ready => Main.hardMode;
    public override void SetDefaults() => Configure(48, 16, ModContent.ProjectileType<StarAbyssSpirit>(),
        ModContent.BuffType<StarAbyssSpiritBuff>(), 3, ItemRarityID.LightRed);
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetDamage(DamageClass.Summon) += 0.10f;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyCostMultiplier *= 1.05f;
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<StarCalamityCore>()
        .AddIngredient<StarAbyssMembrane>(8).AddIngredient<LowGradeSpiritStone>(16)
        .AddTile(ModContent.TileType<StarPatternCauldronTile>()).AddCondition(Condition.Hardmode).Register();
}
