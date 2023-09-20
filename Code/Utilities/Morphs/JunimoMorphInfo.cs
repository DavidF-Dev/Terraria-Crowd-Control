using System;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Code.Utilities.Morphs;

public sealed class JunimoMorphInfo : MorphInfo
{
    #region Constructors

    public JunimoMorphInfo()
    {
        Main.instance.LoadProjectile(ProjectileID.JunimoPet);
        Texture = TextureAssets.Projectile[ProjectileID.JunimoPet].Value;
        TotalFrames = 16;

        IdleStartFrame = 0;
        IdleFrameCount = 4;
        IdleAnimSpeed = 0.1f;
        FallingStartFrame = 13;
        FallingFrameCount = 3;
        FallingAnimSpeed = IdleAnimSpeed;
        WalkingStartFrame = 4;
        WalkingFrameCount = 8;
        WalkingAnimSpeed = IdleAnimSpeed;
    }

    #endregion

    public override void ModifyPosition(ref Vector2 position, in PlayerDrawSet drawData)
    {
        if (drawData.headOnlyRender)
        {
            return;
        }
        
        position.Y += 2;
    }

    public override void ModifyDirection(ref int direction, in PlayerDrawSet drawData)
    {
        direction *= -1;
    }

    public override void OnDrawFront(in PlayerDrawSet drawData, Vector2 position, Color colour, float rotation, float scale, int direction, int currentFrame)
    {
        if (DateTime.Now.Month != 9)
        {
            return;
        }

        // Show a party hat if September (Faye's birthday month)
        Main.instance.LoadItem(ItemID.PartyHat);
        var texture = TextureAssets.Item[ItemID.PartyHat].Value;

        position.X += direction * 4f;
        position.Y += 2f + 2f * currentFrame switch
        {
            0 or 3 or 6 or 10 => 0,
            1 or 2 or 5 or 7 or 9 or 11 => -1,
            4 or 8 or 12 or 13 or 14 or 15 => -2,
            _ => 0
        };

        scale *= 1.2f;

        drawData.DrawDataCache.Add(new DrawData(
            texture,
            position,
            null,
            colour,
            rotation,
            new Vector2(texture.Width * 0.5f, texture.Width),
            scale,
            direction != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
        ));
    }

    public override void ModifyHurt(Player player, ref Player.HurtModifiers modifiers)
    {
        modifiers.DisableSound();
    }

    public override void OnHurt(Player player, in Player.HurtInfo info)
    {
        if (NetUtils.IsServer)
        {
            return;
        }

        SoundEngine.PlaySound(new SoundStyle("CrowdControlMod/Assets/Sounds/JunimoMeep")
        {
            PitchVariance = 0.2f,
            Volume = 0.75f,
            MaxInstances = SoundID.PlayerHit.MaxInstances,
            SoundLimitBehavior = SoundID.PlayerHit.SoundLimitBehavior,
            PlayOnlyIfFocused = SoundID.PlayerHit.PlayOnlyIfFocused
        }, player.Center);
    }
}