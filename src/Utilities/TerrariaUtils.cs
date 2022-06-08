using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

namespace CrowdControlMod.Utilities;

public static class TerrariaUtils
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

    /// <inheritdoc cref="IsLocalPlayer(Terraria.Player)"/>
    [PublicAPI] [Pure]
    public static bool IsLocalPlayer([NotNull] CrowdControlPlayer player)
    {
        return player.Player.whoAmI == Main.myPlayer;
    }

    /// <summary>
    ///     Write a message to the game chat.
    /// </summary>
    [PublicAPI]
    public static void WriteMessage([NotNull] string message, Color colour = default)
    {
        var netText = NetworkText.FromLiteral(message);
        if (netText == NetworkText.Empty)
        {
            // Ignore
            return;
        }
        
        if (Main.netMode == NetmodeID.Server)
        {
            // Broadcast if called from a server
            ChatHelper.BroadcastChatMessage(netText, colour);
            return;
        }

        // Send to client
        Main.NewText(netText, colour);
    }

    /// <summary>
    ///     Write a message to the game chat, prefixed with the provided item.
    /// </summary>
    [PublicAPI]
    public static void WriteMessage(short itemId, [NotNull] string message, Color colour)
    {
        if (itemId == 0)
        {
            WriteMessage(message, colour);
            return;
        }

        WriteMessage($"{GetItemRichText(itemId)} {message}", colour);
    }

    /// <summary>
    ///     Write a message to the game chat, only if in a debug build.
    /// </summary>
    [PublicAPI]
    public static void WriteDebug([NotNull] string message, Color? colour = null)
    {
        if (!CrowdControlConfig.GetInstance().DeveloperMode)
        {
            return;
        }
        
        WriteMessage(ItemID.Cog, message, colour.GetValueOrDefault(Color.Yellow));
    }
    
    /// <summary>
    ///     Get the rich text tag for the specified item id.
    /// </summary>
    [PublicAPI] [Pure] [NotNull]
    public static string GetItemRichText(short itemId)
    {
        return $"[i:{itemId}]";
    }

    #endregion
}