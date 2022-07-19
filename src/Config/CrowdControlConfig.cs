﻿using System.ComponentModel;
using System.Diagnostics.Contracts;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

// ReSharper disable UnassignedField.Global

namespace CrowdControlMod.Config;

[Label("Configuration")]
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlConfig : ModConfig
{
    #region Static Fields and Constants

    private static CrowdControlConfig _instance = null!;

    #endregion

    #region Static Methods

    /// <summary>
    ///     Get the crowd control config instance.
    /// </summary>
    [Pure]
    public static CrowdControlConfig GetInstance()
    {
        return _instance;
    }

    #endregion

    #region Fields

    [Label("Show effect messages in chat")]
    [Tooltip("Disable to stop effect messages from showing in chat.\nUseful if you would like to use the browser source.")]
    [DefaultValue(true)]
    public bool ShowEffectMessagesInChat;

    [Label("Use anonymous names in chat")]
    [Tooltip("Enable to hide viewer names in the effect messages.\nUseful if you are worried about inappropriate names showing.")]
    [DefaultValue(false)]
    public bool UseAnonymousNamesInChat;

    [Label("Use effect music")]
    [Tooltip("A few effects override the music whilst active.\nDisable this to stop effects from overriding the music.")]
    [DefaultValue(true)]
    public bool UseEffectMusic;

    [Label("Use effect emotes")]
    [Tooltip("A few effects show an emote bubble beside the player.\nDisable this to stop effects from displaying any emote bubbles.")]
    [DefaultValue(true)]
    public bool UseEffectEmotes;

    [Label("Disable tombstones")]
    [Tooltip("Enable to prevent your tombstone from spawning when you die.")]
    [DefaultValue(false)]
    public bool DisableTombstones;

    [Label("Respawn timer")]
    [Tooltip("Reduce the respawn timer by this factor.\nThis allows you to get back into the game quicker after being killed.\nx1 is default time.")]
    [Range(0.2f, 1f)]
    [Increment(0.1f)]
    [DrawTicks]
    [DefaultValue(0.5f)]
    public float RespawnTimeFactor;

    [Label("Enable spawn protection for explosive effects")]
    [Tooltip("Enable to delay explosive-related effects if you are too close to spawn.")]
    [DefaultValue(false)]
    public bool EnableSpawnProtection;

    [Label("Spawn protection radius")]
    [Tooltip("If spawn protection is enabled, then this is the range around your spawn point that will be protected.")]
    [Range(10, 100)]
    [Increment(10)]
    [DrawTicks]
    [DefaultValue(30)]
    public int SpawnProtectionRadius;

    [Label("Allow time-changing effects during bosses")]
    [Tooltip("Disable to prevent time-changing effects during boss fights, invasions or events.")]
    [DefaultValue(false)]
    public bool AllowTimeChangeDuringBoss;

    [Label("Allow teleporting to other players")]
    [Tooltip("Enable to allow yourself to teleport to other players on a server without requiring a wormhole potion.")]
    [DefaultValue(false)]
    public bool AllowPlayerTeleportation;

    [Label("Teleportation cooldown (minutes)")]
    [Tooltip("Cooldown, in minutes, between usages of the teleportation to other players feature.")]
    [Range(0f, 10f)]
    [Increment(0.25f)]
    [DrawTicks]
    [DefaultValue(0.25)]
    public float PlayerTeleportationCooldown;

    [Label("Calamity mod integration")]
    [Tooltip("Disable to stop effects from using Calamity mod content if the Calamity mod is enabled.")]
    [DefaultValue(true)]
    public bool AllowCalamity;

    [Label("[Advanced] Show developer messages in chat")]
    [Tooltip("Enable to show developer messages in chat.\nThis is for debugging purposes for advanced users.")]
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
    public void SendConfigToServer()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            // Ignore
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