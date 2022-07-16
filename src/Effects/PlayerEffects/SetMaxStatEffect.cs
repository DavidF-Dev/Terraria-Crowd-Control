using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Increase or decrease the player's current max health or max mana.
/// </summary>
public sealed class SetMaxStatEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int Amount = 20;

    #endregion

    #region Fields

    private readonly bool _increase;
    private readonly bool _life;

    #endregion

    #region Constructors

    public SetMaxStatEffect(string id, bool increase, bool life) : base(id, null, increase ? EffectSeverity.Positive : EffectSeverity.Negative)
    {
        _increase = increase;
        _life = life;
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if ((_life && _increase && player.Player.statLifeMax >= 500) ||
            (_life && !_increase && player.Player.statLifeMax <= 20) ||
            (!_life && _increase && player.Player.statManaMax >= 200) ||
            (!_life && !_increase && player.Player.statManaMax <= 20))
        {
            // Ignore if the stat cannot be further altered
            return CrowdControlResponseStatus.Failure;
        }

        if (_life)
        {
            if (_increase)
            {
                // Increase the player's current max health
                player.Player.statLifeMax += Amount;
                player.Player.statLife += Amount;
                player.Player.AddBuff(BuffID.Lovestruck, 60 * 5);
            }
            else
            {
                // Decrease the player's current max health
                player.Player.statLifeMax -= Amount;
            }
        }
        else
        {
            if (_increase)
            {
                // Increase the player's current max mana
                player.Player.statManaMax += Amount;
                player.Player.statMana += Amount;
            }
            else
            {
                // Decrease the player's current max mana
                player.Player.statManaMax -= Amount;
            }
        }


        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        // Send the appropriate effect message
        var type = _life ? "health" : "mana";
        var message = _increase
            ? $"{viewerString} added {Amount} {type} to {playerString}'s total {type}"
            : $"{viewerString} removed {Amount} {type} from {playerString}'s total {type}";
        TerrariaUtils.WriteEffectMessage(_life ? ItemID.LifeCrystal : ItemID.ManaCrystal, message, Severity);
    }

    #endregion
}