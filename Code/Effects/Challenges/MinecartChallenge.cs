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

    public MinecartChallenge(int duration) : base(EffectID.MinecartChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override string GetChallengeDescription()
    {
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty);
    }

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        return !GetLocalPlayer().Player.mount.Cart ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Retry;
    }

    protected override void OnUpdate(float delta)
    {
        if (!GetLocalPlayer().Player.mount.Cart)
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}