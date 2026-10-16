using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

// Vanilla client-owned characters ignore their own incoming SyncEquipment packet.
// Send the canonical server-mutated slot to the owner through a server-only mod message.
public static class AuthoritativeInventorySync
{
    public static void Send(Player player, int slot)
    {
        if (Main.netMode != NetmodeID.Server || player == null || player.whoAmI < 0 || player.whoAmI >= Main.maxPlayers
            || slot < 0 || slot >= 58 || slot >= player.inventory.Length) return;
        NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, slot);
        var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket();
        packet.Write((byte)13); packet.Write((byte)player.whoAmI); packet.Write((byte)slot);
        packet.Write(player.GetModPlayer<InventoryAuthorityPlayer>().Next());
        ItemIO.Send(player.inventory[slot], packet, true, false); packet.Send(player.whoAmI);
    }
    public static void Receive(BinaryReader reader, int sender)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient || sender != 256) return;
        byte owner = reader.ReadByte(), slot = reader.ReadByte(); ulong revision = reader.ReadUInt64();
        if (owner != Main.myPlayer || owner >= Main.maxPlayers || slot >= 58 || revision == 0) return;
        Item item = ItemIO.Receive(reader, true, false);
        Main.player[owner].GetModPlayer<InventoryAuthorityPlayer>().Apply(slot, revision, item);
    }
}
