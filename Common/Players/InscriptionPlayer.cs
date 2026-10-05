using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Systems;

namespace XianXia.Common.Players;

public class InscriptionPlayer : ModPlayer
{
    private int accessoryKinds;
    private int activeKinds;
    private int pollutionTimer;
    public int TransactionCooldown { get; private set; }
    public override void Initialize() { accessoryKinds = activeKinds = pollutionTimer = TransactionCooldown = 0; }
    public override void ResetEffects() { accessoryKinds = activeKinds = 0; }
    public void RegisterAccessory(InscriptionKind kind)
    {
        if (kind != InscriptionKind.None && InscriptionRules.Normalize((int)kind) == kind)
            accessoryKinds |= 1 << (int)kind;
    }
    private bool Has(InscriptionKind kind) => (activeKinds & (1 << (int)kind)) != 0;
    private bool AccessoryHas(InscriptionKind kind) => (accessoryKinds & (1 << (int)kind)) != 0;
    public bool BeginTransaction()
    {
        if (TransactionCooldown > 0) return false;
        TransactionCooldown = 60;
        return true;
    }
    public override void PostUpdateEquips()
    {
        if (!Player.active || Player.dead) return;
        activeKinds = accessoryKinds;
        InscriptionKind held = InscribedEquipment.GetKind(Player.HeldItem);
        if (!Player.HeldItem.accessory && held != InscriptionKind.None) activeKinds |= 1 << (int)held;
        var cultivation = Player.GetModPlayer<XianXiaPlayer>();
        if (Has(InscriptionKind.Greenwood)) cultivation.spiritualEnergyRegenBonus += 1;
        if (Has(InscriptionKind.Furnace)) Player.GetArmorPenetration(DamageClass.Generic) += 6;
        if (Has(InscriptionKind.Thunder)) { Player.moveSpeed += 0.08f; Player.GetAttackSpeed(DamageClass.Generic) += 0.08f; }
        if (Has(InscriptionKind.BrokenHeaven)) cultivation.spiritualEnergyCostMultiplier *= 1.15f;
        if (AccessoryHas(InscriptionKind.Furnace)) Player.statDefense += 3;
        if (AccessoryHas(InscriptionKind.StarAbyss)) Player.GetDamage(DamageClass.Generic) += 0.08f;
        if (AccessoryHas(InscriptionKind.BrokenHeaven)) {
            Player.GetCritChance(DamageClass.Generic) += 4f;
            Player.GetKnockback(DamageClass.Generic) += 0.15f;
        }
    }
    public override void PostUpdate()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (TransactionCooldown > 0) TransactionCooldown--;
        if (!Player.active || Player.dead) { pollutionTimer = 0; return; }
        if (!Has(InscriptionKind.StarAbyss)) return;
        if (++pollutionTimer < 120) return;
        pollutionTimer = 0;
        var state = Player.GetModPlayer<XianXiaPlayer>();
        state.spiritPressure = System.Math.Min(100, state.spiritPressure + 4);
    }
    public override void UpdateLifeRegen() { if (Has(InscriptionKind.Greenwood)) Player.lifeRegen += 2; }
    public override void UpdateDead() { pollutionTimer = 0; if (TransactionCooldown > 0) TransactionCooldown--; }
}
