using System.ComponentModel;
using JetBrains.Annotations;
using Terraria.ModLoader.Config;

namespace CrowdControlMod;

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
    /// <returns></returns>
    [PublicAPI] [Pure] [NotNull]
    public static CrowdControlConfig GetInstance()
    {
        return _instance;
    }

    #endregion

    #region Properties

    [PublicAPI]
    [Label("Show effect messages in chat")]
    [Tooltip("Disable this to stop effect messages from showing in chat.\nUseful if you would like is use the browser source.")]
    [DefaultValue(true)]
    public bool ShowEffectMessagesInChat { get; private set; }

    [PublicAPI]
    [Label("Respawn Timer")]
    [Tooltip("Reduce the respawn timer by this factor.\nThis allows you to get back into the game quicker after being killed.\nx1 is default time.")]
    [Range(0.4f, 1f)]
    [Increment(0.1f)]
    [DrawTicks]
    [DefaultValue(0.5f)]
    public float RespawnTimeFactor { get; private set; }

    [PublicAPI]
    [Label("[Advanced] Show developer messages in chat")]
    [Tooltip("Enable this to show developer messages in chat.\nThis is for debugging purposes for advanced users.")]
    [DefaultValue(false)]
    public bool DeveloperMode { get; private set; }

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