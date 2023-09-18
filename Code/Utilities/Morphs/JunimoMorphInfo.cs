using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
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
        position.Y += 2;
    }

    public override void ModifyDirection(ref int direction, in PlayerDrawSet drawData)
    {
        direction *= -1;
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