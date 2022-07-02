using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

[UsedImplicitly]
public sealed class CrowdControlWall : GlobalWall
{
    #region Delegates

    /// <inheritdoc cref="PreDraw" />
    [PublicAPI]
    public delegate bool PreDrawDelegate(int i, int j, int type, SpriteBatch spriteBatch);

    #endregion

    #region Events

    /// <inheritdoc cref="PreDraw" />
    [PublicAPI]
    public static event PreDrawDelegate PreDrawHook;

    #endregion

    #region Methods

    public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        return PreDrawHook?.Invoke(i, j, type, spriteBatch) ?? base.PreDraw(i, j, type, spriteBatch);
    }

    #endregion
}