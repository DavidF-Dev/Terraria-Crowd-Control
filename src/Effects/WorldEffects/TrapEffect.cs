using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
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

    #region Static Methods

    [NotNull]
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
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Spawn the trap in single-player
            SpawnTrap(GetLocalPlayer());
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

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
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

    private void SpawnTrap([NotNull] CrowdControlPlayer player)
    {
        int halfWidth;
        int halfHeight;

        switch (_type)
        {
            case TrapType.Cobweb:
            {
                halfWidth = 14;
                halfHeight = halfWidth;

                // Set the empty tiles around the player radially to cobwebs
                foreach (var (x, y) in PlayerUtils.GetTilesAround(player, halfWidth))
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
                halfWidth = 14;
                halfHeight = 14;

                // Set the empty tiles around the player to sand blocks
                foreach (var (x, y) in PlayerUtils.GetTilesAround(player, halfWidth, halfHeight))
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
                halfWidth = 30;
                halfHeight = 24;

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
                foreach (var (x, y) in PlayerUtils.GetTilesAround(player, halfWidth, halfHeight))
                {
                    WorldGen.PlaceLiquid(x, y, (byte)liquidId, 255);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Update framing
            WorldGen.RangeFrame(player.TileX - halfWidth, player.TileY - halfHeight, halfWidth * 2, halfHeight * 2);
        }
        else
        {
            // Update clients on change
            NetMessage.SendTileSquare(-1, player.TileX - halfWidth, player.TileY - halfHeight, halfWidth * 2, halfHeight * 2);
        }
    }

    #endregion
}