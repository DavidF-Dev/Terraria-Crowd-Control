using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Start a challenge for the player to touch a grass block.
/// </summary>
public sealed class TouchGrassChallenge : ChallengeEffect
{
    #region Constructors

    public TouchGrassChallenge(int duration) : base(EffectID.TouchGrassChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        return !GetLocalPlayer().Player.IsStandingOn(TileID.Grass) ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Retry;
    }

    protected override string GetChallengeDescription()
    {
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty);
    }

    protected override void OnUpdate(float delta)
    {
        if (!GetLocalPlayer().Player.IsStandingOn(TileID.Grass))
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}