using System;
using CrowdControlMod.Config;
using CrowdControlMod.ID;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Utilities;

public static class PlayerUtils
{
    #region Static Methods

    /// <summary>
    ///     Is the provided player the local player / client?
    /// </summary>
    [PublicAPI] [Pure]
    public static bool IsLocalPlayer([NotNull] Player player)
    {
        return player.whoAmI == Main.myPlayer;
    }

    /// <inheritdoc cref="IsLocalPlayer(Terraria.Player)" />
    [PublicAPI] [Pure]
    public static bool IsLocalPlayer([NotNull] CrowdControlPlayer player)
    {
        return player.Player.whoAmI == Main.myPlayer;
    }

    /// <summary>
    ///     Check if the player is currently invincible.
    /// </summary>
    [PublicAPI] [Pure]
    public static bool IsInvincible([NotNull] CrowdControlPlayer player)
    {
        return player.Player.dead || player.Player.creativeGodMode || CrowdControlMod.GetInstance().IsEffectActive(EffectID.GodModePlayer);
    }

    /// <summary>
    ///     Check if the player is currently grounded.
    /// </summary>
    [PublicAPI] [Pure]
    public static bool IsGrounded([NotNull] CrowdControlPlayer player)
    {
        return Main.tileSolid[Main.tile[player.TileX, player.TileY + 3].TileType] && player.Player.velocity.Y == 0f;
    }

    /// <summary>
    ///     Check if the player is currently riding a minecart.
    /// </summary>
    [PublicAPI] [Pure]
    public static bool IsInMinecart([NotNull] CrowdControlPlayer player)
    {
        return player.Player.mount.Cart;
    }

    /// <summary>
    ///     Check if the player is within spawn protection (if enabled in the configuration).
    /// </summary>
    [PublicAPI] [Pure]
    public static bool IsWithinSpawnProtection([NotNull] CrowdControlPlayer player)
    {
        if (!CrowdControlConfig.GetInstance().EnableSpawnProtection)
        {
            return false;
        }

        float radius = CrowdControlConfig.GetInstance().SpawnProtectionRadius;
        var playerTile = new Vector2(player.TileX, player.TileY);
        var spawnTile = new Vector2(Main.spawnTileX, Main.spawnTileY);
        var bedTile = new Vector2(player.Player.SpawnX, player.Player.SpawnY);
        return playerTile.Distance(spawnTile) < radius || playerTile.Distance(bedTile) < radius;
    }

    /// <summary>
    ///     Set the hair dye of the player.
    /// </summary>
    [PublicAPI]
    public static void SetHairDye([NotNull] CrowdControlPlayer player, int hairDyeItemId)
    {
        var item = new Item(hairDyeItemId);
        player.Player.hairDye = item.hairDye;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.Player.whoAmI);
        }
    }

    /// <summary>
    ///     Give the player coins.
    /// </summary>
    [PublicAPI]
    public static void GiveCoins([NotNull] CrowdControlPlayer player, int coins)
    {
        // Copied from the Terraria source code
        while (coins > 0)
        {
            if (coins > 1000000)
            {
                var num12 = coins / 1000000;
                coins -= 1000000 * num12;
                var number7 = Item.NewItem(null, (int)player.Player.position.X, (int)player.Player.position.Y, player.Player.width, player.Player.height, 74, num12);
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
                var number6 = Item.NewItem(null, (int)player.Player.position.X, (int)player.Player.position.Y, player.Player.width, player.Player.height, 73, num11);
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
                var number5 = Item.NewItem(null, (int)player.Player.position.X, (int)player.Player.position.Y, player.Player.width, player.Player.height, 72, num10);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number5, 1f);
                }

                continue;
            }

            var num9 = coins;
            coins -= num9;
            var number4 = Item.NewItem(null, (int)player.Player.position.X, (int)player.Player.position.Y, player.Player.width, player.Player.height, 71, num9);
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
    [PublicAPI] [Pure]
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

    #endregion
}