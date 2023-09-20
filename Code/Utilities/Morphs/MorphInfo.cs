using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace CrowdControlMod.Code.Utilities.Morphs;

/// <summary>
///     Information regarding a morph, including draw data.
/// </summary>
public abstract class MorphInfo
{
    #region Properties

    public Texture2D? Texture { get; protected init; }

    public int TotalFrames { get; protected init; }

    public int IdleStartFrame { get; protected init; }

    public int IdleFrameCount { get; protected init; } = 1;

    public float IdleAnimSpeed { get; protected init; } = 0.25f;

    public int FallingStartFrame { get; protected init; }

    public int FallingFrameCount { get; protected init; } = 1;

    public float FallingAnimSpeed { get; protected init; } = 0.25f;

    public int WalkingStartFrame { get; protected init; }

    public int WalkingFrameCount { get; protected init; } = 1;

    public float WalkingAnimSpeed { get; protected init; } = 0.25f;

    #endregion

    #region Methods

    public virtual void ModifyTexture(ref Texture2D? texture, ref int totalFrames)
    {
    }
    
    public virtual void ModifyPosition(ref Vector2 position, in PlayerDrawSet drawData)
    {
    }

    public virtual void ModifyRotation(ref float rotation, in PlayerDrawSet drawData)
    {
    }

    public virtual void ModifyScale(ref float scale, in PlayerDrawSet drawData)
    {
    }

    public virtual void ModifyColour(ref Color colour, in PlayerDrawSet drawData)
    {
    }

    public virtual void ModifyDirection(ref int direction, in PlayerDrawSet drawData)
    {
    }

    public virtual void PostUpdateEquips(Player player)
    {
    }

    public virtual void DrawEffects(Player player)
    {
    }

    public virtual void ModifyHurt(Player player, ref Player.HurtModifiers modifiers)
    {
    }

    public virtual void OnHurt(Player player, in Player.HurtInfo info)
    {
    }

    #endregion
}