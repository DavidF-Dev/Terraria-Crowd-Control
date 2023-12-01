using System;
using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Challenge the player to stand on a specific block, chosen at random based on the world progression.
/// </summary>
public sealed class StandOnBlockChallenge : ChallengeEffect
{
    #region Static Fields and Constants

    private static readonly (ushort tileId, short itemId)[] PreEyeTiles =
    {
        (TileID.Dirt, ItemID.DirtBlock), (TileID.Stone, ItemID.StoneBlock), (TileID.ClayBlock, ItemID.ClayBlock),
        (TileID.Mud, ItemID.MudBlock), (TileID.Sand, ItemID.SandBlock), (TileID.WoodBlock, ItemID.Wood), (TileID.Sunflower, ItemID.Sunflower)
    };

    private static readonly (ushort tileId, short itemId)[] PreSkeletronTiles =
    {
        (TileID.SnowBlock, ItemID.SnowBlock), (TileID.IceBlock, ItemID.IceBlock),
        (TileID.RichMahogany, ItemID.RichMahogany), (TileID.BorealWood, ItemID.BorealWood), (TileID.Campfire, ItemID.Campfire)
    };

    private static readonly (ushort tileId, short itemId)[] PreWallTiles =
    {
        (TileID.GrayBrick, ItemID.GrayBrick), (TileID.Glass, ItemID.Glass), (TileID.PalmWood, ItemID.PalmWood)
    };

    private static readonly (ushort tileId, short itemId)[] PreMechTiles =
    {
        (TileID.Pearlstone, ItemID.PearlstoneBlock), (TileID.Pearlsand, ItemID.PearlsandBlock)
    };

    private static readonly (ushort tileId, short itemId)[] PreGolemTiles =
    {
        (TileID.SnowBrick, ItemID.SnowBrick), (TileID.IceBrick, ItemID.IceBrick), (TileID.Cloud, ItemID.Cloud)
    };

    private static readonly (ushort tileId, short itemId)[] PreLunarTiles =
    {
        (TileID.Silt, ItemID.SiltBlock), (TileID.Ash, ItemID.AshBlock)
    };

    private static readonly (ushort tileId, short itemId)[] PreMoonLordTiles = Array.Empty<(ushort tileId, short itemId)>();

    private static readonly (ushort tileId, short itemId)[] PostGameTiles = Array.Empty<(ushort tileId, short itemId)>();

    #endregion

    #region Fields

    private readonly HashSet<int> _woodTileIds = new();
    private (ushort tileId, short itemId)? _chosen;

    #endregion

    #region Constructors

    public StandOnBlockChallenge(int duration) : base(EffectID.StandOnBlockChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override void OnSessionStarted()
    {
        base.OnSessionStarted();

        // Collate wood tiles
        _woodTileIds.Clear();
        var group = RecipeGroup.recipeGroups[RecipeGroupID.Wood];
        foreach (var item in ContentSamples.ItemsByType.Values)
        {
            if (item.createTile > -1 && group.ContainsItem(item.type))
            {
                _woodTileIds.Add(item.createTile);
            }
        }
    }

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        var player = GetLocalPlayer();

        // Get options
        var options = ProgressionUtils.ChooseUpToProgression(
                PreEyeTiles, PreSkeletronTiles, PreWallTiles, PreMechTiles,
                PreGolemTiles, PreLunarTiles, PreMoonLordTiles, PostGameTiles
            )
            .SelectMany(x => x)
            .Distinct()
            .Where(x => !player.Player.IsStandingOn(x.tileId))
            .ToList();
        if (!options.Any())
        {
            // Fail if there are no options (this shouldn't happen!)
            TerrariaUtils.WriteDebug($"No item options for {Id} challenge to start");
            return CrowdControlResponseStatus.Failure;
        }

        _chosen = Main.rand.Next(options);
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnChallengeStop()
    {
        _chosen = null;
    }

    protected override void OnUpdate(float delta)
    {
        if (_chosen != null && (GetLocalPlayer().Player.IsStandingOn(_chosen.Value.tileId) || CheckWood() || CheckCloud()))
        {
            SetChallengeCompleted();
        }
    }

    protected override string GetChallengeDescription()
    {
        var itemName = Lang.GetItemName(_chosen!.Value.itemId).Value;
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty, itemName);
    }

    private bool CheckWood()
    {
        return _chosen!.Value.itemId == ItemID.Wood && GetLocalPlayer().Player.IsStandingOn(t => _woodTileIds.Contains(Main.tile[t.x, t.y].TileType));
    }

    private bool CheckCloud()
    {
        return _chosen!.Value.itemId == ItemID.Cloud && (GetLocalPlayer().Player.IsStandingOn(TileID.RainCloud) || GetLocalPlayer().Player.IsStandingOn(TileID.SnowCloud));
    }

    #endregion
}