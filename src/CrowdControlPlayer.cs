using System;
using CrowdControlMod.Config;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlPlayer : ModPlayer
{
    #region Properties

    /// <summary>
    ///     Is this player instance the local player / client?
    /// </summary>
    [PublicAPI]
    public bool IsLocalPlayer => TerrariaUtils.IsLocalPlayer(this);

    [PublicAPI]
    public int TileX => (int)(Player.position.X / 16);

    [PublicAPI]
    public int TileY => (int)(Player.position.Y / 16);

    #endregion

    #region Events

    /// <inheritdoc cref="PlayerDisconnect" />
    [PublicAPI]
    public static event Action<Player> PlayerDisconnectHook;

    /// <inheritdoc cref="PreUpdateBuffs" />
    [PublicAPI]
    public event Action PreUpdateBuffsHook;

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

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (IsLocalPlayer && CrowdControlMod.GetInstance().IsSessionActive)
        {
            // Reduce the respawn timer by the mod configuration factor
            Player.respawnTimer = (int)(Player.respawnTimer * CrowdControlConfig.GetInstance().RespawnTimeFactor);
        }
    }

    public override void PreUpdateBuffs()
    {
        PreUpdateBuffsHook?.Invoke();
    }

    #endregion
}