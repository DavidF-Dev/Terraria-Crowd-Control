using System;
using CrowdControlMod.Config;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlPlayer : ModPlayer
{
    #region Delegates

    /// <inheritdoc cref="CanBeHitByNPC" />
    public delegate bool CanBeHitByNpcDelegate(NPC npc, ref int cooldownSlot);

    /// <inheritdoc cref="CanBeHitByProjectile" />
    public delegate bool CanBeHitByProjectileDelegate(Projectile projectile);

    /// <inheritdoc cref="CanConsumeAmmo" />
    public delegate bool CanConsumeAmmoDelegate(Item weapon, Item ammo);

    /// <inheritdoc cref="Shoot" />
    [PublicAPI]
    public delegate bool ShootDelegate(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback);

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
    [PublicAPI]
    public bool IsLocalPlayer => PlayerUtils.IsLocalPlayer(this);

    [PublicAPI]
    public int TileX => (int)(Player.position.X / 16);

    [PublicAPI]
    public int TileY => (int)(Player.position.Y / 16);

    /// <summary>
    ///     Whether tombstones are disabled for this player. Correct for client and server.
    /// </summary>
    public bool DisableTombstones => Main.netMode == NetmodeID.Server ? ServerDisableTombstones : CrowdControlConfig.GetInstance().DisableTombstones;

    #endregion

    #region Events

    /// <inheritdoc cref="PlayerDisconnect" />
    [PublicAPI]
    public static event Action<Player> PlayerDisconnectHook;

    /// <inheritdoc cref="OnRespawn" />
    [PublicAPI]
    public event Action OnRespawnHook;

    /// <inheritdoc cref="CanBeHitByNPC" />
    [PublicAPI]
    public event CanBeHitByNpcDelegate CanBeHitByNpcHook;

    /// <inheritdoc cref="CanBeHitByProjectile" />
    [PublicAPI]
    public event CanBeHitByProjectileDelegate CanBeHitByProjectileHook;

    /// <inheritdoc cref="CanConsumeAmmo" />
    [PublicAPI]
    public event CanConsumeAmmoDelegate CanConsumeAmmoHook;

    /// <inheritdoc cref="PreUpdateBuffs" />
    [PublicAPI]
    public event Action PreUpdateBuffsHook;

    /// <inheritdoc cref="PostUpdateEquips" />
    [PublicAPI]
    public event Action PostUpdateEquipsHook;

    /// <inheritdoc cref="PostUpdateRunSpeeds" />
    [PublicAPI]
    public event Action PostUpdateRunSpeedsHook;

    /// <inheritdoc cref="PostUpdate" />
    [PublicAPI]
    public event Action PostUpdateHook;

    /// <inheritdoc cref="Shoot" />
    [PublicAPI]
    public event ShootDelegate ShootHook;

    #endregion

    #region Methods

    public override void OnEnterWorld(Player player)
    {
        if (IsLocalPlayer)
        {
            // Start the crowd control session upon entering a world
            CrowdControlMod.GetInstance().StartCrowdControlSession(this);
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
        if (IsLocalPlayer && CrowdControlMod.GetInstance().IsSessionActive)
        {
            // Reduce the respawn timer by the mod configuration factor
            Player.respawnTimer = (int)(Player.respawnTimer * CrowdControlConfig.GetInstance().RespawnTimeFactor);
        }
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

    #endregion
}