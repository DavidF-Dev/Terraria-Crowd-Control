using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

    #endregion

    #region Properties

    /// <summary>
    ///     Session is active if started and successfully connected to the crowd control service.
    /// </summary>
    public bool IsSessionActive => _isSessionRunning && _isSessionConnected;

    public override uint ExtraPlayerBuffSlots => 32;

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
        // Add effects
        AddAllEffects();

        // Add features
        _features.Add(FeatureID.DespawnNPC, new DespawnNPCFeature());
        _features.Add(FeatureID.PlayerTeleportation, new PlayerTeleportationFeature());
        _features.Add(FeatureID.ReduceRespawnTime, new ReduceRespawnTimeFeature());
        _features.Add(FeatureID.RemoveTombstone, new RemoveTombstoneFeature());
        _features.Add(FeatureID.TimedEffectDisplay, new TimedEffectDisplayFeature());

        // Ignore silent exceptions
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
        if (_isSessionRunning || _sessionThread != null || Main.netMode == NetmodeID.Server)
        {
            TerrariaUtils.WriteDebug("Could not start the Crowd Control session");
            return;
        }

        _isSessionRunning = true;
        _sessionCallerThread = Thread.CurrentThread;

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
    public void QueueResponseToCrowdControl(int effectNetId, string effectId, CrowdControlResponseStatus status)
    {
        if (!IsSessionActive)
        {
            // Ignore
            return;
        }

        if (effectNetId == -1)
        {
            TerrariaUtils.WriteDebug($"Attempted to queue a response for '{effectId}' with an invalid {nameof(effectNetId)}: {effectNetId}");
            return;
        }

        // Generate a response and queue it to be sent in the connection thread
        var response = new CrowdControlResponse(effectNetId, (int)status, $"{effectId}: {status}");
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
        return _effects.TryGetValue(id, out var effect) ? effect : null;
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
        foreach (var effect in _effects.Values.Where(x => x.IsActive))
        {
            // Ignore if the effect does not play music or is too low priority
            if (effect is not IMusicEffect musicEffect || musicEffect.MusicPriority <= priority)
            {
                continue;
            }

            // Update the music and current priority
            musicId = musicEffect.MusicId;
            priority = musicEffect.MusicPriority;
        }

        return musicId != 0;
    }

    private void HandleClientPacket(BinaryReader reader)
    {
        // Determine what to do with the incoming packet (client-side)
        // Note, this runs even if the session is not active
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
        }
    }

    private void HandleServerPacket(BinaryReader reader, int sender)
    {
        // Read the packet and determine what to do with it (server-side)
        var packetId = (PacketID)reader.ReadByte();
        var player = Main.player[sender].GetModPlayer<CrowdControlPlayer>();
        switch (packetId)
        {
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
        }
    }

    private void HandleSessionConnection()
    {
        // Note, this is run on a managed thread

        const string host = "127.0.0.1";
        const int port = 58430;
        const int socketTimeout = 10000; // 0.01s (1000000 = 1s)
        const int reconnectTimeout = 2000; // 2s (1000 = 1s)

        // Initialisation
        _isSessionConnected = false;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var writeAttempt = true;

        // Wait for the world to load
        while (ShouldSessionThreadContinue && Main.gameMenu)
        {
        }

        TerrariaUtils.WriteDebug("Started the Crowd Control session");
        if (ShouldSessionThreadContinue && Main.netMode == NetmodeID.MultiplayerClient)
        {
            // Send the client's config settings to the server
            CrowdControlConfig.GetInstance().SendConfigToServer();
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
                TerrariaUtils.WriteMessage(ItemID.LargeEmerald, "Connected to Crowd Control", Color.Green);

                // Connection successful, so keep polling the socket for incoming packets
                while (ShouldSessionThreadContinue && socket.Connected && socket.Poll(socketTimeout, SelectMode.SelectWrite))
                {
                    // Check if there is any data to receive
                    if (!socket.Poll(socketTimeout, SelectMode.SelectRead))
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

                        continue;
                    }

                    try
                    {
                        // Read incoming data
                        var buffer = new byte[1024];
                        var size = socket.Receive(buffer);
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
                            CrowdControlResponseStatus responseStatus;
                            try
                            {
                                responseStatus = ProcessEffect(request.Id, request.Code, request.Viewer, (CrowdControlRequestType)request.Type);
                            }
                            catch (Exception e)
                            {
                                TerrariaUtils.WriteDebug($"Cannot process Crowd Control effect '{request.Code}' due to an exception {e.Message}");
                                responseStatus = CrowdControlResponseStatus.Failure;
                            }

                            // Create a response object
                            response = CrowdControlResponse.ToJson(new CrowdControlResponse(request.Id, (int)responseStatus, $"{request.Code}: {responseStatus}"));
                            TerrariaUtils.WriteDebug($"Outgoing response: {response}");
                        }
                        catch (Exception e)
                        {
                            TerrariaUtils.WriteDebug($"Cannot parse Crowd Control request due to an exception: {e.Message}");
                            response = CrowdControlResponse.ToJson(new CrowdControlResponse(0, 0, e.Message));
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

                if (ShouldSessionThreadContinue)
                {
                    TerrariaUtils.WriteMessage(ItemID.LargeRuby, "Lost connection to Crowd Control", Color.Red);
                }
            }
            else
            {
                // Connection failed, so wait before attempting to reconnect
                Thread.Sleep(reconnectTimeout);

                if (!writeAttempt)
                {
                    continue;
                }

                writeAttempt = false;
                TerrariaUtils.WriteMessage(ItemID.LargeAmber, "Attempting to connect to Crowd Control", Color.Yellow);
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

    private CrowdControlResponseStatus ProcessEffect(int netId, string code, string viewer, CrowdControlRequestType requestType)
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
            // Get the provided ids and attempt to process them, grouping them by the results
            // Note, an infinite loop is possible if the provider processes another (or the same) provider
            var results = provider.GetEffectIds(requestType)
                .Where(x => !string.IsNullOrEmpty(x) && !_effectProviders.ContainsKey(x))
                .Distinct()
                .GroupBy(x => ProcessEffect(netId, x, viewer, requestType))
                .ToDictionary(x => x.Key, x => x.Count());

            TerrariaUtils.WriteDebug($"Processed {results.Sum(x => x.Value)} handler request(s) '{requestType} {code}': " +
                                     $"(suc={results.GetValueOrDefault(CrowdControlResponseStatus.Success)}, " +
                                     $"retry={results.GetValueOrDefault(CrowdControlResponseStatus.Retry)}, " +
                                     $"fail={results.GetValueOrDefault(CrowdControlResponseStatus.Failure)}, " +
                                     $"unavailable={results.GetValueOrDefault(CrowdControlResponseStatus.Unavailable)})");

            // Choose an appropriate course of action based on the results
            return results.ContainsKey(CrowdControlResponseStatus.Success) ? CrowdControlResponseStatus.Success
                : results.ContainsKey(CrowdControlResponseStatus.Retry) ? CrowdControlResponseStatus.Retry
                : results.ContainsKey(CrowdControlResponseStatus.Failure) || !results.Any() ? CrowdControlResponseStatus.Failure
                : CrowdControlResponseStatus.Unavailable;
        }

        // Ensure the effect is supported
        if (!_effects.TryGetValue(code, out var effect))
        {
            TerrariaUtils.WriteDebug($"Failed to process effect request '{code}' as it is not supported by the mod");
            return CrowdControlResponseStatus.Unavailable;
        }

        // Re-attempt the effect at a later point if the session is paused
        if (IsSessionPaused() && requestType != CrowdControlRequestType.Stop)
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
            result = effect.Start(netId, viewer);
        }
        else
        {
            effect.Stop(true);
            result = CrowdControlResponseStatus.Success;
        }

        // TerrariaUtils.WriteDebug($"Processed effect request '{requestType} {code}' with response '{result}'");
        return result;
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
        if (effect is IModEffect modEffect && !ModUtils.TryGetMod(modEffect.ModName, out _))
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
        if (provider is IModEffect modEffectProvider && !ModUtils.TryGetMod(modEffectProvider.ModName, out _))
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
        AddEffect(new GodModeEffect(20f));
        AddEffect(new SetMaxStatEffect(EffectID.IncreaseMaxLife, true, true));
        AddEffect(new SetMaxStatEffect(EffectID.DecreaseMaxLife, false, true));
        AddEffect(new SetMaxStatEffect(EffectID.IncreaseMaxMana, true, false));
        AddEffect(new SetMaxStatEffect(EffectID.DecreaseMaxMana, false, false));
        AddEffect(new IncreaseSpawnRateEffect(20f));
        AddEffect(new InfiniteAmmoEffect(20f));
        AddEffect(new TeleportToDeathEffect());
        AddEffect(new GivePetEffect(GivePetEffect.PetType.Pet));
        AddEffect(new GivePetEffect(GivePetEffect.PetType.LightPet));
        AddEffect(new ChangeGenderEffect());
        AddEffect(new ForceMountEffect(20f));
        AddEffect(new ShootExplosives(15f, ShootExplosives.Shoot.Bombs));
        AddEffect(new ShootExplosives(20f, ShootExplosives.Shoot.Grenades));
        AddEffect(new JumpBoostEffect(20f));
        AddEffect(new RunBoostEffect(20f));
        AddEffect(new IcyFeetEffect(20f));
        AddEffect(new NoItemPickupEffect(20f));
        AddEffect(new FlingUpwardsEffect());
        AddEffect(new PlaySoundEffect(EffectID.FartSound, null, SoundID.Item16));

        // --- Buff effects (positive)
        AddEffect(new BuffEffect(EffectID.BuffSurvivability, EffectSeverity.Positive, 25f,
            ItemID.PaladinsShield, -1, (v, p) => $"{v} provided {p} with survivability buffs", null,
            BuffID.Ironskin, BuffID.Endurance, BuffID.CatBast));
        AddEffect(new BuffEffect(EffectID.BuffRegen, EffectSeverity.Positive, 25f,
            ItemID.Heart, -1, (v, p) => $"{v} provided {p} with regeneration buffs",
            p => p.Player.SetHairDye(ItemID.LifeHairDye),
            BuffID.Regeneration, BuffID.SoulDrain, BuffID.HeartyMeal, BuffID.ManaRegeneration, BuffID.Lovestruck));
        AddEffect(new BuffEffect(EffectID.BuffLight, EffectSeverity.Positive, 25f,
            ItemID.MagicLantern, -1, (v, p) => $"{v} provided {p} with light",
            p => p.Player.SetHairDye(ItemID.MartianHairDye),
            BuffID.NightOwl, BuffID.Shine));
        AddEffect(new BuffEffect(EffectID.BuffTreasure, EffectSeverity.Positive, 25f,
            ItemID.GoldChest, -1, (v, p) => $"{v} helped {p} to search for treasure",
            p => p.Player.SetHairDye(ItemID.DepthHairDye),
            BuffID.Spelunker, BuffID.Hunter, BuffID.Dangersense));
        AddEffect(new BuffEffect(EffectID.BuffMovement, EffectSeverity.Positive, 25f,
            ItemID.Aglet, EmoteID.PartyCake, (v, p) => $"{v} boosted the movement speed of {p}", null,
            BuffID.Swiftness, BuffID.SugarRush, BuffID.Panic, BuffID.WaterWalking, BuffID.Sunflower));
        AddEffect(new BuffEffect(EffectID.BuffObsidianSkin, EffectSeverity.Positive, 45f,
            ItemID.ObsidianSkull, EmoteID.MiscFire, (v, p) => $"{v} provided {p} with lava immunity buffs", null,
            BuffID.ObsidianSkin, BuffID.Warmth));

        // --- Buff effects (negative)
        AddEffect(new BuffEffect(EffectID.BuffFreeze, EffectSeverity.Negative, 6f,
            ItemID.IceCream, -1, (v, p) => $"{v} cast a chilly spell over {p}", null,
            BuffID.Frozen));
        AddEffect(new BuffEffect(EffectID.BuffFire, EffectSeverity.Negative, 6f,
            ItemID.LivingFireBlock, EmoteID.DebuffBurn, (v, p) => $"{v} threw a molotov at {p}'s feet",
            p => Projectile.NewProjectile(null, p.Player.position, new Vector2(0f, 10f), ProjectileID.MolotovCocktail, 1, 1f, p.Player.whoAmI),
            BuffID.OnFire));
        AddEffect(new BuffEffect(EffectID.BuffDaze, EffectSeverity.Negative, 8f,
            ItemID.FallenStar, -1, (v, p) => $"{v} dazed {p}", null,
            BuffID.Dazed, BuffID.NoBuilding));
        AddEffect(new BuffEffect(EffectID.BuffLevitate, EffectSeverity.Negative, 8f,
            ItemID.FragmentVortex, -1, (v, p) => $"{v} distorted gravity around {p}", null,
            BuffID.VortexDebuff));
        AddEffect(new BuffEffect(EffectID.BuffConfuse, EffectSeverity.Negative, 15f,
            ItemID.BrainOfConfusion, EmoteID.EmoteConfused, (v, p) => $"{v} confused {p}", null,
            BuffID.Confused));
        AddEffect(new BuffEffect(EffectID.BuffInvisible, EffectSeverity.Neutral, 15f,
            ItemID.InvisibilityPotion, -1, (v, p) => $"{v} stole {p}'s body...", null,
            BuffID.Invisibility));
        AddEffect(new BuffEffect(EffectID.BuffBlind, EffectSeverity.Negative, 10f,
            ItemID.Blindfold, -1, (v, p) => $"{v} obstructed {p}'s screen",
            p => p.Player.SetHairDye(ItemID.TwilightHairDye),
            BuffID.Obstructed));

        // -- Inventory effects
        AddEffect(new DropItemEffect());
        AddEffect(new ExplodeInventoryEffect());
        AddEffect(new ClearInventoryEffect());
        AddEffect(new ShuffleInventoryEffect());
        AddEffect(new ReforgeItemEffect());
        AddEffect(new MoneyBoostEffect(25f));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Pickaxe));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Sword));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Armour));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.HealingPotion));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Potion));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Food));
        AddEffect(new GiveItemEffect(GiveItemEffect.GiveItem.Kite));

        // --- World effects
        AddEffect(new UseSundialEffect());
        AddEffect(new SetTimeEffect(EffectID.SetTimeNoon, "noon", 27000, true));
        AddEffect(new SetTimeEffect(EffectID.SetTimeMidnight, "midnight", 16200, false));
        AddEffect(new SetTimeEffect(EffectID.SetTimeSunrise, "sunrise", 0, true));
        AddEffect(new SetTimeEffect(EffectID.SetTimeSunset, "sunset", 0, false));
        AddEffect(new SpawnStructureEffect());
        AddEffect(new TrapEffect(TrapEffect.TrapType.Cobweb));
        AddEffect(new TrapEffect(TrapEffect.TrapType.Sand));
        AddEffect(new TrapEffect(TrapEffect.TrapType.Water));
        AddEffect(new TrapEffect(TrapEffect.TrapType.Honey));
        AddEffect(new TrapEffect(TrapEffect.TrapType.Lava));
        AddEffect(new RandomTeleportEffect());
        AddEffect(new SummonNpcsEffect());
        AddEffect(new RainbowFeetEffect(20f));
        AddEffect(new SpawnGuardian(false));
        AddEffect(new SpawnGuardian(true));
        AddEffect(new GoldenSlimeRainEffect(60f));
        AddEffect(new SpawnCritters());
        AddEffect(new SetWeatherEffect(WorldUtils.Weather.Clear));
        AddEffect(new SetWeatherEffect(WorldUtils.Weather.Rain));
        AddEffect(new SetWeatherEffect(WorldUtils.Weather.Storm));
        AddEffect(new SetWeatherEffect(WorldUtils.Weather.Windy));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.ForTheWorthy, true));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.ForTheWorthy, false));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.DontStarve, true));
        AddEffect(new ToggleWorldSeedEffect(ToggleWorldSeedEffect.SeedType.DontStarve, false));
        AddEffect(new SwitchSoundtrack());
        AddEffect(new MysteryBlocksEffect(30f));

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
        AddEffect(new FlipScreenEffect(15f));
        AddEffect(new DrunkModeEffect(15f));
        AddEffect(new ZoomEffect(15f, true));
        AddEffect(new ZoomEffect(15f, false));
        AddEffect(new WallOfFishEffect(20f));
        AddEffect(new CritterTakeoverEffect(35f));
        AddEffect(new ScreenShakeEffect(20f));
        AddEffect(new SniperModeEffect(15f));

        // --- Challenge effects
        AddEffectProvider(EffectID.RandomChallenge, new RandomChallengeEffectProvider());
        AddEffect(new SwimChallenge(30f));
        AddEffect(new StandOnBlockChallenge(40f));
        AddEffect(new CraftItemChallenge(40f));
        AddEffect(new SleepChallenge(30f));
        AddEffect(new MinecartChallenge(30f));
        AddEffect(new TouchGrassChallenge(10f));
        AddEffect(new EatFoodChallenge(30f));
    }

    private bool IsSessionPaused()
    {
        // Effects should be paused if the session is not running or Terraria is paused or the player is dead
        return !IsSessionActive || Main.gamePaused || GetLocalPlayer().Player.dead;
    }

    private void OnGameUpdate(GameTime gameTime)
    {
        // Update the active effects (so that their timers are reduced)
        var sessionPaused = IsSessionPaused();
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var effect in _effects.Values.Where(x => x.IsActive))
        {
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
}