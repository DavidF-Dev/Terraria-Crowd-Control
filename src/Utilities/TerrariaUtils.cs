using System;
using CrowdControlMod.Effects;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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
    ///     Write an effect message to the game chat, prefixed with the provided item.<br />
    ///     Colour is determined by the provided effect type.<br />
    ///     Message will only appear if configured to.
    /// </summary>
    [PublicAPI]
    public static void WriteEffectMessage(short itemId, [NotNull] string message, EffectSeverity severity)
    {
        if (!CrowdControlConfig.GetInstance().ShowEffectMessagesInChat)
        {
            return;
        }
        
        // Determine colour based on provided effect type
        var colour = severity switch
        {
            EffectSeverity.Neutral => Color.White,
            EffectSeverity.Positive => Color.Green,
            EffectSeverity.Negative => Color.Red,
            _ => Color.Black
        };
        
        WriteMessage(itemId, message, colour);
    }

    /// <summary>
    ///     Write a message to the game chat, only if in a debug build.
    /// </summary>
    [PublicAPI]
    public static void WriteDebug([NotNull] string message, Color? colour = null)
    {
        // TODO: Allow server to write debug messages (send to clients with developer mode enabled?)
        
        #if !DEBUG
        if (!CrowdControlConfig.GetInstance().DeveloperMode)
        {
            return;
        }
        #endif
        
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

    /// <summary>
    ///     Colour the provided message using rich text tags.
    /// </summary>
    [PublicAPI] [Pure] [NotNull]
    public static string GetColouredRichText([NotNull] string message, Color colour)
    {
        return $"[c/{colour.Hex3()}:{message}]";
    }

    /// <summary>
    ///     Attempt to write the provided data to a packet.
    /// </summary>
    [PublicAPI]
    public static void WriteToPacket([NotNull] ModPacket packet, object data)
    {
        switch (data)
        {
            case bool @bool:
                packet.Write(@bool);
                break;
            case byte @byte:
                packet.Write(@byte);
                break;
            case byte[] bytes:
                packet.Write(bytes);
                break;
            case int @int:
                packet.Write(@int);
                break;
            case float @float:
                packet.Write(@float);
                break;
            case string @string:
                packet.Write(@string);
                break;
            case char @char:
                packet.Write(@char);
                break;
            case short @short:
                packet.Write(@short);
                break;
            default:
                throw new NotImplementedException($"Sending '{data.GetType().Name}' in a packet is unsupported");
        }
    }
    
    #endregion
}