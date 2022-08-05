using System.IO;
using CrowdControlMod.Config;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Set the time in the world.
/// </summary>
public sealed class SetTimeEffect : CrowdControlEffect
{
    #region Fields

    private readonly int _time;

    private readonly bool _isDay;

    #endregion

    #region Constructors

    public SetTimeEffect(string id, int time, bool isDay) : base(id, null, EffectSeverity.Neutral)
    {
        _time = time;
        _isDay = isDay;
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

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Simply set the time in single-player
            Main.time = _time;
            Main.dayTime = _isDay;
        }
        else
        {
            // Send a packet telling the server to change the time
            SendPacket(PacketID.HandleEffect, _time, _isDay);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var item = _isDay ? ItemID.SunMask : ItemID.MoonMask;
        TerrariaUtils.WriteEffectMessage(
            item,
            LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString),
            EffectSeverity.Neutral);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Incoming packet: (int)time (bool)isDay
        // Set the time on the server, then update the clients on the changes
        var time = reader.ReadInt32();
        var isDay = reader.ReadBoolean();
        Main.time = time;
        Main.dayTime = isDay;
        NetMessage.SendData(MessageID.WorldData);
    }

    #endregion
}