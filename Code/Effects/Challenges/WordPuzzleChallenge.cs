using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Challenge the player to unscramble a Terraria-related word. Other players can assist in multiplayer.
/// </summary>
public sealed class WordPuzzleChallenge : ChallengeEffect
{
    #region Static Fields and Constants

    /// <summary>
    ///     Possible categories to choose from. Values matching localisation keys.
    /// </summary>
    private static readonly string[] Categories = {"TownNPC", "Gemstone", "Biome", "Metal", "Herb"};

    #endregion

    #region Fields

    private readonly Dictionary<string, string> _previousWords = new();
    private string? _word;
    private string? _scrambledWord;
    private string? _category;
    private string? _localisedCategory;
    private Player? _correctGuesser;

    #endregion

    #region Constructors

    public WordPuzzleChallenge(float duration) : base(EffectID.WordPuzzleChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        if (Categories.Length == 0)
        {
            return CrowdControlResponseStatus.Unavailable;
        }

        // Determine category
        _category = Categories[Main.rand.Next(Categories.Length)];
        _localisedCategory = LangUtils.GetEffectMiscText(Id, "Categories." + _category);

        // Get possible words
        var words = LangUtils.GetEffectMiscText(Id, "Words." + _category).Split('|').Distinct().Where(x => x.Length != 0).ToArray();

        // Determine which word to scramble
        switch (words.Length)
        {
            case 0:
                return CrowdControlResponseStatus.Failure;
            case 1:
                _word = words[0];
                break;
            default:
                var previousWord = _previousWords.GetValueOrDefault(_category);
                _word = Main.rand.Next(string.IsNullOrEmpty(previousWord) ? words : words.Where(x => x != previousWord).ToArray());
                _previousWords[_category] = _word;
                break;
        }

        // Scramble the word
        _scrambledWord = new string(_word.OrderBy(_ => Main.rand.Next()).ToArray());

        if (NetUtils.IsClient)
        {
            // Notify other players that they can assist in completing the challenge
            TerrariaUtils.WriteMessage(LangUtils.GetEffectMiscText(Id, "Assist", Main.LocalPlayer.name, _word.ToUpper(), _localisedCategory!), excludedPlayer: Main.myPlayer);
        }

        On_ChatHelper.DisplayMessage += OnDisplayMessage;

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnChallengeStop()
    {
        if (NetUtils.IsClient && _correctGuesser != null && !string.IsNullOrEmpty(_word))
        {
            // Notify everyone that the word was guessed
            var excluded = _correctGuesser == Main.LocalPlayer ? Main.myPlayer : -1;
            TerrariaUtils.WriteMessage(LangUtils.GetEffectMiscText(Id, "Completed", _correctGuesser.name, _word.ToUpper(), _localisedCategory!), excludedPlayer: excluded);
        }

        _word = null;
        _scrambledWord = null;
        _category = default;
        _localisedCategory = null;
        _correctGuesser = null;

        On_ChatHelper.DisplayMessage -= OnDisplayMessage;
    }

    protected override string GetChallengeDescription()
    {
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty, _scrambledWord!.ToUpper(), _localisedCategory!);
    }

    protected override void OnUpdate(float delta)
    {
        if (_correctGuesser != null)
        {
            SetChallengeCompleted();
        }
    }

    private void OnDisplayMessage(On_ChatHelper.orig_DisplayMessage orig, NetworkText text, Color colour, byte messageAuthor)
    {
        orig.Invoke(text, colour, messageAuthor);
        if (messageAuthor < byte.MaxValue && text.ToString().ToLower().Split(' ').Contains(_word))
        {
            // Someone unscrambled the word; doesn't have to be the challenged player
            _correctGuesser = Main.player[messageAuthor];
        }
    }

    #endregion
}