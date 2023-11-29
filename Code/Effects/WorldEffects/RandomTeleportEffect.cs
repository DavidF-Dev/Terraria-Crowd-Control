using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Force the player to randomly teleport (either using random tp potion or conch).
/// </summary>
public sealed class RandomTeleportEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int RandomPotionChance = 85;

    #endregion

    #region Constructors

    public RandomTeleportEffect() : base(EffectID.RandomTeleport, 0, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (Main.rand.Next(100) < RandomPotionChance)
        {
            // Teleport somewhere random
            player.Player.TeleportationPotion();
        }
        else if (Main.rand.Next(100) < 50 && !player.Player.ZoneUnderworldHeight && Main.spawnTileY > Main.tile.Height - 200)
        {
            // Teleport to hell
            if (NetUtils.IsSinglePlayer)
            {
                player.Player.DemonConch();
            }
            else
            {
                NetMessage.SendData(MessageID.RequestTeleportationByServer, number: 2);
            }
        }
        else
        {
            // Teleport to the ocean
            if (NetUtils.IsSinglePlayer)
            {
                player.Player.MagicConch();
            }
            else
            {
                NetMessage.SendData(MessageID.RequestTeleportationByServer, number: 1);
            }
        }

        SoundEngine.PlaySound(SoundID.Item6, player.Player.position);
        player.Player.SetHairDye(ItemID.BiomeHairDye);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.TeleportationPotion, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}