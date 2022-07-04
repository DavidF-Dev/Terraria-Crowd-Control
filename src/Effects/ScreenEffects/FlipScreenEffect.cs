using System.Diagnostics.CodeAnalysis;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Shaders;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Flips the screen using a screen shader for a short duration.
/// </summary>
public sealed class FlipScreenEffect : CrowdControlEffect
{
    #region Fields

    [NotNull]
    private readonly ScreenShader _flipScreenShader;

    #endregion

    #region Constructors

    public FlipScreenEffect(float duration) : base(EffectID.FlipScreen, duration, EffectSeverity.Negative)
    {
        _flipScreenShader = new ScreenShader("SH_FlipVertical", "FilterMyShader", Id);
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        return _flipScreenShader.Enable() != null ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Failure;
    }

    protected override void OnStop()
    {
        _flipScreenShader.Disable();
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.GravityGlobe, $"{viewerString} flipped {playerString}'s screen for {durationString} seconds", Severity);
    }

    #endregion
}