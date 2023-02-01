using System;
using CrowdControlMod.Config;
using Terraria;

namespace CrowdControlMod.Features;

/// <summary>
///     Allow the player to teleport to other players without a wormhole potion (unity).
/// </summary>
public sealed class PlayerTeleportationFeature : IFeature
{
    #region Fields

    private DateTime _nextUsageTime = DateTime.MinValue;
    private bool _canUseFreeTeleport;

    #endregion

    #region Properties

    /// <summary>
    ///     Feature is allowed to be used.
    /// </summary>
    private static bool IsAllowed => CrowdControlConfig.GetInstance().AllowPlayerTeleportation;

    /// <summary>
    ///     Cooldown, in minutes, between feature usages.
    /// </summary>
    private static float Cooldown => CrowdControlConfig.GetInstance().PlayerTeleportationCooldown / 60f;

    #endregion

    #region Methods

    public void SessionStarted()
    {
        On_Player.HasUnityPotion += HasUnityPotion;
        On_Player.TakeUnityPotion += TakeUnityPotion;
    }

    public void SessionStopped()
    {
        _nextUsageTime = DateTime.MinValue;
        _canUseFreeTeleport = false;
        On_Player.HasUnityPotion -= HasUnityPotion;
        On_Player.TakeUnityPotion -= TakeUnityPotion;
    }

    public void Dispose()
    {
    }

    private bool HasUnityPotion(On_Player.orig_HasUnityPotion orig, Player self)
    {
        if (CrowdControlMod.GetLocalPlayer().Player != self || !CrowdControlMod.GetInstance().IsSessionActive || !IsAllowed || DateTime.Now < _nextUsageTime)
        {
            _canUseFreeTeleport = false;
            return orig.Invoke(self);
        }

        // Allow the player to teleport if enabled in the configuration and cooldown has expired
        _canUseFreeTeleport = true;
        return true;
    }

    private void TakeUnityPotion(On_Player.orig_TakeUnityPotion orig, Player self)
    {
        // Do not take the potion if a free teleportation was used
        if (CrowdControlMod.GetLocalPlayer().Player == self && _canUseFreeTeleport)
        {
            // Consume the free teleport and trigger the cooldown
            _nextUsageTime = DateTime.Now.AddMinutes(Cooldown);
            _canUseFreeTeleport = false;
            return;
        }

        orig.Invoke(self);
    }

    #endregion
}