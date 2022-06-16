using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;

namespace CrowdControlMod.Effects.Challenges;

public sealed class JumpChallenge : ChallengeEffect
{
    #region Constructors

    public JumpChallenge(float duration) : base(EffectID.JumpChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnChallengeStop()
    {
    }

    protected override string GetChallengeDescription()
    {
        return "Jump";
    }

    protected override void OnUpdate(float delta)
    {
        var player = GetLocalPlayer();
        if (player.Player.velocity.Y >= 0f)
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}