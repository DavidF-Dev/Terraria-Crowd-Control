using System;
using CrowdControlMod.Config;
using On.Terraria;

namespace CrowdControlMod.Features;

/// <summary>
///     Allow the player to teleport to other players without a wormhole potion (unity).
/// </summary>
public sealed class PlayerTeleportationFeature : IFeature
{
    #region Fields

    private DateTime _nextUsageTime = DateTime.MinValue;
    private bool _teleported;

    #endregion

    #region Properties

    /// <summary>
    ///     Feature is allowed to be used.
    /// </summary>
    private static bool IsAllowed => CrowdControlConfig.GetInstance().AllowPlayerTeleportation;

    /// <summary>
    ///     Cooldown, in minutes, between feature usages.
    /// </summary>
    private static float Cooldown => CrowdControlConfig.GetInstance().PlayerTeleportationCooldown;

    #endregion

    #region Methods

    public void SessionStarted()
    {
        Player.HasUnityPotion += HasUnityPotion;
        Player.TakeUnityPotion += TakeUnityPotion;
    }

    public void SessionStopped()
    {
        _nextUsageTime = DateTime.MinValue;
        _teleported = false;
        Player.HasUnityPotion -= HasUnityPotion;
        Player.TakeUnityPotion -= TakeUnityPotion;
    }

    public void Dispose()
    {
    }

    private bool HasUnityPotion(Player.orig_HasUnityPotion orig, Terraria.Player self)
    {
        if (CrowdControlMod.GetLocalPlayer().Player != self || !IsAllowed || DateTime.Now < _nextUsageTime)
        {
            return orig.Invoke(self);
        }

        // Allow the player to teleport if enabled in the configuration and cooldown has expired
        _nextUsageTime = DateTime.Now.AddMinutes(Cooldown);
        _teleported = true;
        return true;
    }

    private void TakeUnityPotion(Player.orig_TakeUnityPotion orig, Terraria.Player self)
    {
        // Do not take the potion if a free teleportation was used
        if (CrowdControlMod.GetLocalPlayer().Player == self && _teleported)
        {
            _teleported = false;
            return;
        }

        orig.Invoke(self);
    }

    #endregion
}