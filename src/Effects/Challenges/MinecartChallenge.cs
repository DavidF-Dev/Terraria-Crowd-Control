using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Challenge the player to ride in a minecart.
/// </summary>
public sealed class MinecartChallenge : ChallengeEffect
{
    #region Constructors

    public MinecartChallenge(float duration) : base(EffectID.MinecartChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override string GetChallengeDescription()
    {
        return "Ride a minecart";
    }

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        return !PlayerUtils.IsInMinecart(GetLocalPlayer()) ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Failure;
    }

    protected override void OnUpdate(float delta)
    {
        if (!PlayerUtils.IsInMinecart(GetLocalPlayer()))
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}