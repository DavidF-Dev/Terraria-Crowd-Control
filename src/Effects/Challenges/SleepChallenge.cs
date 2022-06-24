using CrowdControlMod.ID;

namespace CrowdControlMod.Effects.Challenges;

public sealed class SleepChallenge : ChallengeEffect
{
    #region Constructors

    public SleepChallenge(float duration) : base(EffectID.SleepChallenge, duration)
    {
    }

    #endregion

    #region Methods

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
        return "Rest in a bed";
    }

    #endregion
}