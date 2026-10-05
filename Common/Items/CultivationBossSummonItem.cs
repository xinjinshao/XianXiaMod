using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Common.Items;

// Each concrete summon retains its own recipe, texture and eligibility rules.
public abstract class CultivationBossSummonItem : ModItem
{
    public abstract int BossType { get; }
    private bool singlePlayerSummoned;

    public sealed override bool? UseItem(Player player)
    {
        singlePlayerSummoned = false;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            if (player.whoAmI == Main.myPlayer && player.HeldItem == Item)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)6);
                packet.Write((byte)player.selectedItem);
                packet.Write(Item.type);
                packet.Send();
            }
            return true;
        }
        // The server's normal replicated item-use hook must not spawn a second
        // boss; the inventory transaction is the sole multiplayer entry point.
        if (Main.netMode == NetmodeID.Server) return true;
        singlePlayerSummoned = BossSummonTransactions.TrySummon(player, this);
        return singlePlayerSummoned;
    }

    public sealed override bool ConsumeItem(Player player)
    {
        bool consume = Main.netMode == NetmodeID.SinglePlayer && singlePlayerSummoned;
        singlePlayerSummoned = false;
        return consume;
    }
}
