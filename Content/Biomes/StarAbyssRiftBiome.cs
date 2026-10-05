using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ModLoader;

namespace XianXia.Content.Biomes;

public class StarAbyssRiftBiome : ModBiome

{

    public override int Music => 0;

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

    public override string BackgroundPath => MapBackground;

    public override string MapBackground => "Terraria/Images/MapBG1";

    public override Color? BackgroundColor => new(90, 170, 150);



    public override bool IsBiomeActive(Player player)

    {

        if (Main.netMode == Terraria.ID.NetmodeID.Server)
            return player.GetModPlayer<global::XianXia.Common.Players.ServerBiomePlayer>().Count(ModContent.TileType<global::XianXia.Content.Tiles.StarAbyssCrystalTile>()) >= 140;
        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().starAbyssRiftBiomeTileCount >= 140;

    }

}
