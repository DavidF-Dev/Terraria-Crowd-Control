using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Features;
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

    public override EffectCategory Category => EffectCategory.Player;
    
    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (_life)
        {
            if (_increase)
            {
                // Increase the player's current max health
                if (player.Player.AddStatLifeMax(Amount) == Amount)
                {
                    return CrowdControlResponseStatus.Failure;
                }

                player.Player.statLife += Amount;
                player.Player.AddBuff(BuffID.Lovestruck, 60 * 5);
            }
            else
            {
                // Decrease the player's current max health
                if (player.Player.AddStatLifeMax(-Amount) == -Amount)
                {
                    return CrowdControlResponseStatus.Failure;
                }
            }
        }
        else
        {
            if (_increase)
            {
                // Increase the player's current max mana
                if (player.Player.AddStatManaMax(Amount) == Amount)
                {
                    return CrowdControlResponseStatus.Failure;
                }

                player.Player.statMana += Amount;
            }
            else
            {
                // Decrease the player's current max mana
                if (player.Player.AddStatManaMax(-Amount) == -Amount)
                {
                    return CrowdControlResponseStatus.Failure;
                }
            }
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        // Send the appropriate effect message
        var itemId = _life ? ItemID.LifeCrystal : ItemID.ManaCrystal;
        TerrariaUtils.WriteEffectMessage(itemId, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, Amount), Severity);
    }

    #endregion
}