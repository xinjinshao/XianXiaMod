using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Buffs;
using XianXia.Content.Items.Materials;
using XianXia.Content.Projectiles;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.HandGenerated;

public class SmallArtifactPendant : ModItem
{
    public override void SetStaticDefaults() { Item.ResearchUnlockCount = 1; ItemID.Sets.StaffMinionSlotsRequired[Type] = 1; }
    public override void SetDefaults()
    {
        Item.width = Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold:1); Item.rare = ItemRarityID.Blue;
        Item.accessory = true; Item.damage = 12; Item.DamageType = DamageClass.Summon;
        Item.mana = 10; Item.noMelee = true; Item.useStyle = ItemUseStyleID.HoldUp; Item.useTime = Item.useAnimation = 30;
        Item.buffType = ModContent.BuffType<SmallArtifactSpiritBuff>(); Item.shoot = ModContent.ProjectileType<SmallArtifactSpirit>();
    }
    public override void UpdateAccessory(Player player,bool hideVisual) => player.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus += 1;
    public override bool CanUseItem(Player player)
    {
        if (player.slotsMinions+1 > player.maxMinions) return false;
        bool capacity = false;
        for (int i = 0; i < Main.maxProjectiles; i++) if (!Main.projectile[i].active) { capacity = true; break; }
        if (!capacity) return false;
        foreach (Projectile projectile in Main.ActiveProjectiles)
            if (projectile.owner == player.whoAmI && projectile.type == Item.shoot) return false;
        return true;
    }
    public override bool Shoot(Player player,EntitySource_ItemUse_WithAmmo source,Vector2 position,Vector2 velocity,int type,int damage,float knockback)
    {
        if (player.whoAmI != Main.myPlayer || !player.active || player.dead || !CanUseItem(player)) return false;
        int index = Projectile.NewProjectile(source,player.Center,Vector2.Zero,type,damage,knockback,player.whoAmI);
        if (index >= 0 && index < Main.maxProjectiles) { Main.projectile[index].originalDamage = Item.damage; player.AddBuff(Item.buffType,2); }
        return false;
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<LowGradeSpiritStone>(8).AddIngredient(ItemID.FallenStar,3)
        .AddTile(ModContent.TileType<ArtifactForgeTile>()).Register();
}
