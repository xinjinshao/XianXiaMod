using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Items.Accessories;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public class DebugDropGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        if (!ModContent.GetInstance<XianXiaConfig>().DebugDrops || npc.ModNPC?.Mod != Mod)
        {
            return;
        }

        IEntitySource source = npc.GetSource_Loot();
        int spiritStoneStack = npc.boss ? 25 : Main.rand.Next(2, 5);
        Item.NewItem(source, npc.Hitbox, ModContent.ItemType<LowGradeSpiritStone>(), spiritStoneStack);

        if (npc.boss)
        {
            Item.NewItem(source, npc.Hitbox, ModContent.ItemType<ArtifactBlankShard>(), 3);
        }
    }
}
