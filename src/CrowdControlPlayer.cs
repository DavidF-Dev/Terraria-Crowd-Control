using System;
using CrowdControlMod.Config;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlPlayer : ModPlayer
{
    #region Delegates

    /// <inheritdoc cref="Kill" />
    public delegate void KillDelegate(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource);

    /// <inheritdoc cref="CanBeHitByNPC" />
    public delegate bool CanBeHitByNpcDelegate(NPC npc, ref int cooldownSlot);

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

    #endregion

    #region Properties

    /// <summary>
    ///     Is this player instance the local player / client?
    /// </summary>
    public bool IsLocalPlayer => Player.whoAmI == Main.myPlayer;

    public int TileX => (int)(Player.position.X / 16);

    public int TileY => (int)(Player.position.Y / 16);

    public int CenterTileX => (int)(Player.Center.X / 16);

    public int CenterTileY => (int)(Player.Center.Y / 16);

    /// <summary>
    ///     Whether tombstones are disabled for this player. Correct for client and server.
    /// </summary>
    public bool DisableTombstones => Main.netMode == NetmodeID.Server ? ServerDisableTombstones : CrowdControlConfig.GetInstance().DisableTombstones;

    #endregion

    #region Events

    /// <inheritdoc cref="PlayerDisconnect" />
    public static event Action<Player>? PlayerDisconnectHook;

    /// <inheritdoc cref="OnRespawn" />
    public event Action? OnRespawnHook;

    /// <inheritdoc cref="Kill" />
    public event KillDelegate? KillHook;

    /// <inheritdoc cref="CanBeHitByNPC" />
    public event CanBeHitByNpcDelegate? CanBeHitByNpcHook;

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

    #endregion

    #region Methods

    public override void OnEnterWorld(Player player)
    {
        if (IsLocalPlayer)
        {
            // Start the crowd control session upon entering a world
            CrowdControlMod.GetInstance().StartCrowdControlSession();
        }
    }

    public override void PlayerDisconnect(Player player)
    {
        if (IsLocalPlayer)
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
        return CanBeHitByNpcHook?.Invoke(npc, ref cooldownSlot) ?? base.CanBeHitByNPC(npc, ref cooldownSlot);
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

    #endregion
}