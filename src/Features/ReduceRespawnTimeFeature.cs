using CrowdControlMod.Config;
using Terraria.DataStructures;

namespace CrowdControlMod.Features;

/// <summary>
///     Reduce the player respawn timer if enabled in the config.
/// </summary>
public sealed class ReduceRespawnTimeFeature : IFeature
{
    #region Static Methods

    private static void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        // Reduce the respawn timer by the mod configuration factor
        var player = CrowdControlMod.GetInstance().GetLocalPlayer();
        player.Player.respawnTimer = (int)(player.Player.respawnTimer * CrowdControlConfig.GetInstance().RespawnTimeFactor);
    }

    #endregion

    #region Methods

    public void SessionStarted()
    {
        CrowdControlMod.GetInstance().GetLocalPlayer().KillHook += Kill;
    }

    public void SessionStopped()
    {
        CrowdControlMod.GetInstance().GetLocalPlayer().KillHook -= Kill;
    }

    public void Dispose()
    {
    }

    #endregion
}