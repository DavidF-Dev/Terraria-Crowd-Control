using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Challenge the player to consume any food.
/// </summary>
public sealed class EatFoodChallenge : ChallengeEffect
{
    #region Constructors

    public EatFoodChallenge(float duration) : base(EffectID.EatFoodChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        CrowdControlItem.OnItemConsumedHook += OnItemConsumed;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnChallengeStop()
    {
        CrowdControlItem.OnItemConsumedHook -= OnItemConsumed;
    }

    protected override string GetChallengeDescription()
    {
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty);
    }

    private void OnItemConsumed(Item item, Player player)
    {
        // Check if the consumed item is food/drink based on the buff it provides when consumed
        if (item.buffType is BuffID.WellFed or BuffID.WellFed2 or BuffID.WellFed3 or BuffID.Tipsy)
        {
            SetChallengeCompleted();
        }
    }

    #endregion
}