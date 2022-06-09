using System;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlPlayer : ModPlayer
{
    #region Events

    /// <inheritdoc cref="OnRespawn" />
    public event Action<CrowdControlPlayer> OnRespawnHook;

    /// <inheritdoc cref="PreUpdateBuffs" />
    public event Action PreUpdateBuffsHook; 

    #endregion

    /// <summary>
    ///     Is this player instance the local player / client?
    /// </summary>
    [PublicAPI]
    public bool IsLocalPlayer => TerrariaUtils.IsLocalPlayer(this);

    #region Methods

    public override void OnEnterWorld(Player player)
    {
        if (IsLocalPlayer)
        {
            // Start the crowd control session upon entering a world
            CrowdControlMod.GetInstance().StartCrowdControlSession(this);
        }
        
        base.OnEnterWorld(player);
    }

    public override void PlayerDisconnect(Player player)
    {
        if (IsLocalPlayer)
        {
            // Stop the crowd control session upon disconnecting from a server
            CrowdControlMod.GetInstance().StopCrowdControlSession();
        }
        
        base.PlayerDisconnect(player);
    }

    public override void OnRespawn(Player player)
    {
        OnRespawnHook?.Invoke(this);
        base.OnRespawn(player);
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (IsLocalPlayer && CrowdControlMod.GetInstance().IsSessionActive)
        {
            // Reduce the respawn timer by the mod configuration factor
            Player.respawnTimer = (int)(Player.respawnTimer * CrowdControlConfig.GetInstance().RespawnTimeFactor);
        }
        
        base.Kill(damage, hitDirection, pvp, damageSource);
    }

    public override void PreUpdateBuffs()
    {
        PreUpdateBuffsHook?.Invoke();
        base.PreUpdateBuffs();
    }

    #endregion
}