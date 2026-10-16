using Terraria;
using Terraria.ModLoader;

namespace XianXia.Common.Players;

public class InventoryAuthorityPlayer : ModPlayer
{
    private ulong sent;
    private ulong[] received = new ulong[58];
    public override void Initialize() { sent = 0; received = new ulong[58]; }
    public override void OnEnterWorld() => Initialize();
    internal ulong Next() { sent = unchecked(sent + 1); if (sent == 0) sent = 1; return sent; }
    internal bool Apply(int slot, ulong revision, Item item)
    {
        if (slot < 0 || slot >= received.Length || slot >= Player.inventory.Length || revision == 0
            || received[slot] != 0 && unchecked((long)(revision - received[slot])) <= 0) return false;
        Player.inventory[slot] = item; received[slot] = revision; return true;
    }
}
