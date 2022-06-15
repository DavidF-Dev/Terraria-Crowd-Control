using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
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
            return CrowdControlResponseStatus.Failure;
        }

        var result = OnChallengeStart();
        if (result != CrowdControlResponseStatus.Success)
        {
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
    protected abstract CrowdControlResponseStatus OnChallengeStart();

    /// <summary>
    ///     Invoked when the challenge has ended.
    /// </summary>
    protected abstract void OnChallengeStop();

    /// <summary>
    ///     Get the challenge description string.<br />
    ///     e.g. "Craft a pickaxe"
    /// </summary>
    protected abstract string GetChallengeDescription();

    private void PostDrawInterface(SpriteBatch spriteBatch)
    {
        // Draw the challenge text below the player
        var player = GetLocalPlayer();
        var pos = player.Player.position;
        const float offset1 = 16f * 4f;
        const float offset2 = 16f * 4.5f;
        const float scale = 2f;

        // TODO: Text not appearing
        
        spriteBatch.DrawString(
            FontAssets.DeathText.Value,
            $"Complete challenge in {TimeLeft:0.00} seconds",
            new Vector2(pos.X, pos.Y + offset1),
            Color.White,
            0f,
            Vector2.Zero,
            Vector2.One * scale,
            SpriteEffects.None,
            0f);

        spriteBatch.DrawString(
            FontAssets.DeathText.Value,
            GetChallengeDescription(),
            new Vector2(pos.X, pos.Y + offset2),
            Color.Yellow,
            0f,
            Vector2.Zero,
            Vector2.One * scale,
            SpriteEffects.None,
            0f);
    }

    #endregion
}