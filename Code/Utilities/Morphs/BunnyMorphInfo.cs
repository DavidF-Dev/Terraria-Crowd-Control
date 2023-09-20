using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Code.Utilities.Morphs;

public sealed class BunnyMorphInfo : MorphInfo
{
    #region Constructors

    public BunnyMorphInfo() : this(NPCID.Bunny)
    {
    }

    public BunnyMorphInfo(int bunnyNpcType)
    {
        Main.instance.LoadNPC(bunnyNpcType);
        Texture = TextureAssets.Npc[bunnyNpcType].Value;
        TotalFrames = 7;

        IdleStartFrame = 0;
        IdleFrameCount = 1;
        IdleAnimSpeed = 0;
        WalkingStartFrame = 1;
        WalkingFrameCount = 6;
        FallingStartFrame = 6;
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

        position.Y += 4;
    }

    #endregion
}