using System.IO;
using CrowdControlMod.Config;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Use an enchanted dial to fast forward time.
/// </summary>
public sealed class UseDialEffect : CrowdControlEffect
{
    private readonly bool _sun;
    
    #region Constructors

    public UseDialEffect(bool sun) : base(sun ? EffectID.UseSunDial : EffectID.UseMoonDial, null, EffectSeverity.Neutral)
    {
        _sun = sun;
    }

    #endregion

    public override EffectCategory Category => EffectCategory.World;
    
    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!CrowdControlConfig.GetInstance().AllowTimeChangeDuringBoss && WorldUtils.ActiveBossEventOrInvasion())
        {
            // Cannot change time during boss or invasion
            return CrowdControlResponseStatus.Failure;
        }

        if (Main.IsFastForwardingTime())
        {
            // A dial is already in progress
            return CrowdControlResponseStatus.Retry;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // Simply fast forward time in single-player
            if (_sun)
            {
                Main.fastForwardTimeToDawn = true;
            }
            else
            {
                Main.fastForwardTimeToDusk = true;
            }
        }
        else
        {
            // Start the dial on the server
            SendPacket(PacketID.HandleEffect);
        }

        GetLocalPlayer().Player.SetHairDye(ItemID.TimeHairDye);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Sundial, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Fast forward time and notify the clients
        if (_sun)
        {
            Main.fastForwardTimeToDawn = true;
        }
        else
        {
            Main.fastForwardTimeToDusk = true;
        }
        
        NetMessage.SendData(MessageID.WorldData);
    }

    #endregion
}