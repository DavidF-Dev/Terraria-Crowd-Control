using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.BuffEffects;

public sealed class IcyFeetEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float AccelerationFactor = 0.4f;

    #endregion

    #region Static Methods

    private static void PostUpdateRunSpeeds()
    {
        var player = GetLocalPlayer();
        if (!PlayerUtilities.IsGrounded(player))
        {
            // Ignore if not grounded
            return;
        }

        player.Player.runAcceleration *= AccelerationFactor;
        player.Player.runSlowdown = 0f;
    }

    #endregion

    #region Constructors

    public IcyFeetEffect(float duration) : base(EffectID.IcyFeet, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        GetLocalPlayer().PostUpdateRunSpeedsHook += PostUpdateRunSpeeds;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().PostUpdateRunSpeedsHook -= PostUpdateRunSpeeds;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.IceSkates, $"{viewerString} made the ground very slippery", Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, "Ground is no longer slippery", EffectSeverity.Neutral);
    }

    #endregion
}