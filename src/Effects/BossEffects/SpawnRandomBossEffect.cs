using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
///     Spawn a random boss based on the world progression.
/// </summary>
public sealed class SpawnRandomBossEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    [NotNull]
    private static readonly Dictionary<ProgressionUtils.Progression, short[]> TypesByProgression = new()
    {
        {
            ProgressionUtils.Progression.PreEye, new[]
            {
                NPCID.KingSlime, NPCID.EyeofCthulhu, NPCID.QueenBee
            }
        },
        {
            ProgressionUtils.Progression.PreSkeletron, new[]
            {
                NPCID.KingSlime, NPCID.EyeofCthulhu, NPCID.QueenBee, NPCID.EaterofWorldsHead, NPCID.BrainofCthulhu, NPCID.Deerclops
            }
        },
        {
            ProgressionUtils.Progression.PreWall, new[]
            {
                NPCID.KingSlime, NPCID.EyeofCthulhu, NPCID.QueenBee, NPCID.EaterofWorldsHead, NPCID.BrainofCthulhu, NPCID.Deerclops,
                NPCID.SkeletronHead, NPCID.WallofFlesh
            }
        },
        {
            ProgressionUtils.Progression.PreMech, new[]
            {
                NPCID.KingSlime, NPCID.SkeletronHead, NPCID.Deerclops, NPCID.Retinazer, NPCID.TheDestroyer, NPCID.SkeletronPrime,

                NPCID.WyvernHead, NPCID.PirateCaptain, NPCID.PirateShip, NPCID.GoblinSummoner,
                NPCID.BigMimicCorruption, NPCID.BigMimicCrimson, NPCID.BigMimicHallow, NPCID.BigMimicJungle
            }
        },
        {
            ProgressionUtils.Progression.PreGolem, new[]
            {
                NPCID.KingSlime, NPCID.Retinazer, NPCID.TheDestroyer, NPCID.SkeletronPrime, NPCID.QueenSlimeBoss, NPCID.Plantera, NPCID.Golem, NPCID.DukeFishron,

                NPCID.BigMimicCorruption, NPCID.BigMimicCrimson, NPCID.BigMimicHallow, NPCID.BigMimicJungle,
                NPCID.MourningWood, NPCID.Pumpking, NPCID.Everscream, NPCID.SantaNK1, NPCID.IceQueen
            }
        },
        {
            ProgressionUtils.Progression.PreLunar, new[]
            {
                NPCID.KingSlime, NPCID.Retinazer, NPCID.TheDestroyer, NPCID.SkeletronPrime, NPCID.QueenSlimeBoss, NPCID.HallowBoss, NPCID.Plantera, NPCID.Golem,
                NPCID.DukeFishron,

                NPCID.CultistDragonHead, NPCID.Mothron,
                NPCID.MourningWood, NPCID.Pumpking, NPCID.Everscream, NPCID.SantaNK1, NPCID.IceQueen, NPCID.MartianSaucerCore
            }
        },
        {
            ProgressionUtils.Progression.PreMoonLord, new[]
            {
                NPCID.KingSlime, NPCID.Retinazer, NPCID.TheDestroyer, NPCID.SkeletronPrime, NPCID.QueenSlimeBoss, NPCID.HallowBoss, NPCID.Plantera, NPCID.Golem,
                NPCID.DukeFishron, NPCID.MoonLordCore,

                NPCID.MourningWood, NPCID.Pumpking, NPCID.Everscream, NPCID.SantaNK1, NPCID.IceQueen, NPCID.MartianSaucerCore, NPCID.DD2Betsy
            }
        },
        {
            ProgressionUtils.Progression.PostGame, new[]
            {
                NPCID.KingSlime, NPCID.Retinazer, NPCID.TheDestroyer, NPCID.SkeletronPrime, NPCID.QueenSlimeBoss, NPCID.HallowBoss, NPCID.Plantera, NPCID.Golem,
                NPCID.DukeFishron, NPCID.MoonLordCore, NPCID.LunarTowerStardust, NPCID.LunarTowerNebula, NPCID.LunarTowerSolar, NPCID.LunarTowerVortex,

                NPCID.MourningWood, NPCID.Pumpking, NPCID.Everscream, NPCID.SantaNK1, NPCID.IceQueen, NPCID.MartianSaucerCore, NPCID.DD2Betsy
            }
        }
    };

    #endregion

    #region Static Methods

    private static void Spawn([NotNull] CrowdControlPlayer player, short npcType)
    {
        // Spawn above the player and to the side
        var boss = SpawnableNpc.Get(npcType);
        var spawnPos = player.Player.Center + new Vector2(16 * 22 * (Main.rand.Next(100) > 50 ? 1 : -1), -16 * 13);
        boss.Spawn(player, spawnPos);
    }

    #endregion

    #region Fields

    private SpawnableNpc _chosenSpawnableNpc;

    #endregion

    #region Constructors

    public SpawnRandomBossEffect() : base(EffectID.RandomBoss, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        var candidates = TypesByProgression.GetValueOrDefault(ProgressionUtils.GetProgression(), Array.Empty<short>())
            .Select(SpawnableNpc.Get)
            .Where(x => x.CanSpawn(player))
            .ToArray();
        if (!candidates.Any())
        {
            // No candidates
            return CrowdControlResponseStatus.Failure;
        }

        // Choose a random candidate to spawn
        _chosenSpawnableNpc = candidates[Main.rand.Next(candidates.Length)];

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Spawn the boss in single-player
            Spawn(player, _chosenSpawnableNpc.NpcType);
        }
        else
        {
            // Notify the server to spawn the boss
            SendPacket(PacketID.HandleEffect, _chosenSpawnableNpc.NpcType);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _chosenSpawnableNpc = null;
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Incoming packet: (short)bossType
        Spawn(player, reader.ReadInt16());
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.TrueNightsEdge, $"{viewerString} summoned {_chosenSpawnableNpc.DisplayName} on {playerString}", Severity);
    }

    #endregion
}