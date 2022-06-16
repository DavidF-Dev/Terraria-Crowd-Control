using System;
using System.Collections.Generic;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;

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

    #region Fields

    [CanBeNull]
    private readonly float? _duration;

    private readonly bool _isTimedEffect;

    /// <inheritdoc cref="ActiveOnServer" />
    [NotNull]
    private readonly HashSet<int> _activeOnServer = new();

    #endregion

    #region Constructors

    protected CrowdControlEffect([NotNull] string id, [CanBeNull] float? duration, EffectSeverity severity)
    {
        Id = id;
        _duration = duration;
        _isTimedEffect = _duration is > 0f;
        Severity = severity;

        if (Main.netMode == NetmodeID.Server)
        {
            CrowdControlPlayer.PlayerDisconnectHook += PlayerDisconnect;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Unique id that correlates to the crowd control effect ids.
    /// </summary>
    [NotNull]
    public string Id { get; }

    /// <summary>
    ///     Effect is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    ///     Current time remaining on the effect.
    /// </summary>
    protected float TimeLeft { get; private set; }

    /// <summary>
    ///     Severity of the effect on the streamer.
    /// </summary>
    protected EffectSeverity Severity { get; }

    /// <summary>
    ///     Collection of players with this timed effect active (server-side).
    /// </summary>
    [NotNull]
    protected IEnumerable<int> ActiveOnServer => _activeOnServer;

    #endregion

    #region Methods

    /// <summary>
    ///     Initialise the effect when the session has started.
    /// </summary>
    public void SessionStarted()
    {
        OnSessionStarted();
    }

    /// <summary>
    ///     Clean up the effect when the session has ended.
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
        if (Main.netMode == NetmodeID.Server)
        {
            CrowdControlPlayer.PlayerDisconnectHook -= PlayerDisconnect;
        }
        
        OnDisposed();
    }
    
    /// <summary>
    ///     Start the effect.
    /// </summary>
    public CrowdControlResponseStatus Start([NotNull] string viewer)
    {
        if (IsActive)
        {
            return CrowdControlResponseStatus.Retry;
        }

        TimeLeft = _duration.GetValueOrDefault();
        var responseStatus = OnStart();
        IsActive = responseStatus == CrowdControlResponseStatus.Success;

        if (IsActive)
        {
            SendStartMessage(viewer, GetLocalPlayer().Player.name, _isTimedEffect ? $"{TimeLeft:0.}" : null);

            if (!_isTimedEffect)
            {
                // Effect should stop straight away as it isn't timed
                Stop();
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Notify the server that the timed effect is active
                SendPacket(PacketID.EffectStatus, true);
            }
        }
        else
        {
            // Effect could not start, so reset variables
            TimeLeft = 0;
        }

        return responseStatus;
    }

    /// <summary>
    ///     Stop the effect instantly, without fail.
    /// </summary>
    public CrowdControlResponseStatus Stop()
    {
        if (!IsActive)
        {
            return CrowdControlResponseStatus.Failure;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient && _isTimedEffect)
        {
            // Notify the server that the timed effect finished
            SendPacket(PacketID.EffectStatus, false);
        }

        SendStopMessage();

        IsActive = false;
        TimeLeft = 0f;

        OnStop();

        return CrowdControlResponseStatus.Success;
    }

    /// <summary>
    ///     Update the effect whilst active each frame so that the time remaining is reduced.
    /// </summary>
    public void Update(float delta)
    {
        if (!IsActive)
        {
            return;
        }

        // Reduce the timer, stopping the effect if it reaches zero
        if (_isTimedEffect)
        {
            TimeLeft -= delta;
            if (TimeLeft <= 0)
            {
                Stop();
                TerrariaUtils.WriteDebug($"Stopped effect '{Id}' because the duration reached zero");
                return;
            }
        }

        OnUpdate(delta);
    }

    /// <summary>
    ///     Whether the active effect should be updated and have its timer reduced.
    /// </summary>
    public virtual bool ShouldUpdate()
    {
        return true;
    }

    /// <summary>
    ///     Receive a packet meant for this effect, sent from a client.
    /// </summary>
    public void ReceivePacket(PacketID packetId, [NotNull] CrowdControlPlayer player, [NotNull] BinaryReader reader)
    {
        if (Main.netMode != NetmodeID.Server)
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

            OnEffectStatusChanged(player, isActive);
            return;
        }

        OnReceivePacket(packetId, player, reader);
    }

    /// <summary>
    ///     Send a packet to be handled by the effect on the server-side.
    /// </summary>
    protected void SendPacket(PacketID packetId, [NotNull] params object[] args)
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
    ///     Invoked when the session is started.
    /// </summary>
    protected virtual void OnSessionStarted()
    {
    }
    
    /// <summary>
    ///     Invoked when the session is ended.
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
    protected virtual void OnReceivePacket(PacketID packetId, [NotNull] CrowdControlPlayer player, [NotNull] BinaryReader reader)
    {
    }

    /// <summary>
    ///     Invoked when a client notifies the server about a change in effect status.<br />
    ///     Invoked on the server.
    /// </summary>
    protected virtual void OnEffectStatusChanged([NotNull] CrowdControlPlayer player, bool isActive)
    {
    }

    private void PlayerDisconnect(Player player)
    {
        // Server-side
        // Remove player from server-side collection
        _activeOnServer.Remove(player.whoAmI);
        TerrariaUtils.WriteDebug($"Removed '{player.name}' from effect '{Id}' active collection");
    }

    #endregion
}