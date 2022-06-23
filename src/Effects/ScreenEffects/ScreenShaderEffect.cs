using System;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Used by screen effects that want to manage a screen shader.
/// </summary>
public abstract class ScreenShaderEffect : CrowdControlEffect
{
    #region Fields

    /// <summary>
    ///     Loaded shader (effect) asset.
    /// </summary>
    private Ref<Effect> _effect;

    /// <summary>
    ///     Local path to the shader asset.<br />
    ///     E.g. src/Shaders/SH_MyShader
    /// </summary>
    [NotNull]
    private readonly string _shaderAssetPath;

    /// <summary>
    ///     Name of the pass method in the shader asset to use.
    /// </summary>
    [NotNull]
    private readonly string _shaderPassName;

    /// <summary>
    ///     Name of the scene filter that the shader asset is loaded in to.
    /// </summary>
    [NotNull]
    private readonly string _filterName;

    #endregion

    #region Constructors

    protected ScreenShaderEffect([NotNull] string id, float? duration, EffectSeverity severity,
        [NotNull]
        string shaderAssetName, [NotNull] string shaderPassName) : base(id, duration, severity)
    {
        _shaderAssetPath = $"src/Shaders/{shaderAssetName}";
        _shaderPassName = shaderPassName;
        _filterName = $"{Id}_Shader";
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Enable the screen shader, returning false if the shader asset couldn't be loaded.
    /// </summary>
    protected bool EnableScreenShader()
    {
        if (_effect != null && Filters.Scene[_filterName].IsActive())
        {
            // Already active
            return true;
        }

        if (_effect == null)
        {
            // Load the shader and apply to a screen filter
            try
            {
                _effect = new Ref<Effect>(CrowdControlMod.GetInstance().Assets.Request<Effect>(_shaderAssetPath, AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene[_filterName] = new Filter(new ScreenShaderData(_effect, _shaderPassName), EffectPriority.VeryHigh);
                Filters.Scene[_filterName].Load();
            }
            catch (Exception)
            {
                _effect = null;
                TerrariaUtils.WriteDebug($"Failed to load shader asset '{_shaderAssetPath}' for effect '{Id}'");
                return false;
            }
        }

        // Active the screen shader filter
        Filters.Scene.Activate(_filterName);
        return true;
    }

    /// <summary>
    ///     Disable the screen shader.
    /// </summary>
    protected void DisableScreenShader()
    {
        if (_effect == null || !Filters.Scene[_filterName].IsActive())
        {
            // Already inactive
            return;
        }

        // Disable the screen shader filter
        Filters.Scene.Deactivate(_filterName);
    }

    #endregion
}