using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using Terraria;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Effect that causes the player to hiccup randomly over a duration.
/// </summary>
public sealed class HiccupEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int MinTime = 60;
    private const int MaxTime = 60 * 8;

    #endregion

    #region Fields

    private int _timer;

    #endregion

    #region Constructors

    public HiccupEffect(float duration) : base(EffectID.Hiccup, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        GetLocalPlayer().PostUpdateRunSpeedsHook += OnPostUpdateRunSpeeds;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().PostUpdateRunSpeedsHook -= OnPostUpdateRunSpeeds;
        _timer = 0;
    }

    private void OnPostUpdateRunSpeeds()
    {
        if (_timer-- > 0)
        {
            return;
        }

        _timer = Main.rand.Next(MinTime, MaxTime);
    }

    #endregion
}