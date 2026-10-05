using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Content.Items.Accessories;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public static class ArtifactSkillTransactions
{
    public static ArtifactSkill? HeldSkill(Item item) => RefinedArtifact.IsSample(item) ? item.ModItem.Name switch
    { "CloudpiercerFlyingSword" => ArtifactSkill.SwordBurst, "GreenwoodArrayPlate" => ArtifactSkill.ArrayPulse, _ => null } : null;
    public static bool CanUseAlternative(Player player, Item item) => RefinedArtifact.IsAwakened(item)
        && HeldSkill(item) is ArtifactSkill skill && player.GetModPlayer<XianXiaPlayer>().activeSkillCooldown == 0
        && player.GetModPlayer<XianXiaPlayer>().CanConsumeSpiritualEnergy(DaoArtifactRules.SkillCost(skill,RefinedArtifact.ActiveDaoRoute(item)));
    public static void Request(Player player, ArtifactSkill skill)
    {
        if (Main.netMode == NetmodeID.Server || player.whoAmI != Main.myPlayer || !ArtifactSkillRules.IsValid(skill)) return;
        if (Main.netMode == NetmodeID.SinglePlayer) {
            HandleRequest(player, skill, player.selectedItem, player.HeldItem.type, Main.MouseWorld); return;
        }
        var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket();
        packet.Write((byte)10); packet.Write((byte)skill); packet.Write((byte)player.selectedItem); packet.Write(player.HeldItem.type);
        packet.Write(Main.MouseWorld.X); packet.Write(Main.MouseWorld.Y); packet.Send();
    }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
    public static bool HasWard(Player player)
    {
        for (int slot = 3; slot < Math.Min(player.armor.Length, 10); slot++)
            if (player.IsItemSlotUnlockedAndUsable(slot) && player.armor[slot].type == ModContent.ItemType<LightningWardJade>() && !player.armor[slot].IsAir) return true;
        return false;
    }
    public static void HandleRequest(Player player, ArtifactSkill skill, int slot, int type, Vector2 aim)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !ArtifactSkillRules.IsValid(skill) || !player.active || player.dead
            || player.noItems || player.CCed || !Finite(aim) || slot < 0 || slot >= player.inventory.Length || slot != player.selectedItem) return;
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        if (Main.netMode == NetmodeID.Server && !state.NetworkInitialized) return;
        if (state.skillRequestCooldown > 0) return;
        state.skillRequestCooldown = 10;
        Item item = player.inventory[slot];
        string reason = "Mods.XianXia.Skills.Unavailable";
        bool eligible = item.type == type && (skill == ArtifactSkill.WardGuard ? HasWard(player)
            : item.stack == 1 && HeldSkill(item) == skill && RefinedArtifact.IsAwakened(item));
        if (!eligible) { Reply(player, reason); return; }
        if (state.activeSkillCooldown > 0) { Reply(player, "Mods.XianXia.Skills.CoolingDown"); return; }
        List<Player> healTargets = new();
        if (skill == ArtifactSkill.SwordBurst && Main.maxProjectiles - CountActive() < 3) { Reply(player,"Mods.XianXia.Skills.Capacity"); return; }
        if (skill == ArtifactSkill.ArrayPulse) {
            Projectile field = FindArray(player);
            if (field == null) { Reply(player, "Mods.XianXia.Skills.NeedArray"); return; }
            foreach (Player target in Main.ActivePlayers)
                if (!target.dead && target.statLife < target.statLifeMax2 && Friendly(player, target)
                    && Vector2.DistanceSquared(target.Center, field.Center) <= 160 * 160) healTargets.Add(target);
            if (healTargets.Count == 0) { Reply(player, "Mods.XianXia.Skills.NoHealing"); return; }
        }
        CultivationSnapshot before = state.CaptureSnapshot();
        var daoRoute = RefinedArtifact.ActiveDaoRoute(item);
        if (!state.TryConsumeSpiritualEnergy(DaoArtifactRules.SkillCost(skill,daoRoute))) { Reply(player, "Mods.XianXia.Skills.NeedEnergy"); return; }
        if (skill == ArtifactSkill.SwordBurst && !Burst(player, item, aim)) {
            state.ApplySnapshot(before); state.SyncPlayer(player.whoAmI, -1, false); return;
        }
        if (skill == ArtifactSkill.ArrayPulse) foreach (Player target in healTargets) {
            Heal(target,DaoArtifactRules.PulseHeal(daoRoute));
        }
        if (skill == ArtifactSkill.SwordBurst && daoRoute == DownedBossSystem.EndgameRoute.RebuildHeaven) Heal(player,20);
        if (skill == ArtifactSkill.WardGuard) {
            state.wardGuardTimer = 180;
            player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.ArtifactWardBuff>(), 2);
        }
        if (skill != ArtifactSkill.WardGuard && daoRoute == DownedBossSystem.EndgameRoute.AcceptStarAbyss)
            state.spiritPressure = Math.Min(100,state.spiritPressure + 8);
        state.activeSkillCooldown = ArtifactSkillRules.Cooldown(skill); state.AdvanceResourceRevision();
        state.SyncPlayer(-1, -1, false);
        Reply(player, "Mods.XianXia.Skills.Success");
    }
    private static bool Friendly(Player owner, Player target) => owner == target || !owner.hostile || !target.hostile || (owner.team > 0 && owner.team == target.team);
    private static void Heal(Player target,int amount)
    {
        int heal = Math.Min(amount,Math.Max(0,target.statLifeMax2 - target.statLife));
        if (heal == 0) return;
        if (Main.netMode == NetmodeID.SinglePlayer) target.Heal(heal);
        else { target.statLife += heal; NetMessage.SendData(MessageID.SpiritHeal,target.whoAmI,-1,null,target.whoAmI,heal); }
    }
    private static Projectile FindArray(Player player)
    {
        int type = ModContent.ProjectileType<GreenwoodArrayField>();
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile p = Main.projectile[i];
            if (p.active && p.owner == player.whoAmI && p.type == type && Vector2.DistanceSquared(player.Center, p.Center) <= 160 * 160) return p;
        }
        return null;
    }
    private static int CountActive() { int count = 0; for (int i = 0; i < Main.maxProjectiles; i++) if (Main.projectile[i].active) count++; return count; }
    private static bool Burst(Player player, Item item, Vector2 aim)
    {
        Vector2 position = player.RotatedRelativePoint(player.MountedCenter), delta = aim - position;
        if (!Finite(position) || !Finite(delta) || !float.IsFinite(delta.LengthSquared())) return false;
        Vector2 velocity = delta.SafeNormalize(Vector2.UnitX * player.direction) * 14f;
        int damage = player.GetWeaponDamage(item) * DaoArtifactRules.BurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)), type = ModContent.ProjectileType<CloudpiercerSwordProjectile>();
        var created = new List<int>();
        var source = new EntitySource_ItemUse_WithAmmo(player, item, 0);
        for (int i = -1; i <= 1; i++) {
            int index = Projectile.NewProjectile(source, position, velocity.RotatedBy(i * 0.18), type, damage, player.GetWeaponKnockback(item, item.knockBack), player.whoAmI);
            if (index < 0 || index >= Main.maxProjectiles) {
                foreach (int old in created) {
                    Projectile p = Main.projectile[old]; p.active = false;
                    if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, p.identity, p.owner);
                }
                return false;
            }
            created.Add(index);
        }
        return true;
    }
    private static void Reply(Player player, string key)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(Language.GetTextValue(key), 120, 230, 210);
        else { var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket(); packet.Write((byte)7); NetworkText.FromKey(key).Serialize(packet); packet.Send(player.whoAmI); }
    }
}
