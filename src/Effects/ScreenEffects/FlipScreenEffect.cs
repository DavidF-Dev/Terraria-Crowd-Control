using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Flips the screen using a screen shader for a short duration.
/// </summary>
public sealed class FlipScreenEffect : ScreenShaderEffect
{
    #region Constructors

    public FlipScreenEffect(float duration) : base(EffectID.FlipScreen, duration, EffectSeverity.Negative, "SH_FlipVertical", "FilterMyShader")
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        return EnableScreenShader() ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Failure;
    }

    protected override void OnStop()
    {
        DisableScreenShader();
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.GravityGlobe, $"{viewerString} flipped {playerString}'s screen for {durationString} seconds", Severity);
    }

    #endregion
}