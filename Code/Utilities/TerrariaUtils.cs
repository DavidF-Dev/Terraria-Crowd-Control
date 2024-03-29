﻿using System;
using System.Diagnostics.Contracts;
using CrowdControlMod.Config;
using CrowdControlMod.Effects;
using CrowdControlMod.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.UI.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CrowdControlMod.Utilities;

public static class TerrariaUtils
{
    #region Static Methods

    /// <summary>
    ///     Check if the mod is in developer mode.
    /// </summary>
    public static bool IsInDeveloperMode()
    {
        // Client is in developer mode if enabled in the config
        if (NetUtils.IsSinglePlayer || NetUtils.IsClient)
        {
            return CrowdControlConfig.GetInstance().DeveloperMode;
        }

        // Server is in developer mode if any connected client is in developer mode
        for (var i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player.active && player.GetModPlayer<CrowdControlPlayer>().ServerDeveloperMode)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Write a message to the game chat.<br />
    ///     Server will broadcast the message to all clients.
    /// </summary>
    public static void WriteMessage(string message, Color? colour = null, bool doLog = true, int excludedPlayer = -1)
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

        if (NetUtils.IsServer)
        {
            // Broadcast if called from a server
            ChatHelper.BroadcastChatMessage(netText, colour.GetValueOrDefault(Color.White), excludedPlayer);
            return;
        }

        // Send to client
        Main.NewText(netText, colour.GetValueOrDefault(Color.White));
    }

    /// <summary>
    ///     Write a message to the game chat, prefixed with the provided item.<br />
    ///     Server will broadcast the message to all clients.
    /// </summary>
    public static void WriteMessage(short itemId, string message, Color? colour = null, bool doLog = true, int excludedPlayer = -1)
    {
        if (itemId == 0)
        {
            WriteMessage(message, colour, doLog, excludedPlayer);
            return;
        }

        WriteMessage($"{GetItemRichText(itemId)} {message}", colour, doLog, excludedPlayer);
    }

    /// <summary>
    ///     Broadcast a message to all client's game chats.
    /// </summary>
    public static void BroadcastMessage(string message, Color? colour = null, bool doLog = true, int excludedPlayer = -1)
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

        if (NetUtils.IsSinglePlayer)
        {
            if (excludedPlayer != Main.myPlayer)
            {
                // Send to client
                Main.NewText(netText, colour.GetValueOrDefault(Color.White));
            }

            return;
        }

        if (NetUtils.IsServer)
        {
            // Simply broadcast normally if we're already on the server
            ChatHelper.BroadcastChatMessage(netText, colour.GetValueOrDefault(Color.White), excludedPlayer);
            return;
        }

        // Broadcast to all clients (except the included player if any)
        var packet = CrowdControlMod.GetInstance().GetPacket();
        packet.Write((byte)PacketID.BroadcastMessage);
        netText.Serialize(packet);
        packet.Write(colour.GetValueOrDefault(Color.White).PackedValue);
        packet.Write(excludedPlayer);
        packet.Send();
    }

    /// <summary>
    ///     Write an effect message to the game chat, prefixed with the provided item.<br />
    ///     Server will notify clients of the effect message, letting them handle it.
    ///     Message will only appear if configured to.
    /// </summary>
    public static void WriteEffectMessage(short itemId, string message, EffectSeverity severity)
    {
        if (NetUtils.IsServer)
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
            // Ignore if configured not to show
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

        if (SteamUtils.IsAllFunNGamez && Main.rand.NextBool(6) && message.Length > 0 && !char.IsPunctuation(message[^1]))
        {
            // Easter egg :-)
            message += ", eh";
        }

        WriteMessage(itemId, message, colour);
    }

    /// <summary>
    ///     Send an effect message to a client's game chat, prefixed with the provided item.<br />
    ///     Message will only appear if configured to.
    /// </summary>
    public static void SendEffectMessage(CrowdControlPlayer player, short itemId, string message, EffectSeverity severity)
    {
        if (!NetUtils.IsServer)
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
        if (NetUtils.IsServer)
        {
            // Ignore if not in developer mode (don't even send the packet!)    
            if (!IsInDeveloperMode())
            {
                return;
            }

            // Ignore if on the game menu
            if (Main.gameMenu)
            {
                return;
            }

            try
            {
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

        // Ignore if not in developer mode (could be an incoming packet meant for someone else)
        if (!IsInDeveloperMode())
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
    ///     Get the rich text tag for the specified button glyph.<br />
    ///     Returns an empty string if the glyph could not be found for the specified button.
    /// </summary>
    [Pure]
    public static string GetGlyphRichText(Buttons button)
    {
        var tag = GlyphTagHandler.GenerateTag(button.ToString());
        return !tag.Equals(button.ToString()) ? tag : string.Empty;
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