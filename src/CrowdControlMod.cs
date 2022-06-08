using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Terraria;
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

    private bool _isSessionStarted;

    private bool _isSessionConnected;

    [NotNull]
    private readonly Dictionary<string, CrowdControlEffect> _effects = new();
    
    #endregion

    #region Properties

    /// <summary>
    ///     Session is active if started and successfully connected to the crowd control service.
    /// </summary>
    [PublicAPI]
    public bool IsSessionActive => _isSessionStarted && _isSessionConnected;

    #endregion

    #region Methods

    /// <summary>
    ///     Get the crowd control player instance.
    /// </summary>
    [PublicAPI] [Pure] [NotNull]
    public CrowdControlPlayer GetPlayer()
    {
        return _player;
    }
    
    /// <summary>
    ///     Get an effect by id if it is currently active.
    /// </summary>
    [PublicAPI] [Pure]
    public bool TryGetActiveEffect<T>([NotNull] string id, [CanBeNull] out T effect) where T : CrowdControlEffect
    {
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
    
    public override void Load()
    {
        _instance = this;

        base.Load();
    }

    public override void Close()
    {
        // Ensure that the session is stopped
        StopCrowdControlSession();

        // Null references
        _player = null!;
        _instance = null!;

        base.Close();
    }

    private void CrowdControlConnection()
    {
        // Initialisation
        _isSessionConnected = false;
        
        // Connection loop
        while (_isSessionStarted)
        {
            // TODO
        }

        // Clean up
        _isSessionConnected = false;
        _sessionThread = null;
    }

    private void StartCrowdControlSession([NotNull] Player player)
    {
        if (_isSessionStarted || _sessionThread != null)
        {
            return;
        }
        
        _player = player.GetModPlayer<CrowdControlPlayer>();

        _isSessionStarted = true;
        
        // Start the connection thread
        _sessionThread = new Thread(CrowdControlConnection);
        _sessionThread.Start();
    }

    private void StopCrowdControlSession()
    {
        if (!_isSessionStarted)
        {
            return;
        }

        // Allow the threaded method to clean up itself when it exits its loop
        _isSessionStarted = false;
        
        // Stop all active effects
        foreach (var effect in _effects.Values.Where(effect => effect.IsActive))
        {
            effect.Stop();
        }
    }

    #endregion
}