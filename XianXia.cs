using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia;

public class XianXia : Mod
{
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        byte message = reader.ReadByte();
        if (message != 0)
        {
            return;
        }

        byte playerIndex = reader.ReadByte();
        int spiritualEnergy = reader.ReadInt32();
        CultivationStage stage = (CultivationStage)reader.ReadInt32();

        if (playerIndex >= Main.maxPlayers)
        {
            return;
        }

        XianXiaPlayer modPlayer = Main.player[playerIndex].GetModPlayer<XianXiaPlayer>();
        modPlayer.spiritualEnergy = spiritualEnergy;
        modPlayer.cultivationStage = stage;

        if (Main.netMode == NetmodeID.Server)
        {
            ModPacket packet = GetPacket();
            packet.Write((byte)0);
            packet.Write(playerIndex);
            packet.Write(spiritualEnergy);
            packet.Write((int)stage);
            packet.Send(-1, whoAmI);
        }
    }
}
