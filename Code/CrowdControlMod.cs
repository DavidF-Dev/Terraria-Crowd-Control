using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CrowdControlMod.Code.Utilities;
using CrowdControlMod.Config;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects;
using CrowdControlMod.Effects.BossEffects;
using CrowdControlMod.Effects.BuffEffects;
using CrowdControlMod.Effects.Challenges;
using CrowdControlMod.Effects.Interfaces;
using CrowdControlMod.Effects.InventoryEffects;
using CrowdControlMod.Effects.PlayerEffects;
using CrowdControlMod.Effects.ScreenEffects;
using CrowdControlMod.Effects.WorldEffects;
using CrowdControlMod.Features;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CrowdControlMod;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlMod : Mod
{
    #region Static Methods

    /// <summary>
    ///     Get the crowd control mod instance.
    /// </summary>
    [Pure]
    public static CrowdControlMod GetInstance()
    {
        return ModContent.GetInstance<CrowdControlMod>();
    }

    /// <summary>
    ///     Get the local crowd control player instance (client-side).
    /// </summary>
    [Pure]
    public static CrowdControlPlayer GetLocalPlayer()
    {
        return Main.LocalPlayer.GetModPlayer<CrowdControlPlayer>();
    }

    #endregion

    #region Fields

    private Thread? _sessionThread;

    private Thread? _sessionCallerThread;

    private readonly Queue<string> _responseQueue = new();

    /// <summary>
    ///     Session is running, but might not be connected.
    /// </summary>
    private bool _isSessionRunning;

    /// <summary>
    ///     Session is connected, but might not be running (due to being on a different thread).
    /// </summary>
    private bool _isSessionConnected;

    /// <summary>
    ///     Effects that this mod handles.
    /// </summary>
    private readonly Dictionary<string, CrowdControlEffect> _effects = new();

    /// <summary>
    ///     Special effect providers that this mod recognises.
    /// </summary>
    private readonly Dictionary<string, IEffectProvider> _effectProviders = new();

    /// <summary>
    ///     Features that this mod handles.
    /// </summary>
    private readonly Dictionary<int, IFeature> _features = new();

    private readonly List<string> _rememberedViewerNames = new();

    #endregion

    #region Properties

    /// <summary>
    ///     Session is active if started and successfully connected to the crowd control service.
    /// </summary>
    public bool IsSessionActive => _isSessionRunning && _isSessionConnected;

    /// <summary>
    ///     Effects are paused if the session is not running, Terraria is paused, or the player is dead.
    /// </summary>
    private bool IsSessionPaused => !IsSessionActive || Main.gamePaused || GetLocalPlayer().Player.dead;

    /// <summary>
    ///     Should the session thread continue operating.
    /// </summary>
    private bool ShouldSessionThreadContinue => _isSessionRunning && IsSessionCallerAlive;

    /// <summary>
    ///     Thread that started the session thread is alive.
    /// </summary>
    private bool IsSessionCallerAlive => _sessionCallerThread is {IsAlive: true};

    #endregion

    #region Methods

    public override void Load()
    {
        AddAllEffects();
        AddAllFeatures();

        // Ignore silent exceptions such that they are not displayed in chat
        Logging.IgnoreExceptionContents("System.Net.Sockets.Socket.Connect");
        Logging.IgnoreExceptionContents("System.Net.Sockets.Socket.DoConnect");
        Logging.IgnoreExceptionContents("System.Net.Sockets.Socket.Receive");
    }

    public override void Close()
    {
        // Ensure that the session is stopped
        StopCrowdControlSession();

        // Dispose the effects before clearing them
        foreach (var effect in _effects.Values)
        {
            effect.Dispose();
        }

        _effects.Clear();

        // Dispose the features before clearing them
        foreach (var feature in _features.Values)
        {
            feature.Dispose();
        }

        _features.Clear();

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
                    // Handle incoming packet as the multiplayer client
                    HandleClientPacket(reader);
                    break;
                }
                case NetmodeID.Server:
                {
                    // Handle incoming packet as the server
                    HandleServerPacket(reader, whoAmI);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            TerrariaUtils.WriteDebug($"Failed to handle an incoming packet: {e.Message}");
        }
    }

    /// <summary>
    ///     Start the crowd control session. Wait after stopping, otherwise the thread might not be ready (client-side).
    /// </summary>
    public void StartCrowdControlSession()
    {
        // Cannot start if already running, or the thread is active in the background, or we're on a server
        if (_isSessionRunning || _sessionThread != null || NetUtils.IsServer)
        {
            TerrariaUtils.WriteDebug("Could not start the Crowd Control session");
            return;
        }

        _isSessionRunning = true;
        _sessionCallerThread = Thread.CurrentThread;
        _rememberedViewerNames.Clear();

        // Initialise the effects
        foreach (var effect in _effects.Values)
        {
            effect.SessionStarted();
        }

        // Initialise the features
        foreach (var feature in _features.Values)
        {
            feature.SessionStarted();
        }

        // Start the connection thread
        _sessionThread = new Thread(HandleSessionConnection);
        _sessionThread.Start();

        CrowdControlModSystem.GameUpdateHook += OnGameUpdate;
    }

    /// <summary>
    ///     Stop the crowd control session if it is currently running (client-side).
    /// </summary>
    public void StopCrowdControlSession()
    {
        // Cannot stop unless the session is running (obviously!)
        if (!_isSessionRunning)
        {
            return;
        }

        // Allow the threaded method to clean up itself when it exits its loop
        _isSessionRunning = false;
        TerrariaUtils.WriteDebug("Stopped the Crowd Control session");

        // Stop effects
        foreach (var effect in _effects.Values.Where(x => x.IsActive))
        {
            effect.Stop();
        }

        // Notify effects that the session has been stopped
        foreach (var effect in _effects.Values)
        {
            effect.SessionStopped();
        }

        // Notify features that the session has been stopped
        foreach (var feature in _features.Values)
        {
            feature.SessionStopped();
        }

        // Clear response queue
        lock (_responseQueue)
        {
            _responseQueue.Clear();
        }

        CrowdControlModSystem.GameUpdateHook -= OnGameUpdate;
    }

    /// <summary>
    ///     Queue a response to be sent to Crowd Control by an effect.<br />
    ///     Used by <see cref="CrowdControlEffect" /> when pausing/resuming/finishing.
    /// </summary>
    public void QueueResponseToCrowdControl(int effectNetId, CrowdControlEffect effect, CrowdControlResponseStatus status)
    {
        if (!IsSessionActive)
        {
            // Ignore
            return;
        }

        if (effectNetId == -1)
        {
            TerrariaUtils.WriteDebug($"Attempted to queue a response for '{effect.Id}' with an invalid {nameof(effectNetId)}: {effectNetId}");
            return;
        }

        // Generate a response and queue it to be sent in the connection thread
        var timeRemaining = (int)(effect.TimeLeft * 1000);
        var response = new CrowdControlResponse(effectNetId, (int)status, $"{effect.Id}: {status}", timeRemaining);
        var json = CrowdControlResponse.ToJson(response);
        lock (_responseQueue)
        {
            _responseQueue.Enqueue(json);
        }
    }

    /// <summary>
    ///     Attempt to get a feature by id.
    /// </summary>
    [Pure]
    public T? GetFeature<T>(int id) where T : IFeature
    {
        return _features.TryGetValue(id, out var feature) ? feature is T castedFeature ? castedFeature : default : default;
    }

    /// <summary>
    ///     Check whether the provided effect is currently active (client-side).
    /// </summary>
    [Pure]
    public bool IsEffectActive(string id)
    {
        return _effects.TryGetValue(id, out var effect) && effect.IsActive;
    }

    /// <summary>
    ///     Get an effect instance by id.
    /// </summary>
    [Pure]
    public CrowdControlEffect? GetEffect(string id)
    {
        return _effects.GetValueOrDefault(id);
    }

    /// <summary>
    ///     Get all the effects supported by the mod.
    /// </summary>
    [Pure]
    public IEnumerable<CrowdControlEffect> GetEffects(bool active)
    {
        return active ? _effects.Values.Where(x => x.IsActive) : _effects.Values;
    }

    /// <summary>
    ///     Check if any of the effects want to play music.
    /// </summary>
    public bool TryGetEffectMusic(out int musicId)
    {
        if (!CrowdControlConfig.GetInstance().UseEffectMusic)
        {
            // No music if disabled in the mod configuration
            musicId = 0;
            return false;
        }

        var priority = int.MinValue;
        musicId = 0;
        foreach (var effect in _effects.Values)
        {
            // Ignore if the effect does not play music or is too low priority
            if (!effect.IsActive || effect is not IMusicEffect musicEffect || musicEffect.MusicPriority <= priority)
            {
                continue;
            }

            // Update the music and current priority
            musicId = musicEffect.MusicId;
            priority = musicEffect.MusicPriority;
        }

        return musicId != 0;
    }

    /// <summary>
    ///     Get the remembered viewer names.
    /// </summary>
    public IReadOnlyList<string> GetRememberedViewerNames()
    {
        return _rememberedViewerNames;
    }

    private void HandleClientPacket(BinaryReader reader)
    {
        // Determine what to do with the incoming packet (client-side)
        // Note; this runs even if the session is not active
        var packetId = (PacketID)reader.ReadByte();
        switch (packetId)
        {
            // Let this client handle the debug message
            case PacketID.DebugMessage:
            {
                var message = reader.ReadString();
                var colour = new Color {PackedValue = reader.ReadUInt32()};
                TerrariaUtils.WriteDebug(message, colour);
                return;
            }

            // Let this client handle the effect message
            case PacketID.EffectMessage:
            {
                var itemId = reader.ReadInt16();
                var message = reader.ReadString();
                var severity = (EffectSeverity)reader.ReadInt32();
                TerrariaUtils.WriteEffectMessage(itemId, message, severity);
                break;
            }

            // Sync the weather sent from the server
            case PacketID.SyncWeather:
            {
                Main.cloudAlpha = reader.ReadSingle();
                Main.windSpeedTarget = reader.ReadSingle();
                // Terraria.Main.windSpeedCurrent = Terraria.Main.windSpeedTarget;
                Main.windCounter = reader.ReadInt32();
                Main.extremeWindCounter = reader.ReadInt32();
                TerrariaUtils.WriteDebug($"Synced weather from server (cloud={Main.cloudAlpha}, speed={Main.windSpeedTarget}, wind={Main.windCounter} extreme={Main.extremeWindCounter})");
                break;
            }

            // Sync the given NPC in non-vanilla ways
            case PacketID.SyncNPCSpecial:
            {
                var npcWhoAmI = reader.ReadInt32();
                var lifeMax = reader.ReadInt32();
                var life = reader.ReadInt32();
                var npc = Main.npc[npcWhoAmI];
                npc.lifeMax = lifeMax;
                npc.life = life;
                break;
            }

            // Sync the given item in non-vanilla ways
            case PacketID.SyncItemSpecial:
            {
                var itemWhoAmI = reader.ReadInt32();
                var noGrabDelay = reader.ReadInt32();
                var item = Main.item[itemWhoAmI];
                item.noGrabDelay = noGrabDelay;
                break;
            }

            // Cause a player to fart
            case PacketID.Fart:
            {
                FartEffect.HandleClientFart(Main.player[reader.ReadInt32()]);
                break;
            }

            // Client wants to change its morph
            case PacketID.SyncMorph:
                MorphUtils.HandleSync(reader);
                break;

            // Client wants to spawn a new gore
            case PacketID.SyncNewGore:
                NetUtils.HandleSyncNewGore(reader, -1);
                break;
        }
    }

    private void HandleServerPacket(BinaryReader reader, int sender)
    {
        // Read the packet and determine what to do with it (server-side)
        var packetId = (PacketID)reader.ReadByte();
        var player = Main.player[sender].GetModPlayer<CrowdControlPlayer>();
        switch (packetId)
        {
            // Client wants the server to broadcast a chat message
            case PacketID.BroadcastMessage:
            {
                var netText = NetworkText.Deserialize(reader);
                var colour = default(Color);
                colour.PackedValue = reader.ReadUInt32();
                var excludedPlayer = reader.ReadInt32();
                TerrariaUtils.WriteDebug($"Server is forwarding broadcast message: \"{TerrariaUtils.GetColouredRichText(netText.ToString(), colour)}\", excluding {excludedPlayer}.");
                ChatHelper.BroadcastChatMessage(netText, colour, excludedPlayer);
                break;
            }

            // Client is letting the server know about their configuration settings
            case PacketID.ConfigState:
                player.ServerDisableTombstones = reader.ReadBoolean();
                player.ServerForcefullyDespawnBosses = reader.ReadBoolean();
                TerrariaUtils.WriteDebug($"Server received config for '{player.Player.name}' (disableTombstones={player.ServerDisableTombstones}, despawnBosses={player.ServerForcefullyDespawnBosses})");
                break;

            // Client wants to trigger an effect on the server
            case PacketID.HandleEffect:
            case PacketID.EffectStatus:
            {
                var effectId = reader.ReadString();

                // Check that the effect exists
                if (player != null && !string.IsNullOrEmpty(effectId) && _effects.TryGetValue(effectId, out var effect))
                {
                    // Let the effect handle the packet
                    effect.ReceivePacket(packetId, player, reader);
                    TerrariaUtils.WriteDebug($"'{effectId}' responded to packet '{packetId}' from client '{player.Player.name}'");
                }

                break;
            }

            // Client wants to despawn an NPC on the server & update clients
            case PacketID.DespawnNPC:
                var whoAmI = reader.ReadInt32();
                Main.npc[whoAmI].active = false;
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, whoAmI);
                break;

            // Client wants to change its morph
            case PacketID.SyncMorph:
                MorphUtils.HandleSync(reader);
                break;

            // Client wants to spawn a new gore
            case PacketID.SyncNewGore:
                NetUtils.HandleSyncNewGore(reader, sender);
                break;
        }
    }

    private void HandleSessionConnection()
    {
        // Note; this is run on a managed thread

        const string host = "127.0.0.1";
        const int port = 58430;
        const int pollTimeout = 20000; // 0.02s (1000000 = 1s)
        const int connectDelay = 1000; // 1s (1000 = 1s)
        var specialMessageColour = new Color(90, 136, 252); // blue-ish

        // Initialisation
        _isSessionConnected = false;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var writeAttempt = true;

        // Wait for the world to load
        while (ShouldSessionThreadContinue && Main.gameMenu)
        {
        }

        TerrariaUtils.WriteDebug("Started the Crowd Control session");
        if (ShouldSessionThreadContinue && NetUtils.IsClient)
        {
            // Send the client's config settings to the server
            CrowdControlConfig.GetInstance().SendConfigToServer();
        }

        ref var isFirstTimeUser = ref GetLocalPlayer().IsFirstTimeUser;
        if (isFirstTimeUser)
        {
            // Send a special greeting message when playing a world for the first time with the mod
            TerrariaUtils.WriteMessage(LangUtils.FirstTimeStartText, specialMessageColour);
        }

        // Connection loop
        while (ShouldSessionThreadContinue)
        {
            // Attempt to connect the socket to the crowd control service
            try
            {
                socket.Connect(host, port);
                _isSessionConnected = true;
            }
            catch (Exception)
            {
                // ignored
            }

            if (_isSessionConnected)
            {
                TerrariaUtils.WriteMessage(ItemID.LargeEmerald, LangUtils.ConnectedText, Color.Green);
                if (Version.Major < 2)
                {
                    TerrariaUtils.WriteMessage(ItemID.BrokenHeroSword, LangUtils.ConnectedOutOfDateText, Color.White);
                }

                // Connection successful, so keep checking for received data whilst the socket remains connected
                while (ShouldSessionThreadContinue && socket.Connected)
                {
                    // If there is no data to receive, check the response queue in case we should send anything back to Crowd Control
                    // The response queue handles Pause/Resume/Finish packets from effects
                    if (!socket.Poll(pollTimeout, SelectMode.SelectRead))
                    {
                        lock (_responseQueue)
                        {
                            if (_responseQueue.Count > 0)
                            {
                                HandleResponseQueue(socket);
                            }
                        }

                        continue;
                    }

                    // There is data to read
                    try
                    {
                        // Read incoming data
                        var buffer = new byte[1024];
                        var size = socket.Receive(buffer);
                        if (size == 0)
                        {
                            // A packet with zero bytes means the remote connection has closed, so break out of the loop
                            break;
                        }

                        var data = Encoding.ASCII.GetString(buffer, 0, size);
                        if (!data.StartsWith("{"))
                        {
                            // No data (or it is invalid), so wait until next poll
                            continue;
                        }

                        // Parse and process the request
                        string response;
                        try
                        {
                            // Read the request object
                            TerrariaUtils.WriteDebug($"Incoming request: {data}");
                            var request = CrowdControlRequest.FromJson(data);

                            // Process the request
                            ProcessEffectResult processResult;
                            try
                            {
                                processResult = ProcessEffect(request.Id, request.Code, request.Viewer, request.Duration / 1000, (CrowdControlRequestType)request.Type);
                            }
                            catch (Exception e)
                            {
                                TerrariaUtils.WriteDebug($"Cannot process Crowd Control effect '{request.Code}' due to an exception {e.Message}");
                                processResult = CrowdControlResponseStatus.Failure;
                            }

                            // Create a response object
                            var timeRemaining = processResult.Effect == null ? 0 : (int)(processResult.Effect.TimeLeft * 1000);
                            response = CrowdControlResponse.ToJson(new CrowdControlResponse(request.Id, (int)processResult.Response, $"{request.Code}: {processResult.Response}", timeRemaining));
                            TerrariaUtils.WriteDebug($"Outgoing response: {response}");
                        }
                        catch (Exception e)
                        {
                            TerrariaUtils.WriteDebug($"Cannot parse Crowd Control request due to an exception: {e.Message}");
                            response = CrowdControlResponse.ToJson(new CrowdControlResponse(0, 0, e.Message, 0));
                        }

                        // Send a response back to the crowd control service
                        var tmp = Encoding.ASCII.GetBytes(response);
                        var outBuffer = new byte[tmp.Length + 1];
                        Array.Copy(tmp, 0, outBuffer, 0, tmp.Length);
                        outBuffer[^1] = 0x00;
                        socket.Send(outBuffer);
                    }
                    catch (Exception e)
                    {
                        // If an exception occurs, abort the connection
                        TerrariaUtils.WriteDebug($"Aborted connection due to an exception: {e.Message}");
                        break;
                    }
                }

                // Disconnect and shutdown - then create a new socket
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    socket.Dispose();
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }

                _isSessionConnected = false;
                writeAttempt = true;

                if (!ShouldSessionThreadContinue)
                {
                    continue;
                }

                TerrariaUtils.WriteMessage(ItemID.LargeRuby, LangUtils.DisconnectedText, Color.Red);
                if (!isFirstTimeUser)
                {
                    continue;
                }

                // Send a special farewell message when playing a world for the first time with the mod
                TerrariaUtils.WriteMessage(LangUtils.FirstTimeStopText, specialMessageColour);
                isFirstTimeUser = false;
            }
            else
            {
                // Connection failed, so wait before attempting to reconnect
                Thread.Sleep(connectDelay);

                if (!writeAttempt)
                {
                    continue;
                }

                writeAttempt = false;
                TerrariaUtils.WriteMessage(ItemID.LargeAmber, LangUtils.ConnectingText, Color.Yellow);
            }
        }

        // Clean up
        socket.Dispose();
        _sessionThread = null;

        if (IsSessionCallerAlive)
        {
            TerrariaUtils.WriteDebug("Exited the Crowd Control session thread");
        }

        // Thread is about to end, so null the caller thread reference here
        _sessionCallerThread = null;
    }

    private void HandleResponseQueue(Socket socket)
    {
        // Check if the response queue has any entries to be sent over the socket
        lock (_responseQueue)
        {
            while (_responseQueue.Count > 0)
            {
                // Send each response over the socket connection
                var response = _responseQueue.Dequeue();
                if (string.IsNullOrEmpty(response))
                {
                    continue;
                }

                try
                {
                    var tmp = Encoding.ASCII.GetBytes(response);
                    var outBuffer = new byte[tmp.Length + 1];
                    Array.Copy(tmp, 0, outBuffer, 0, tmp.Length);
                    outBuffer[^1] = 0x00;
                    socket.Send(outBuffer);
                    TerrariaUtils.WriteDebug($"Outgoing response: {response}");
                }
                catch (Exception e)
                {
                    // If an exception occurs, abort the connection
                    TerrariaUtils.WriteDebug($"Aborted connection due to an exception: {e.Message}");
                    break;
                }
            }
        }
    }

    private ProcessEffectResult ProcessEffect(int netId, string code, string viewer, int duration, CrowdControlRequestType requestType)
    {
        // Ensure the session is active (in case of multi-threaded shenanigans)
        if (!IsSessionActive)
        {
            TerrariaUtils.WriteDebug($"Failed to process effect request '{code}' as the session is not active");
            return CrowdControlResponseStatus.Failure;
        }

        // Check if this effect code should be handled specially
        if (_effectProviders.TryGetValue(code, out var provider))
        {
            // Get the provided ids and attempt to process them
            // Note, an infinite loop is possible if the provider processes another (or the same) provider
            var providerIds = provider.GetEffectIds(requestType);
            if (requestType == CrowdControlRequestType.Start)
            {
                // Process the provided effect; ensuring there is only one
                return providerIds.Count switch
                {
                    0 => CrowdControlResponseStatus.Failure,
                    > 1 => CrowdControlResponseStatus.Failure,
                    _ => ProcessEffect(netId, providerIds.First(), viewer, duration, requestType)
                };
            }

            // Simply stop all provided effects
            foreach (var providerId in providerIds)
            {
                ProcessEffect(netId, providerId, viewer, duration, requestType);
            }

            return CrowdControlResponseStatus.Success;
        }

        // Ensure the effect is supported
        if (!_effects.TryGetValue(code, out var effect))
        {
            TerrariaUtils.WriteDebug($"Failed to process effect request '{code}' as it is not supported by the mod");
            return CrowdControlResponseStatus.Unavailable;
        }

        // Re-attempt the effect at a later point if the session is paused
        if (IsSessionPaused && requestType != CrowdControlRequestType.Stop)
        {
            TerrariaUtils.WriteDebug($"Retrying effect '{code}' as the session is paused");
            return CrowdControlResponseStatus.Retry;
        }

        // If the viewer name should be anonymous, or is empty, then default to 'Chat'
        if (CrowdControlConfig.GetInstance().UseAnonymousNamesInChat || string.IsNullOrEmpty(viewer) || NetworkText.FromLiteral(viewer) == NetworkText.Empty)
        {
            viewer = "Chat";
        }

        // Start or stop the effect
        CrowdControlResponseStatus result;
        if (requestType == CrowdControlRequestType.Start)
        {
            result = effect.Start(netId, viewer, duration);
            if (result == CrowdControlResponseStatus.Success)
            {
                RememberViewer(viewer);
            }
        }
        else
        {
            effect.Stop(true);
            result = CrowdControlResponseStatus.Success;
        }

        // TerrariaUtils.WriteDebug($"Processed effect request '{requestType} {code}' with response '{result}'");
        return new ProcessEffectResult(effect, result);
    }

    private void AddEffect(CrowdControlEffect effect)
    {
        if (_effects.ContainsKey(effect.Id))
        {
            TerrariaUtils.WriteDebug($"Effect '{effect.Id}' is already added");
            return;
        }

        // ReSharper disable once SuspiciousTypeConversion.Global (unimplemented feature)
        // Note that in a future Crowd Control api, this may be determined when the session starts
        if (effect is IModEffect modEffect && !string.IsNullOrEmpty(modEffect.ModName) && !ModUtils.TryGetMod(modEffect.ModName, out _))
        {
            effect.Dispose();
            TerrariaUtils.WriteDebug($"Effect '{effect.Id}' is unavailable because a required mod is not active: '{modEffect.ModName}'");
            return;
        }

        _effects.Add(effect.Id, effect);
    }

    private void AddEffectProvider(string id, IEffectProvider provider)
    {
        if (_effectProviders.ContainsKey(id))
        {
            TerrariaUtils.WriteDebug($"Effect provider '{id}' is already added");
            return;
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        // Note that in a future Crowd Control api, this may be determined when the session starts
        if (provider is IModEffect modEffectProvider && !string.IsNullOrEmpty(modEffectProvider.ModName) && !ModUtils.TryGetMod(modEffectProvider.ModName, out _))
        {
            TerrariaUtils.WriteDebug($"Effect provider '{id}' is unavailable because a required mod is not active: '{modEffectProvider.ModName}'");
            return;
        }

        _effectProviders.Add(id, provider);
    }

    private void AddAllEffects()
    {
        // --- Player effects
        AddEffect(new KillPlayerEffect());
        AddEffect(new ExplodePlayerEffect());
        AddEffect(new HealPlayerEffect());
        AddEffect(new DamagePlayerEffect());
        AddEffect(new GodModeEffect(30));
        AddEffect(new SetMaxStatEffect(EffectID.IncreaseMaxLife, true, true));
        AddEffect(new SetMaxStatEffect(EffectID.DecreaseMaxLife, false, true));
        AddEffect(new SetMaxStatEffect(EffectID.IncreaseMaxMana, true, false));
        AddEffect(new SetMaxStatEffect(EffectID.DecreaseMaxMana, false, false));
        AddEffect(new IncreaseSpawnRateEffect(25));
        AddEffect(new InfiniteAmmoEffect(30));
        AddEffect(new TeleportToDeathEffect());
        AddEffect(new GivePetEffect(GivePetEffect.PetType.Pet));
        AddEffect(new GivePetEffect(GivePetEffect.PetType.LightPet));
        AddEffect(new ChangeGenderEffect());
        AddEffect(new ForceMountEffect(25));
        AddEffect(new ShootExplosives(15, ShootExplosives.Shoot.Bombs));
        AddEffect(new ShootExplosives(20, ShootExplosives.Shoot.Grenades));
        AddEffect(new IncreaseKnockbackEffect(40));
        AddEffect(new JumpBoostEffect(25));
        AddEffect(new RunBoostEffect(25));
        AddEffect(new IcyFeetEffect(25));
        AddEffect(new FlingUpwardsEffect());
        AddEffect(new FartEffect());
        AddEffect(new HiccupEffect(60));

        // --- Buff effects (positive)
        AddEffect(new BuffEffect(EffectID.BuffSurvivability, EffectSeverity.Positive, 60,
            ItemID.PaladinsShield, -1, null,
            BuffID.Ironskin, BuffID.Endurance, BuffID.CatBast));
        AddEffect(new BuffEffect(EffectID.BuffRegen, EffectSeverity.Positive, 60,
            ItemID.Heart, -1,
            p => p.Player.SetHairDye(ItemID.LifeHairDye),
            BuffID.Regeneration, BuffID.SoulDrain, BuffID.HeartyMeal, BuffID.ManaRegeneration, BuffID.Lovestruck));
        AddEffect(new BuffEffect(EffectID.BuffLight, EffectSeverity.Positive, 60,
            ItemID.MagicLantern, -1,
            p => p.Player.SetHairDye(ItemID.MartianHairDye),
            BuffID.NightOwl, BuffID.Shine));
        AddEffect(new BuffEffect(EffectID.BuffTreasure, EffectSeverity.Positive, 60,
            ItemID.GoldChest, -1,
            p => p.Player.SetHairDye(ItemID.DepthHairDye),
            BuffID.Spelunker, BuffID.Hunter, BuffID.Dangersense));
        AddEffect(new BuffEffect(EffectID.BuffMovement, EffectSeverity.Positive, 60,
            ItemID.Aglet, EmoteID.PartyCake, null,
            BuffID.Swiftness, BuffID.SugarRush, BuffID.Panic, BuffID.WaterWalking, BuffID.Sunflower));
        AddEffect(new BuffEffect(EffectID.BuffObsidianSkin, EffectSeverity.Positive, 60,
            ItemID.ObsidianSkull, EmoteID.MiscFire, null,
            BuffID.ObsidianSkin, BuffID.Warmth));
        AddEffect(new BuffEffect(EffectID.BuffMiningSpeed, EffectSeverity.Positive, 60,
            ItemID.ShroomiteDiggingClaw, EmoteID.ItemPickaxe, null,
            BuffID.Mining, BuffID.SugarRush));
        AddEffect(new BuffEffect(EffectID.BuffSwimming, EffectSeverity.Positive, 60,
            ItemID.Flipper, -1, null,
            BuffID.Gills, BuffID.Flipper, BuffID.WaterWalking, BuffID.Merfolk, BuffID.Wet));

        // --- Buff effects (negative)
        AddEffect(new BuffEffect(EffectID.BuffFreeze, EffectSeverity.Negative, 8,
            ItemID.IceCream, -1, null,
            BuffID.Frozen));
        AddEffect(new BuffEffect(EffectID.BuffFire, EffectSeverity.Negative, 10,
            ItemID.LivingFireBlock, EmoteID.DebuffBurn,
            p => Projectile.NewProjectile(null, p.Player.position, new Vector2(0f, 10f), ProjectileID.MolotovCocktail, 1, 1f, p.Player.whoAmI),
            BuffID.OnFire));
        AddEffect(new BuffEffect(EffectID.BuffDaze, EffectSeverity.Negative, 20,
            ItemID.FallenStar, -1, null,
            BuffID.Dazed, BuffID.NoBuilding));
        AddEffect(new BuffEffect(EffectID.BuffLevitate, EffectSeverity.Negative, 25,
            ItemID.FragmentVortex, -1, null,
            BuffID.VortexDebuff));
        AddEffect(new BuffEffect(EffectID.BuffConfuse, EffectSeverity.Negative, 25,
            ItemID.BrainOfConfusion, EmoteID.EmoteConfused, null,
            BuffID.Confused));
        AddEffect(new BuffEffect(EffectID.BuffInvisible, EffectSeverity.Neutral, 30,
            ItemID.InvisibilityPotion, -1, null,
            BuffID.Invisibility));
        AddEffect(new BuffEffect(EffectID.BuffBlind, EffectSeverity.Negative, 25,
            ItemID.Blindfold, -1,
            p => p.Player.SetHairDye(ItemID.TwilightHairDye),
            BuffID.Obstructed));
        AddEffect(new BuffEffect(EffectID.BuffCurse, EffectSeverity.Negative, 8,
            ItemID.DemonScythe, EmoteID.DebuffCurse, null,
            BuffID.Cursed, BuffID.NoBuilding));
        AddEffect(new NoclipEffect(2));

        // -- Inventory effects
        AddEffect(new DropItemEffect());
        AddEffect(new ExplodeInventoryEffect());
        AddEffect(new ClearInventoryEffect());
        AddEffect(new ShuffleInventoryEffect());
        AddEffect(new SwitchLoadoutEffect());
        AddEffect(new NoItemPickupEffect(20));
        AddEffect(new ItemMagnetEffect(true, 25));
        AddEffect(new ItemMagnetEffect(false, 25));
        AddEffect(new ReforgeItemEffect());
        AddEffect(new MoneyBoostEffect(60));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Pickaxe));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Sword));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Yoyo));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Magic));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Summon));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Ranged));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Armour));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Accessory));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Vanity));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.HealingPotion));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Potion));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Food));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Kite));
        AddEffect(new SpawnPiggyBankEffect());

        // --- World effects
        AddEffect(new UseDialEffect(true));
        AddEffect(new UseDialEffect(false));
        AddEffect(new SetTimeEffect(EffectID.SetTimeNoon, 26600, true));
        AddEffect(new SetTimeEffect(EffectID.SetTimeMidnight, 15800, false));
        AddEffect(new SetTimeEffect(EffectID.SetTimeSunrise, 0, true));
        AddEffect(new SetTimeEffect(EffectID.SetTimeSunset, 0, false));
        AddEffect(new SpawnStructureEffect());
        AddEffect(new TrapEffect(TrapEffect.TrapType.Cobweb));
        AddEffect(new TrapEffect(TrapEffect.TrapType.Sand));
        AddEffect(new TrapEffect(TrapEffect.TrapType.Water));
        AddEffect(new TrapEffect(TrapEffect.TrapType.Honey));
        AddEffect(new TrapEffect(TrapEffect.TrapType.Lava));
        AddEffect(new RandomTeleportEffect());
        AddEffect(new SummonNpcsEffect());
        AddEffect(new SpawnTownNPCEffect());
        AddEffect(new RainbowFeetEffect(60));
        AddEffect(new SpawnGuardian(false));
        AddEffect(new SpawnGuardian(true));
        AddEffect(new GoldenSlimeRainEffect(60));
        AddEffect(new SpawnCritters());
        AddEffect(new SetWeatherEffect(WorldUtils.Weather.Clear));
        AddEffect(new SetWeatherEffect(WorldUtils.Weather.Rain));
        AddEffect(new SetWeatherEffect(WorldUtils.Weather.Storm));
        AddEffect(new SetWeatherEffect(WorldUtils.Weather.Windy));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.ForTheWorthy, true));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.ForTheWorthy, false));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.ForTheWorthy, 60 * 5));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.DontStarve, true));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.DontStarve, false));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.DontStarve, 60 * 5));
        AddEffect(new SwitchSoundtrack());
        AddEffect(new ShuffleSfxEffect(45));
        AddEffect(new MysteryBlocksEffect(30));
        AddEffect(new FloorIsLavaEffect(25));

        // --- Boss effects
        AddEffect(new SpawnRandomBossEffect());
        AddEffect(new SpawnBossEffect(NPCID.KingSlime));
        AddEffect(new SpawnBossEffect(NPCID.EyeofCthulhu));
        AddEffect(new SpawnBossEffect(NPCID.EaterofWorldsHead));
        AddEffect(new SpawnBossEffect(NPCID.BrainofCthulhu));
        AddEffect(new SpawnBossEffect(NPCID.QueenBee));
        AddEffect(new SpawnBossEffect(NPCID.SkeletronHead));
        AddEffect(new SpawnBossEffect(NPCID.Deerclops));
        AddEffect(new SpawnBossEffect(NPCID.WallofFlesh));
        AddEffect(new SpawnBossEffect(NPCID.QueenSlimeBoss));
        AddEffect(new SpawnBossEffect(NPCID.Retinazer));
        AddEffect(new SpawnBossEffect(NPCID.TheDestroyer));
        AddEffect(new SpawnBossEffect(NPCID.SkeletronPrime));
        AddEffect(new SpawnBossEffect(NPCID.Plantera));
        AddEffect(new SpawnBossEffect(NPCID.Golem));
        AddEffect(new SpawnBossEffect(NPCID.DukeFishron));
        AddEffect(new SpawnBossEffect(NPCID.HallowBoss));
        AddEffect(new SpawnBossEffect(NPCID.MoonLordCore));

        // --- Screen effects
        AddEffect(new FlipScreenEffect(25));
        AddEffect(new DrunkModeEffect(25));
        AddEffect(new ZoomEffect(25, true));
        AddEffect(new ZoomEffect(25, false));
        AddEffect(new WallOfFishEffect(25));
        AddEffect(new CritterTakeoverEffect(60));
        AddEffect(new ScreenShakeEffect(20));
        AddEffect(new SniperModeEffect(25));
        AddEffect(new MonolithEffect(EffectID.MonolithShimmer, 30, MonolithEffect.MonolithType.Shimmer));
        AddEffect(new MonolithEffect(EffectID.MonolithMoonLord, 30, MonolithEffect.MonolithType.MoonLord));

        // --- Challenge effects
        AddEffectProvider(EffectID.RandomChallenge, new RandomChallengeEffectProvider());
        AddEffect(new SwimChallenge(30));
        AddEffect(new StandOnBlockChallenge(40));
        AddEffect(new CraftItemChallenge(40));
        AddEffect(new SleepChallenge(30));
        AddEffect(new SitChallenge(30));
        AddEffect(new CatchCritterChallenge(30));
        AddEffect(new MinecartChallenge(30));
        AddEffect(new TouchGrassChallenge(15));
        AddEffect(new EatFoodChallenge(30));
        AddEffect(new WordPuzzleChallenge(30));
    }

    private void AddAllFeatures()
    {
        _features.Add(FeatureID.DespawnNPC, new DespawnNPCFeature());
        _features.Add(FeatureID.PlayerTeleportation, new PlayerTeleportationFeature());
        _features.Add(FeatureID.ReduceRespawnTime, new ReduceRespawnTimeFeature());
        _features.Add(FeatureID.RemoveTombstone, new RemoveTombstoneFeature());
        _features.Add(FeatureID.TimedEffectDisplay, new TimedEffectDisplayFeature());
        _features.Add(FeatureID.MorphUntilDeath, new MorphUntilDeathFeature());
        _features.Add(FeatureID.KaylaEgg, new KaylaEggFeature());
        _features.Add(FeatureID.OfficialConduitEgg, new OfficialConduitEggFeature());
        _features.Add(FeatureID.MoonlitFayeAndMakenBaconEgg, new MoonlitFayeAndMakenBaconEggFeature());
        _features.Add(FeatureID.RespawnImmunity, new RespawnImmunityFeature());
    }

    private void RememberViewer(string viewer)
    {
        if (string.IsNullOrEmpty(viewer) || viewer is "Chat" or "a ghost")
        {
            return;
        }

        _rememberedViewerNames.Remove(viewer);
        _rememberedViewerNames.Add(viewer);
    }

    private void OnGameUpdate(GameTime gameTime)
    {
        // Update the active effects (so that their timers are reduced)
        var sessionPaused = IsSessionPaused;
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var effect in _effects.Values)
        {
            if (!effect.IsActive)
            {
                continue;
            }

            // Check if the effect should be paused or resumed
            var shouldUpdate = !sessionPaused && effect.ShouldUpdate();
            switch (shouldUpdate)
            {
                case false when !effect.IsPaused:
                    effect.Pause();
                    break;
                case true when effect.IsPaused:
                    effect.Resume();
                    break;
            }

            if (effect.IsPaused)
            {
                // Ignore paused effect
                continue;
            }

            // Update the effect
            effect.Update(delta);
        }
    }

    #endregion

    #region Nested Types

    private readonly struct ProcessEffectResult
    {
        #region Static Methods

        public static implicit operator ProcessEffectResult(CrowdControlResponseStatus response)
        {
            return new ProcessEffectResult(null, response);
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Processed effect, if any.
        /// </summary>
        public readonly CrowdControlEffect? Effect;

        /// <summary>
        ///     Resulting response.
        /// </summary>
        public readonly CrowdControlResponseStatus Response;

        #endregion

        #region Constructors

        public ProcessEffectResult(CrowdControlEffect? effect, CrowdControlResponseStatus response)
        {
            Effect = effect;
            Response = response;
        }

        #endregion
    }

    #endregion
}