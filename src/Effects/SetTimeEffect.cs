using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects;

public sealed class SetTimeEffect : CrowdControlEffect
{
    #region Fields

    [NotNull]
    private readonly string _timeString;

    private readonly int _time;
    private readonly bool _isDay;

    #endregion

    #region Constructors

    public SetTimeEffect([NotNull] string id, [NotNull] string timeString, int time, bool isDay) : base(id, null, EffectSeverity.Neutral)
    {
        _timeString = timeString;
        _time = time;
        _isDay = isDay;
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // TODO:
        // if !allow time change in boss and ActiveBossEventOrInvasion(false true)
        // Failure

        // TODO: Check if game time is already at desired time: Retry (?)

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Simply set the time in single-player
            Main.time = _time;
            Main.dayTime = _isDay;
        }
        else
        {
            // Send a packet telling the server to change the time
            SendPacket(PacketID.SetTime, _time, _isDay);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(_isDay ? ItemID.SunMask : ItemID.MoonMask, $"{viewerString} set the time to {_timeString}", EffectSeverity.Neutral);
    }

    protected override void OnReceivePacket(PacketID packetId, CrowdControlPlayer player, BinaryReader reader)
    {
        if (packetId != PacketID.SetTime)
        {
            // Ignore (this shouldn't happen)
            return;
        }

        // Set the time on the server, then update the clients on the changes
        var time = reader.ReadInt32();
        var isDay = reader.ReadBoolean();
        Main.time = time;
        Main.dayTime = isDay;
        NetMessage.SendData(MessageID.WorldData);
    }

    #endregion
}