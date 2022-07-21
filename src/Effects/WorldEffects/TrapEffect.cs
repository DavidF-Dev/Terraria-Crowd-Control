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
///     Spawn tiles around the player to "trap" them.
/// </summary>
public sealed class TrapEffect : CrowdControlEffect
{
    #region Enums

    public enum TrapType
    {
        Cobweb,
        Sand,
        Water,
        Lava,
        Honey
    }

    #endregion

    #region Static Fields and Constants

    private const int CobwebHalfWidth = 14;
    private const int CobwebHalfHeight = CobwebHalfWidth;
    private const int SandHalfWidth = 14;
    private const int SandHalfHeight = SandHalfWidth;
    private const int LiquidHalfWidth = 10;
    private const int LiquidHalfHeight = 8;

    #endregion

    #region Static Methods

    [Pure]
    private static string GetId(TrapType type)
    {
        return type switch
        {
            TrapType.Cobweb => EffectID.CobwebTrap,
            TrapType.Sand => EffectID.SandTrap,
            TrapType.Water => EffectID.WaterTrap,
            TrapType.Lava => EffectID.LavaTrap,
            TrapType.Honey => EffectID.HoneyTrap,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    [Pure]
    private static (int halfWidth, int halfHeight) GetTrapSize(TrapType trapType)
    {
        return trapType switch
        {
            TrapType.Cobweb => (CobwebHalfWidth, CobwebHalfHeight),
            TrapType.Sand => (SandHalfWidth, SandHalfHeight),
            TrapType.Water => (LiquidHalfWidth, LiquidHalfHeight),
            TrapType.Lava => (LiquidHalfWidth, LiquidHalfHeight),
            TrapType.Honey => (LiquidHalfWidth, LiquidHalfHeight),
            _ => throw new ArgumentOutOfRangeException(nameof(trapType), trapType, null)
        };
    }

    #endregion

    #region Fields

    private readonly TrapType _type;

    #endregion

    #region Constructors

    public TrapEffect(TrapType type) : base(GetId(type), null, EffectSeverity.Negative)
    {
        _type = type;
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        var (halfWidth, halfHeight) = GetTrapSize(_type);
        if (player.Player.IsWithinSpawnProtection(Math.Max(halfWidth, halfHeight) / 2f))
        {
            return CrowdControlResponseStatus.Retry;
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Spawn the trap in single-player
            SpawnTrap(player);
        }
        else
        {
            // Handle on the server
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        SpawnTrap(player);
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var itemId = _type switch
        {
            TrapType.Cobweb => ItemID.Cobweb,
            TrapType.Sand => ItemID.SandBlock,
            TrapType.Water => ItemID.WaterBucket,
            TrapType.Lava => ItemID.LavaBucket,
            TrapType.Honey => ItemID.HoneyBucket,
            _ => throw new ArgumentOutOfRangeException()
        };

        var message = _type switch
        {
            TrapType.Cobweb => $"{viewerString} encased {playerString} in cobwebs",
            TrapType.Sand => $"{viewerString} trapped {playerString} in a layer of sand",
            TrapType.Water => $"{viewerString} filled the area around {playerString} with water",
            TrapType.Lava => $"{viewerString} filled the area around {playerString} with lava",
            TrapType.Honey => $"{viewerString} filled the area around {playerString} with honey",
            _ => throw new ArgumentOutOfRangeException()
        };

        TerrariaUtils.WriteEffectMessage(itemId, message, Severity);
    }

    private void SpawnTrap(CrowdControlPlayer player)
    {
        var (halfWidth, halfHeight) = GetTrapSize(_type);
        switch (_type)
        {
            case TrapType.Cobweb:
            {
                // Set the empty tiles around the player radially to cobwebs
                foreach (var (x, y) in player.Player.GetTilesAround(halfWidth))
                {
                    if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType > 0)
                    {
                        // Ignore if a tile already exists there
                        continue;
                    }

                    Main.tile[x, y].ResetToType(TileID.Cobweb);
                }

                break;
            }
            case TrapType.Sand:
            {
                // Set the empty tiles around the player to sand blocks
                foreach (var (x, y) in player.Player.GetTilesAround(halfWidth, halfHeight))
                {
                    if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType > 0)
                    {
                        // Ignore if a tile already exists there
                        continue;
                    }

                    Main.tile[x, y].ResetToType(Main.rand.Next(new[] {TileID.Sand, TileID.Pearlsand, TileID.Crimsand, TileID.Ebonsand}));
                }

                break;
            }
            case TrapType.Water:
            case TrapType.Lava:
            case TrapType.Honey:
            {
                // Determine liquid id
                var liquidId = _type switch
                {
                    TrapType.Water => LiquidID.Water,
                    TrapType.Lava => LiquidID.Lava,
                    TrapType.Honey => LiquidID.Honey,
                    _ => throw new ArgumentOutOfRangeException()
                };

                // Set the tiles around the player to contain liquid
                // PlaceLiquid() syncs with the clients
                foreach (var (x, y) in player.Player.GetTilesAround(halfWidth, halfHeight))
                {
                    WorldGen.PlaceLiquid(x, y, (byte)liquidId, 255);
                }

                break;
            }
        }

        var tile = player.Player.position.ToTileCoordinates();
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Update framing
            WorldGen.RangeFrame(tile.X - halfWidth, tile.Y - halfHeight, halfWidth * 2, halfHeight * 2);
        }
        else
        {
            // Update clients on change
            NetMessage.SendTileSquare(-1, tile.X - halfWidth, tile.Y - halfHeight, halfWidth * 2, halfHeight * 2);
        }
    }

    #endregion
}