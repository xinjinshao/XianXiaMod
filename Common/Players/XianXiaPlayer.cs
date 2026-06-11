using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Players;

public class XianXiaPlayer : ModPlayer
{
    public const int BaseMaxSpiritualEnergy = 60;
    public const int QiAwakeningBonus = 40;

    public int spiritualEnergy;
    public int maxSpiritualEnergy;
    public int spiritPressure;
    public bool discoveredSpiritualEnergy;
    public CultivationStage cultivationStage;

    private int regenTimer;

    public override void Initialize()
    {
        maxSpiritualEnergy = BaseMaxSpiritualEnergy;
        spiritualEnergy = 0;
        spiritPressure = 0;
        cultivationStage = CultivationStage.None;
        discoveredSpiritualEnergy = false;
    }

    public override void ResetEffects()
    {
        maxSpiritualEnergy = BaseMaxSpiritualEnergy;
        if (cultivationStage >= CultivationStage.QiAwakening)
        {
            maxSpiritualEnergy += QiAwakeningBonus;
        }

        if (spiritualEnergy > maxSpiritualEnergy)
        {
            spiritualEnergy = maxSpiritualEnergy;
        }
    }

    public override void PostUpdate()
    {
        if (!discoveredSpiritualEnergy)
        {
            return;
        }

        regenTimer++;
        int interval = cultivationStage >= CultivationStage.QiAwakening ? 42 : 60;
        if (regenTimer >= interval)
        {
            regenTimer = 0;
            spiritualEnergy = Math.Clamp(spiritualEnergy + 1, 0, maxSpiritualEnergy);
            if (spiritPressure > 0)
            {
                spiritPressure--;
            }
        }
    }

    public bool TryConsumeSpiritualEnergy(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        discoveredSpiritualEnergy = true;
        if (spiritualEnergy < amount)
        {
            return false;
        }

        spiritualEnergy -= amount;
        return true;
    }

    public void RestoreSpiritualEnergy(int amount)
    {
        discoveredSpiritualEnergy = true;
        spiritualEnergy = Math.Clamp(spiritualEnergy + amount, 0, maxSpiritualEnergy);
    }

    public void UnlockQiAwakening()
    {
        discoveredSpiritualEnergy = true;
        if (cultivationStage < CultivationStage.QiAwakening)
        {
            cultivationStage = CultivationStage.QiAwakening;
            maxSpiritualEnergy = BaseMaxSpiritualEnergy + QiAwakeningBonus;
        }
        RestoreSpiritualEnergy(30);
    }

    public override void SaveData(TagCompound tag)
    {
        tag["spiritualEnergy"] = spiritualEnergy;
        tag["spiritPressure"] = spiritPressure;
        tag["discoveredSpiritualEnergy"] = discoveredSpiritualEnergy;
        tag["cultivationStage"] = (int)cultivationStage;
    }

    public override void LoadData(TagCompound tag)
    {
        spiritualEnergy = tag.GetInt("spiritualEnergy");
        spiritPressure = tag.GetInt("spiritPressure");
        discoveredSpiritualEnergy = tag.GetBool("discoveredSpiritualEnergy");
        cultivationStage = (CultivationStage)tag.GetInt("cultivationStage");
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        XianXiaPlayer clone = (XianXiaPlayer)targetCopy;
        clone.spiritualEnergy = spiritualEnergy;
        clone.maxSpiritualEnergy = maxSpiritualEnergy;
        clone.spiritPressure = spiritPressure;
        clone.discoveredSpiritualEnergy = discoveredSpiritualEnergy;
        clone.cultivationStage = cultivationStage;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        XianXiaPlayer old = (XianXiaPlayer)clientPlayer;
        if (old.spiritualEnergy != spiritualEnergy || old.cultivationStage != cultivationStage)
        {
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
        }
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)0);
        packet.Write((byte)Player.whoAmI);
        packet.Write(spiritualEnergy);
        packet.Write((int)cultivationStage);
        packet.Send(toWho, fromWho);
    }
}
