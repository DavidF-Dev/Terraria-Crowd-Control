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
        var tileY = player.position.ToTileCoordinates().Y;

        if (player.ZoneCorrupt || player.ZoneCrimson)
        {
            return Structure.DeepChasm;
        }

        if (player.ZoneUnderworldHeight)
        {
            return Structure.HellHouse;
        }

        if (tileY < Main.worldSurface - 100)
        {
            return Structure.IslandHouse;
        }

        return Structure.MineHouse;
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
        if (Main.netMode == NetmodeID.SinglePlayer)
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

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();

        // Determine which structure to generate based on the player's location
        _chosenStructure = ChooseStructure(player.Player);
        if (_chosenStructure == Structure.None)
        {
            return CrowdControlResponseStatus.Retry;
        }

        var tile = player.Player.position.ToTileCoordinates();
        if (Main.netMode == NetmodeID.SinglePlayer)
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
        var message = _chosenStructure switch
        {
            Structure.None => string.Empty,
            Structure.DeepChasm => $"{viewerString} generated a deep chasm below {playerString}",
            Structure.HellHouse => $"{viewerString} generated a hell fortress around {playerString}",
            Structure.IslandHouse => $"{viewerString} generated a sky island house around {playerString}",
            Structure.MineHouse => $"{viewerString} generated an abandoned house around {playerString}",
            _ => throw new ArgumentOutOfRangeException()
        };

        TerrariaUtils.WriteEffectMessage(ItemID.TinHammer, message, Severity);
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