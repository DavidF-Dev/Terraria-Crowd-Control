using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Gives the player some coins and boosts enemy money drop-rates for a short duration.
/// </summary>
public sealed class MoneyBoostEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int MidasDuration = 15;

    #endregion

    #region Static Methods

    private static void PostUpdateEquips()
    {
        // Lucky coin causes enemies to drop more coins
        var player = GetLocalPlayer();
        player.Player.hasLuckyCoin = true;
    }

    private static bool StrikeNpc(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
    {
        // Midas causes enemy to drop more coins
        npc.AddBuff(BuffID.Midas, 60 * MidasDuration);
        return true;
    }

    #endregion

    #region Fields

    private int _coins;

    #endregion

    #region Constructors

    public MoneyBoostEffect(float duration) : base(EffectID.MoneyBoost, duration, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Properties

    protected override int StartEmote => EmoteID.ItemGoldpile;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();

        // Give coins to the player (at least 1 gold)
        _coins = Item.buyPrice(0, Math.Max(Main.rand.Next(-7, 2), 0), Main.rand.Next(100, 200));
        player.Player.GiveCoins(_coins);
        player.Player.SetHairDye(ItemID.MoneyHairDye);

        // Boost coin drops from enemies
        player.PostUpdateEquipsHook += PostUpdateEquips;
        CrowdControlNPC.StrikeNPCHook += StrikeNpc;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _coins = 0;
        GetLocalPlayer().PostUpdateEquipsHook -= PostUpdateEquips;
        CrowdControlNPC.StrikeNPCHook -= StrikeNpc;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.LuckyCoin, $"{viewerString} donated {Main.ValueToCoins(_coins)} to {playerString} and increased coins drops from enemies for {durationString} seconds", Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, "Coin drop-rate is back to normal", EffectSeverity.Neutral);
    }

    #endregion
}