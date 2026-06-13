using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Materials;

public class HeavenDaoFragment : ModItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;

    public override void SetDefaults()

    {

        Item.width = 32;

        Item.height = 32;

        Item.maxStack = 30;

        Item.value = Item.buyPrice(silver: 10);

        Item.rare = ItemRarityID.Yellow;



        Item.useStyle = ItemUseStyleID.DrinkLiquid;

        Item.useTime = 20;

        Item.useAnimation = 20;

        Item.UseSound = SoundID.Item3;

        Item.consumable = true;

    }



    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanUseBreakthroughItem(global::XianXia.Common.Players.CultivationStage.SpiritSevering);

    }



    public override bool? UseItem(Player player)

    {

        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.SpiritSevering);

        return true;

    }



}
