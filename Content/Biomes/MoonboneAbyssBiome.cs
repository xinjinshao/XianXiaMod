using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ModLoader;

namespace XianXia.Content.Biomes;

public class MoonboneAbyssBiome : ModBiome

{

    public override int Music => 0;

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

    public override string BackgroundPath => MapBackground;

    public override string MapBackground => "Terraria/Images/MapBG1";

    public override Color? BackgroundColor => new(90, 170, 150);



    public override bool IsBiomeActive(Player player)

    {

        return ModContent.GetInstance<GeneratedBiomeTileCountSystem>().moonboneAbyssBiomeTileCount >= 200;

    }

}
