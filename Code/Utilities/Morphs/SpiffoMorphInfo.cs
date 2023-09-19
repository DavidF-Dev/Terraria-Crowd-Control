using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Code.Utilities.Morphs;

public sealed class SpiffoMorphInfo : MorphInfo
{
    #region Constructors

    public SpiffoMorphInfo()
    {
        Main.instance.LoadProjectile(ProjectileID.Spiffo);
        Texture = TextureAssets.Projectile[ProjectileID.Spiffo].Value;
        TotalFrames = 16;

        IdleStartFrame = 0;
        IdleFrameCount = 1;
        IdleAnimSpeed = 0;
        WalkingStartFrame = 2;
        WalkingFrameCount = 10;
        FallingStartFrame = 12;
        FallingFrameCount = 4;
    }

    #endregion

    #region Methods

    public override void ModifyPosition(ref Vector2 position, in PlayerDrawSet drawData)
    {
        if (drawData.headOnlyRender)
        {
            return;
        }

        position.Y -= 2;
    }

    #endregion
}