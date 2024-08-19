using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Effect that drops smoke bombs near the player.
/// </summary>
public sealed class ShootSmokeBombsEffect : CrowdControlEffect
{
    #region Constructors

    public ShootSmokeBombsEffect() : base(EffectID.ShootSmokeBombs, 0, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (!player.Player.IsGrounded() || player.Player.shimmering)
        {
            return CrowdControlResponseStatus.Retry;
        }

        // Spawn smoke bombs
        const int n = 8;
        const float minSpeed = 0.25f;
        const float maxSpeed = 3.0f;
        for (var i = 0; i < n; i++)
        {
            var direction = (MathHelper.PiOver2 * 0.6f).ToRotationVector2();
            direction.Y *= -1;
            var speed = MathHelper.Lerp(minSpeed, maxSpeed, i / (float)n);
            var velocity = direction * speed;
            SpawnSmokeBomb(velocity);
            SpawnSmokeBomb(velocity with {X = -velocity.X});
        }

        // Spawn middle smoke bomb
        SpawnSmokeBomb(Vector2.UnitY * minSpeed * -1);

        // Give the player a bit of immunity so that they blink
        player.Player.immune = true;
        player.Player.immuneNoBlink = false;
        player.Player.immuneTime = Math.Max(player.Player.immuneTime, 60);

        return CrowdControlResponseStatus.Success;

        static int SpawnSmokeBomb(Vector2 velocity)
        {
            var index = Projectile.NewProjectile(null, Main.LocalPlayer.Center, velocity, ProjectileID.SmokeBomb, 1, 0, Main.myPlayer);
            Main.projectile[index].timeLeft = 60 * 20;
            if (NetUtils.IsClient)
            {
                Main.projectile[index].netUpdate = true;
            }

            return index;
        }
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.SmokeBomb, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}