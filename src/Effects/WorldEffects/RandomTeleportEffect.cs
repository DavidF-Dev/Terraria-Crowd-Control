using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.Audio;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

public sealed class RandomTeleportEffect : CrowdControlEffect
{
    #region Constructors

    public RandomTeleportEffect() : base(EffectID.RandomTeleport, null, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        player.Player.TeleportationPotion();
        SoundEngine.PlaySound(SoundID.Item6, player.Player.position);
        PlayerUtils.SetHairDye(player, ItemID.BiomeHairDye);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.TeleportationPotion, $"{viewerString} randomly teleported {playerString}", Severity);
    }

    #endregion
}