using System.ComponentModel;
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

    private const int EffectR = 164;
    private const int EffectG = 120;
    private const int EffectB = 240;
    private const int WorldR = 120;
    private const int WorldG = 240;
    private const int WorldB = 128;
    private const int DeveloperR = 240;
    private const int DeveloperG = 120;
    private const int DeveloperB = 120;

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

    // --- Effect settings
    [Header("[i:855] Effect settings")]
    [Label("[i:3617]  Show effect messages in chat")]
    [Tooltip("Disable to stop effect messages from showing in chat.\nUseful if you would like to use the browser source.")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool ShowEffectMessagesInChat;

    [Label("[i:888]  Use anonymous names in chat")]
    [Tooltip("Enable to hide viewer names in the effect messages.\nUseful if you are worried about inappropriate names showing.")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(false)]
    public bool UseAnonymousNamesInChat;

    [Label("[i:494]  Use effect music")]
    [Tooltip("A few effects override the music whilst active.\nDisable this to stop effects from overriding the music.")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectMusic;

    [Label("[i:5061]  Use effect emotes")]
    [Tooltip("A few effects show an emote bubble beside the player.\nDisable this to stop effects from displaying any emote bubbles.")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectEmotes;

    [Label("[i:1982]  Use effect hair dyes")]
    [Tooltip("A few effects change the player's hair dye.\nDisable this to stop effects from changing the hair dye.")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectHairDyes;

    [Label("[i: 890]  Hide \"Drop Item\" effect message")]
    [Tooltip("Disable to show the effect message just like any other effect, otherwise it will be hidden.")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool HideDropItemMessage;
    
    [Label("[i: 4765]  Calamity mod integration")]
    [Tooltip("Disable to stop effects from using Calamity mod content if the Calamity mod is enabled.")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    [ReloadRequired]
    public bool AllowCalamity;

    // --- World settings
    [Header("[i:4766] World settings")]
    [Label("[i:321]  Disable tombstones")]
    [Tooltip("Enable to prevent your tombstone from spawning when you die.\nIn multi-player, this will only affect your player.")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool DisableTombstones;

    [Label("[i:224]  Respawn timer")]
    [Tooltip("Reduce the respawn timer by this factor.\nThis allows you to get back into the game quicker after being killed.\nx1 is default time.")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(0.2f, 1f)]
    [Increment(0.1f)]
    [DrawTicks]
    [DefaultValue(0.5f)]
    public float RespawnTimeFactor;

    [Label("[i:63]  Enable spawn protection for world-altering effects")]
    [Tooltip("Enable to delay world-altering effects if you are too close to spawn.")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(true)]
    public bool EnableSpawnProtection;

    [Label("[i:2799]  Spawn protection radius")]
    [Tooltip("If spawn protection is enabled, then this is the range around your spawn point that will be protected.")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(10, 100)]
    [Increment(10)]
    [DrawTicks]
    [DefaultValue(30)]
    public int SpawnProtectionRadius;

    [Label("[i:4747]  Allow time-changing effects during bosses")]
    [Tooltip("Disable to prevent time-changing effects during boss fights, invasions or events.")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool AllowTimeChangeDuringBoss;

    [Label("[i:3117]  Forcefully despawn bosses if all players are dead")]
    [Tooltip("Enable this to override Terraria's default behaviour for all bosses spawned via effects.\nBosses & mini-bosses will despawn if all players are dead.\nIn multi-player, only one player needs to have this option enabled.")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool ForceDespawnBosses;

    [Label("[i:2997]  Allow teleporting to other players")]
    [Tooltip("Enable to allow yourself to teleport to other players on a server without requiring a wormhole potion.\nYou can only teleport to players if you're on the same in-game team.\nYou can only teleport to players if you have Crowd Control connected.")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool AllowPlayerTeleportation;

    [Label("[i:709]  Teleportation cooldown (seconds)")]
    [Tooltip("Cooldown, in seconds, between usages of the teleportation to other players feature.")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(0f, 600f)]
    [Increment(5)]
    [DrawTicks]
    [DefaultValue(25)]
    public int PlayerTeleportationCooldown;

    // --- Developer settings
    [Header("[i:1757] [Advanced] Developer settings")]
    [Label("[i:1344]  Show developer messages in chat")]
    [Tooltip("Enable to show developer messages in chat.\nThis is for debugging purposes for advanced users.")]
    [BackgroundColor(DeveloperR, DeveloperG, DeveloperB)]
    [DefaultValue(false)]
    public bool DeveloperMode;

    [Label("[i:3857]  Forcefully enable easter eggs")]
    [Tooltip("Enable easter eggs that are usually unavailable except in unique cases.")]
    [BackgroundColor(DeveloperR, DeveloperG, DeveloperB)]
    [DefaultValue(false)]
    [ReloadRequired]
    public bool ForceEasterEggs;

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
        if (Main.netMode != NetmodeID.MultiplayerClient)
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
        var packet = CrowdControlMod.GetInstance().GetPacket(3);
        packet.Write((byte)PacketID.ConfigState);
        packet.Write(DisableTombstones);
        packet.Write(ForceDespawnBosses);
        packet.Send();

        TerrariaUtils.WriteDebug("Sending config to server");
    }

    #endregion
}