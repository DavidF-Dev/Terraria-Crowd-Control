using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Prevent the player from picking up any items in their inventory for a short duration.
/// </summary>
public sealed class NoItemPickupEffect : CrowdControlEffect
{
    #region Static Methods

    private static bool CanPickup(Item item, Player player)
    {
        return GetLocalPlayer().Player != player;
    }

    #endregion

    #region Constructors

    public NoItemPickupEffect(float duration) : base(EffectID.NoItemPickup, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    protected override int StartEmote => EmoteID.DebuffCurse;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        CrowdControlItem.CanPickupHook += CanPickup;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        CrowdControlItem.CanPickupHook -= CanPickup;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.EncumberingStone, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}