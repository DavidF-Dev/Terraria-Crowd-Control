using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.Graphics;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

public sealed class ZoomEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float ZoomInAmount = 5f;
    private const float ZoomOutAmount = 0.25f;
    private static bool _anyZoomActive;

    #endregion

    #region Fields

    private readonly bool _zoomIn;

    #endregion

    #region Constructors

    public ZoomEffect(float duration, bool zoomIn) : base(zoomIn ? EffectID.ZoomIn : EffectID.ZoomOut, duration, EffectSeverity.Negative)
    {
        _zoomIn = zoomIn;
    }

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

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        if (_zoomIn)
        {
            TerrariaUtils.WriteEffectMessage(ItemID.Binoculars, $"{viewerString} is getting a very good look at {playerString} for {durationString} seconds", Severity);
            return;
        }
        
        TerrariaUtils.WriteEffectMessage(ItemID.Binoculars, $"{viewerString} zoomed way out for {durationString} seconds", Severity);
    }

    private void ModifyTransformMatrix(ref SpriteViewMatrix transform)
    {
        transform.Zoom = new Vector2(_zoomIn ? ZoomInAmount : ZoomOutAmount);
    }

    #endregion
}