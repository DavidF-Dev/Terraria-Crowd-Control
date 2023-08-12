using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
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

    private enum SeedState
    {
        /// <summary>
        ///     Enable the seed.
        /// </summary>
        Enable,

        /// <summary>
        ///     Disable the seed.
        /// </summary>
        Disable,

        /// <summary>
        ///     Temporarily enable the seed.
        /// </summary>
        Temp
    }

    #endregion

    #region Static Methods

    private static string GetId(SeedType seedType, SeedState seedState)
    {
        return seedType switch
        {
            SeedType.ForTheWorthy => seedState switch
            {
                SeedState.Enable => EffectID.EnableForTheWorthy,
                SeedState.Disable => EffectID.DisableForTheWorthy,
                _ => EffectID.TempForTheWorthy
            },
            SeedType.DontStarve => seedState switch
            {
                SeedState.Enable => EffectID.EnableTheConstant,
                SeedState.Disable => EffectID.DisableTheConstant,
                _ => EffectID.TempTheConstant
            },
            _ => throw new ArgumentOutOfRangeException(nameof(seedType), seedType, null)
        };
    }

    #endregion

    #region Fields

    private readonly SeedType _seedType;
    private readonly SeedState _seedState;

    #endregion

    #region Constructors

    public ToggleWorldSeedEffect(SeedType seedType, bool enabled) : base(GetId(seedType, enabled ? SeedState.Enable : SeedState.Disable), 0, EffectSeverity.Neutral)
    {
        _seedType = seedType;
        _seedState = enabled ? SeedState.Enable : SeedState.Disable;
    }

    public ToggleWorldSeedEffect(SeedType seedType, int duration) : base(GetId(seedType, SeedState.Temp), duration, EffectSeverity.Neutral)
    {
        _seedType = seedType;
        _seedState = SeedState.Temp;
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (_seedState is SeedState.Enable or SeedState.Temp &&
            ((_seedType == SeedType.ForTheWorthy && WorldUtils.IsForTheWorthy) ||
             (_seedType == SeedType.DontStarve && WorldUtils.IsDontStarve)))
        {
            // Already enabled
            return CrowdControlResponseStatus.Failure;
        }

        if (_seedState == SeedState.Disable &&
            ((_seedType == SeedType.ForTheWorthy && (!WorldUtils.IsForTheWorthy || (CrowdControlMod.GetInstance().GetEffect(EffectID.TempForTheWorthy)?.IsActive ?? false))) ||
             (_seedType == SeedType.DontStarve && (!WorldUtils.IsDontStarve || (CrowdControlMod.GetInstance().GetEffect(EffectID.TempTheConstant)?.IsActive ?? false)))))
        {
            // Already disabled (or it is enabled temporarily)
            return CrowdControlResponseStatus.Failure;
        }

        if (NetUtils.IsSinglePlayer)
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

    protected override void OnStop()
    {
        if (_seedState != SeedState.Temp)
        {
            // Ignore
            return;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // Toggle in single-player
            PerformToggle();
        }
        else
        {
            // Let the server do its thing
            SendPacket(PacketID.HandleEffect);
        }
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        PerformToggle();
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        // Determine item to display
        var item = _seedType switch
        {
            SeedType.ForTheWorthy => ItemID.SkeletronMasterTrophy,
            SeedType.DontStarve => ItemID.ChesterPetItem,
            _ => (short)0
        };

        TerrariaUtils.WriteEffectMessage(item, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
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

        if (NetUtils.IsServer)
        {
            // Update the clients
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    #endregion
}