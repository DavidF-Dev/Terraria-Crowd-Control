using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Code.Utilities.Morphs;

public sealed class FoxMorphInfo : MorphInfo
{
    #region Constructors

    public FoxMorphInfo()
    {
        Main.instance.LoadProjectile(ProjectileID.FennecFox);
        Texture = TextureAssets.Projectile[ProjectileID.FennecFox].Value;
        TotalFrames = 17;

        IdleStartFrame = 0;
        IdleFrameCount = 3;
        IdleAnimSpeed = 0.25f;
        FallingStartFrame = 11;
        FallingFrameCount = 5;
        FallingAnimSpeed = IdleAnimSpeed;
        WalkingStartFrame = 4;
        WalkingFrameCount = 6;
        WalkingAnimSpeed = IdleAnimSpeed;
    }

    #endregion

    public override void ModifyPosition(ref Vector2 position, in PlayerDrawSet drawData)
    {
        if (drawData.headOnlyRender)
        {
            return;
        }
        
        position.Y -= 4;
    }

    public override void ModifyScale(ref float scale, in PlayerDrawSet drawData)
    {
        if (drawData.headOnlyRender)
        {
            return;
        }
        
        scale = 1.5f;
    }

    public override void ModifyColour(ref Color colour, in PlayerDrawSet drawData)
    {
        if (SteamUtils.IsMagicMalaraith)
        {
            colour = new Color(0, 102, 255, 255);
        }
    }
}