using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Cause the player to fling upwards into the air at high speed.
/// </summary>
public sealed class FlingUpwardsEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float FlingSpeed = 24f;

    #endregion

    #region Constructors

    public FlingUpwardsEffect() : base(EffectID.FlingUpwards, null, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.mount.Active)
        {
            // Ensure the player isn't riding a mount
            player.Player.mount.Dismount(player.Player);
        }

        // Set the player's Y velocity
        player.Player.velocity.Y = -FlingSpeed;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // Sync with the server
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.Player.whoAmI);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Abeemination, $"{viewerString} flung {playerString} upwards into the air", Severity);
    }

    #endregion
}