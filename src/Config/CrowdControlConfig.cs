using System.ComponentModel;
using System.Diagnostics.Contracts;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ModLoader.Config;

// ReSharper disable UnassignedField.Global

namespace CrowdControlMod.Config;

[Label("Configuration")]
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlConfig : ModConfig
{
    #region Static Fields and Constants

    private const string ConfigPath = $"{LangUtils.ModPath}Config.";
    private const string HeaderPath = $"{ConfigPath}Header.";
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
    [Header($"${HeaderPath}Effect")]
    [Label($"${ConfigPath}ShowEffectMessagesInChat.Label")]
    [Tooltip($"${ConfigPath}ShowEffectMessagesInChat.Tooltip")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool ShowEffectMessagesInChat;

    [Label($"${ConfigPath}UseAnonymousNamesInChat.Label")]
    [Tooltip($"${ConfigPath}UseAnonymousNamesInChat.Tooltip")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(false)]
    public bool UseAnonymousNamesInChat;

    [Label($"${ConfigPath}UseEffectMusic.Label")]
    [Tooltip($"${ConfigPath}UseEffectMusic.Tooltip")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectMusic;

    [Label($"${ConfigPath}UseEffectEmotes.Label")]
    [Tooltip($"${ConfigPath}UseEffectEmotes.Tooltip")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectEmotes;

    [Label($"${ConfigPath}UseEffectHairDyes.Label")]
    [Tooltip($"${ConfigPath}UseEffectHairDyes.Tooltip")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectHairDyes;

    [Label($"${ConfigPath}HideDropItemMessage.Label")]
    [Tooltip($"${ConfigPath}HideDropItemMessage.Tooltip")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(false)]
    public bool HideDropItemMessage;

    [Label($"${ConfigPath}AllowCalamity.Label")]
    [Tooltip($"${ConfigPath}AllowCalamity.Tooltip")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    [ReloadRequired]
    public bool AllowCalamity;

    // --- World settings
    [Header($"${HeaderPath}World")]
    [Label($"${ConfigPath}DisableTombstones.Label")]
    [Tooltip($"${ConfigPath}DisableTombstones.Tooltip")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool DisableTombstones;

    [Label($"${ConfigPath}RespawnTimeFactor.Label")]
    [Tooltip($"${ConfigPath}RespawnTimeFactor.Tooltip")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(0.2f, 1f)]
    [Increment(0.1f)]
    [DrawTicks]
    [DefaultValue(0.5f)]
    public float RespawnTimeFactor;

    [Label($"${ConfigPath}EnableSpawnProtection.Label")]
    [Tooltip($"${ConfigPath}EnableSpawnProtection.Tooltip")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(true)]
    public bool EnableSpawnProtection;

    [Label($"${ConfigPath}SpawnProtectionRadius.Label")]
    [Tooltip($"${ConfigPath}SpawnProtectionRadius.Tooltip")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(10, 100)]
    [Increment(10)]
    [DrawTicks]
    [DefaultValue(30)]
    public int SpawnProtectionRadius;

    [Label($"${ConfigPath}AllowTimeChangeDuringBoss.Label")]
    [Tooltip($"${ConfigPath}AllowTimeChangeDuringBoss.Tooltip")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool AllowTimeChangeDuringBoss;

    [Label($"${ConfigPath}ForceDespawnBosses.Label")]
    [Tooltip($"${ConfigPath}ForceDespawnBosses.Tooltip")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool ForceDespawnBosses;

    [Label($"${ConfigPath}AllowPlayerTeleportation.Label")]
    [Tooltip($"${ConfigPath}AllowPlayerTeleportation.Tooltip")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool AllowPlayerTeleportation;

    [Label($"${ConfigPath}PlayerTeleportationCooldown.Label")]
    [Tooltip($"${ConfigPath}PlayerTeleportationCooldown.Tooltip")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(0f, 600f)]
    [Increment(5)]
    [DrawTicks]
    [DefaultValue(25)]
    public int PlayerTeleportationCooldown;

    // --- Developer settings
    [Header($"${HeaderPath}Developer")]
    [Label($"${ConfigPath}DeveloperMode.Label")]
    [Tooltip($"${ConfigPath}DeveloperMode.Tooltip")]
    [BackgroundColor(DeveloperR, DeveloperG, DeveloperB)]
    [DefaultValue(false)]
    public bool DeveloperMode;

    [Label($"${ConfigPath}ForceEasterEggs.Label")]
    [Tooltip($"${ConfigPath}ForceEasterEggs.Tooltip")]
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
        if (!NetUtils.IsClient)
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
        if (!NetUtils.IsClient)
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