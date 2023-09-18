using System.ComponentModel;
using System.Diagnostics.Contracts;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ModLoader.Config;

// ReSharper disable UnassignedField.Global

namespace CrowdControlMod.Config;

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
    [Header("Effect")]
    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool ShowEffectMessagesInChat;

    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(false)]
    public bool UseAnonymousNamesInChat;

    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectMusic;

    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectEmotes;

    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    public bool UseEffectHairDyes;

    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(false)]
    public bool HideDropItemMessage;

    [BackgroundColor(EffectR, EffectG, EffectB)]
    [Range(0.0f, 2f)]
    [Increment(0.1f)]
    [DrawTicks]
    [DefaultValue(1f)]
    public float ShuffleSfxVolumeFactor;

    [BackgroundColor(EffectR, EffectG, EffectB)]
    [DefaultValue(true)]
    [ReloadRequired]
    public bool AllowCalamity;

    // --- World settings
    [Header("World")]
    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool DisableTombstones;

    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(0.1f, 1f)]
    [Increment(0.1f)]
    [DrawTicks]
    [DefaultValue(0.5f)]
    public float RespawnTimeFactor;

    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(true)]
    public bool EnableSpawnProtection;

    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(10, 100)]
    [Increment(10)]
    [DrawTicks]
    [DefaultValue(30)]
    public int SpawnProtectionRadius;

    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool AllowTimeChangeDuringBoss;

    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool ForceDespawnBosses;

    [BackgroundColor(WorldR, WorldG, WorldB)]
    [DefaultValue(false)]
    public bool AllowPlayerTeleportation;

    [BackgroundColor(WorldR, WorldG, WorldB)]
    [Range(0f, 600f)]
    [Increment(5)]
    [DrawTicks]
    [DefaultValue(25)]
    public int PlayerTeleportationCooldown;

    // --- Developer settings
    [Header("Developer")]
    [BackgroundColor(DeveloperR, DeveloperG, DeveloperB)]
    [DefaultValue(false)]
    public bool DeveloperMode;

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