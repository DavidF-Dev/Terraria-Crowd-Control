﻿using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Shaders;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Induces a drunken feeling by swaying the screen using shaders for a short duration.
/// </summary>
public sealed class DrunkModeEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float SineIntensity = 0.05f;

    private const float GlitchIntensity = 24f;

    #endregion

    #region Fields

    [NotNull]
    private readonly ScreenShader _sineShader;

    [NotNull]
    private readonly ScreenShader _glitchShader;

    #endregion

    #region Constructors

    public DrunkModeEffect(float duration) : base(EffectID.DrunkMode, duration, EffectSeverity.Negative)
    {
        _sineShader = new ScreenShader("SH_Sine", "CreateSine", $"{Id}_1");
        _glitchShader = new ScreenShader("SH_Glitch", "CreateGlitch", $"{Id}_2");
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Enable the shader effects
        var sineShaderData = _sineShader.Enable();
        var glitchShaderData = _glitchShader.Enable();
        if (sineShaderData == null || glitchShaderData == null)
        {
            // Failed to enable one of the shader effects, so we failed
            _sineShader.Disable();
            _glitchShader.Disable();
            return CrowdControlResponseStatus.Failure;
        }

        // Set the intensity of the shader effects
        sineShaderData.UseIntensity(SineIntensity);
        glitchShaderData.UseIntensity(GlitchIntensity);

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _sineShader.Disable();
        _glitchShader.Disable();
    }

    protected override void OnUpdate(float delta)
    {
        // Set the intensity of the shader effects
        _sineShader.GetShader()?.UseIntensity(SineIntensity);
        _glitchShader.GetShader()?.UseIntensity(GlitchIntensity);
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Ale, $"{viewerString} made {playerString} feel drunk for {durationString} seconds", Severity);
    }

    #endregion
}