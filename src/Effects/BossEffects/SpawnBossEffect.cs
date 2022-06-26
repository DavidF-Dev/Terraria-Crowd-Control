﻿using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Spawnables;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
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

    [NotNull] [Pure]
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

    [NotNull]
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

        if (Main.netMode == NetmodeID.SinglePlayer)
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

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.TrueNightsEdge, $"{viewerString} summoned {_spawnableNpc.DisplayName} on {playerString}", Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Spawn the boss on the server
        Spawn(player);
    }

    private void Spawn([NotNull] CrowdControlPlayer player)
    {
        // Spawn above the player and to the side
        var spawnPos = player.Player.Center + new Vector2(16 * 22 * (Main.rand.Next(100) > 50 ? 1 : -1), -16 * 13);
        _spawnableNpc.Spawn(player, spawnPos);
    }

    #endregion
}