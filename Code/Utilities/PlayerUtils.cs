using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using CrowdControlMod.Config;
using CrowdControlMod.ID;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Utilities;

public static class PlayerUtils
{
    #region Static Fields and Constants

    private static readonly PropertyInfo? PlayerDrawLayerVisiblePropertyInfo;

    #endregion

    #region Static Methods

    /// <summary>
    ///     Damage the player directly, not taking into affect armour/accessories/etc.
    /// </summary>
    public static void HurtDirect(this Player player, int damage)
    {
        CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.DamagedFriendly, damage, true);
        player.statLife -= damage;
    }

    /// <summary>
    ///     Cause the player to perform an emote (client-side).
    /// </summary>
    public static void Emote(this Player player, int emoteId, int time = 360)
    {
        if (player.whoAmI != Main.myPlayer || !CrowdControlConfig.GetInstance().UseEffectEmotes)
        {
            // Ignore
            return;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // Spawn in single-player
            EmoteBubble.NewBubble(emoteId, new WorldUIAnchor(player), time);
            EmoteBubble.CheckForNPCsToReactToEmoteBubble(emoteId, player);
        }
        else
        {
            // Spawn on server
            NetMessage.SendData(MessageID.Emoji, number: player.whoAmI, number2: emoteId);
        }
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
    public static bool IsGrounded(this Player player, bool acceptTopSurface = true)
    {
        return player.velocity.Y >= 0f && Collision.SolidCollision(player.BottomLeft, 16, 2, acceptTopSurface);
    }

    /// <summary>
    ///     Check if the player is standing on or in the given tile type.
    /// </summary>
    [Pure]
    public static bool IsStandingOn(this Player player, ushort id)
    {
        var tile = player.position.ToTileCoordinates();
        for (var x = tile.X; x <= tile.X + 1; x++)
        {
            for (var y = tile.Y + 2; y < tile.Y + 4; y++)
            {
                if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY &&
                    Main.tile[x, y].HasTile && Main.tile[x, y].TileType == id)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [Pure]
    public static bool IsStandingOn(this Player player, Predicate<(int x, int y)> predicate)
    {
        var tile = player.position.ToTileCoordinates();
        for (var x = tile.X; x <= tile.X + 1; x++)
        {
            for (var y = tile.Y + 2; y < tile.Y + 4; y++)
            {
                if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY &&
                    Main.tile[x, y].HasTile && predicate((x, y)))
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    [Pure]
    public static bool IsStandingIn(this Player player, ushort id)
    {
        var tile = player.position.ToTileCoordinates();
        for (var x = tile.X; x < tile.X + 1; x++)
        {
            for (var y = tile.Y; y < tile.Y + 3; y++)
            {
                if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY &&
                    Main.tile[x, y].HasTile && Main.tile[x, y].TileType == id)
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
                if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY &&
                    Main.tile[x, y].LiquidAmount > 0 && (type == -1 || Main.tile[x, y].LiquidType == type))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Check if the player is grappling or grappled.
    /// </summary>
    [Pure]
    public static bool IsGrappling(this Player player)
    {
        return player.grapCount > 0;
    }

    /// <summary>
    ///     Check if the player is within spawn protection (if enabled in the configuration).
    /// </summary>
    [Pure]
    public static bool IsWithinSpawnProtection(this Player player, float extra = 0f)
    {
        if (!CrowdControlConfig.GetInstance().EnableSpawnProtection)
        {
            return false;
        }

        var radius = CrowdControlConfig.GetInstance().SpawnProtectionRadius + extra;
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
        if (!CrowdControlConfig.GetInstance().UseEffectHairDyes)
        {
            // Ignore if disabled in config
            return;
        }

        var item = new Item(hairDyeItemId);
        player.hairDye = item.hairDye;

        if (NetUtils.IsClient)
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
                if (NetUtils.IsClient)
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
                if (NetUtils.IsClient)
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
                if (NetUtils.IsClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number5, 1f);
                }

                continue;
            }

            var num9 = coins;
            coins -= num9;
            var number4 = Item.NewItem(null, (int)player.position.X, (int)player.position.Y, player.width, player.height, 71, num9);
            if (NetUtils.IsClient)
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
    ///     Finds the closest player to the specified center.
    /// </summary>
    [Pure]
    public static bool TryFindClosestPlayer(Vector2 center, out int playerWhoAmI, out float distanceToPlayer)
    {
        playerWhoAmI = FindClosestPlayer(center, out distanceToPlayer);
        return playerWhoAmI != -1;
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

    /// <summary>
    ///     Set the provided player draw layer to be visible. May have unexpected results.
    /// </summary>
    public static void SetVisible(this PlayerDrawLayer layer)
    {
        if (PlayerDrawLayerVisiblePropertyInfo == null)
        {
            return;
        }

        PlayerDrawLayerVisiblePropertyInfo.SetValue(layer, true);
    }

    #endregion

    #region Constructors

    static PlayerUtils()
    {
        PlayerDrawLayerVisiblePropertyInfo = typeof(PlayerDrawLayer).GetProperty("Visible", BindingFlags.Instance | BindingFlags.Public);
    }

    #endregion
}