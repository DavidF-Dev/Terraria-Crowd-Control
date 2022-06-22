using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.DataStructures;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Forcefully kill the player instantly.
/// </summary>
public sealed class KillPlayerEffect : CrowdControlEffect
{
    #region Constructors

    public KillPlayerEffect() : base(EffectID.KillPlayer, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Succeed if the player is not invincible (the player is killed when the start message is sent)
        return !PlayerUtils.IsInvincible(GetLocalPlayer()) ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Retry;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        // Kill the player here
        // TODO: Choose a random death message
        GetLocalPlayer().Player.KillMe(PlayerDeathReason.ByCustomReason($"{playerString} was killed by {viewerString}"), 1000, 0);
    }

    #endregion
}