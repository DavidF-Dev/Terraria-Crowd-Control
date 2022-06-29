using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Toggle the world seed to change how the game is played.
/// </summary>
public sealed class ToggleWorldSeedEffect : CrowdControlEffect
{
    #region Enums

    public enum SeedType
    {
        ForTheWorthy,
        DontStarve
    }

    #endregion

    #region Static Methods

    [NotNull]
    private static string GetId(SeedType seedType, bool enable)
    {
        return seedType switch
        {
            SeedType.ForTheWorthy => enable ? EffectID.EnableForTheWorthy : EffectID.DisableForTheWorthy,
            SeedType.DontStarve => enable ? EffectID.EnableTheConstant : EffectID.DisableTheConstant,
            _ => throw new ArgumentOutOfRangeException(nameof(seedType), seedType, null)
        };
    }

    #endregion

    #region Fields

    private readonly SeedType _seedType;
    private readonly bool _enable;

    #endregion

    #region Constructors

    public ToggleWorldSeedEffect(SeedType seedType, bool enable) : base(GetId(seedType, enable), null, EffectSeverity.Neutral)
    {
        _seedType = seedType;
        _enable = enable;
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Fail if cannot be toggled
        switch (_seedType)
        {
            case SeedType.ForTheWorthy:
                switch (_enable)
                {
                    case true when WorldUtils.IsForTheWorthy:
                    case false when !WorldUtils.IsForTheWorthy:
                        return CrowdControlResponseStatus.Failure;
                }

                break;
            case SeedType.DontStarve:
                switch (_enable)
                {
                    case true when WorldUtils.IsDontStarve:
                    case false when !WorldUtils.IsDontStarve:
                        return CrowdControlResponseStatus.Failure;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Toggle in single-player
            PerformToggle();
        }
        else
        {
            // Let the server do its thing
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        PerformToggle();
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        var itemId = _seedType switch
        {
            SeedType.ForTheWorthy => ItemID.SkeletronMasterTrophy,
            SeedType.DontStarve => ItemID.ChesterPetItem,
            _ => throw new ArgumentOutOfRangeException()
        };

        var mode = _seedType switch
        {
            SeedType.ForTheWorthy => "For the Worthy",
            SeedType.DontStarve => "Don't Starve",
            _ => throw new ArgumentOutOfRangeException()
        };

        TerrariaUtils.WriteEffectMessage(itemId, $"{viewerString} {(_enable ? "enabled" : "disabled")} \"{mode}\" mode in {playerString}'s world", Severity);
    }

    private void PerformToggle()
    {
        // Toggle the correct value
        switch (_seedType)
        {
            case SeedType.ForTheWorthy:
                WorldUtils.IsForTheWorthy = !WorldUtils.IsForTheWorthy;
                TerrariaUtils.WriteDebug($"\"For the Worthy\" mode: {WorldUtils.IsForTheWorthy}");
                break;
            case SeedType.DontStarve:
                WorldUtils.IsDontStarve = !WorldUtils.IsDontStarve;
                TerrariaUtils.WriteDebug($"\"Don't Starve\" mode: {WorldUtils.IsForTheWorthy}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (Main.netMode == NetmodeID.Server)
        {
            // Update the clients
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    #endregion
}