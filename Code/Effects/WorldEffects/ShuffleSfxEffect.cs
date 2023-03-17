using System;
using System.Linq;
using System.Reflection;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using SoundPlayer = On.Terraria.Audio.SoundPlayer;

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
        SoundPlayer.Play += OnPlaySfx;
        _seed = Main.rand.Next(VanillaSfx.Length);

        return VanillaSfx.Length == 0 ? CrowdControlResponseStatus.Unavailable : CrowdControlResponseStatus.Success;
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
        // Play a random sfx for the attempted sfx
        var hash = Math.Abs((style.Identifier ?? style.SoundPath).GetHashCode());
        style = VanillaSfx[(hash + _seed) % VanillaSfx.Length];
        return orig.Invoke(self, ref style, position);
    }

    #endregion
}