using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.Challenges;

/// <summary>
///     Challenge effects require a goal to be completed within a specified time.
///     The player is punished or rewarded depending on if they were able to complete the challenge.
/// </summary>
public abstract class ChallengeEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private static readonly string LocId = "challenge";

    /// <summary>
    ///     Any challenge is active (any effect that implements ChallengeEffect)
    /// </summary>
    private static bool _isAnyChallengeActive;

    #endregion

    #region Fields

    /// <summary>
    ///     Challenge is marked as completed so the player won't be punished.
    /// </summary>
    private bool _isCompleted;

    #endregion

    public sealed override EffectCategory Category => EffectCategory.Challenge;

    #region Constructors

    protected ChallengeEffect(string id, float duration) : base(id, duration, EffectSeverity.Neutral)
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
            player.Player.statLife = Math.Max(player.Player.statLife, player.Player.statLifeMax2);
            SoundEngine.PlaySound(SoundID.AchievementComplete, player.Player.position);
            player.Player.Emote(EmoteID.EmoteLaugh);
            var index = Projectile.NewProjectile(null, player.Player.position, Vector2.UnitY * Main.rand.NextFloat(3f, 5f) * -1f, ProjectileID.ConfettiGun, 1, 0f, player.Player.whoAmI);
            Main.projectile[index].friendly = true;

            if (NetUtils.IsClient)
            {
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
        }

        _isAnyChallengeActive = false;
        _isCompleted = false;
        CrowdControlModSystem.PostDrawInterfaceHook -= PostDrawInterface;
        OnChallengeStop();
    }

    protected sealed override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        // Write normal message
        var challengeString = GetChallengeDescription();
        TerrariaUtils.WriteEffectMessage(ItemID.FastClock, LangUtils.GetEffectStartText(LocId, viewerString, playerString, durationString, challengeString), Severity);
    }

    protected sealed override void SendStopMessage()
    {
        // Stop message depends on challenge outcome
        if (_isCompleted)
        {
            TerrariaUtils.WriteMessage(ItemID.LargeEmerald, LangUtils.GetEffectMiscText(LocId, "Completed"), Color.Green);
        }
        else if (TimeLeft == 0 && CrowdControlMod.GetInstance().IsSessionActive)
        {
            var player = GetLocalPlayer();
            var reason = LangUtils.GetEffectMiscText(LocId, "Failed", player.Player.name);
            player.Player.KillMe(PlayerDeathReason.ByCustomReason(reason), 1000, 0);
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
        const float scale = 0.95f;
        const float scaleMagnitude = 0.12f;
        const float scaleFrequency = 0.75f;

        // Draw 'challenge description' string
        Utils.DrawBorderString(
            spriteBatch,
            GetChallengeDescription(),
            new Vector2(center.X, center.Y + 45f),
            Color.Yellow,
            scale + Math.Abs((float)Math.Sin(Main.GlobalTimeWrappedHourly * scaleFrequency) * scaleMagnitude),
            0.5f, 0.5f);

        // Choose colour based on time left
        const float redStart = 5.5f;
        const float redEnd = 2.5f;
        const float yellowStart = 8.5f;
        const float yellowEnd = 5.5f;
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
        Utils.DrawBorderString(
            spriteBatch,
            LangUtils.GetEffectMiscText(LocId, "TimeLeft", TimeLeft.ToString("0.0")),
            new Vector2(center.X, center.Y + 66f),
            colour,
            scale,
            0.5f, 0.5f);
    }

    #endregion
}