using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Items;

// Equipment bonuses are rebuilt every tick by native player and ModPlayer resets.
public static class ExpertRewardEffects
{
    public static void Apply(Player player, XianXiaPlayer cultivation, int reward)
    {
        switch (reward)
        {
            case 0: cultivation.spiritualEnergyRegenBonus += 1; player.statDefense += 2; break;
            case 1: player.lifeRegen += 2; player.buffImmune[BuffID.Poisoned] = true; break;
            case 2: player.statDefense += 6; player.buffImmune[BuffID.OnFire] = true; break;
            case 3: cultivation.spiritualEnergyCostMultiplier *= 0.92f; player.endurance += 0.03f; break;
            case 4: player.moveSpeed += 0.12f; player.jumpSpeedBoost += 1.5f; break;
            case 5: cultivation.spiritualEnergyRegenBonus += 2; player.GetDamage(DamageClass.Magic) += 0.06f; break;
            case 6: player.GetDamage(DamageClass.Melee) += 0.08f; player.GetArmorPenetration(DamageClass.Melee) += 8; break;
            case 7: player.lifeRegen += 4; cultivation.spiritualEnergyRegenBonus += 1; break;
            case 8: player.endurance += 0.06f; player.noKnockback = true; break;
            case 9: player.GetDamage(DamageClass.Ranged) += 0.08f; player.GetArmorPenetration(DamageClass.Ranged) += 8; break;
            case 10: player.maxMinions += 1; player.GetDamage(DamageClass.Summon) += 0.08f; break;
            case 11: player.GetDamage(DamageClass.Generic) += 0.10f; player.statDefense += 6; cultivation.spiritualEnergyRegenBonus += 2; break;
        }
    }
}
