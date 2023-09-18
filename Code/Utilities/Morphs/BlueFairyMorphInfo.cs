using System;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Code.Utilities.Morphs;

public sealed class BlueFairyMorphInfo : MorphInfo
{
    #region Constructors

    public BlueFairyMorphInfo()
    {
        Main.instance.LoadProjectile(ProjectileID.BlueFairy);
        Texture = TextureAssets.Projectile[ProjectileID.BlueFairy].Value;
        TotalFrames = 4;

        IdleStartFrame = 0;
        IdleFrameCount = 3;
        IdleAnimSpeed = 0.1f;
        FallingStartFrame = IdleStartFrame;
        FallingFrameCount = IdleFrameCount;
        FallingAnimSpeed = IdleAnimSpeed;
        WalkingStartFrame = IdleStartFrame;
        WalkingFrameCount = IdleFrameCount;
        WalkingAnimSpeed = IdleAnimSpeed;
    }

    public override void ModifyPosition(ref Vector2 position, in PlayerDrawSet drawData)
    {
        position.X += MathF.Sin(Main.GlobalTimeWrappedHourly * 1f) * 3;
        position.Y -= 7 + MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 6;
    }

    public override void PostUpdateEquips(Player player)
    {
        Lighting.AddLight(player.Center, TorchID.Blue);
    }

    public override void DrawEffects(Player player)
    {
        if (!Main.rand.NextBool(3))
        {
            return;
        }

        const int dustCount = 1;
        for (var i = 0; i < dustCount; i++)
        {
            Dust.NewDust(player.position + new Vector2(0f, 8f), player.width, player.height / 2, DustID.BlueFairy, Scale: 0.5f);
        }
    }

    public override void ModifyHurt(Player player, ref Player.HurtModifiers modifiers)
    {
        modifiers.DisableSound();
        modifiers.DisableDust();
    }

    public override void OnHurt(Player player, in Player.HurtInfo info)
    {
        if (NetUtils.IsServer)
        {
            return;
        }

        SoundEngine.PlaySound(SoundID.NPCHit5 with
        {
            MaxInstances = SoundID.PlayerHit.MaxInstances,
            SoundLimitBehavior = SoundID.PlayerHit.SoundLimitBehavior,
            PlayOnlyIfFocused = SoundID.PlayerHit.PlayOnlyIfFocused
        }, player.Center);

        const int dustCount = 15;
        var speed = new Vector2(info.HitDirection * info.Knockback, -3f);
        for (var i = 0; i < dustCount; i++)
        {
            Dust.NewDust(player.position + new Vector2(0f, 8f), player.width, player.height / 2, DustID.BlueFairy, speed.X, speed.Y, Scale: 0.85f);
        }
    }

    #endregion
}