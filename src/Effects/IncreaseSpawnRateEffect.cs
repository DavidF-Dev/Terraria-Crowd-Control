using System.Linq;
using CrowdControlMod.Globals;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects;

public sealed class IncreaseSpawnRateEffect : CrowdControlEffect
{
    #region Fields

    private readonly float _factor;

    #endregion

    #region Constructors

    public IncreaseSpawnRateEffect([NotNull] string id, float duration, float factor) : base(id, duration, EffectSeverity.Negative)
    {
        _factor = factor;
        CrowdControlNPC.EditSpawnRateHook += EditSpawnRate;
    }

    #endregion

    #region Methods

    protected override void OnDispose()
    {
        CrowdControlNPC.EditSpawnRateHook -= EditSpawnRate;
    }

    private void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        // Set the spawn rate if the effect is active for the player
        if ((Main.netMode == NetmodeID.SinglePlayer && IsActive) ||
            (Main.netMode == NetmodeID.Server && ActiveOnServer.Contains(player.whoAmI)))
        {
            spawnRate = (int)(spawnRate / _factor);
            maxSpawns *= (int)(maxSpawns * _factor);
        }
    }

    #endregion
}