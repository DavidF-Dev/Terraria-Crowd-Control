using System.ComponentModel;
using JetBrains.Annotations;
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

    #endregion
}