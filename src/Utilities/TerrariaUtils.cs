using System;
using System.Diagnostics.Contracts;
using CrowdControlMod.Config;
using CrowdControlMod.Effects;
using CrowdControlMod.ID;
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
    ///     Write a message to the game chat.<br />
    ///     Server will broadcast the message to all clients.
    /// </summary>
    public static void WriteMessage(string message, Color? colour = null, bool doLog = true)
    {
        var netText = NetworkText.FromLiteral(message);
        if (netText == NetworkText.Empty)
        {
            // Ignore
            return;
        }

        // Check if we should log this message to client.log or server.log
        if (doLog)
        {
            CrowdControlMod.GetInstance().Logger.Info(message);
        }

        if (Main.gameMenu)
        {
            // Cannot send chat if on the main menu
            // If chat is sent on the main menu, the sound engine thread gets stuck
            return;
        }

        if (Main.netMode == NetmodeID.Server)
        {
            // Broadcast if called from a server
            ChatHelper.BroadcastChatMessage(netText, colour.GetValueOrDefault(Color.White));
            return;
        }

        // Send to client
        Main.NewText(netText, colour.GetValueOrDefault(Color.White));
    }

    /// <summary>
    ///     Write a message to the game chat, prefixed with the provided item.<br />
    ///     Server will broadcast the message to all clients.
    /// </summary>
    public static void WriteMessage(short itemId, string message, Color? colour = null, bool doLog = true)
    {
        if (itemId == 0)
        {
            WriteMessage(message, colour, doLog);
            return;
        }

        WriteMessage($"{GetItemRichText(itemId)} {message}", colour, doLog);
    }

    /// <summary>
    ///     Write an effect message to the game chat, prefixed with the provided item.<br />
    ///     Server will notify clients of the effect message, letting them handle it.
    ///     Message will only appear if configured to.
    /// </summary>
    public static void WriteEffectMessage(short itemId, string message, EffectSeverity severity)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            // Create a packet to send to all clients
            var packet = CrowdControlMod.GetInstance().GetPacket(4);
            packet.Write((byte)PacketID.EffectMessage);
            packet.Write(itemId);
            packet.Write(message);
            packet.Write((int)severity);
            packet.Send();
            return;
        }

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
    ///     Send an effect message to a client's game chat, prefixed with the provided item.<br />
    ///     Message will only appear if configured to.
    /// </summary>
    public static void SendEffectMessage(CrowdControlPlayer player, short itemId, string message, EffectSeverity severity)
    {
        if (Main.netMode != NetmodeID.Server)
        {
            // Ignored on client
            return;
        }

        // Create a packet to send to the specific client
        var packet = CrowdControlMod.GetInstance().GetPacket(4);
        packet.Write((byte)PacketID.EffectMessage);
        packet.Write(itemId);
        packet.Write(message);
        packet.Write((int)severity);
        packet.Send(player.Player.whoAmI);
    }

    /// <summary>
    ///     Write a message to the game chat, only if in a debug build.<br />
    ///     Server will notify clients of the debug message, letting them handle it.
    /// </summary>
    public static void WriteDebug(string message, Color? colour = null)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            try
            {
                if (Main.gameMenu)
                {
                    // Ignore if on the game menu
                    return;
                }

                // Create a packet to send to all clients
                var packet = CrowdControlMod.GetInstance().GetPacket(3);
                packet.Write((byte)PacketID.DebugMessage);
                packet.Write(message);
                packet.Write(colour.GetValueOrDefault(Color.Yellow).PackedValue);
                packet.Send();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                // Always log the message to the server log file
                CrowdControlMod.GetInstance().Logger.Debug(message);
            }

            return;
        }

        // Ignore if not in developer mode        
        if (!CrowdControlConfig.GetInstance().DeveloperMode)
        {
            return;
        }

        WriteMessage(ItemID.Cog, message, colour.GetValueOrDefault(Color.Yellow));
    }

    /// <summary>
    ///     Get the rich text tag for the specified item id.
    /// </summary>
    [Pure]
    public static string GetItemRichText(short itemId)
    {
        return $"[i:{itemId}]";
    }

    /// <summary>
    ///     Colour the provided message using rich text tags.
    /// </summary>
    [Pure]
    public static string GetColouredRichText(string message, Color colour)
    {
        return $"[c/{colour.Hex3()}:{message}]";
    }

    /// <summary>
    ///     Attempt to write the provided data to a packet.
    /// </summary>
    public static void WriteToPacket(ModPacket packet, object data)
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
                throw new ArgumentOutOfRangeException(nameof(data), $"Sending '{data.GetType().Name}' in a packet is unsupported");
        }
    }

    #endregion
}