using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Switch the player's loadout to a different one.
/// </summary>
public class SwitchLoadoutEffect : CrowdControlEffect
{
    #region Constructors

    public SwitchLoadoutEffect() : base(EffectID.SwitchLoadout, 0, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        var next = Main.rand.Next(player.Player.Loadouts.Length);
        if (next == player.Player.CurrentLoadoutIndex)
        {
            next = (next + 1) % player.Player.Loadouts.Length;
        }

        if (next == player.Player.CurrentLoadoutIndex)
        {
            return CrowdControlResponseStatus.Unavailable;
        }

        player.Player.TrySwitchingLoadout(next);
        return next == player.Player.CurrentLoadoutIndex ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Failure;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Mannequin, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}