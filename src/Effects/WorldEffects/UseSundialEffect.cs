using System.IO;
using CrowdControlMod.Config;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Use an enchanted sundial to fast forward time.
/// </summary>
public sealed class UseSundialEffect : CrowdControlEffect
{
    #region Constructors

    public UseSundialEffect() : base(EffectID.UseSunDial, null, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!CrowdControlConfig.GetInstance().AllowTimeChangeDuringBoss && WorldUtils.ActiveBossEventOrInvasion())
        {
            // Cannot change time during boss or invasion
            return CrowdControlResponseStatus.Failure;
        }

        if (Main.fastForwardTime)
        {
            // A sundial is already in progress
            return CrowdControlResponseStatus.Retry;
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Simply fast forward time in single-player
            Main.fastForwardTime = true;
        }
        else
        {
            // Start the sundial on the server
            SendPacket(PacketID.HandleEffect);
        }

        PlayerUtils.SetHairDye(GetLocalPlayer(), ItemID.TimeHairDye);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Sundial, $"{viewerString} fast-forward time to the next morning", Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Fast forward time and notify the clients
        Main.fastForwardTime = true;
        NetMessage.SendData(MessageID.WorldData);
    }

    #endregion
}