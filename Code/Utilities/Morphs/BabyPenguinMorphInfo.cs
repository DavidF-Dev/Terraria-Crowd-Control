using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Code.Utilities.Morphs;

public sealed class BabyPenguinMorphInfo : MorphInfo
{
    #region Constructors

    public BabyPenguinMorphInfo()
    {
        Main.instance.LoadProjectile(ProjectileID.Penguin);
        Texture = TextureAssets.Projectile[ProjectileID.Penguin].Value;
        TotalFrames = 6;

        IdleStartFrame = 0;
        IdleFrameCount = 1;
        IdleAnimSpeed = 0;
        WalkingStartFrame = 1;
        WalkingFrameCount = 2;
        FallingStartFrame = 1;
        FallingFrameCount = 1;
        FallingAnimSpeed = 0;
    }

    #endregion

    #region Methods

    public override void ModifyPosition(ref Vector2 position, in PlayerDrawSet drawData)
    {
        if (drawData.headOnlyRender)
        {
            return;
        }

        position.Y += 6;
    }

    #endregion
}