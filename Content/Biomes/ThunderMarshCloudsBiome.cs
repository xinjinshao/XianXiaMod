using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ModLoader;

namespace XianXia.Content.Biomes;

public class ThunderMarshCloudsBiome : ModBiome

{

    public override int Music => 0;

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

    public override string BackgroundPath => MapBackground;

    public override string MapBackground => "Terraria/Images/MapBG1";

    public override Color? BackgroundColor => new(90, 170, 150);



    public override bool IsBiomeActive(Player player)

    {

        if (Main.netMode == Terraria.ID.NetmodeID.Server)
            return player.GetModPlayer<global::XianXia.Common.Players.ServerBiomePlayer>().Count(ModContent.TileType<global::XianXia.Content.Tiles.ThunderCloudTile>()) + player.GetModPlayer<global::XianXia.Common.Players.ServerBiomePlayer>().Count(ModContent.TileType<global::XianXia.Content.Tiles.Construction.ThunderConstructedTile>()) >= 100;
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().thunderMarshCloudsBiomeTileCount >= 100;

    }

}
