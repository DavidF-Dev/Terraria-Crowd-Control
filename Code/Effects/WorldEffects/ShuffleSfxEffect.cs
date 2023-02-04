using System;
using System.Linq;
using System.Reflection;
using CrowdControlMod.Code.Utilities;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour.HookGen;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Effect that will shuffle sound effects (producing random vanilla sound effects).
/// </summary>
public sealed class ShuffleSfxEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private static readonly SoundStyle[] VanillaSfx;

    #endregion

    #region Fields

    private int _seed;

    #endregion

    #region Constructors

    static ShuffleSfxEffect()
    {
        if (NetUtils.IsServer)
        {
            VanillaSfx = Array.Empty<SoundStyle>();
            return;
        }

        // Cache all the vanilla sfx for future use
        VanillaSfx = typeof(SoundID).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(SoundStyle))
            .Select(x => (SoundStyle)x.GetValue(null)!)
            .ToArray();
    }

    public ShuffleSfxEffect(float duration) : base(EffectID.ShuffleSfx, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // TODO: Detour not supported (temporary solution in place)
        // On_SoundPlayer.Play += OnPlaySfx;
        HookEndpointManager.Add(DetourUtils.PlaySoundMethod, OnPlaySfx);
        _seed = Main.rand.Next(VanillaSfx.Length);

        return VanillaSfx.Length == 0 ? CrowdControlResponseStatus.Unavailable : CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        // On_SoundPlayer.Play -= OnPlaySfx;
        HookEndpointManager.Remove(DetourUtils.PlaySoundMethod, OnPlaySfx);
        _seed = 0;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Bell, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    private SlotId OnPlaySfx(DetourUtils.PlaySoundDelegate orig, SoundPlayer self, ref SoundStyle style, Vector2? position, SoundUpdateCallback callback)
    {
        // Play a random sfx for the attempted sfx
        var hash = Math.Abs((style.Identifier ?? style.SoundPath).GetHashCode());
        var original = style;
        try
        {
            style = VanillaSfx[(hash + _seed) % VanillaSfx.Length];
            return orig.Invoke(self, ref style, position, callback);
        }
        catch (AssetLoadException e)
        {
            // "AssetLoadException: Asset could not be found: "Sounds\Zombie_131" (seems to be very rare)
            TerrariaUtils.WriteDebug($"Failed to shuffle sfx due to an exception. {nameof(AssetLoadException)}: {original.SoundPath} -> {style.SoundPath} (seed: {_seed}).");
            style = original;
            return orig.Invoke(self, ref style, position, callback);
        }
    }

    #endregion
}