using System.Linq;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Drastically increase the spawn-rate around the player for a short duration.
/// </summary>
public sealed class IncreaseSpawnRateEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float Factor = 20f;

    #endregion

    #region Constructors

    public IncreaseSpawnRateEffect(float duration) : base(EffectID.IncreaseSpawnRate, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override void OnSessionStarted()
    {
        CrowdControlNPC.EditSpawnRateHook += EditSpawnRate;
    }

    protected override void OnSessionStopped()
    {
        CrowdControlNPC.EditSpawnRateHook -= EditSpawnRate;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.WaterCandle, $"{viewerString} increased the enemy spawn-rate around {playerString} for {durationString} seconds", Severity);
    }

    private void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        // Set the spawn rate if the effect is active for the player
        if ((Main.netMode == NetmodeID.SinglePlayer && IsActive) ||
            (Main.netMode == NetmodeID.Server && ActiveOnServer.Contains(player.whoAmI)))
        {
            spawnRate = (int)(spawnRate / Factor);
            maxSpawns *= (int)(maxSpawns * Factor);
        }
    }

    #endregion
}