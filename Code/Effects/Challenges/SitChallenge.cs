using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;

namespace CrowdControlMod.Effects.Challenges;

public sealed class SitChallenge : ChallengeEffect
{
    #region Constructors

    public SitChallenge(int duration) : base(EffectID.SitChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        return !GetLocalPlayer().Player.sitting.isSitting ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Retry;
    }

    protected override void OnUpdate(float delta)
    {
        if (!GetLocalPlayer().Player.sitting.isSitting)
        {
            return;
        }

        SetChallengeCompleted();
    }

    protected override string GetChallengeDescription()
    {
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty);
    }

    #endregion
}