using System;
using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects.Interfaces;
using CrowdControlMod.ID;
using Terraria;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Choose a random challenge effect to trigger.
/// </summary>
public sealed class RandomChallengeEffectProvider : IEffectProvider
{
    #region Static Fields and Constants

    /// <summary>
    ///     Ids of the challenges that can be chosen from.
    /// </summary>
    private static readonly string[] ChallengeIds =
    {
        EffectID.SwimChallenge,
        EffectID.StandOnBlockChallenge,
        EffectID.CraftItemChallenge,
        EffectID.SleepChallenge,
        EffectID.MinecartChallenge,
        // EffectID.TouchGrassChallenge,
        EffectID.EatFoodChallenge,
        EffectID.WordPuzzleChallenge
    };

    #endregion

    #region Static Methods

    private static IEnumerable<string> GetShuffledChallengeIds()
    {
        // Shuffle the challenge ids
        return ChallengeIds.OrderBy(_ => Main.rand.Next());
    }

    #endregion

    #region Fields

    private List<string> _choices;

    #endregion

    #region Constructors

    public RandomChallengeEffectProvider()
    {
        _choices = GetShuffledChallengeIds().ToList();
    }

    #endregion

    #region Methods

    public IReadOnlyCollection<string> GetEffectIds(CrowdControlRequestType requestType)
    {
        if (requestType == CrowdControlRequestType.Stop)
        {
            return ChallengeIds;
        }

        // In the case that there's no choices (shouldn't happen!)
        if (!ChallengeIds.Any())
        {
            return Array.Empty<string>();
        }

        // In the case that there's only one choice (shouldn't happen!)
        if (ChallengeIds.Length == 1)
        {
            return new[] {ChallengeIds.First()};
        }

        // Return the next challenge id in the randomised sequence
        var choice = _choices.First();
        if (_choices.Count == 1)
        {
            // Reshuffle the list if this was the last choice
            // It's technically possible to get the same challenge twice in a row when the list is reshuffled
            _choices = GetShuffledChallengeIds().ToList();
        }
        else
        {
            // Remove so that it isn't chosen again for a while
            _choices.Remove(choice);
        }

        return new[] {choice};
    }

    #endregion
}