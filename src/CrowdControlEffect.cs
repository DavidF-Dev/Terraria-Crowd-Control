using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod;

public abstract class CrowdControlEffect
{
    #region Static Methods

    /// <summary>
    ///     Get the local player to be effected.
    /// </summary>
    [Pure] [NotNull]
    protected static CrowdControlPlayer GetLocalPlayer()
    {
        return CrowdControlMod.GetInstance().GetLocalPlayer();
    }

    #endregion

    #region Constructors

    protected CrowdControlEffect([NotNull] string id, EffectSeverity severity)
    {
        Id = id;
        Severity = severity;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Unique id that correlates to the crowd control effect ids.
    /// </summary>
    [PublicAPI] [NotNull]
    public string Id { get; }

    /// <summary>
    ///     Effect is currently active.
    /// </summary>
    [PublicAPI]
    public bool IsActive { get; private set; }

    /// <summary>
    ///     Name of the viewer that triggered the effect.
    /// </summary>
    [PublicAPI] [NotNull]
    protected string Viewer { get; private set; } = string.Empty;

    /// <summary>
    ///     Total time that the effect takes to complete.
    /// </summary>
    [PublicAPI] [CanBeNull]
    protected float? Duration { get; set; }

    /// <summary>
    ///     Current time remaining on the effect.
    /// </summary>
    [PublicAPI] [CanBeNull]
    protected float? TimeLeft { get; private set; }

    /// <summary>
    ///     Severity of the effect on the streamer.
    /// </summary>
    protected EffectSeverity Severity { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Start the effect.
    /// </summary>
    [PublicAPI]
    public CrowdControlResponseStatus Start([NotNull] string viewer)
    {
        if (IsActive)
        {
            return CrowdControlResponseStatus.Retry;
        }

        Viewer = viewer;
        TimeLeft = Duration;
        var responseStatus = OnStart();
        IsActive = responseStatus == CrowdControlResponseStatus.Success;

        if (IsActive)
        {
            var hasTimeLeft = TimeLeft is > 0f;
            SendStartMessage(viewer, GetLocalPlayer().Player.name, hasTimeLeft ? $"{TimeLeft.Value:0.}" : null);
            if (!hasTimeLeft)
            {
                // Effect should stop straight away as it isn't timed
                Stop();
            }
        }
        else
        {
            // Effect could not start, so reset variables
            Viewer = string.Empty;
            TimeLeft = null;
        }

        return responseStatus;
    }

    /// <summary>
    ///     Stop the effect instantly, without fail.
    /// </summary>
    [PublicAPI]
    public CrowdControlResponseStatus Stop()
    {
        if (!IsActive)
        {
            return CrowdControlResponseStatus.Failure;
        }

        SendStopMessage();
        IsActive = false;
        Viewer = string.Empty;
        TimeLeft = null;

        OnStop();

        return CrowdControlResponseStatus.Success;
    }

    /// <summary>
    ///     Update the effect whilst active each frame so that the time remaining is reduced.
    /// </summary>
    [PublicAPI]
    public void Update(float delta)
    {
        if (!IsActive || !TimeLeft.HasValue)
        {
            return;
        }

        // Reduce the timer, stopping the effect if it reaches zero
        TimeLeft -= delta;
        if (TimeLeft <= 0)
        {
            Stop();
            TerrariaUtils.WriteDebug($"Stopped effect '{Id}' because the duration reached zero");
            return;
        }

        OnUpdate(delta);
    }

    /// <summary>
    ///     Receive a packet meant for this effect, sent from a client.
    /// </summary>
    [PublicAPI]
    public void ReceivePacket(CrowdControlPacket packetId, [NotNull] CrowdControlPlayer player, [NotNull] BinaryReader reader)
    {
        if (Main.netMode != NetmodeID.Server)
        {
            // Ignore if not running on the server
            return;
        }
        
        OnReceivePacket(packetId, player, reader);
    }

    /// <summary>
    ///     Send a packet to be handled by the effect on the server-side.
    /// </summary>
    protected void SendPacket(CrowdControlPacket packetId, [NotNull] params object[] args)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            // Ignore if running on the server
            return;
        }

        try
        {
            // Create a new packet and assign the data
            var packet = CrowdControlMod.GetInstance().GetPacket(args.Length + 2);
            packet.Write((byte)packetId);
            packet.Write(Id);
            foreach (var obj in args)
            {
                TerrariaUtils.WriteToPacket(packet, obj);
            }
            
            // Send the packet to the server
            packet.Send();
            TerrariaUtils.WriteDebug($"'{Id}' sent packet '{packetId}' to the server");
        }
        catch (Exception e)
        {
            TerrariaUtils.WriteDebug($"'{Id}' failed to send the packet '{packetId}' to the server: {e.Message}");
        }
    }
    
    /// <summary>
    ///     Invoked when the effect is triggered.
    /// </summary>
    protected virtual CrowdControlResponseStatus OnStart()
    {
        return CrowdControlResponseStatus.Success;
    }

    /// <summary>
    ///     Invoked when the effect is stopped. Stops without fail.
    /// </summary>
    protected virtual void OnStop()
    {
    }

    /// <summary>
    ///     Invoked each frame whilst the effect is active.
    /// </summary>
    protected virtual void OnUpdate(float delta)
    {
    }

    protected virtual void SendStartMessage([NotNull] string viewerString, [NotNull] string playerString, [CanBeNull] string durationString)
    {
        TerrariaUtils.WriteEffectMessage(0, $"{viewerString} started {Id} on {playerString}", EffectSeverity.Neutral);
    }

    protected virtual void SendStopMessage()
    {
    }

    /// <summary>
    ///     Invoked when a packet is received, meant for this effect to handle on the server-side.
    /// </summary>
    protected virtual void OnReceivePacket(CrowdControlPacket packetId, [NotNull] CrowdControlPlayer player, [NotNull] BinaryReader reader)
    {
    }

    #endregion
}