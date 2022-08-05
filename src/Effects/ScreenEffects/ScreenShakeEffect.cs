using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Shake the screen for a short time.
/// </summary>
public sealed class ScreenShakeEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float Magnitude = 10f;

    #endregion

    #region Static Methods

    private static void ModifyScreenPosition()
    {
        Main.screenPosition += Main.rand.NextVector2Unit(1f, 1f) * Magnitude;
    }

    #endregion

    #region Constructors

    public ScreenShakeEffect(float duration) : base(EffectID.ScreenShake, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        GetLocalPlayer().ModifyScreenPositionHook += ModifyScreenPosition;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().ModifyScreenPositionHook -= ModifyScreenPosition;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.SoulofFright, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}