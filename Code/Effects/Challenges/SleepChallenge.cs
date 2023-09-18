using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;

namespace CrowdControlMod.Effects.Challenges;

public sealed class SleepChallenge : ChallengeEffect
{
    #region Constructors

    public SleepChallenge(int duration) : base(EffectID.SleepChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        return !GetLocalPlayer().Player.sleeping.isSleeping ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Retry;
    }
    
    protected override void OnUpdate(float delta)
    {
        if (!GetLocalPlayer().Player.sleeping.isSleeping)
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