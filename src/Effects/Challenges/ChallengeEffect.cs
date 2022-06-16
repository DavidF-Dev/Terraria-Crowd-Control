using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Effects.Challenges;

public abstract class ChallengeEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private static bool _isAnyChallengeActive;

    #endregion

    #region Fields

    private bool _isCompleted;

    #endregion

    #region Constructors

    protected ChallengeEffect([NotNull] string id, float duration) : base(id, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected sealed override CrowdControlResponseStatus OnStart()
    {
        if (_isAnyChallengeActive)
        {
            // Fail if there is already any challenge active
            return CrowdControlResponseStatus.Failure;
        }

        var result = OnChallengeStart();
        if (result != CrowdControlResponseStatus.Success)
        {
            // Return early if cannot start the effect
            return result;
        }

        // Challenge has begun
        _isAnyChallengeActive = true;
        SoundEngine.PlaySound(SoundID.Thunder, GetLocalPlayer().Player.position);
        CrowdControlModSystem.PostDrawInterfaceHook += PostDrawInterface;
        return CrowdControlResponseStatus.Success;
    }

    protected sealed override void OnStop()
    {
        if (_isCompleted)
        {
            // Reward the player
            var player = GetLocalPlayer();
            SoundEngine.PlaySound(SoundID.AchievementComplete, player.Player.position);
            var index = Projectile.NewProjectile(null, player.Player.position, Vector2.UnitY * Main.rand.NextFloat(3f, 5f) * -1f, ProjectileID.ConfettiGun, 1, 0f, player.Player.whoAmI);
            var proj = Main.projectile[index];
            proj.friendly = true;
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
        }

        _isAnyChallengeActive = false;
        _isCompleted = false;
        CrowdControlModSystem.PostDrawInterfaceHook -= PostDrawInterface;
        OnChallengeStop();
    }

    protected sealed override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        // Write normal message
        var challengeString = TerrariaUtils.GetColouredRichText($"{GetChallengeDescription()} within {durationString} seconds", Color.Yellow);
        TerrariaUtils.WriteEffectMessage(ItemID.FastClock, $"{viewerString} challenged {playerString}: {challengeString}", Severity);
    }

    protected sealed override void SendStopMessage()
    {
        // Stop message depends on challenge outcome
        if (_isCompleted)
        {
            TerrariaUtils.WriteMessage(ItemID.LargeEmerald, "Challenge completed", Color.Green);
        }
        else if (CrowdControlMod.GetInstance().IsSessionActive)
        {
            var player = GetLocalPlayer();
            player.Player.KillMe(PlayerDeathReason.ByCustomReason($"{player.Player.name} failed their challenge"), 1000, 0);
        }
    }

    /// <summary>
    ///     Set the challenge as completed and reward the player.
    /// </summary>
    protected void SetChallengeCompleted()
    {
        if (!IsActive || _isCompleted)
        {
            return;
        }

        // Challenge is complete, so stop the effect and reward the player
        _isCompleted = true;
        Stop();
    }

    /// <summary>
    ///     Invoked when the challenge begins.
    /// </summary>
    protected virtual CrowdControlResponseStatus OnChallengeStart()
    {
        return CrowdControlResponseStatus.Success;
    }

    /// <summary>
    ///     Invoked when the challenge has ended.
    /// </summary>
    protected virtual void OnChallengeStop()
    {
    }

    /// <summary>
    ///     Get the challenge description string.<br />
    ///     e.g. "Craft a pickaxe"
    /// </summary>
    protected abstract string GetChallengeDescription();

    private void PostDrawInterface(SpriteBatch spriteBatch)
    {
        var center = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
        var font = FontAssets.MouseText.Value;
        const float scale = 0.95f;

        // Determine 'challenge description' string size and set scale 'bob' values
        var str1 = GetChallengeDescription();
        var str1Size = font.MeasureString(str1);
        const float scaleMagnitude = 0.12f;
        const float scaleFrequency = 0.75f;
        
        // Draw 'challenge description' string
        spriteBatch.DrawString(
            font,
            str1,
            new Vector2(center.X, center.Y + 45f),
            Color.Yellow,
            0f,
            str1Size * 0.5f,
            Vector2.One * (scale + Math.Abs((float)Math.Sin(Main.GlobalTimeWrappedHourly * scaleFrequency) * scaleMagnitude)),
            SpriteEffects.None,
            0f);

        // Determine 'time left' string size and set colour timings
        var str2 = $"{TimeLeft:0.0} seconds remaining";
        var str2Size = font.MeasureString(str2);
        const float redStart = 5.5f;
        const float redEnd = 2.5f;
        const float yellowStart = 8.5f;
        const float yellowEnd = 5.5f;

        // Choose colour based on time left
        Color colour;
        if (TimeLeft < redEnd)
        {
            colour = Color.Red;
        }
        else if (TimeLeft < redStart)
        {
            const float offset = redStart - redEnd;
            colour = Color.Lerp(Color.Red, Color.Yellow, (TimeLeft - redEnd) / offset);
        }
        else if (TimeLeft < yellowEnd)
        {
            colour = Color.Yellow;
        }
        else if (TimeLeft < yellowStart)
        {
            const float offset = yellowStart - yellowEnd;
            colour = Color.Lerp(Color.Yellow, Color.White, (TimeLeft - yellowEnd) / offset);
        }
        else
        {
            colour = Color.White;
        }
        
        // Draw 'time left' string
        spriteBatch.DrawString(
            font,
            str2,
            new Vector2(center.X, center.Y + 70f),
            colour,
            0f,
            str2Size * 0.5f,
            Vector2.One * scale,
            SpriteEffects.None,
            0f);
    }

    #endregion
}