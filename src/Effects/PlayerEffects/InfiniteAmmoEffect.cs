using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Provide the player with infinite ammo and mana for a short duration.
/// </summary>
public sealed class InfiniteAmmoEffect : CrowdControlEffect
{
    #region Static Methods

    private static bool CanConsumeAmmo(Item _, Item __)
    {
        // Do not consume ammo
        return false;
    }

    private static void PostUpdate()
    {
        // Refresh the mana stat
        var player = GetLocalPlayer();
        player.Player.statMana = player.Player.statManaMax2;
    }

    private static void PostUpdateEquips()
    {
        // Slightly increase ranged damage
        var player = GetLocalPlayer();
        player.Player.arrowDamage += 0.1f;
        player.Player.bulletDamage += 0.1f;
    }

    #endregion

    #region Constructors

    public InfiniteAmmoEffect(float duration) : base(EffectID.InfiniteAmmo, duration, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Properties

    protected override int StartEmote => EmoteID.ItemMinishark;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        player.CanConsumeAmmoHook += CanConsumeAmmo;
        player.PostUpdateHook += PostUpdate;
        player.PostUpdateEquipsHook += PostUpdateEquips;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.CanConsumeAmmoHook -= CanConsumeAmmo;
        player.PostUpdateHook -= PostUpdate;
        player.PostUpdateEquipsHook -= PostUpdateEquips;
        base.OnStop();
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.EndlessQuiver, $"{viewerString} provided infinite ammo and mana to {playerString} for {durationString} seconds", Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, "Ammo and mana are back to normal", EffectSeverity.Neutral);
    }

    #endregion
}