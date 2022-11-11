using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Utilities;

public static class NetUtils
{
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