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

    public TouchGrassChallenge(float duration) : base(EffectID.TouchGrassChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override string GetChallengeDescription()
    {
        return "Touch grass";
    }

    protected override void OnUpdate(float delta)
    {
        if (!PlayerUtils.IsStandingOn(GetLocalPlayer(), TileID.Grass))
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}