using System.Linq;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

public sealed class IncreaseSpawnRateEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float Factor = 20f;

    #endregion

    #region Constructors

    public IncreaseSpawnRateEffect(float duration) : base(EffectID.IncreaseSpawnRate, duration, EffectSeverity.Negative)
    {
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
            spawnRate = (int)(spawnRate / Factor);
            maxSpawns *= (int)(maxSpawns * Factor);
        }
    }

    #endregion
}