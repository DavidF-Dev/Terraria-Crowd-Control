using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.WorldEffects;

public sealed class SpawnKingSlime : CrowdControlEffect
{
    #region Static Methods

    private static void Spawn([NotNull] ModPlayer player)
    {
        // Spawn king slime
        var index = NPC.NewNPC(null, (int)player.Player.Center.X, (int)player.Player.Center.Y, NPCID.KingSlime);
        var npc = Main.npc[index];

        // Set king slime settings
        npc.target = player.Player.whoAmI;
        npc.lifeMax = GetLife(ProgressionUtils.GetProgression());
        npc.life = npc.lifeMax;

        if (Main.netMode == NetmodeID.Server)
        {
            // Notify clients if spawned on server
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        }
    }

    private static int GetLife(ProgressionUtils.Progression progress)
    {
        return progress switch
        {
            ProgressionUtils.Progression.PreEye => 1000,
            ProgressionUtils.Progression.PreSkeletron => 1500,
            ProgressionUtils.Progression.PreWall => 2000,
            ProgressionUtils.Progression.PreMech => 2500,
            ProgressionUtils.Progression.PreGolem => 3000,
            ProgressionUtils.Progression.PreLunar => 3500,
            ProgressionUtils.Progression.PreMoonLord => 4000,
            ProgressionUtils.Progression.PostGame => 4500,
            _ => 1
        };
    }

    #endregion

    #region Constructors

    public SpawnKingSlime() : base(EffectID.SpawnKingSlime, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Simply spawn if in single-player
            Spawn(player);
        }
        else
        {
            // Notify server (no need to send arguments)
            SendPacket(PacketID.HandleEffect);
        }

        player.Player.AddBuff(BuffID.ShadowDodge, 60 * 5);
        player.Player.AddBuff(BuffID.Slimed, 60 * 60);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.SlimeCrown, $"{viewerString} summoned a King Slime", Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Spawn on server
        Spawn(player);
    }

    #endregion
}