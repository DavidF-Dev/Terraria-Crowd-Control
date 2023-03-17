using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Drastically increase the spawn-rate around the player for a short duration.
/// </summary>
public sealed class IncreaseSpawnRateEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float Factor = 28f;

    #endregion

    #region Constructors

    public IncreaseSpawnRateEffect(float duration) : base(EffectID.IncreaseSpawnRate, duration, EffectSeverity.Negative)
    {
        CrowdControlNPC.EditSpawnRateHook += EditSpawnRate;
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    protected override int StartEmote => EmoteID.EmotionAlert;

    #endregion

    #region Methods

    protected override void OnDisposed()
    {
        CrowdControlNPC.EditSpawnRateHook -= EditSpawnRate;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.WaterCandle, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    private void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if ((NetUtils.IsSinglePlayer && CrowdControlMod.GetInstance().IsSessionActive && IsActive) ||
            (NetUtils.IsServer && IsActiveOnServer(player)))
        {
            // Set the spawn rate if the effect is active for the player
            spawnRate = (int)(spawnRate / Factor);
            maxSpawns = (int)(maxSpawns * Factor);
        }
    }

    #endregion
}