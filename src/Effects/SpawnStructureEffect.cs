using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects;

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
    private static Structure ChooseStructure([NotNull] CrowdControlPlayer player)
    {
        var tileY = player.TileY;

        if (player.Player.ZoneCorrupt || player.Player.ZoneCrimson)
        {
            return Structure.DeepChasm;
        }

        if (player.Player.ZoneUnderworldHeight)
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

        const int extentsX = 100;
        const int extentsY = 150;
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            WorldGen.RangeFrame(tileX - extentsX, tileY - extentsY, tileX + extentsX, tileY + extentsY);
        }
        else
        {
            NetMessage.SendTileSquare(-1, tileX, tileY, extentsX * 2, extentsY * 2);
        }
    }

    #endregion

    #region Fields

    private Structure _chosenStructure;

    #endregion

    #region Constructors

    public SpawnStructureEffect([NotNull] string id) : base(id, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();

        // Determine which structure to generate based on the player's location
        _chosenStructure = ChooseStructure(player);
        if (_chosenStructure == Structure.None)
        {
            return CrowdControlResponseStatus.Retry;
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Spawn the structure if we're in single-player
            SpawnStructure(_chosenStructure, player.TileX, player.TileY);
        }
        else
        {
            // Let the server generate the structure if we are a client
            SendPacket(CrowdControlPacket.SpawnStructure, (int)_chosenStructure, player.TileX, player.TileY);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _chosenStructure = Structure.None;
        base.OnStop();
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
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

    protected override void OnReceivePacket(CrowdControlPacket packetId, CrowdControlPlayer player, BinaryReader reader)
    {
        if (packetId != CrowdControlPacket.SpawnStructure)
        {
            return;
        }

        // Spawn the structure on the server
        var structure = (Structure)reader.ReadInt32();
        var tileX = reader.ReadInt32();
        var tileY = reader.ReadInt32();
        SpawnStructure(structure, tileX, tileY);
    }

    #endregion
}