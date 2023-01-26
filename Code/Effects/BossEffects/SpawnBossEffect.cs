using System;
using System.Diagnostics.Contracts;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Features;
using CrowdControlMod.ID;
using CrowdControlMod.Spawnables;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.BossEffects;

/// <summary>
///     Spawn a boss on the player.
/// </summary>
public sealed class SpawnBossEffect : CrowdControlEffect
{
    #region Static Methods

    [Pure]
    private static string GetId(short npcType)
    {
        return npcType switch
        {
            NPCID.KingSlime => EffectID.SpawnKingSlime,
            NPCID.EyeofCthulhu => EffectID.SpawnEyeOfCthulhu,
            NPCID.EaterofWorldsHead => EffectID.SpawnEaterOfWorlds,
            NPCID.BrainofCthulhu => EffectID.SpawnBrainOfCthulhu,
            NPCID.QueenBee => EffectID.SpawnQueenBee,
            NPCID.SkeletronHead => EffectID.SpawnSkeletron,
            NPCID.Deerclops => EffectID.SpawnDeerclops,
            NPCID.WallofFlesh => EffectID.SpawnWallOfFlesh,
            NPCID.QueenSlimeBoss => EffectID.SpawnQueenSlime,
            NPCID.Retinazer => EffectID.SpawnTwins,
            NPCID.TheDestroyer => EffectID.SpawnDestroyer,
            NPCID.SkeletronPrime => EffectID.SpawnSkeletronPrime,
            NPCID.Plantera => EffectID.SpawnPlantera,
            NPCID.Golem => EffectID.SpawnGolem,
            NPCID.DukeFishron => EffectID.SpawnDukeFishron,
            NPCID.HallowBoss => EffectID.SpawnEmpressOfLight,
            NPCID.MoonLordCore => EffectID.SpawnMoonLord,
            _ => throw new ArgumentOutOfRangeException(nameof(npcType), npcType, null)
        };
    }

    #endregion

    #region Fields

    private readonly SpawnableNpc _spawnableNpc;

    #endregion

    #region Constructors

    public SpawnBossEffect(short bossType) : base(GetId(bossType), null, EffectSeverity.Negative)
    {
        _spawnableNpc = SpawnableNpc.Get(bossType);
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (!_spawnableNpc.CanSpawn(player))
        {
            // Unable to spawn the boss at this time
            return CrowdControlResponseStatus.Failure;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // Spawn the boss in single-player
            Spawn(player);
        }
        else
        {
            // Tell the server to spawn the boss
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var bossName = _spawnableNpc.DisplayName;
        TerrariaUtils.WriteEffectMessage(ItemID.TrueNightsEdge, LangUtils.GetEffectStartText(EffectID.RandomBoss, viewerString, playerString, durationString, bossName), Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Spawn the boss on the server
        Spawn(player);
    }

    private void Spawn(CrowdControlPlayer player)
    {
        // Spawn above the player and to the side
        var spawnPos = player.Player.Center + new Vector2(16 * 22 * (Main.rand.Next(100) > 50 ? 1 : -1), -16 * 13);
        var npc = _spawnableNpc.Spawn(player, spawnPos);
        CrowdControlMod.GetInstance().GetFeature<DespawnNPCFeature>(FeatureID.DespawnNPC)?.RegisterNPC(npc.whoAmI);
    }

    #endregion
}