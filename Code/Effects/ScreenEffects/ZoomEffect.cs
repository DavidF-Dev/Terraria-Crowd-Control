using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.Graphics;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Zoom the screen in or out by a huge factor for a short duration.
/// </summary>
public sealed class ZoomEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float ZoomInAmount = 5f;
    private const float ZoomOutAmount = 0.6f;
    private static bool _anyZoomActive;

    #endregion

    #region Fields

    private readonly bool _zoomIn;

    #endregion

    #region Constructors

    public ZoomEffect(int duration, bool zoomIn) : base(zoomIn ? EffectID.ZoomIn : EffectID.ZoomOut, duration, EffectSeverity.Negative)
    {
        _zoomIn = zoomIn;
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Screen;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (_anyZoomActive)
        {
            return CrowdControlResponseStatus.Failure;
        }

        _anyZoomActive = true;
        CrowdControlModSystem.ModifyTransformMatrixHook += ModifyTransformMatrix;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _anyZoomActive = false;
        CrowdControlModSystem.ModifyTransformMatrixHook -= ModifyTransformMatrix;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Binoculars, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    private void ModifyTransformMatrix(ref SpriteViewMatrix transform)
    {
        transform.Zoom = new Vector2(_zoomIn ? ZoomInAmount : ZoomOutAmount);
    }

    #endregion
}