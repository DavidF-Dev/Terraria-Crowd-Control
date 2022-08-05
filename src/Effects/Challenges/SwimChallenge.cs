using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Player must touch any liquid (water, lava or honey) to complete this challenge.
/// </summary>
public sealed class SwimChallenge : ChallengeEffect
{
    #region Constructors

    public SwimChallenge(float duration) : base(EffectID.SwimChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        return GetLocalPlayer().Player.IsInLiquid() ? CrowdControlResponseStatus.Retry : CrowdControlResponseStatus.Success;
    }

    protected override string GetChallengeDescription()
    {
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty);
    }

    protected override void OnUpdate(float delta)
    {
        if (!GetLocalPlayer().Player.IsInLiquid())
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}