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
    public static ArtifactSkill? HeldSkill(Item item) => RefinedArtifact.SupportsAwakening(item) ? item.ModItem.Name switch
    { "CloudpiercerFlyingSword" => ArtifactSkill.SwordBurst, "GreenwoodArrayPlate" => ArtifactSkill.ArrayPulse, "HeavenTabletWardSeal" => ArtifactSkill.WardGuard, "MoonboneDharmaSword" => ArtifactSkill.MoonCrescent, "FormlessSwordWheel" => ArtifactSkill.WheelFormation, "BlackFurnaceWarhammer" => ArtifactSkill.HammerVolley, "ThunderPatternSwordCase" => ArtifactSkill.ThunderVolley, "CinnabarTalismanFlameItem" => ArtifactSkill.FlameFan, "WoodgrainFlyingSword" => ArtifactSkill.WoodgrainRecall, "SpiritwoodCrossbow" => ArtifactSkill.SpiritwoodVolley, "StarEclipseArbalest" => ArtifactSkill.StarSplitVolley, "SectMechanismCrossbow" => ArtifactSkill.SectVolley, "HeavenLawArbalest" => ArtifactSkill.HeavenLawVolley, "StarCalamityMechanismCase" => ArtifactSkill.CalamityVolley, "ArchiveStarCodex" => ArtifactSkill.CodexConstellation, "BrokenHeavenDecree" => ArtifactSkill.DecreeTriad, "GreenwoodMedicineCauldron" => ArtifactSkill.CauldronBloom, "ThunderTalismanArrayPlate" => ArtifactSkill.ArrayDischarge, _ => null } : null;
    public static bool CanUseAlternative(Player player, Item item) => RefinedArtifact.IsAwakened(item)
        && HeldSkill(item) is ArtifactSkill skill && player.GetModPlayer<XianXiaPlayer>().activeSkillCooldown == 0
        && (skill != ArtifactSkill.HammerVolley || HammerRealmAllowed(player))
        && (skill is not (ArtifactSkill.SectVolley or ArtifactSkill.CauldronBloom) || NascentRealmAllowed(player))
        && (skill != ArtifactSkill.HeavenLawVolley || HeavenLawRealmAllowed(player))
        && (skill is not (ArtifactSkill.CalamityVolley or ArtifactSkill.CodexConstellation) || EndgameRealmAllowed(player))
        && player.GetModPlayer<XianXiaPlayer>().CanConsumeSpiritualEnergy(DaoArtifactRules.SkillCost(skill,SkillDaoRoute(player,skill)));
    public static DownedBossSystem.EndgameRoute SkillDaoRoute(Player player, ArtifactSkill skill) => HeldSkill(player.HeldItem) == skill
        && (skill != ArtifactSkill.WardGuard || HasAwakenedWardSeal(player)) ? RefinedArtifact.ActiveDaoRoute(player.HeldItem) : DownedBossSystem.EndgameRoute.None;
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
    private static bool EndgameRealmAllowed(Player player) => player.GetModPlayer<XianXiaPlayer>().cultivationStage >= CultivationStage.Tribulation
        && player.GetModPlayer<XianXiaPlayer>().cultivationStage <= CultivationStage.DaoSevering
        && CultivationRules.GetWorldFailure(CultivationStage.Tribulation,Main.hardMode,NPC.downedPlantBoss,NPC.downedGolemBoss,NPC.downedMoonlord).Length == 0;
    private static bool HeavenLawRealmAllowed(Player player) => player.GetModPlayer<XianXiaPlayer>().cultivationStage >= CultivationStage.SpiritSevering
        && player.GetModPlayer<XianXiaPlayer>().cultivationStage <= CultivationStage.DaoSevering
        && CultivationRules.GetWorldFailure(CultivationStage.SpiritSevering,Main.hardMode,NPC.downedPlantBoss,NPC.downedGolemBoss,NPC.downedMoonlord).Length == 0;
    private static bool NascentRealmAllowed(Player player) => player.GetModPlayer<XianXiaPlayer>().cultivationStage >= CultivationStage.NascentSoul
        && player.GetModPlayer<XianXiaPlayer>().cultivationStage <= CultivationStage.DaoSevering
        && CultivationRules.GetWorldFailure(CultivationStage.NascentSoul,Main.hardMode,NPC.downedPlantBoss,NPC.downedGolemBoss,NPC.downedMoonlord).Length == 0;
    private static bool HammerRealmAllowed(Player player) => player.GetModPlayer<XianXiaPlayer>().cultivationStage >= CultivationStage.QiCondensation
        && player.GetModPlayer<XianXiaPlayer>().cultivationStage <= CultivationStage.DaoSevering;
    public static bool HasAwakenedWardSeal(Player player) => player.HeldItem.stack == 1
        && player.HeldItem.ModItem?.Name == "HeavenTabletWardSeal" && RefinedArtifact.IsAwakened(player.HeldItem)
        && player.GetModPlayer<XianXiaPlayer>().cultivationStage >= CultivationStage.SpiritSevering
        && player.GetModPlayer<XianXiaPlayer>().cultivationStage <= CultivationStage.DaoSevering
        && CultivationRules.GetWorldFailure(CultivationStage.SpiritSevering, Main.hardMode,
            NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord).Length == 0;
    public static bool HasWard(Player player)
    {
        for (int slot = 3; slot < Math.Min(player.armor.Length, 10); slot++)
            if (player.IsItemSlotUnlockedAndUsable(slot) && player.armor[slot].type == ModContent.ItemType<LightningWardJade>() && !player.armor[slot].IsAir) return true;
        return false;
    }
    // A null result means the current replicated prerequisites allow a request; aim and terrain are checked when casting.
    public static string UnavailableReason(Player player, ArtifactSkill skill) => EvaluateReadiness(player,skill,out _);
    private static string EvaluateReadiness(Player player, ArtifactSkill skill, out List<Player> healTargets)
    {
        healTargets = null;
        if (!ArtifactSkillRules.IsValid(skill) || player == null || !player.active || player.dead || player.noItems || player.CCed)
            return "Mods.XianXia.Skills.Unavailable";
        Item item = player.HeldItem;
        bool eligible = skill == ArtifactSkill.WardGuard ? HasWard(player) || HasAwakenedWardSeal(player)
            : item.stack == 1 && HeldSkill(item) == skill && RefinedArtifact.IsAwakened(item)
                && (skill != ArtifactSkill.HammerVolley || HammerRealmAllowed(player))
                && (skill is not (ArtifactSkill.SectVolley or ArtifactSkill.CauldronBloom) || NascentRealmAllowed(player))
                && (skill != ArtifactSkill.HeavenLawVolley || HeavenLawRealmAllowed(player))
                && (skill is not (ArtifactSkill.CalamityVolley or ArtifactSkill.CodexConstellation) || EndgameRealmAllowed(player));
        if (!eligible) return "Mods.XianXia.Skills.Unavailable";
        var state = player.GetModPlayer<XianXiaPlayer>();
        if (state.activeSkillCooldown > 0) return "Mods.XianXia.Skills.CoolingDown";
        if (skill == ArtifactSkill.ArrayDischarge && FindThunderArray(player) == null) return "Mods.XianXia.Skills.NeedThunderArray";
        if (IsBurst(skill) && Main.maxProjectiles - CountActive() < (skill is ArtifactSkill.WheelFormation or ArtifactSkill.SectVolley or ArtifactSkill.CalamityVolley ? 5 : 3)) return "Mods.XianXia.Skills.Capacity";
        if (skill is ArtifactSkill.ArrayPulse or ArtifactSkill.CauldronBloom) {
            bool cauldron = skill == ArtifactSkill.CauldronBloom;
            Projectile field = cauldron ? FindCauldron(player) : FindArray(player);
            if (field == null) return cauldron ? "Mods.XianXia.Skills.NeedCauldron" : "Mods.XianXia.Skills.NeedArray";
            int radius = cauldron ? 200 : 160;
            healTargets = new List<Player>();
            foreach (Player target in Main.ActivePlayers)
                if (!target.dead && target.statLife > 0 && target.statLife < target.statLifeMax2 && Finite(target.Center) && Friendly(player,target)
                    && Vector2.DistanceSquared(target.Center,field.Center) <= radius * radius) healTargets.Add(target);
            if (healTargets.Count == 0) return "Mods.XianXia.Skills.NoHealing";
        }
        return state.CanConsumeSpiritualEnergy(DaoArtifactRules.SkillCost(skill,SkillDaoRoute(player,skill))) ? null : "Mods.XianXia.Skills.NeedEnergy";
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
        if (item.type != type) { Reply(player,"Mods.XianXia.Skills.Unavailable"); return; }
        string reason = EvaluateReadiness(player,skill,out List<Player> healTargets);
        if (reason != null) { Reply(player,reason); return; }
        CultivationSnapshot before = state.CaptureSnapshot();
        var daoRoute = SkillDaoRoute(player,skill);
        if (!state.TryConsumeSpiritualEnergy(DaoArtifactRules.SkillCost(skill,daoRoute))) { Reply(player, "Mods.XianXia.Skills.NeedEnergy"); return; }
        if (IsBurst(skill) && !Burst(player, item, aim)) {
            state.ApplySnapshot(before); state.SyncPlayer(player.whoAmI, -1, false); return;
        }
        if (skill is ArtifactSkill.ArrayPulse or ArtifactSkill.CauldronBloom) foreach (Player target in healTargets) {
            AuthoritativeHealing.Apply(target,skill == ArtifactSkill.CauldronBloom ? DaoArtifactRules.CauldronHeal(daoRoute) : DaoArtifactRules.PulseHeal(daoRoute));
        }
        if (IsBurst(skill) && daoRoute == DownedBossSystem.EndgameRoute.RebuildHeaven) AuthoritativeHealing.Apply(player,20);
        if (skill == ArtifactSkill.WardGuard) {
            if (daoRoute == DownedBossSystem.EndgameRoute.RebuildHeaven) AuthoritativeHealing.Apply(player,20);
            state.wardGuardTimer = 180;
            player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.ArtifactWardBuff>(), 2);
        }
        if (daoRoute == DownedBossSystem.EndgameRoute.AcceptStarAbyss)
            state.spiritPressure = Math.Min(100,state.spiritPressure + 8);
        state.activeSkillCooldown = ArtifactSkillRules.Cooldown(skill); state.AdvanceResourceRevision();
        state.SyncPlayer(-1, -1, false);
        Reply(player, "Mods.XianXia.Skills.Success");
    }
    private static bool Friendly(Player owner, Player target) => owner == target || !owner.hostile || !target.hostile || (owner.team > 0 && owner.team == target.team);
    private static bool IsBurst(ArtifactSkill skill) => skill is ArtifactSkill.SwordBurst or ArtifactSkill.MoonCrescent or ArtifactSkill.WheelFormation or ArtifactSkill.HammerVolley or ArtifactSkill.ThunderVolley or ArtifactSkill.FlameFan or ArtifactSkill.WoodgrainRecall or ArtifactSkill.SpiritwoodVolley or ArtifactSkill.StarSplitVolley or ArtifactSkill.SectVolley or ArtifactSkill.HeavenLawVolley or ArtifactSkill.CalamityVolley or ArtifactSkill.CodexConstellation or ArtifactSkill.DecreeTriad or ArtifactSkill.ArrayDischarge;
    private static Projectile FindArray(Player player)
    {
        int type = ModContent.ProjectileType<GreenwoodArrayField>();
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile p = Main.projectile[i];
            if (p != null && p.active && p.owner == player.whoAmI && p.type == type
                && p.ModProjectile is GreenwoodArrayField field && field.CanPulse(player)) return p;
        }
        return null;
    }
    private static Projectile FindCauldron(Player player)
    {
        int type = ModContent.ProjectileType<MedicineCauldronField>();
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile p = Main.projectile[i];
            if (p != null && p.active && p.owner == player.whoAmI && p.type == type
                && p.ModProjectile is MedicineCauldronField field && field.CanBloom(player)) return p;
        }
        return null;
    }
    private static Projectile FindThunderArray(Player player)
    {
        int type = ModContent.ProjectileType<ThunderTalismanArray>();
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile p = Main.projectile[i];
            if (p != null && p.active && p.owner == player.whoAmI && p.type == type
                && p.ModProjectile is ThunderTalismanArray field && field.CanDischarge(player)) return p;
        }
        return null;
    }
    private static int CountActive() { int count = 0; for (int i = 0; i < Main.maxProjectiles; i++) if (Main.projectile[i].active) count++; return count; }
    private static bool Burst(Player player, Item item, Vector2 aim)
    {
        if (HeldSkill(item) == ArtifactSkill.ArrayDischarge) return ArrayDischargeBurst(player,item);
        if (HeldSkill(item) == ArtifactSkill.DecreeTriad) return DecreeBurst(player,item,aim);
        if (HeldSkill(item) == ArtifactSkill.ThunderVolley) return ThunderBurst(player,item,aim);
        Vector2 position = player.RotatedRelativePoint(player.MountedCenter), delta = aim - position;
        if (!Finite(position) || !Finite(delta) || !float.IsFinite(delta.LengthSquared())) return false;
        bool codex = HeldSkill(item) == ArtifactSkill.CodexConstellation;
        bool calamity = HeldSkill(item) == ArtifactSkill.CalamityVolley;
        bool heavenLaw = HeldSkill(item) == ArtifactSkill.HeavenLawVolley;
        bool sect = HeldSkill(item) == ArtifactSkill.SectVolley;
        bool starSplit = HeldSkill(item) == ArtifactSkill.StarSplitVolley;
        bool spiritwood = HeldSkill(item) == ArtifactSkill.SpiritwoodVolley;
        bool woodgrain = HeldSkill(item) == ArtifactSkill.WoodgrainRecall;
        bool flame = HeldSkill(item) == ArtifactSkill.FlameFan;
        bool moon = HeldSkill(item) == ArtifactSkill.MoonCrescent;
        bool wheel = HeldSkill(item) == ArtifactSkill.WheelFormation;
        bool hammer = HeldSkill(item) == ArtifactSkill.HammerVolley;
        Vector2 velocity = delta.SafeNormalize(Vector2.UnitX * player.direction) * (codex ? 10f : calamity ? 18f : heavenLaw ? 20f : sect ? 18f : starSplit ? 14f : spiritwood ? 16f : woodgrain ? 12f : hammer ? 9f : 14f);
        int damage = codex ? Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.CodexBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)))) : calamity ? Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.CalamityBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)))) : heavenLaw ? Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.HeavenLawBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)))) : sect ? Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.SectBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)))) : starSplit ? Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.StarSplitBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)))) : spiritwood ? Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.SpiritwoodBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)))) : woodgrain ? Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.WoodgrainBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)))) : flame ? Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.FlameBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item)))) : hammer ? (int)(player.GetWeaponDamage(item) * DaoArtifactRules.HammerBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item))) : wheel ? (int)(player.GetWeaponDamage(item) * DaoArtifactRules.WheelBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item))) : moon ? (int)(player.GetWeaponDamage(item) * DaoArtifactRules.MoonBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item))) : player.GetWeaponDamage(item) * DaoArtifactRules.BurstMultiplier(RefinedArtifact.ActiveDaoRoute(item));
        int type = codex ? ModContent.ProjectileType<ArchiveStarOrb>() : calamity ? ModContent.ProjectileType<StarCalamityMechanismBolt>() : heavenLaw ? ModContent.ProjectileType<HeavenLawBolt>() : sect ? ModContent.ProjectileType<SectMechanismBolt>() : starSplit ? ModContent.ProjectileType<StarEclipseSplitBolt>() : spiritwood ? ModContent.ProjectileType<SpiritBoltProjectile>() : woodgrain ? ModContent.ProjectileType<WoodgrainSwordProjectile>() : flame ? ModContent.ProjectileType<CinnabarTalismanFlame>() : hammer ? ModContent.ProjectileType<FurnaceHammerProjectile>() : wheel ? ModContent.ProjectileType<FormlessSwordWheelProjectile>() : moon ? ModContent.ProjectileType<MoonboneShardProjectile>() : ModContent.ProjectileType<CloudpiercerSwordProjectile>();
        var created = new List<int>();
        var source = new EntitySource_ItemUse_WithAmmo(player, item, 0);
        int halfCount = wheel || sect || calamity ? 2 : 1;
        for (int i = -halfCount; i <= halfCount; i++) {
            int index = Projectile.NewProjectile(source, position, velocity.RotatedBy(i * (codex ? 0.24 : calamity ? 0.18 : heavenLaw ? 0.08 : sect ? 0.1 : starSplit ? 0.2 : spiritwood ? 0.12 : woodgrain ? 0.28 : hammer ? 0.24 : wheel ? 0.3 : 0.18)), type, damage, player.GetWeaponKnockback(item, item.knockBack), player.whoAmI);
            if (index < 0 || index >= Main.maxProjectiles) {
                RemoveCreated(created);
                return false;
            }
            created.Add(index);
        }
        PublishCreated(created);
        return true;
    }
    private static bool ArrayDischargeBurst(Player player,Item item)
    {
        Projectile field = FindThunderArray(player);
        if (field == null) return false;
        var positions = new Vector2[3];
        for (int side = -1; side <= 1; side++) {
            Vector2 spawn = field.Center + new Vector2(side * 64,-220);
            if (spawn.X < 12 || spawn.Y < 40 || spawn.X > Main.maxTilesX * 16f - 12 || spawn.Y > Main.maxTilesY * 16f - 40
                || Collision.SolidCollision(spawn - new Vector2(12,40),24,80)
                || !Collision.CanHitLine(spawn,1,1,field.Center + new Vector2(side * 64,0),1,1)) return false;
            positions[side + 1] = spawn;
        }
        int damage = Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.ArrayDischargeMultiplier(RefinedArtifact.ActiveDaoRoute(item))));
        var created = new List<int>();
        foreach (Vector2 spawn in positions) {
            int index = Projectile.NewProjectile(field.GetSource_FromAI(),spawn,Vector2.UnitY * 13,
                ModContent.ProjectileType<MinorThunderboltProjectile>(),damage,player.GetWeaponKnockback(item,item.knockBack),player.whoAmI);
            if (index < 0 || index >= Main.maxProjectiles) { RemoveCreated(created); return false; }
            created.Add(index);
        }
        PublishCreated(created); return true;
    }

    private static bool DecreeBurst(Player player,Item item,Vector2 aim)
    {
        Vector2 position = player.RotatedRelativePoint(player.MountedCenter), delta = aim - position;
        if (!Finite(position) || !Finite(delta) || !float.IsFinite(delta.LengthSquared())) return false;
        Vector2 focus = position + delta.SafeNormalize(Vector2.UnitX * player.direction) * Math.Min(520f,MathF.Sqrt(delta.LengthSquared()));
        var positions = new Vector2[3];
        for (int side = -1; side <= 1; side++) {
            Vector2 spawn = focus + new Vector2(side * 80,0);
            if (spawn.X < 20 || spawn.Y < 80 || spawn.X > Main.maxTilesX * 16f - 20 || spawn.Y > Main.maxTilesY * 16f - 80
                || Vector2.DistanceSquared(position,spawn) > 600 * 600 || !Collision.CanHitLine(position,1,1,spawn,1,1)
                || Collision.SolidCollision(spawn - new Vector2(20,80),40,160)) return false;
            positions[side + 1] = spawn;
        }
        int damage = Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.DecreeBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item))));
        var created = new List<int>(); var source = new EntitySource_ItemUse_WithAmmo(player,item,0);
        foreach (Vector2 spawn in positions) {
            int index = Projectile.NewProjectile(source,spawn,Vector2.Zero,ModContent.ProjectileType<DecreeJudgementBeam>(),damage,
                player.GetWeaponKnockback(item,item.knockBack),player.whoAmI);
            if (index < 0 || index >= Main.maxProjectiles) { RemoveCreated(created); return false; }
            created.Add(index);
        }
        PublishCreated(created); return true;
    }

    private static bool ThunderBurst(Player player,Item item,Vector2 aim)
    {
        Vector2 position = player.RotatedRelativePoint(player.MountedCenter), delta = aim - position;
        float worldWidth = Main.maxTilesX * 16f, worldHeight = Main.maxTilesY * 16f;
        float horizontalMargin = ArtifactSkillRules.ThunderSpacing + 28f, verticalMinimum = ArtifactSkillRules.ThunderDrop + 56f;
        if (!Finite(position) || !Finite(delta) || !float.IsFinite(delta.LengthSquared()) || position.X < 0 || position.Y < 0
            || position.X >= worldWidth || position.Y >= worldHeight || worldWidth < horizontalMargin * 2 || worldHeight < verticalMinimum + 16) return false;
        Vector2 focus = position + delta.SafeNormalize(Vector2.UnitX * player.direction) * Math.Min(ArtifactSkillRules.ThunderRange,MathF.Sqrt(delta.LengthSquared()));
        focus.X = Math.Clamp(focus.X,horizontalMargin,worldWidth - horizontalMargin); focus.Y = Math.Clamp(focus.Y,verticalMinimum,worldHeight - 16);
        if (Vector2.DistanceSquared(position,focus) > ArtifactSkillRules.ThunderRange * ArtifactSkillRules.ThunderRange
            || !Collision.CanHitLine(position,1,1,focus,1,1)) return false;
        var positions = new Vector2[3];
        for (int i = -1; i <= 1; i++) {
            Vector2 spawn = focus + new Vector2(i * ArtifactSkillRules.ThunderSpacing,-ArtifactSkillRules.ThunderDrop);
            if (Collision.SolidCollision(spawn - new Vector2(12,40),24,80) || !Collision.CanHitLine(spawn,1,1,focus + new Vector2(i * ArtifactSkillRules.ThunderSpacing,0),1,1)) return false;
            positions[i + 1] = spawn;
        }
        int damage = Math.Max(1,(int)(player.GetWeaponDamage(item) * DaoArtifactRules.ThunderBurstMultiplier(RefinedArtifact.ActiveDaoRoute(item))));
        var created = new List<int>(); var source = new EntitySource_ItemUse_WithAmmo(player,item,0);
        foreach (Vector2 spawn in positions) {
            int index = Projectile.NewProjectile(source,spawn,Vector2.UnitY * 12,ModContent.ProjectileType<ThunderCaseStrikeProjectile>(),damage,
                player.GetWeaponKnockback(item,item.knockBack),player.whoAmI);
            if (index < 0 || index >= Main.maxProjectiles) { RemoveCreated(created); return false; }
            created.Add(index);
        }
        PublishCreated(created); return true;
    }
    private static void RemoveCreated(List<int> created)
    {
        foreach (int index in created) {
            Projectile projectile = Main.projectile[index]; projectile.damage = 0; projectile.active = false;
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.KillProjectile,-1,-1,null,projectile.identity,projectile.owner);
        }
    }
    private static void PublishCreated(List<int> created)
    {
        foreach (int index in created) {
            Main.projectile[index].netUpdate = true;
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncProjectile,-1,-1,null,index);
        }
    }
    private static void Reply(Player player, string key)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(Language.GetTextValue(key), 120, 230, 210);
        else { var packet = ModContent.GetInstance<global::XianXia.XianXia>().GetPacket(); packet.Write((byte)7); NetworkText.FromKey(key).Serialize(packet); packet.Send(player.whoAmI); }
    }
}
