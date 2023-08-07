using System;
using System.Linq;
using System.Reflection;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SoundPlayer = On.Terraria.Audio.SoundPlayer;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Effect that will shuffle sound effects (producing random vanilla sound effects).
/// </summary>
public sealed class ShuffleSfxEffect : CrowdControlEffect
{
    #region Fields

    private readonly SoundStyle[] _vanillaSfx;
    private int _seed;

    #endregion

    #region Constructors

    public ShuffleSfxEffect(float duration) : base(EffectID.ShuffleSfx, duration, EffectSeverity.Neutral)
    {
        // Cache all the vanilla sfx for future use
        _vanillaSfx = typeof(SoundID).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(SoundStyle))
            .Select(x => (SoundStyle)x.GetValue(null)!)
            .Where(x => ModContent.HasAsset(x.Variants.Length == 0 ? x.SoundPath : x.SoundPath + "_" + x.Variants[0]))
            .ToArray();
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        SoundPlayer.Play += OnPlaySfx;
        _seed = Main.rand.Next(_vanillaSfx.Length);
        return _vanillaSfx.Length == 0 ? CrowdControlResponseStatus.Unavailable : CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        SoundPlayer.Play -= OnPlaySfx;
        _seed = 0;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Bell, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    private SlotId OnPlaySfx(SoundPlayer.orig_Play orig, Terraria.Audio.SoundPlayer self, ref SoundStyle style, Vector2? position)
    {
        SoundStyle shuffled = default;
        try
        {
            // Play a random sfx for the attempted sfx; do not modify the provided style EVER!
            var hash = Math.Abs((style.Identifier ?? style.SoundPath).GetHashCode());
            shuffled = _vanillaSfx[(hash + _seed) % _vanillaSfx.Length] with
            {
                Identifier = style.Identifier,
                IsLooped = style.IsLooped,
                MaxInstances = style.MaxInstances,
                PlayOnlyIfFocused = style.PlayOnlyIfFocused,
                SoundLimitBehavior = style.SoundLimitBehavior,
                Type = style.Type,
                Volume = style.Volume
            };

            // Check that the asset exists before committing
            // The VanillaSfx collection has been stripped of invalid assets, so this shouldn't be an issue
            if (ModContent.HasAsset(shuffled.Variants.Length == 0 ? shuffled.SoundPath : shuffled.SoundPath + "_" + shuffled.Variants[0]))
            {
                return orig.Invoke(self, ref shuffled, position);
            }

            // Asset doesn't exist
            TerrariaUtils.WriteDebug($"Failed to shuffle sfx. Asset doesn't exist: {style.SoundPath} (original) -> {shuffled.SoundPath} (shuffled) (seed: {_seed}).");
            return orig.Invoke(self, ref style, position);
        }
        catch (AssetLoadException)
        {
            // "AssetLoadException: Asset could not be found: "Sounds\Zombie_131" (seems to be very rare)
            TerrariaUtils.WriteDebug($"Failed to shuffle sfx due to an exception. {nameof(AssetLoadException)}: {style.SoundPath} -> {shuffled.SoundPath} (seed: {_seed}).");
            return orig.Invoke(self, ref style, position);
        }
    }

    #endregion
}