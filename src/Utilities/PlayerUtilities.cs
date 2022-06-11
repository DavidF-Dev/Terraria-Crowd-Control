using CrowdControlMod.Config;
using CrowdControlMod.ID;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;

namespace CrowdControlMod.Utilities;

public static class PlayerUtilities
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
    
    #endregion
}