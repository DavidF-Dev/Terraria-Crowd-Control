using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlMod : Mod
{
    #region Static Fields and Constants

    [NotNull]
    private static CrowdControlMod _instance = null!;

    #endregion

    #region Static Methods

    /// <summary>
    ///     Get the crowd control mod instance.
    /// </summary>
    [PublicAPI] [Pure] [NotNull]
    public static CrowdControlMod GetInstance()
    {
        return _instance;
    }

    #endregion

    #region Fields
    
    [NotNull]
    private CrowdControlPlayer _player = null!;
    
    [CanBeNull]
    private Thread _sessionThread;

    private bool _isSessionRunning;

    private bool _isSessionConnected;

    [NotNull]
    private readonly Dictionary<string, CrowdControlEffect> _effects = new();
    
    #endregion

    #region Properties

    /// <summary>
    ///     Session is active if started and successfully connected to the crowd control service.
    /// </summary>
    [PublicAPI]
    public bool IsSessionActive => _isSessionRunning && _isSessionConnected;

    public override uint ExtraPlayerBuffSlots => 32;

    #endregion

    #region Methods

    public override void Load()
    {
        _instance = this;
        
        // Load stuff if not running on a server
        if (Main.netMode != NetmodeID.Server)
        { 
            // TODO: Load shaders
        }
        
        // Add effects
        AddAllEffects();

        // Ignore silent exceptions
        Logging.IgnoreExceptionContents("System.Net.Sockets.Socket.Connect");
        Logging.IgnoreExceptionContents("System.Net.Sockets.Socket.DoConnect");
        Logging.IgnoreExceptionContents("System.Net.Sockets.Socket.Receive");
        
        base.Load();
    }

    public override void Close()
    {
        // Ensure that the session is stopped
        StopCrowdControlSession();

        // Null references
        _player = null!;
        _instance = null!;
        _effects.Clear();

        base.Close();
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        try
        {
            switch (Main.netMode)
            {
                case NetmodeID.MultiplayerClient:
                {
                    HandleClientPacket(reader);
                    break;
                }
                case NetmodeID.Server:
                {
                    HandleServerPacket(reader, whoAmI);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            TerrariaUtils.WriteDebug($"Failed to handle an incoming packet: {e.Message}");
        }

        base.HandlePacket(reader, whoAmI);
    }

    /// <summary>
    ///     Start the crowd control session. Wait after stopping, otherwise the thread might not be ready.
    /// </summary>
    [PublicAPI]
    public void StartCrowdControlSession([NotNull] CrowdControlPlayer player)
    {
        if (_isSessionRunning || _sessionThread != null || Main.netMode == NetmodeID.Server)
        {
            TerrariaUtils.WriteDebug("Could not start the Crowd Control session");
            return;
        }
        
        _player = player;

        _isSessionRunning = true;
        TerrariaUtils.WriteDebug("Started the Crowd Control session");
        
        // Start the connection thread
        _sessionThread = new Thread(HandleSessionConnection);
        _sessionThread.Start();
        
        On.Terraria.Main.Update += OnUpdate;
    }

    /// <summary>
    ///     Stop the crowd control session if it is currently running.
    /// </summary>
    [PublicAPI]
    public void StopCrowdControlSession()
    {
        if (!_isSessionRunning)
        {
            return;
        }

        _player = null!;

        // Allow the threaded method to clean up itself when it exits its loop
        _isSessionRunning = false;
        TerrariaUtils.WriteDebug("Stopped the Crowd Control session");
        
        // Stop all active effects
        foreach (var effect in _effects.Values.Where(effect => effect.IsActive))
        {
            effect.Stop();
        }

        On.Terraria.Main.Update -= OnUpdate;
    }

    /// <summary>
    ///     Get the local crowd control player instance.
    /// </summary>
    [PublicAPI] [Pure] [NotNull]
    public CrowdControlPlayer GetLocalPlayer()
    {
        return _player;
    }
    
    /// <summary>
    ///     Get an effect by id if it is currently active.
    /// </summary>
    [PublicAPI] [Pure]
    public bool TryGetActiveEffect<T>([NotNull] string id, [CanBeNull] out T effect) where T : CrowdControlEffect
    {
        // TODO: Remove method if unused
        
        if (!_effects.TryGetValue(id, out var e) || !e.IsActive)
        {
            effect = null;
            return false;
        }
        
        try
        {
            effect = (T)e;
            return true;
        }
        catch (Exception)
        {
            effect = null;
            return false;
        }
    }

    private void HandleClientPacket(BinaryReader reader)
    {
        if (!IsSessionActive)
        {
            return;
        }
        
        // Determine what to do with the incoming packet
        var packetId = (CrowdControlPacket)reader.ReadByte();
        switch (packetId)
        {
            case CrowdControlPacket.DebugMessage:
            {
                // Let this client handle the debug message
                var message = reader.ReadString();
                var colour = new Color {PackedValue = reader.ReadUInt32()};
                TerrariaUtils.WriteDebug(message, colour);
                break;
            }
            case CrowdControlPacket.EffectMessage:
            {
                // Let this client handle the effect message
                var itemId = reader.ReadInt16();
                var message = reader.ReadString();
                var severity = (EffectSeverity)reader.ReadInt32();
                TerrariaUtils.WriteEffectMessage(itemId, message, severity);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleServerPacket(BinaryReader reader, int sender)
    {
        // Let the server handle the effect packet
        var packetId = (CrowdControlPacket)reader.ReadByte();
        var player = Main.player[sender].GetModPlayer<CrowdControlPlayer>();
        var effectId = reader.ReadString();
        
        // Check that the effect exists
        if (player != null && !string.IsNullOrEmpty(effectId) && _effects.TryGetValue(effectId, out var effect))
        {
            // Let the effect handle the packet
            effect.ReceivePacket(packetId, player, reader); 
            TerrariaUtils.WriteDebug($"'{effectId}' responded to packet '{packetId}' from client '{player.Player.name}'");
        }
    }
    
    private void OnUpdate(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
    {
        if (IsSessionPaused())
        {
            orig.Invoke(self, gameTime);
            return;
        }

        // Update the active effects (so that their timers are reduced)
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var effect in _effects.Values.Where(x => x.IsActive))
        {
            effect.Update(delta);
        }
        
        orig.Invoke(self, gameTime);
    }
    
    private void HandleSessionConnection()
    {
        // Initialisation
        _isSessionConnected = false;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var writeAttempt = true;

        // Connection loop
        while (_isSessionRunning)
        {
            // Attempt to connect the socket to the crowd control service
            try
            {
                socket.Connect("127.0.0.1", 58430);
                _isSessionConnected = true;
            }
            catch (Exception)
            {
                // ignored
            }

            if (_isSessionConnected)
            {
                TerrariaUtils.WriteMessage(ItemID.LargeEmerald, "Connected to Crowd Control", Color.Green);
                
                // Connection successful, so keep polling the socket for incoming packets
                while (_isSessionRunning && socket.Connected && socket.Poll(1000, SelectMode.SelectWrite))
                {
                    // Check if there is any data to receive
                    if (!socket.Poll(1000, SelectMode.SelectRead))
                    {
                        continue;
                    }
                    
                    try
                    {
                        // Read incoming data
                        var buffer = new byte[1024];
                        var size = socket.Receive(buffer);
                        var data = System.Text.Encoding.ASCII.GetString(buffer, 0, size);

                        if (!data.StartsWith("{"))
                        {
                            // No data (or it is invalid), so wait until next poll
                            continue;
                        }
                        
                        // Parse and process the request
                        string response;
                        try
                        {
                            var request = CrowdControlRequest.FromJson(data);
                            var responseStatus = ProcessEffect(request.Code, request.Viewer, (CrowdControlRequestType)request.Type);
                            response = CrowdControlResponse.ToJson(new CrowdControlResponse(request.Id, (int)responseStatus, $"Effect {request.Code}: {responseStatus}"));
                        }
                        catch (Exception e)
                        {
                            response = CrowdControlResponse.ToJson(new CrowdControlResponse(0, 0, e.Message));
                        }

                        // Send a response back to the crowd control service
                        var tmp = System.Text.Encoding.ASCII.GetBytes(response);
                        var outBuffer = new byte[tmp.Length + 1];
                        Array.Copy(tmp, 0, outBuffer, 0, tmp.Length);
                        outBuffer[^1] = 0x00;
                        socket.Send(outBuffer);
                    }
                    catch (Exception)
                    {
                        // TODO: Is it necessary to abort the connection?
                        // If an exception occurs, abort the connection
                        break;
                    }
                }

                // Connection should be closed, so dispose of the socket connection if its still alive
                if (socket.Connected)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    finally
                    {
                        socket.Close();
                    }
                }

                if (_isSessionRunning)
                {
                    TerrariaUtils.WriteMessage(ItemID.LargeRuby, "Lost connection to Crowd Control", Color.Red);
                }
                
                _isSessionConnected = false;
                writeAttempt = true;
            }
            else
            {
                // Connection failed, so wait before attempting to reconnect
                Thread.Sleep(2000);

                if (!writeAttempt)
                {
                    continue;
                }

                writeAttempt = false;
                TerrariaUtils.WriteMessage(ItemID.LargeAmber, "Attempting to connect to Crowd Control", Color.Yellow);
            }
        }

        // Clean up
        _sessionThread = null;
        TerrariaUtils.WriteDebug("Exited the Crowd Control session thread");
    }

    private CrowdControlResponseStatus ProcessEffect([NotNull] string code, [NotNull] string viewer, CrowdControlRequestType requestType)
    {
        // Ensure the session is active (in case of multi-threaded shenanigans)
        if (!IsSessionActive)
        {
            TerrariaUtils.WriteDebug($"Failed to process effect request '{requestType} {code}' as the session is not active");
            return CrowdControlResponseStatus.Failure;
        }
        
        // Ensure the effect is supported
        if (!_effects.TryGetValue(code, out var effect))
        {
            TerrariaUtils.WriteDebug($"Failed to process effect request '{requestType} {code}' as it is not supported by the mod");
            return CrowdControlResponseStatus.Unavailable;
        }
        
        // Re-attempt the effect at a later point if the session is paused
        if (IsSessionPaused() && requestType != CrowdControlRequestType.Stop)
        {
            TerrariaUtils.WriteDebug($"Retrying effect '{requestType} {code}' as the session is paused");
            return CrowdControlResponseStatus.Retry;
        }

        // If the viewer name is blank, or cannot be displayed, then default to 'Chat'
        if (string.IsNullOrEmpty(viewer) || Terraria.Localization.NetworkText.FromLiteral(viewer) == Terraria.Localization.NetworkText.Empty)
        {
            viewer = "Chat";
        }
        
        var result = requestType == CrowdControlRequestType.Start ? effect.Start(viewer) : effect.Stop();
        TerrariaUtils.WriteDebug($"Processed effect request '{requestType} {code}' with response '{result}'");
        return result;
    }

    private bool IsSessionPaused()
    {
        return !IsSessionActive || Main.gamePaused || GetLocalPlayer().Player.dead;
    }

    private void AddEffect([NotNull] CrowdControlEffect effect)
    {
        if (_effects.ContainsKey(effect.Id))
        {
            TerrariaUtils.WriteDebug($"Effect '{effect.Id}' is already added");
            return;
        }
        
        _effects.Add(effect.Id, effect);
    }

    private void AddAllEffects()
    {
        // --- Time effects
        AddEffect(new SetTimeEffect(EffectId.SetTimeNoon, "noon", 27000, true));
        AddEffect(new SetTimeEffect(EffectId.SetTimeMidnight, "midnight", 16200, false));
        AddEffect(new SetTimeEffect(EffectId.SetTimeSunrise, "sunrise", 0, true));
        AddEffect(new SetTimeEffect(EffectId.SetTimeSunset, "sunset", 0, false));
        
        // --- Positive buff effects
        AddEffect(new BuffEffect(EffectId.BuffSurvivability, EffectSeverity.Positive, 12f,
            ItemID.PaladinsShield, (v, p) => $"{v} provided {p} with survivability buffs",
            BuffID.Ironskin, BuffID.Endurance, BuffID.BeetleEndurance1));
        
        AddEffect(new SpawnStructureEffect(EffectId.SpawnStructure));
    }
    
    #endregion
}