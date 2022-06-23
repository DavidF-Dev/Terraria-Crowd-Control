using System;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace CrowdControlMod.Shaders;

/// <summary>
///     Represents a screen shader that can be enabled and disabled.
///     Shader data can be edited whilst active.
/// </summary>
public sealed class ScreenShader
{
    #region Fields

    /// <summary>
    ///     Loaded shader (effect) asset.
    /// </summary>
    [CanBeNull]
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
    ///     Name of the scene filter that the shader is attached to.
    /// </summary>
    [NotNull]
    private readonly string _filterName;

    #endregion

    #region Constructors

    public ScreenShader([NotNull] string shaderAssetName, [NotNull] string shaderPassName, [NotNull] string filterName)
    {
        _shaderAssetPath = $"src/Shaders/{shaderAssetName}";
        _shaderPassName = shaderPassName;
        _filterName = filterName;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Shader is currently active.
    /// </summary>
    private bool IsActive => _effect != null && Filters.Scene[_filterName].IsActive();

    #endregion

    #region Methods

    /// <summary>
    ///     Enable the screen shader, returning null if unable to load the shader at all.
    /// </summary>
    [PublicAPI] [CanBeNull]
    public ScreenShaderData Enable()
    {
        if (IsActive)
        {
            return Filters.Scene[_filterName].GetShader();
        }

        if (_effect == null)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                TerrariaUtils.WriteDebug($"Failed to load shader asset '{_shaderAssetPath}': cannot be used on a server");
                return null;
            }

            // Load the shader and apply to a scene filter
            try
            {
                _effect = new Ref<Effect>(CrowdControlMod.GetInstance().Assets.Request<Effect>(_shaderAssetPath, AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene[_filterName] = new Filter(new ScreenShaderData(_effect, _shaderPassName), EffectPriority.VeryHigh);
                Filters.Scene[_filterName].Load();
            }
            catch (Exception)
            {
                _effect = null;
                TerrariaUtils.WriteDebug($"Failed to load shader asset '{_shaderAssetPath}'");
                return null;
            }
        }

        // Activate the scene filter
        return Filters.Scene.Activate(_filterName).GetShader();
    }

    /// <summary>
    ///     Disable the screen shader.
    /// </summary>
    [PublicAPI]
    public void Disable()
    {
        if (!IsActive)
        {
            // Already disabled
            return;
        }

        // Disable the scene filter
        Filters.Scene.Deactivate(_filterName);
    }

    /// <summary>
    ///     Get the screen shader data used, if the shader is active.
    /// </summary>
    [PublicAPI] [CanBeNull] [Pure]
    public ScreenShaderData GetShader()
    {
        return !IsActive ? null : Filters.Scene[_filterName].GetShader();
    }

    #endregion
}