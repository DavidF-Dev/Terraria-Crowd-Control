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
    #region Static Methods

    private static void ModifyTransformMatrix(ref SpriteViewMatrix transform)
    {
        transform.Zoom = new Vector2(CrowdControlMod.GetInstance().IsEffectActive(EffectID.ZoomIn) ? 5f : 0.6f);
        if (CrowdControlMod.GetInstance().IsEffectActive(EffectID.ResizePlayerDown))
        {
            transform.Zoom += new Vector2(2f);
        }
    }

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
        if ((_zoomIn && CrowdControlMod.GetInstance().IsEffectActive(EffectID.ZoomOut)) ||
            (!_zoomIn && CrowdControlMod.GetInstance().IsEffectActive(EffectID.ZoomIn)))
        {
            return CrowdControlResponseStatus.Retry;
        }

        CrowdControlModSystem.ModifyTransformMatrixHook += ModifyTransformMatrix;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        CrowdControlModSystem.ModifyTransformMatrixHook -= ModifyTransformMatrix;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Binoculars, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}