using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Cause the camera to pan around the world, following the cursor.
///     Allows the world to be viewed.
/// </summary>
public sealed class SniperModeEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float SinglePlayerRange = 16f * 0.12f; // 16f * 1.3f;
    private const float ClientRange = 16f * 0.12f; // 16f * 0.4f;

    #endregion

    #region Static Methods

    private static void ModifyScreenPosition()
    {
        // Get the target position (mouse world position, clamped to within the window)
        var target = Main.screenPosition + Vector2.Clamp(Main.MouseScreen, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight));

        // Get the current position (use player center)
        var current = GetLocalPlayer().Player.Center;

        if (target == current)
        {
            // Ignore if the same
            return;
        }

        // Determine offset (different for multi-player as whole map cannot be shown)
        var range = NetUtils.IsSinglePlayer ? SinglePlayerRange : ClientRange;
        var offset = (target - current) * range;
        Main.screenPosition += offset;
    }

    #endregion

    #region Constructors

    public SniperModeEffect(int duration) : base(EffectID.SniperMode, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Screen;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        GetLocalPlayer().ModifyScreenPositionHook += ModifyScreenPosition;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().ModifyScreenPositionHook -= ModifyScreenPosition;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.SniperRifle, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}