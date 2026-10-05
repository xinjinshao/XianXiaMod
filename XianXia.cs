using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using XianXia.Common.Players;
using XianXia.Content.NPCs.Town;

namespace XianXia;

public class XianXia : Mod
{
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        try
        {
            ReadPacket(reader, whoAmI);
        }
        catch (IOException)
        {
            // A truncated request must not terminate the server or mutate a partial state.
        }
    }

    private void ReadPacket(BinaryReader reader, int whoAmI)
    {
        byte message = reader.ReadByte();
        if (message == 10)
        {
            var skill = (Common.Systems.ArtifactSkill)reader.ReadByte(); byte slot = reader.ReadByte(); int type = reader.ReadInt32();
            var aim = new Microsoft.Xna.Framework.Vector2(reader.ReadSingle(), reader.ReadSingle());
            if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
                Common.Systems.ArtifactSkillTransactions.HandleRequest(Main.player[whoAmI], skill, slot, type, aim);
            return;
        }
        if (message == 9)
        {
            byte toolSlot = reader.ReadByte();
            int toolType = reader.ReadInt32();
            byte targetSlot = reader.ReadByte();
            int targetType = reader.ReadInt32();
            int prefix = reader.ReadInt32();
            byte previous = reader.ReadByte();
            byte previousLevel = reader.ReadByte();
            bool previousAwakened = reader.ReadBoolean();
            if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
                Common.Systems.InscriptionTransactions.HandleRequest(Main.player[whoAmI], toolSlot, toolType, targetSlot, targetType, prefix, previous, previousLevel, previousAwakened);
            return;
        }
        if (message == 12)
        {
            byte toolSlot = reader.ReadByte(); int toolType = reader.ReadInt32();
            byte targetSlot = reader.ReadByte(); int targetType = reader.ReadInt32(); int prefix = reader.ReadInt32();
            byte kind = reader.ReadByte(), level = reader.ReadByte(); bool awakened = reader.ReadBoolean();
            byte daoRoute = reader.ReadByte(), worldRoute = reader.ReadByte();
            if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
                Common.Systems.DaoArtifactTransactions.Handle(Main.player[whoAmI],toolSlot,toolType,targetSlot,targetType,prefix,kind,level,awakened,daoRoute,worldRoute);
            return;
        }
        if (message == 11)
        {
            byte slot = reader.ReadByte();
            int type = reader.ReadInt32();
            byte route = reader.ReadByte();
            if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
                Common.Systems.EndgameRouteTransactions.Handle(Main.player[whoAmI], slot, type, route);
            return;
        }
        if (message == 8)
        {
            byte slot = reader.ReadByte();
            int type = reader.ReadInt32();
            var aim = new Microsoft.Xna.Framework.Vector2(reader.ReadSingle(), reader.ReadSingle());
            if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
                Common.Systems.WeaponShotTransactions.HandleRequest(Main.player[whoAmI], slot, type, aim);
            return;
        }
        if (message == 6)
        {
            byte slot = reader.ReadByte();
            int type = reader.ReadInt32();
            if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
                Common.Systems.BossSummonTransactions.HandleRequest(Main.player[whoAmI], slot, type);
            return;
        }
        if (message == 7)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            NetworkText text = NetworkText.Deserialize(reader);
            Main.NewText(text.ToString(), 255, 210, 120);
            return;
        }
        if (message == 3 || message == 4)
        {
            byte index = reader.ReadByte();
            CultivationSnapshot state = CultivationSnapshot.Read(reader);
            if (index >= Main.maxPlayers || !state.IsValid()) return;
            XianXiaPlayer target = Main.player[index].GetModPlayer<XianXiaPlayer>();
            if (message == 3 && Main.netMode == NetmodeID.Server && index == whoAmI)
            {
                if (target.TryInitializeNetwork(state)) target.SyncPlayer(-1, -1, false);
            }
            else if (message == 4 && Main.netMode == NetmodeID.MultiplayerClient)
            {
                target.NotifySnapshot(state);
                target.ApplySnapshot(state);
            }
            return;
        }

        if (message == 5)
        {
            byte slot = reader.ReadByte();
            int type = reader.ReadInt32();
            if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
                Common.Systems.CultivationItemTransactions.HandleRequest(Main.player[whoAmI], slot, type);
            return;
        }
        if (message == 1)
        {
            short npcIndex = reader.ReadInt16();
            if (Main.netMode != NetmodeID.Server || whoAmI < 0 || whoAmI >= Main.maxPlayers
                || npcIndex < 0 || npcIndex >= Main.maxNPCs)
                return;

            Player player = Main.player[whoAmI];
            NPC npc = Main.npc[npcIndex];
            if (!player.active || player.dead || !npc.active
                || npc.ModNPC is not CultivationTownNPC town
                || player.talkNPC != npcIndex
                || Microsoft.Xna.Framework.Vector2.DistanceSquared(player.Center, npc.Center) > 600f * 600f)
                return;

            NetworkText response = town.ClaimCommissionOnServer(player);
            ModPacket reply = GetPacket();
            reply.Write((byte)2);
            reply.Write(npcIndex);
            response.Serialize(reply);
            reply.Send(whoAmI);
            return;
        }

        if (message == 2)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;
            short npcIndex = reader.ReadInt16();
            NetworkText response = NetworkText.Deserialize(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient && Main.LocalPlayer.talkNPC == npcIndex)
                Main.npcChatText = response.ToString();
            return;
        }

        // Type 0 is retired. Runtime resource values are never imported from a client.
    }
}
