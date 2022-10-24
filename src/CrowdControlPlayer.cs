using System;
using CrowdControlMod.Config;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CrowdControlMod;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlPlayer : ModPlayer
{
    #region Delegates

    /// <inheritdoc cref="Kill" />
    public delegate void KillDelegate(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource);

    /// <inheritdoc cref="CanBeHitByNPC" />
    public delegate bool CanBeHitByNPCDelegate(NPC npc, ref int cooldownSlot);

    /// <inheritdoc cref="CanBeHitByProjectile" />
    public delegate bool CanBeHitByProjectileDelegate(Projectile projectile);

    /// <inheritdoc cref="CanConsumeAmmo" />
    public delegate bool CanConsumeAmmoDelegate(Item weapon, Item ammo);

    /// <inheritdoc cref="Shoot" />
    public delegate bool ShootDelegate(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback);

    /// <inheritdoc cref="ModifyDrawInfo" />
    public delegate void ModifyDrawInfoDelegate(ref PlayerDrawSet drawInfo);

    #endregion

    #region Fields

    /// <summary>
    ///     Server-side value for whether this player has tombstones disabled in their config.
    /// </summary>
    public bool ServerDisableTombstones;

    /// <summary>
    ///     Server-side value for whether this player wants bosses to be despawned in their config.
    /// </summary>
    public bool ServerForcefullyDespawnBosses;

    /// <summary>
    ///     Number of life crystals (20HP) removed from the max hp.<br />
    ///     Used by <see cref="Utilities.PlayerUtils.AddStatLifeMax" />.
    /// </summary>
    public int LifeCrystalRemoved;

    #endregion

    #region Properties

    /// <summary>
    ///     Whether tombstones are disabled for this player. Correct for client and server.
    /// </summary>
    public bool DisableTombstones => Main.netMode == NetmodeID.Server ? ServerDisableTombstones : CrowdControlConfig.GetInstance().DisableTombstones;

    /// <summary>
    ///     Whether effect bosses should be despawned if all players are dead. Correct for client and server.
    /// </summary>
    public bool DespawnForcefullyBoss => Main.netMode == NetmodeID.Server ? ServerForcefullyDespawnBosses : CrowdControlConfig.GetInstance().ForceDespawnBosses;

    #endregion

    #region Events

    /// <inheritdoc cref="PlayerDisconnect" />
    public static event Action<Player>? PlayerDisconnectHook;

    /// <inheritdoc cref="OnRespawn" />
    public event Action? OnRespawnHook;

    /// <inheritdoc cref="Kill" />
    public event KillDelegate? KillHook;

    /// <inheritdoc cref="CanBeHitByNPC" />
    public event CanBeHitByNPCDelegate? CanBeHitByNPCHook;

    /// <inheritdoc cref="CanBeHitByProjectile" />
    public event CanBeHitByProjectileDelegate? CanBeHitByProjectileHook;

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

    /// <inheritdoc cref="ModifyDrawInfo" />
    public event ModifyDrawInfoDelegate? ModifyDrawInfoHook;

    /// <inheritdoc cref="ModifyScreenPosition" />
    public event Action? ModifyScreenPositionHook;

    #endregion

    #region Methods

    public override void OnEnterWorld(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            // Start the crowd control session upon entering a world
            CrowdControlMod.GetInstance().StartCrowdControlSession();
        }
    }

    public override void PlayerDisconnect(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            // Stop the crowd control session upon disconnecting from a server
            CrowdControlMod.GetInstance().StopCrowdControlSession();
        }

        PlayerDisconnectHook?.Invoke(player);
    }

    public override void OnRespawn(Player player)
    {
        OnRespawnHook?.Invoke();
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

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        ModifyDrawInfoHook?.Invoke(ref drawInfo);
    }

    public override void ModifyScreenPosition()
    {
        ModifyScreenPositionHook?.Invoke();
    }

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        base.ModifyMaxStats(out health, out mana);
        health.Base = -LifeCrystalRemoved * 20;
    }

    public override void PreSavePlayer()
    {
        // Ensure we a saving the player in such a way that it can be used again without this mod enabled
        while (LifeCrystalRemoved > 0 && Player.ConsumedLifeCrystals > 0)
        {
            LifeCrystalRemoved--;
            Player.ConsumedLifeCrystals--;
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag.Add("LifeCrystalRemoved", LifeCrystalRemoved);
    }

    public override void LoadData(TagCompound tag)
    {
        LifeCrystalRemoved = tag.GetInt("LifeCrystalRemoved");
    }

    #endregion
}