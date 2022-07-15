using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CrowdControlMod.Config;
using CrowdControlMod.ID;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Utilities;

public static class PlayerUtils
{
    #region Static Methods

    /// <summary>
    ///     Remove the player's held item and return an out-of-world clone of it.
    /// </summary>
    public static Item RemoveHeldItem(this Player player)
    {
        var item = player.HeldItem;
        if (item.IsAir)
        {
            // Not holding an item, so return "air"
            return new Item(0, 0);
        }

        // Clone the held item by performing a memberwise clone
        var copy = item.Clone();
        copy.noGrabDelay = 100;
        copy.newAndShiny = false;
        copy.velocity = Vector2.Zero;

        // Remove the held item from the inventory
        item.TurnToAir();

        return copy;
    }

    /// <summary>
    ///     Check if the player is currently invincible (client-side).
    /// </summary>
    [Pure]
    public static bool IsInvincible(this Player player)
    {
        return player.dead || player.creativeGodMode || CrowdControlMod.GetInstance().IsEffectActive(EffectID.GodModePlayer);
    }

    /// <summary>
    ///     Check if the player is currently grounded.
    /// </summary>
    [Pure]
    public static bool IsGrounded(this Player player)
    {
        return player.velocity.Y >= 0f && Collision.SolidCollision(player.BottomLeft, 32, 8, true);
    }

    /// <summary>
    ///     Check if the player is standing on or in the given tile type.
    /// </summary>
    [Pure]
    public static bool IsStandingOn(this Player player, int id)
    {
        var tile = player.position.ToTileCoordinates();
        for (var x = tile.X; x <= tile.X + 1; x++)
        {
            for (var y = tile.Y + 2; y < tile.Y + 4; y++)
            {
                if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == id)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Check if any of the tiles around the player are liquid.
    /// </summary>
    [Pure]
    public static bool IsInLiquid(this Player player, int type = -1)
    {
        var tile = player.position.ToTileCoordinates();
        for (var x = tile.X; x < tile.X + 1; x++)
        {
            for (var y = tile.Y; y < tile.Y + 3; y++)
            {
                if (Main.tile[x, y].LiquidAmount > 0 && (type == -1 || Main.tile[x, y].LiquidType == type))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Check if the player is within spawn protection (if enabled in the configuration).
    /// </summary>
    [Pure]
    public static bool IsWithinSpawnProtection(this Player player)
    {
        if (!CrowdControlConfig.GetInstance().EnableSpawnProtection)
        {
            return false;
        }

        float radius = CrowdControlConfig.GetInstance().SpawnProtectionRadius;
        var playerTile = player.Center.ToTileCoordinates().ToVector2();
        var spawnTile = new Vector2(Main.spawnTileX, Main.spawnTileY);
        var bedTile = new Vector2(player.SpawnX, player.SpawnY);
        return playerTile.Distance(spawnTile) < radius || playerTile.Distance(bedTile) < radius;
    }

    /// <summary>
    ///     Set the hair dye of the player.
    /// </summary>
    public static void SetHairDye(this Player player, int hairDyeItemId)
    {
        var item = new Item(hairDyeItemId);
        player.hairDye = item.hairDye;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
        }
    }

    /// <summary>
    ///     Give the player coins.
    /// </summary>
    public static void GiveCoins(this Player player, int coins)
    {
        // Copied from the Terraria source code
        while (coins > 0)
        {
            if (coins > 1000000)
            {
                var num12 = coins / 1000000;
                coins -= 1000000 * num12;
                var number7 = Item.NewItem(null, (int)player.position.X, (int)player.position.Y, player.width, player.height, 74, num12);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number7, 1f);
                }

                continue;
            }

            if (coins > 10000)
            {
                var num11 = coins / 10000;
                coins -= 10000 * num11;
                var number6 = Item.NewItem(null, (int)player.position.X, (int)player.position.Y, player.width, player.height, 73, num11);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number6, 1f);
                }

                continue;
            }

            if (coins > 100)
            {
                var num10 = coins / 100;
                coins -= 100 * num10;
                var number5 = Item.NewItem(null, (int)player.position.X, (int)player.position.Y, player.width, player.height, 72, num10);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number5, 1f);
                }

                continue;
            }

            var num9 = coins;
            coins -= num9;
            var number4 = Item.NewItem(null, (int)player.position.X, (int)player.position.Y, player.width, player.height, 71, num9);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number4, 1f);
            }
        }
    }

    /// <summary>
    ///     Find the closest player to the specified center.<br />
    ///     Returns the whoAmI value.
    /// </summary>
    [Pure]
    public static int FindClosestPlayer(Vector2 center, out float distanceToPlayer)
    {
        // Copied from the terraria source code
        var d = float.MaxValue;
        var closestPlayer = -1;
        for (var index = 0; index < byte.MaxValue; ++index)
        {
            var player = Main.player[index];
            if (player.active && !player.dead && !player.ghost)
            {
                var num = Vector2.DistanceSquared(center, player.Center);
                if (num < (double)d)
                {
                    d = num;
                    closestPlayer = index;
                }
            }
        }

        if (closestPlayer < 0)
        {
            for (var index = 0; index < byte.MaxValue; ++index)
            {
                var player = Main.player[index];
                if (player.active)
                {
                    var num = Vector2.DistanceSquared(center, player.Center);
                    if (num < (double)d)
                    {
                        d = num;
                        closestPlayer = index;
                    }
                }
            }
        }

        distanceToPlayer = (float)Math.Sqrt(d);
        return closestPlayer;
    }

    /// <summary>
    ///     Get the tiles in a radial area around the player.
    /// </summary>
    [Pure]
    public static IEnumerable<(int x, int y)> GetTilesAround(this Player player, int radius)
    {
        var centerTile = player.Center.ToTileCoordinates();
        return WorldUtils.GetTilesAround(centerTile.X, centerTile.Y, radius);
    }

    /// <summary>
    ///     Get the tiles in a rectangular area around the player.
    /// </summary>
    [Pure]
    public static IEnumerable<(int x, int y)> GetTilesAround(this Player player, int halfWidth, int halfHeight)
    {
        var centerTile = player.Center.ToTileCoordinates();
        return WorldUtils.GetTilesAround(centerTile.X, centerTile.Y, halfWidth, halfHeight);
    }

    #endregion
}