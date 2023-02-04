using System;
using CrowdControlMod.ID;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Utilities;

public static class NetUtils
{
    #region Static Methods

    /// <summary>
    ///     Sync an NPC is non-vanilla ways (server-side).
    /// </summary>
    public static void SyncNPCSpecial(int whoAmI)
    {
        var packet = CrowdControlMod.GetInstance().GetPacket(4);
        packet.Write((byte)PacketID.SyncNPCSpecial);
        packet.Write(whoAmI);
        packet.Write(Main.npc[whoAmI].lifeMax);
        packet.Write(Main.npc[whoAmI].life);
        packet.Send();
    }

    /// <summary>
    ///     Sync an item in non-vanilla ways (server-side).
    /// </summary>
    public static void SyncItemSpecial(int whoAmI)
    {
        var packet = CrowdControlMod.GetInstance().GetPacket(3);
        packet.Write((byte)PacketID.SyncItemSpecial);
        packet.Write(whoAmI);
        packet.Write(Main.item[whoAmI].noGrabDelay);
        packet.Send();
    }

    /// <summary>
    ///     Notify clients that the provided player has farted, triggering the sound effect.
    /// </summary>
    public static void MakeFart(Player player, int toClient = -1, int ignoreClient = -1)
    {
        var packet = CrowdControlMod.GetInstance().GetPacket(2);
        packet.Write((byte)PacketID.Fart);
        packet.Write(player.whoAmI);
        packet.Send(toClient, ignoreClient);
    }

    /// <summary>
    ///     Send a tile square that is split up into multiple calls (16x16 squares).
    /// </summary>
    public static void SendTileSquare(int whoAmI, int tileX, int tileY, int sizeX, int sizeY)
    {
        const int maxSizeX = 16;
        const int maxSizeY = 16;
        for (var i = 0; i < (int)MathF.Ceiling(sizeX / (float)maxSizeX); i++)
        {
            for (var j = 0; j < (int)MathF.Ceiling(sizeY / (float)maxSizeY); j++)
            {
                var x = tileX + maxSizeX * i;
                var y = tileY + maxSizeY * j;
                var width = Math.Min(maxSizeX + maxSizeX * i, sizeX) % maxSizeX;
                var height = Math.Min(maxSizeY + maxSizeY * j, sizeY) % maxSizeY;
                NetMessage.SendTileSquare(whoAmI, x, y, width, height);
            }
        }
    }
    
    #endregion

    #region Properties

    /// <summary>
    ///     Game is running as server.
    /// </summary>
    public static bool IsServer => Main.netMode == NetmodeID.Server;

    /// <summary>
    ///     Game is running as multiplayer client.
    /// </summary>
    public static bool IsClient => Main.netMode == NetmodeID.MultiplayerClient;

    /// <summary>
    ///     Game is running in single player mode.
    /// </summary>
    public static bool IsSinglePlayer => Main.netMode == NetmodeID.SinglePlayer;

    #endregion
}