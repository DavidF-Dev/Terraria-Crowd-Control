using CrowdControlMod.Code.Utilities;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Resize the player for a short time.
/// </summary>
public sealed class ResizePlayerEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float ScaleSmall = 0.2f;
    private const float ScaleLarge = 3;

    #endregion

    #region Fields

    private readonly bool _up;

    #endregion

    #region Constructors

    public ResizePlayerEffect(bool up, int duration) : base(up ? EffectID.ResizePlayerUp : EffectID.ResizePlayerDown, duration, EffectSeverity.Neutral)
    {
        _up = up;
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (ResizedPlayerUtils.GetIsScaled(CrowdControlMod.GetLocalPlayer().Player) ||
            (_up && CrowdControlMod.GetInstance().IsEffectActive(EffectID.ResizePlayerDown)) ||
            (!_up && CrowdControlMod.GetInstance().IsEffectActive(EffectID.ResizePlayerUp)))
        {
            return CrowdControlResponseStatus.Retry;
        }

        ResizedPlayerUtils.SetScale(CrowdControlMod.GetLocalPlayer().Player, _up ? ScaleLarge : ScaleSmall);
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        ResizedPlayerUtils.ResetScale(CrowdControlMod.GetLocalPlayer().Player);
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.BabyGrinchMischiefWhistle, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}