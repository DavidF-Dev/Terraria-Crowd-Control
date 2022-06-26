﻿using System;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Challenge the player to stand on a specific block, chosen at random based on the world progression.
/// </summary>
public sealed class StandOnBlockChallenge : ChallengeEffect
{
    #region Static Fields and Constants

    [NotNull]
    private static readonly short[] PreEyeTiles = {ItemID.DirtBlock, ItemID.StoneBlock, ItemID.ClayBlock, ItemID.MudBlock, ItemID.SandBlock, ItemID.Wood, ItemID.Sunflower};

    [NotNull]
    private static readonly short[] PreSkeletronTiles = {ItemID.SnowBlock, ItemID.IceBlock, ItemID.Cloud, ItemID.RichMahogany, ItemID.BorealWood, ItemID.Campfire};

    [NotNull]
    private static readonly short[] PreWallTiles = {ItemID.GrayBrick, ItemID.Glass, ItemID.PalmWood};

    [NotNull]
    private static readonly short[] PreMechTiles = {ItemID.PearlstoneBlock, ItemID.PearlsandBlock};

    [NotNull]
    private static readonly short[] PreGolemTiles = {ItemID.SnowBrick, ItemID.IceBrick};

    [NotNull]
    private static readonly short[] PreLunarTiles = {ItemID.SiltBlock, ItemID.AshBlock};

    [NotNull]
    private static readonly short[] PreMoonLordTiles = Array.Empty<short>();

    [NotNull]
    private static readonly short[] PostGameTiles = Array.Empty<short>();

    #endregion

    #region Fields

    private Item _chosenTileItem;

    #endregion

    #region Constructors

    public StandOnBlockChallenge(float duration) : base(EffectID.StandOnBlockChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        // Choose a random placeable item
        var player = GetLocalPlayer();
        _chosenTileItem = new Item(Main.rand.Next(ProgressionUtils.ChooseUpToProgression(
            PreEyeTiles, PreSkeletronTiles, PreWallTiles, PreMechTiles,
            PreGolemTiles, PreLunarTiles, PreMoonLordTiles, PostGameTiles
        ).SelectMany(x => x).Distinct().Where(x => PlayerUtils.IsStandingOn(player, x)).ToList()));

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnChallengeStop()
    {
        _chosenTileItem = null;
    }

    protected override void OnUpdate(float delta)
    {
        if (!PlayerUtils.IsStandingOn(GetLocalPlayer(), _chosenTileItem.createTile))
        {
            return;
        }

        SetChallengeCompleted();
    }

    protected override string GetChallengeDescription()
    {
        return $"Stand on a {_chosenTileItem.Name}";
    }

    #endregion
}