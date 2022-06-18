using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlModSystem : ModSystem
{
    #region Delegates

    /// <inheritdoc cref="ModifyTransformMatrix" />
    public delegate void ModifyTransformMatrixDelegate(ref SpriteViewMatrix transform);

    #endregion

    #region Events

    /// <inheritdoc cref="PostDrawTiles" />
    [PublicAPI]
    public static event Action PostDrawTilesHook;

    /// <inheritdoc cref="PostDrawInterface" />
    [PublicAPI]
    public static event Action<SpriteBatch> PostDrawInterfaceHook;

    /// <inheritdoc cref="ModifyTransformMatrix" />
    [PublicAPI]
    public static event ModifyTransformMatrixDelegate ModifyTransformMatrixHook;

    /// <inheritdoc cref="PostUpdateEverything" />
    [PublicAPI]
    public static event Action PostUpdateEverythingHook;

    #endregion

    #region Methods

    public override void PreSaveAndQuit()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            // Stop the crowd control session upon exiting a world
            CrowdControlMod.GetInstance().StopCrowdControlSession();
        }
    }

    public override void PostDrawTiles()
    {
        PostDrawTilesHook?.Invoke();
    }

    public override void PostDrawInterface(SpriteBatch spriteBatch)
    {
        PostDrawInterfaceHook?.Invoke(spriteBatch);
    }

    public override void ModifyTransformMatrix(ref SpriteViewMatrix transform)
    {
        ModifyTransformMatrixHook?.Invoke(ref transform);
    }

    public override void PostUpdateEverything()
    {
        PostUpdateEverythingHook?.Invoke();
    }

    #endregion
}