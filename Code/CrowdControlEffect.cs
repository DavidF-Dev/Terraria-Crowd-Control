﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects;
using CrowdControlMod.Features;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;

namespace CrowdControlMod;

/// <summary>
///     Effect triggered by the Crowd Control application.<br />
///     Can be either instantaneous or timed.<br />
///     Effects are managed by CrowdControlMod.cs.
/// </summary>
public abstract class CrowdControlEffect : IFeature
{
    #region Static Methods

    /// <summary>
    ///     Get the local player to be effected (client-side).
    /// </summary>
    [Pure]
    protected static CrowdControlPlayer GetLocalPlayer()
    {
        return CrowdControlMod.GetLocalPlayer();
    }

    #endregion

    #region Fields

    /// <summary>
    ///     Unique id of the effect instance requested by the Crowd Control session.
    /// </summary>
    private int _netId = -1;

    /// <summary>
    ///     Duration to use if none is provided by the effect request, or 0 if instantaneous.
    /// </summary>
    private readonly int _defaultDuration;
    
    /// <summary>
    ///     Collection of players with this timed effect active (server-side).
    /// </summary>
    private readonly HashSet<int> _activeOnServer = new();

    #endregion

    #region Constructors

    protected CrowdControlEffect(string id, int duration, EffectSeverity severity)
    {
        Id = id;
        _defaultDuration = duration;
        Severity = severity;

        if (NetUtils.IsServer)
        {
            // Hook onto when a player disconnects from the server so we can update the active players collection correctly
            CrowdControlPlayer.PlayerDisconnectHook += PlayerDisconnect;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Unique id that correlates to the crowd control effect ids.
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     Name of the viewer that started the effect.
    /// </summary>
    public string Viewer { get; private set; } = string.Empty;

    /// <summary>
    ///     Effect is currently active (client-side).
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    ///     Timed effect is paused (client-side).
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    ///     How long the effect lasts for, or 0 if it is instantaneous. Only correct whilst the effect is active.
    /// </summary>
    public int Duration { get; private set; }
    
    /// <summary>
    ///     Current time remaining on the effect.
    /// </summary>
    public float TimeLeft { get; private set; }

    /// <summary>
    ///     Effect is a timed effect. Only correct whilst the effect is active.
    /// </summary>
    public bool IsTimedEffect => Duration > 0;

    /// <summary>
    ///     Category of the effect; 1:1 with folders in the Terraria Pack.
    /// </summary>
    public abstract EffectCategory Category { get; }

    /// <summary>
    ///     Severity of the effect on the streamer. Can be used by implementing classes when writing their effect message.
    /// </summary>
    protected EffectSeverity Severity { get; }

    /// <summary>
    ///     Emote displayed when the effect starts (defaults to -1 for none).
    /// </summary>
    protected virtual int StartEmote => -1;

    /// <summary>
    ///     Whether the effect requires game sounds to be unmuted.
    /// </summary>
    protected virtual bool RequireGameSounds => false;

    /// <summary>
    ///     Whether the effect requires game music to be unmuted.
    /// </summary>
    protected virtual bool RequireGameMusic => false;

    #endregion

    #region Methods

    /// <summary>
    ///     Initialise the effect when the session has started (client-side).
    /// </summary>
    public void SessionStarted()
    {
        OnSessionStarted();
    }

    /// <summary>
    ///     Clean up the effect when the session has ended (client-side).
    /// </summary>
    public void SessionStopped()
    {
        OnSessionStopped();
    }

    /// <summary>
    ///     Dispose the effect when the mod is unloaded.
    /// </summary>
    public void Dispose()
    {
        if (NetUtils.IsServer)
        {
            CrowdControlPlayer.PlayerDisconnectHook -= PlayerDisconnect;
        }

        OnDisposed();
    }

    /// <summary>
    ///     Start the effect (client-side).
    /// </summary>
    public CrowdControlResponseStatus Start(int netId, string viewer, int duration)
    {
        if (IsActive)
        {
            return CrowdControlResponseStatus.Retry;
        }

        if ((RequireGameSounds && Main.soundVolume <= 0f) ||
            (RequireGameMusic && Main.musicVolume <= 0f))
        {
            return CrowdControlResponseStatus.Failure;
        }

        _netId = netId;
        Viewer = viewer;

        duration = Math.Max(0, duration);
        if (duration == 0)
        {
            duration = Math.Max(0, _defaultDuration);
        }

        TimeLeft = duration;
        Duration = duration;

        var responseStatus = OnStart();
        IsActive = responseStatus == CrowdControlResponseStatus.Success;

        if (IsActive)
        {
            SendStartMessage(viewer, GetLocalPlayer().Player.name, IsTimedEffect ? $"{TimeLeft:0.}" : null);

            // Show emote if one is provided
            if (StartEmote != -1)
            {
                GetLocalPlayer().Player.Emote(StartEmote);
            }

            if (!IsTimedEffect)
            {
                // Effect should stop straight away as it isn't timed
                Stop();
            }
            else if (NetUtils.IsClient)
            {
                // Notify the server that the timed effect is active
                SendPacket(PacketID.EffectStatus, true);
            }
        }
        else
        {
            // Effect could not start, so reset variables
            TimeLeft = 0;
            Duration = 0;
        }

        return responseStatus;
    }

    /// <summary>
    ///     Stop the effect instantly, without fail (client-side).
    /// </summary>
    public void Stop(bool requested = false)
    {
        if (!IsActive)
        {
            return;
        }

        if (NetUtils.IsClient && IsTimedEffect)
        {
            // Notify the server that the timed effect finished
            SendPacket(PacketID.EffectStatus, false);
        }

        if (IsTimedEffect && !requested)
        {
            // Let Crowd Control know that the timed effect has finished
            CrowdControlMod.GetInstance().QueueResponseToCrowdControl(_netId, this, CrowdControlResponseStatus.Finished);
        }

        SendStopMessage();

        IsActive = false;
        IsPaused = false;
        TimeLeft = 0f;
        Duration = 0;
        _netId = -1;

        OnStop();

        Viewer = string.Empty;
    }

    /// <summary>
    ///     Pause the timed effect.
    /// </summary>
    public void Pause()
    {
        if (!IsActive || !IsTimedEffect || IsPaused)
        {
            // Ignore
            return;
        }

        IsPaused = true;

        // Let Crowd Control know that the effect has been paused
        CrowdControlMod.GetInstance().QueueResponseToCrowdControl(_netId, this, CrowdControlResponseStatus.Paused);
    }

    public void Resume()
    {
        if (!IsActive || !IsTimedEffect || !IsPaused)
        {
            // Ignore
            return;
        }

        IsPaused = false;

        // Let Crowd Control know that the effect has been unpaused
        CrowdControlMod.GetInstance().QueueResponseToCrowdControl(_netId, this, CrowdControlResponseStatus.Resumed);
    }

    /// <summary>
    ///     Whether the active effect should be updated and have its timer reduced.
    /// </summary>
    public virtual bool ShouldUpdate()
    {
        return true;
    }

    /// <summary>
    ///     Update the effect whilst active each frame so that the time remaining is reduced (client-side).
    /// </summary>
    public void Update(float delta)
    {
        if (!IsActive || IsPaused)
        {
            // Ignore
            return;
        }

        // Reduce the timer, stopping the effect if it reaches zero
        if (IsTimedEffect)
        {
            TimeLeft -= delta;
            if (TimeLeft <= 0)
            {
                TimeLeft = 0;
                Stop();
                TerrariaUtils.WriteDebug($"Stopped effect '{Id}' because the duration reached zero");
                return;
            }
        }

        OnUpdate(delta);
    }

    /// <summary>
    ///     Check if the effect is active for the specified player (server-side).
    /// </summary>
    [Pure]
    public bool IsActiveOnServer(Player player)
    {
        if (NetUtils.IsServer)
        {
            return _activeOnServer.Contains(player.whoAmI);
        }

        TerrariaUtils.WriteDebug($"{nameof(IsActiveOnServer)} can only be called on the server");
        return false;
    }

    /// <summary>
    ///     Check if the effect is active for any player (server-side).
    /// </summary>
    [Pure]
    public bool IsActiveOnServer()
    {
        return _activeOnServer.Any();
    }

    /// <summary>
    ///     Receive a packet meant for this effect, sent from a client (server-side).
    /// </summary>
    public void ReceivePacket(PacketID packetId, CrowdControlPlayer player, BinaryReader reader)
    {
        if (!NetUtils.IsServer)
        {
            // Ignore if not running on the server
            return;
        }

        // Check if the packet is in regards to a client's effect status changing
        if (packetId == PacketID.EffectStatus)
        {
            var isActive = reader.ReadBoolean();

            // Add or remove the player from the server-side collection
            if (isActive)
            {
                _activeOnServer.Add(player.Player.whoAmI);
                TerrariaUtils.WriteDebug($"Added '{player.Player.name}' from effect '{Id}' active collection");
            }
            else
            {
                _activeOnServer.Remove(player.Player.whoAmI);
                TerrariaUtils.WriteDebug($"Removed '{player.Player.name}' from effect '{Id}' active collection");
            }

            return;
        }

        // Let the implementing class handle the incoming packet
        OnReceivePacket(player, reader);
    }

    /// <summary>
    ///     Send a packet to be handled by the effect on the server-side (client-side).
    /// </summary>
    protected void SendPacket(PacketID packetId, params object[] args)
    {
        if (NetUtils.IsServer)
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
    ///     Invoked when the session is started (client-side).
    /// </summary>
    protected virtual void OnSessionStarted()
    {
    }

    /// <summary>
    ///     Invoked when the session is ended (client-side).
    /// </summary>
    protected virtual void OnSessionStopped()
    {
    }

    /// <summary>
    ///     Invoked when the mod is unloaded and the effect should be disposed.
    /// </summary>
    protected virtual void OnDisposed()
    {
    }

    /// <summary>
    ///     Invoked when the effect is triggered (client-side).
    /// </summary>
    protected virtual CrowdControlResponseStatus OnStart()
    {
        return CrowdControlResponseStatus.Success;
    }

    /// <summary>
    ///     Invoked when the effect is stopped. Stops without fail (client-side).
    /// </summary>
    protected virtual void OnStop()
    {
    }

    /// <summary>
    ///     Invoked each frame whilst the effect is active (client-side).
    /// </summary>
    protected virtual void OnUpdate(float delta)
    {
    }

    /// <summary>
    ///     Send an effect message when the effect is triggered (client-side).
    /// </summary>
    protected virtual void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
    }

    /// <summary>
    ///     Send an effect message when the timed effect is stopped (client-side).
    /// </summary>
    protected virtual void SendStopMessage()
    {
    }

    /// <summary>
    ///     Invoked when a packet is received, meant for this effect to handle on the server-side (server-side).
    /// </summary>
    protected virtual void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
    }

    private void PlayerDisconnect(Player player)
    {
        // Server-side
        // Remove player from server-side collection
        if (_activeOnServer.Remove(player.whoAmI))
        {
            TerrariaUtils.WriteDebug($"Removed '{player.name} ({player.whoAmI})' from effect '{Id}' active collection");
        }
    }

    #endregion
}