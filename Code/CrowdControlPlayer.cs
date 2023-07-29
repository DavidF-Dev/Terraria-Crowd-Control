using System;
using CrowdControlMod.Config;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CrowdControlMod;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlPlayer : ModPlayer
{
    #region Delegates

    /// <inheritdoc cref="PreKill" />
    public delegate void PreKillDelegate(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);

    /// <inheritdoc cref="Kill" />
    public delegate void KillDelegate(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource);

    /// <inheritdoc cref="CanBeHitByNPC" />
    public delegate bool CanBeHitByNPCDelegate(NPC npc, ref int cooldownSlot);

    /// <inheritdoc cref="CanBeHitByProjectile" />
    public delegate bool CanBeHitByProjectileDelegate(Projectile projectile);

    /// <inheritdoc cref="ModifyHurt" />
    public delegate void ModifyHurtDelegate(ref Player.HurtModifiers modifiers);

    /// <inheritdoc cref="CanConsumeAmmo" />
    public delegate bool CanConsumeAmmoDelegate(Item weapon, Item ammo);

    /// <inheritdoc cref="Shoot" />
    public delegate bool ShootDelegate(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback);

    /// <inheritdoc cref="ModifyWeaponKnockback" />
    public delegate void ModifyWeaponKnockbackDelegate(Item item, ref StatModifier knockback);
    
    /// <inheritdoc cref="ModifyDrawInfo" />
    public delegate void ModifyDrawInfoDelegate(ref PlayerDrawSet drawInfo);

    /// <inheritdoc cref="HideDrawLayers" />
    public delegate void HideDrawLayersDelegate(PlayerDrawSet drawInfo);

    #endregion

    #region Fields

    /// <summary>
    ///     First time using Crowd Control with this player.
    /// </summary>
    public bool IsFirstTimeUser = true;

    /// <summary>
    ///     Server-side value for whether this player has tombstones disabled in their config.
    /// </summary>
    public bool ServerDisableTombstones;

    /// <summary>
    ///     Server-side value for whether this player wants bosses to be despawned in their config.
    /// </summary>
    public bool ServerForcefullyDespawnBosses;

    #endregion

    #region Properties

    /// <summary>
    ///     Whether tombstones are disabled for this player. Correct for client and server.
    /// </summary>
    public bool DisableTombstones => NetUtils.IsServer ? ServerDisableTombstones : CrowdControlConfig.GetInstance().DisableTombstones;

    /// <summary>
    ///     Whether effect bosses should be despawned if all players are dead. Correct for client and server.
    /// </summary>
    public bool DespawnForcefullyBoss => NetUtils.IsServer ? ServerForcefullyDespawnBosses : CrowdControlConfig.GetInstance().ForceDespawnBosses;

    #endregion

    #region Events

    /// <inheritdoc cref="PlayerDisconnect" />
    public static event Action<Player>? PlayerDisconnectHook;

    /// <inheritdoc cref="OnRespawn" />
    public event Action? OnRespawnHook;

    /// <inheritdoc cref="PreKill" />
    public event PreKillDelegate? PreKillHook;

    /// <inheritdoc cref="Kill" />
    public event KillDelegate? KillHook;

    /// <inheritdoc cref="CanBeHitByNPC" />
    public event CanBeHitByNPCDelegate? CanBeHitByNPCHook;

    /// <inheritdoc cref="CanBeHitByProjectile" />
    public event CanBeHitByProjectileDelegate? CanBeHitByProjectileHook;

    /// <inheritdoc cref="ModifyHurt" />
    public event ModifyHurtDelegate? ModifyHurtHook;
    
    /// <inheritdoc cref="CanConsumeAmmo" />
    public event CanConsumeAmmoDelegate? CanConsumeAmmoHook;

    /// <inheritdoc cref="PreUpdateBuffs" />
    public event Action? PreUpdateBuffsHook;

    /// <inheritdoc cref="PostUpdateEquips" />
    public event Action? PostUpdateEquipsHook;

    /// <inheritdoc cref="PostUpdateRunSpeeds" />
    public event Action? PostUpdateRunSpeedsHook;

    /// <inheritdoc cref="PostUpdate" />
    public event Action? PostUpdateHook;

    /// <inheritdoc cref="Shoot" />
    public event ShootDelegate? ShootHook;

    /// <inheritdoc cref="ModifyWeaponKnockback" />
    public event ModifyWeaponKnockbackDelegate? ModifyWeaponKnockbackHook;

    /// <inheritdoc cref="ModifyDrawInfo" />
    public event ModifyDrawInfoDelegate? ModifyDrawInfoHook;

    /// <inheritdoc cref="HideDrawLayers" />
    public event HideDrawLayersDelegate? HideDrawLayersHook;

    /// <inheritdoc cref="ModifyScreenPosition" />
    public event Action? ModifyScreenPositionHook;

    #endregion

    #region Methods

    public override void OnEnterWorld()
    {
        // Start the crowd control session upon entering a world
        CrowdControlMod.GetInstance().StartCrowdControlSession();
    }

    public override void PlayerDisconnect()
    {
        if (Main.myPlayer == Player.whoAmI)
        {
            // Stop the crowd control session upon disconnecting from a server
            CrowdControlMod.GetInstance().StopCrowdControlSession();
        }

        PlayerDisconnectHook?.Invoke(Player);
    }

    public override void OnRespawn()
    {
        OnRespawnHook?.Invoke();
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
    {
        PreKillHook?.Invoke(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
        return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        KillHook?.Invoke(damage, hitDirection, pvp, damageSource);
    }

    public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
    {
        return CanBeHitByNPCHook?.Invoke(npc, ref cooldownSlot) ?? base.CanBeHitByNPC(npc, ref cooldownSlot);
    }

    public override bool CanBeHitByProjectile(Projectile proj)
    {
        return CanBeHitByProjectileHook?.Invoke(proj) ?? base.CanBeHitByProjectile(proj);
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        ModifyHurtHook?.Invoke(ref modifiers);
    }

    public override bool CanConsumeAmmo(Item weapon, Item ammo)
    {
        return CanConsumeAmmoHook?.Invoke(weapon, ammo) ?? base.CanConsumeAmmo(weapon, ammo);
    }

    public override void PreUpdateBuffs()
    {
        PreUpdateBuffsHook?.Invoke();
    }

    public override void PostUpdateEquips()
    {
        PostUpdateEquipsHook?.Invoke();
    }

    public override void PostUpdateRunSpeeds()
    {
        PostUpdateRunSpeedsHook?.Invoke();
    }

    public override void PostUpdate()
    {
        PostUpdateHook?.Invoke();
    }

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return ShootHook?.Invoke(item, source, position, velocity, type, damage, knockback) ?? base.Shoot(item, source, position, velocity, type, damage, knockback);
    }

    public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback)
    {
        ModifyWeaponKnockbackHook?.Invoke(item, ref knockback);
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        ModifyDrawInfoHook?.Invoke(ref drawInfo);
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        HideDrawLayersHook?.Invoke(drawInfo);
    }

    public override void ModifyScreenPosition()
    {
        ModifyScreenPositionHook?.Invoke();
    }

    public override void SaveData(TagCompound tag)
    {
        tag.Add("IsFirstTimeUser", IsFirstTimeUser);
    }

    public override void LoadData(TagCompound tag)
    {
        IsFirstTimeUser = !tag.ContainsKey("IsFirstTimeUser") || tag.GetBool("IsFirstTimeUser");
    }

    #endregion
}