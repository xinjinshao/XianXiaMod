using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
namespace XianXia.Common.Systems;
public class SpiritTideUISystem : ModSystem
{
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if(Main.dedServ)return;
        int index=layers.FindIndex(layer=>layer.Name=="Vanilla: Mouse Text");if(index<0)return;
        layers.Insert(index,new LegacyGameInterfaceLayer("XianXia: Spirit Tide",()=>{
            if(!SpiritTideSystem.Active||Main.gameMenu)return true;
            float x=Main.screenWidth/System.Math.Max(.01f,Main.UIScale)/2f-150;
            var position=new Vector2(x,240);
            string label=Language.GetTextValue("Mods.XianXia.SpiritTide.Progress",SpiritTideSystem.Wave,SpiritTideSystem.Kills,SpiritTideSystem.RequiredKills);
            Utils.DrawBorderString(Main.spriteBatch,label,position,new Color(120,245,220),.8f);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,new Rectangle((int)x,266,300,8),Color.Black);
            int fill=(int)(300f*SpiritTideSystem.Kills/SpiritTideSystem.RequiredKills);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,new Rectangle((int)x,266,fill,8),new Color(120,245,220));
            return true;
        },InterfaceScaleType.UI));
    }
}
