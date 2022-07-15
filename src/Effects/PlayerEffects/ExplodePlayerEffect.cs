using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Kill the player by spawning an explosion.
/// </summary>
public sealed class ExplodePlayerEffect : CrowdControlEffect
{
    #region Constructors

    public ExplodePlayerEffect() : base(EffectID.ExplodePlayer, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.IsInvincible() || player.Player.IsWithinSpawnProtection())
        {
            // Ignore if the player is invincible or within spawn protection
            return CrowdControlResponseStatus.Retry;
        }

        // Spawn an explosion (kill the player when the start message is sent)
        Projectile.NewProjectile(null, player.Player.Center, Vector2.Zero, CrowdControlMod.GetInstance().Find<ModProjectile>(nameof(InstantDynamite)).Type, 1, 1f, player.Player.whoAmI);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        // Kill the player here
        GetLocalPlayer().Player.KillMe(PlayerDeathReason.ByCustomReason($"{playerString} was brutally torn apart by {viewerString}'s explosive"), 1000, 0);
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class InstantDynamite : ModProjectile
    {
        #region Properties

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Dynamite}";

        #endregion

        #region Methods

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dynamite");
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Dynamite);
            AIType = ProjectileID.Dynamite;
            Projectile.timeLeft = 3;
            Projectile.damage = 1000;
            Projectile.knockBack = 10f;
        }

        public override bool PreAI()
        {
            Projectile.type = ProjectileID.Dynamite;
            return base.PreAI();
        }

        #endregion
    }

    #endregion
}