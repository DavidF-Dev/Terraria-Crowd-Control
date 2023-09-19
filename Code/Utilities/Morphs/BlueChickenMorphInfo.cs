using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Code.Utilities.Morphs;

public sealed class BlueChickenMorphInfo : MorphInfo
{
    #region Constructors

    public BlueChickenMorphInfo()
    {
        Main.instance.LoadProjectile(ProjectileID.BlueChickenPet);
        Texture = TextureAssets.Projectile[ProjectileID.BlueChickenPet].Value;
        TotalFrames = 10;

        IdleStartFrame = 0;
        IdleFrameCount = 1;
        IdleAnimSpeed = 0;
        WalkingStartFrame = 1;
        WalkingFrameCount = 5;
        FallingStartFrame = 6;
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

        position.Y += 4;
    }

    #endregion
}