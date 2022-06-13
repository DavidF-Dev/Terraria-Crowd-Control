﻿using System.ComponentModel;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace CrowdControlMod.Config;

[UsedImplicitly]
[Label("Configuration")]
public sealed class CrowdControlConfig : ModConfig
{
    #region Static Fields and Constants

    [NotNull]
    private static CrowdControlConfig _instance = null!;

    #endregion

    #region Static Methods

    /// <summary>
    ///     Get the crowd control config instance.
    /// </summary>
    [PublicAPI] [Pure] [NotNull]
    public static CrowdControlConfig GetInstance()
    {
        return _instance;
    }

    #endregion

    #region Fields

    [UsedImplicitly]
    [Label("Show effect messages in chat")]
    [Tooltip("Disable this to stop effect messages from showing in chat.\nUseful if you would like to use the browser source.")]
    [DefaultValue(true)]
    public bool ShowEffectMessagesInChat;

    [UsedImplicitly]
    [Label("Disable Tombstones")]
    [Description("Enable this to prevent your tombstones from spawning when you die.")]
    [DefaultValue(false)]
    public bool DisableTombstones;

    [UsedImplicitly]
    [Label("Respawn Timer")]
    [Tooltip("Reduce the respawn timer by this factor.\nThis allows you to get back into the game quicker after being killed.\nx1 is default time.")]
    [Range(0.4f, 1f)]
    [Increment(0.1f)]
    [DrawTicks]
    [DefaultValue(0.5f)]
    public float RespawnTimeFactor;

    [UsedImplicitly]
    [Label("Enable Spawn Protection for Explosive Effects")]
    [Tooltip("When enabled, explosive-related effects will be delayed when the player is too close to spawn.")]
    [DefaultValue(true)]
    public bool EnableSpawnProtection;

    [UsedImplicitly]
    [Label("Spawn Protection Radius")]
    [Tooltip("If spawn protection is enabled, then this is the range around the world spawn that will be protected.")]
    [Range(10, 100)]
    [Increment(10)]
    [DrawTicks]
    [DefaultValue(30)]
    public int SpawnProtectionRadius;

    [UsedImplicitly]
    [Label("Allow Time-Changing Effects During Bosses")]
    [Description("Disable this to prevent time-changing effects during boss fights, invasions or events.")]
    [DefaultValue(false)]
    public bool AllowTimeChangeDuringBoss;

    [UsedImplicitly]
    [Label("[Advanced] Show developer messages in chat")]
    [Tooltip("Enable this to show developer messages in chat.\nThis is for debugging purposes for advanced users.")]
    [DefaultValue(false)]
    public bool DeveloperMode;

    #endregion

    #region Properties

    public override ConfigScope Mode => ConfigScope.ClientSide;

    #endregion

    #region Methods

    public override void OnLoaded()
    {
        _instance = this;
        base.OnLoaded();
    }

    public override void OnChanged()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient || !CrowdControlMod.GetInstance().IsSessionActive)
        {
            return;
        }

        // If connected as a client, update the server on the changes
        SendConfigToServer();
    }

    /// <summary>
    ///     As a client, update the server with the relevant config values for our player.
    /// </summary>
    [PublicAPI]
    public void SendConfigToServer()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            return;
        }

        // Update the server on relevant changes
        var packet = CrowdControlMod.GetInstance().GetPacket(2);
        packet.Write((byte)PacketID.ConfigState);
        packet.Write(DisableTombstones);
        packet.Send();

        TerrariaUtils.WriteDebug("Sending config to server");
    }

    #endregion
}