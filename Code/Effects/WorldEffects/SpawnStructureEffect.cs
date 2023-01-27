using System;
using System.Diagnostics.Contracts;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Spawn a structure in the world based on the player's position.
/// </summary>
public sealed class SpawnStructureEffect : CrowdControlEffect
{
    #region Enums

    private enum Structure
    {
        None,
        DeepChasm,
        HellHouse,
        IslandHouse,
        MineHouse
    }

    #endregion

    #region Static Methods

    [Pure]
    private static Structure ChooseStructure(Player player)
    {
        // Choose the structure based on the player's location in the world
        var tile = player.position.ToTileCoordinates();
        var wall = Main.tile[tile.X, tile.Y].WallType;
        
        if (player.ZoneCorrupt || player.ZoneCrimson)
        {
            return wall is not WallID.EbonstoneUnsafe ? Structure.DeepChasm : Structure.None;
        }

        if (player.ZoneUnderworldHeight)
        {
            return wall is not WallID.ObsidianBrick or WallID.ObsidianBrickUnsafe ? Structure.HellHouse : Structure.None;
        }

        if (tile.Y < Main.worldSurface - 100)
        {
            return wall is not WallID.DiscWall or WallID.Glass ? Structure.IslandHouse : Structure.None;
        }

        return wall is not WallID.Planked or WallID.Wood ? Structure.MineHouse : Structure.None;
    }

    private static void SpawnStructure(Structure structure, int tileX, int tileY)
    {
        // Generate the structure
        switch (structure)
        {
            case Structure.None:
                break;
            case Structure.DeepChasm:
                WorldGen.ChasmRunner(tileX, tileY, 24);
                break;
            case Structure.HellHouse:
                WorldGen.HellFort(tileX, tileY);
                break;
            case Structure.IslandHouse:
                WorldGen.IslandHouse(tileX, tileY, 0);
                break;
            case Structure.MineHouse:
                WorldGen.MineHouse(tileX, tileY);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(structure), structure, null);
        }

        // Update the client with the changed tiles
        const int extentsX = 64;
        const int extentsY = 64;
        if (NetUtils.IsSinglePlayer)
        {
            WorldGen.RangeFrame(tileX - extentsX, tileY - extentsY, extentsX * 2, extentsY * 2);
        }
        else
        {
            NetMessage.SendTileSquare(-1, tileX - extentsX, tileY - extentsY, extentsX * 2, extentsY * 2);
        }
    }

    #endregion

    #region Fields

    private Structure _chosenStructure;

    #endregion

    #region Constructors

    public SpawnStructureEffect() : base(EffectID.SpawnStructure, null, EffectSeverity.Neutral)
    {
    }

    #endregion

    public override EffectCategory Category => EffectCategory.World;
    
    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();

        if (player.Player.IsWithinSpawnProtection())
        {
            // Ignore if within spawn protection
            return CrowdControlResponseStatus.Retry;
        }

        // Check that there is enough open space around the player
        const int checkRange = 2;
        var tile = player.Player.position.ToTileCoordinates();
        for (var x = tile.X - checkRange; x < tile.X + 2 + checkRange; x++)
        {
            for (var y = tile.Y; y < tile.Y + 3; y++)
            {
                if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                {
                    continue;
                }

                if (Main.tile[x, y].HasTile || Main.tile[x, y].WallType > 0)
                {
                    return CrowdControlResponseStatus.Retry;
                }
            }
        }
        
        // Determine which structure to generate based on the player's location
        _chosenStructure = ChooseStructure(player.Player);
        if (_chosenStructure == Structure.None)
        {
            return CrowdControlResponseStatus.Retry;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // Spawn the structure if we're in single-player
            SpawnStructure(_chosenStructure, tile.X, tile.Y);
        }
        else
        {
            // Let the server generate the structure if we are a client
            SendPacket(PacketID.HandleEffect, (int)_chosenStructure, tile.X, tile.Y);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _chosenStructure = Structure.None;
        base.OnStop();
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var locKey = $"{Id}_{(int)_chosenStructure}";
        TerrariaUtils.WriteEffectMessage(ItemID.TinHammer, LangUtils.GetEffectStartText(locKey, viewerString, playerString, durationString), Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Incoming packet: (int)chosenStructure (int)tileX (int)tileY

        // Spawn the structure on the server
        var structure = (Structure)reader.ReadInt32();
        var tileX = reader.ReadInt32();
        var tileY = reader.ReadInt32();
        SpawnStructure(structure, tileX, tileY);
    }

    #endregion
}