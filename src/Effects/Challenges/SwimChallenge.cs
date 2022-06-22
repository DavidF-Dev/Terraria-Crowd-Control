using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using Terraria;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Player must touch any liquid (water, lava or honey) to complete this challenge.
/// </summary>
public sealed class SwimChallenge : ChallengeEffect
{
    #region Static Methods

    private static bool IsInLiquid()
    {
        // Check if any of the player tiles are in a liquid tile
        var player = GetLocalPlayer();
        for (var x = player.TileX; x < player.TileX + 1; x++)
        {
            for (var y = player.TileY; y < player.TileY + 3; y++)
            {
                if (Main.tile[x, y].LiquidAmount > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Constructors

    public SwimChallenge(float duration) : base(EffectID.SwimChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        return IsInLiquid() ? CrowdControlResponseStatus.Retry : CrowdControlResponseStatus.Success;
    }

    protected override string GetChallengeDescription()
    {
        return "Go for a swim";
    }

    protected override void OnUpdate(float delta)
    {
        if (!IsInLiquid())
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}