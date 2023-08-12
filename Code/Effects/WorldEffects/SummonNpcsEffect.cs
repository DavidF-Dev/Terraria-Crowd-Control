using System.IO;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Teleports all the NPCs in the world to the player's position.
/// </summary>
public sealed class SummonNpcsEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int MaxMultiplayerTeleports = int.MaxValue;

    #endregion

    #region Static Methods

    private static bool CanBeSummoned(NPC npc)
    {
        return npc.active;
    }

    #endregion

    #region Constructors

    public SummonNpcsEffect() : base(EffectID.SummonNpcs, 0, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Check if there are any npcs that can be teleported
        var npcs = Main.npc.Where(CanBeSummoned).ToList();
        if (!npcs.Any())
        {
            return CrowdControlResponseStatus.Retry;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // Teleport all the npcs to the player in single-player
            foreach (var npc in npcs)
            {
                npc.position = GetLocalPlayer().Player.position;
            }
        }
        else
        {
            // Let the server deal with the chaos
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Teleport all the npcs to the player and update the clients
        var c = 0;
        foreach (var npc in Main.npc.Where(CanBeSummoned))
        {
            if (c++ == MaxMultiplayerTeleports)
            {
                // End early if we've reached our multiplayer maximum
                TerrariaUtils.WriteDebug($"Hit NPC summon maximum: {MaxMultiplayerTeleports}");
                break;
            }

            npc.position = player.Player.position;
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
        }
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.KingStatue, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}