using System;
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

    public IncreaseSpawnRateEffect(int duration) : base(EffectID.IncreaseSpawnRate, duration, EffectSeverity.Negative)
    {
        CrowdControlNPC.EditSpawnRateHook += EditSpawnRate;
        On_NPC.SpawnNPC += OnSpawnNPC;
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
        if (!IsActiveFor(player))
        {
            return;
        }

        // Set the spawn rate if the effect is active for the player
        spawnRate = (int)(spawnRate / Factor);
        maxSpawns = (int)(maxSpawns * Factor);
    }

    private void OnSpawnNPC(On_NPC.orig_SpawnNPC orig)
    {
        // Make it so the town is no longer safe, so that enemies spawn instead of just critters

        Span<float> cachedValues = stackalloc float[Main.maxPlayers];
        for (var i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (!player.active)
            {
                continue;
            }

            cachedValues[i] = player.townNPCs;
            if (!IsActiveFor(player))
            {
                continue;
            }

            // Trick the game into thinking the player isn't in a town
            player.townNPCs = 0f;
        }

        // Naturally spawn an NPC
        orig.Invoke();

        // Restore cached values
        for (var i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (!player.active)
            {
                continue;
            }

            player.townNPCs = cachedValues[i];
        }
    }

    private bool IsActiveFor(Player player)
    {
        return (NetUtils.IsSinglePlayer && CrowdControlMod.GetInstance().IsSessionActive && IsActive) || (NetUtils.IsServer && IsActiveOnServer(player));
    }

    #endregion
}