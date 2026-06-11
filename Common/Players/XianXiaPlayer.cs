using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace XianXia.Common.Players;

public class XianXiaPlayer : ModPlayer
{
    public const int BaseMaxSpiritualEnergy = 60;

    public int spiritualEnergy;
    public int maxSpiritualEnergy;
    public int spiritPressure;
    public int spiritualEnergyRegenBonus;
    public int tribulationTimer;
    public int tribulationIntensity;
    public float spiritualEnergyCostMultiplier = 1f;
    public bool discoveredSpiritualEnergy;
    public CultivationStage cultivationStage;

    private int regenTimer;

    public override void Initialize()
    {
        maxSpiritualEnergy = BaseMaxSpiritualEnergy;
        spiritualEnergy = 0;
        spiritPressure = 0;
        tribulationTimer = 0;
        tribulationIntensity = 0;
        cultivationStage = CultivationStage.None;
        discoveredSpiritualEnergy = false;
    }

    public override void ResetEffects()
    {
        maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage);
        spiritualEnergyRegenBonus = 0;
        spiritualEnergyCostMultiplier = 1f;

        ApplyCultivationStageBonuses();

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

        if (spiritPressure >= 80)
        {
            Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 2);
        }

        UpdateTribulation();

        regenTimer++;
        int interval = cultivationStage >= CultivationStage.QiAwakening ? Math.Max(18, 60 - (int)cultivationStage * 5) : 60;
        if (regenTimer >= interval)
        {
            regenTimer = 0;
            spiritualEnergy = Math.Clamp(spiritualEnergy + 1 + spiritualEnergyRegenBonus, 0, maxSpiritualEnergy);
            if (spiritPressure > 0)
            {
                spiritPressure--;
            }
        }
    }

    public bool TryConsumeSpiritualEnergy(int amount)
    {
        amount = (int)MathF.Ceiling(amount * spiritualEnergyCostMultiplier);
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
            maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage);
        }
        RestoreSpiritualEnergy(30);
    }

    public bool TryAdvanceCultivation(CultivationStage targetStage)
    {
        discoveredSpiritualEnergy = true;
        if (targetStage <= cultivationStage || targetStage > CultivationStage.DaoSevering)
        {
            return false;
        }

        if ((int)targetStage != (int)cultivationStage + 1)
        {
            return false;
        }

        cultivationStage = targetStage;
        maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage);
        RestoreSpiritualEnergy(maxSpiritualEnergy / 3);
        spiritPressure = Math.Clamp(spiritPressure + (int)targetStage * 8, 0, 100);
        BeginTribulation(targetStage);
        return true;
    }

    public void ReduceSpiritPressure(int amount)
    {
        spiritPressure = Math.Clamp(spiritPressure - amount, 0, 100);
    }

    private void BeginTribulation(CultivationStage stage)
    {
        if (stage < CultivationStage.Foundation)
        {
            return;
        }

        tribulationIntensity = Math.Clamp((int)stage - 1, 1, 8);
        tribulationTimer = Math.Max(tribulationTimer, 60 * (18 + tribulationIntensity * 4));
    }

    private void UpdateTribulation()
    {
        if (tribulationTimer <= 0)
        {
            tribulationIntensity = 0;
            return;
        }

        tribulationTimer--;
        Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationPressureBuff>(), 2);

        if (Player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>()) && Main.GameUpdateCount % 30 == 0)
        {
            ReduceSpiritPressure(2);
        }

        int interval = Math.Max(38, 110 - tribulationIntensity * 8);
        if (Main.myPlayer == Player.whoAmI && tribulationTimer % interval == 0)
        {
            SpawnTribulationLightning();
        }

        if (tribulationTimer == 0)
        {
            ReduceSpiritPressure(20 + tribulationIntensity * 2);
            RestoreSpiritualEnergy(20 + tribulationIntensity * 8);
            tribulationIntensity = 0;
        }
    }

    private void SpawnTribulationLightning()
    {
        if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
        {
            return;
        }

        float offsetX = Main.rand.NextFloat(-240f, 240f);
        Microsoft.Xna.Framework.Vector2 position = Player.Center + new Microsoft.Xna.Framework.Vector2(offsetX, -620f);
        Microsoft.Xna.Framework.Vector2 velocity = new(-offsetX * 0.0025f, 10f + tribulationIntensity * 0.45f);
        int damage = 18 + tribulationIntensity * 7;
        Projectile.NewProjectile(
            Player.GetSource_FromThis(),
            position,
            velocity,
            ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationLightningProjectile>(),
            damage,
            1.5f,
            Player.whoAmI);
    }

    public static int GetMaxSpiritualEnergy(CultivationStage stage)
    {
        return stage switch
        {
            CultivationStage.None => BaseMaxSpiritualEnergy,
            CultivationStage.QiAwakening => 100,
            CultivationStage.QiCondensation => 140,
            CultivationStage.Foundation => 190,
            CultivationStage.GoldenCore => 260,
            CultivationStage.NascentSoul => 340,
            CultivationStage.SpiritSevering => 430,
            CultivationStage.Tribulation => 540,
            CultivationStage.DaoSevering => 660,
            _ => BaseMaxSpiritualEnergy
        };
    }

    private void ApplyCultivationStageBonuses()
    {
        if (cultivationStage < CultivationStage.QiAwakening)
        {
            return;
        }

        int stage = (int)cultivationStage;
        Player.GetDamage(DamageClass.Generic) += 0.02f * stage;
        Player.statDefense += stage;
        Player.moveSpeed += 0.01f * stage;
        spiritualEnergyCostMultiplier *= MathF.Max(0.72f, 1f - stage * 0.025f);

        if (cultivationStage >= CultivationStage.GoldenCore)
        {
            Player.GetCritChance(DamageClass.Generic) += 2;
        }

        if (cultivationStage >= CultivationStage.NascentSoul)
        {
            Player.endurance += 0.03f;
            spiritualEnergyRegenBonus += 1;
        }

        if (cultivationStage >= CultivationStage.Tribulation)
        {
            Player.GetCritChance(DamageClass.Generic) += 3;
            Player.endurance += 0.02f;
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag["spiritualEnergy"] = spiritualEnergy;
        tag["spiritPressure"] = spiritPressure;
        tag["tribulationTimer"] = tribulationTimer;
        tag["tribulationIntensity"] = tribulationIntensity;
        tag["discoveredSpiritualEnergy"] = discoveredSpiritualEnergy;
        tag["cultivationStage"] = (int)cultivationStage;
    }

    public override void LoadData(TagCompound tag)
    {
        spiritualEnergy = tag.GetInt("spiritualEnergy");
        spiritPressure = tag.GetInt("spiritPressure");
        tribulationTimer = tag.GetInt("tribulationTimer");
        tribulationIntensity = tag.GetInt("tribulationIntensity");
        discoveredSpiritualEnergy = tag.GetBool("discoveredSpiritualEnergy");
        cultivationStage = (CultivationStage)tag.GetInt("cultivationStage");
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        XianXiaPlayer clone = (XianXiaPlayer)targetCopy;
        clone.spiritualEnergy = spiritualEnergy;
        clone.maxSpiritualEnergy = maxSpiritualEnergy;
        clone.spiritPressure = spiritPressure;
        clone.tribulationTimer = tribulationTimer;
        clone.tribulationIntensity = tribulationIntensity;
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
