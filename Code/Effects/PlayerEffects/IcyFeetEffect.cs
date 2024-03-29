﻿using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Decreases horizontal acceleration to mimic a slippery effect.
/// </summary>
public sealed class IcyFeetEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float GroundedAccelerationFactor = 0.4f;
    private const float InAirAccelerationFactor = 0.8f;
    private const float GroundRunSlowdown = 0f;
    private const float InAirRunSlowdown = 0.6f;

    #endregion

    #region Static Methods

    private static void PostUpdateRunSpeeds()
    {
        var player = GetLocalPlayer();
        var isGrounded = player.Player.IsGrounded();
        player.Player.runAcceleration *= isGrounded ? GroundedAccelerationFactor : InAirAccelerationFactor;
        player.Player.runSlowdown = isGrounded ? GroundRunSlowdown : InAirRunSlowdown;
    }

    #endregion

    #region Constructors

    public IcyFeetEffect(int duration) : base(EffectID.IcyFeet, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

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

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.IceSkates, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, LangUtils.GetEffectStopText(Id), EffectSeverity.Neutral);
    }

    #endregion
}