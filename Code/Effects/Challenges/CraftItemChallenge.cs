using System;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Challenge the player to craft a specific item, chosen at random depending on world progression.
/// </summary>
public sealed class CraftItemChallenge : ChallengeEffect
{
    #region Static Fields and Constants

    private static readonly short[] PreEyeTiles = {ItemID.WoodenSword, ItemID.Torch, ItemID.CactusPickaxe, ItemID.Glass};

    private static readonly short[] PreSkeletronTiles = {ItemID.BottledWater, ItemID.Keg, ItemID.SandstoneBrick, ItemID.SnowBrick};

    private static readonly short[] PreWallTiles = {ItemID.IceBrick, ItemID.Boulder};

    private static readonly short[] PreMechTiles = {ItemID.Toilet};

    private static readonly short[] PreGolemTiles = {ItemID.BottledHoney};

    private static readonly short[] PreLunarTiles = Array.Empty<short>();

    private static readonly short[] PreMoonLordTiles = Array.Empty<short>();

    private static readonly short[] PostGameTiles = Array.Empty<short>();

    #endregion

    #region Fields

    private Item? _chosenItem;

    #endregion

    #region Constructors

    public CraftItemChallenge(int duration) : base(EffectID.CraftItemChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        // Choose a random placeable item
        _chosenItem = new Item(Main.rand.Next(ProgressionUtils.ChooseUpToProgression(
            PreEyeTiles, PreSkeletronTiles, PreWallTiles, PreMechTiles,
            PreGolemTiles, PreLunarTiles, PreMoonLordTiles, PostGameTiles
        ).SelectMany(x => x).Distinct().ToList()));

        CrowdControlItem.OnCraftedHook += OnCrafted;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnChallengeStop()
    {
        CrowdControlItem.OnCraftedHook -= OnCrafted;
    }

    protected override string GetChallengeDescription()
    {
        var itemName = Lang.GetItemName(_chosenItem!.type);
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty, itemName);
    }

    private void OnCrafted(Recipe recipe)
    {
        // Check if the recipe produced the required item
        if (_chosenItem != null && !recipe.HasResult(_chosenItem.type))
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}