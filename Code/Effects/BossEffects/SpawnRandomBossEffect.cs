using System.Collections.Generic;
using System.IO;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Features;
using CrowdControlMod.ID;
using CrowdControlMod.Spawnables;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.BossEffects;

/// <summary>
///     Spawn a random boss based on the world progression.
/// </summary>
public sealed class SpawnRandomBossEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private static readonly Dictionary<ProgressionUtils.Progression, short[]> VanillaTypesByProgression = new()
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

    private static readonly Dictionary<ProgressionUtils.Progression, string[]> CalamityTypesByProgression = new()
    {
        {
            ProgressionUtils.Progression.PreEye, new[]
            {
                ModUtils.Calamity.DesertScourgeNPC, ModUtils.Calamity.CrabulonNPC, ModUtils.Calamity.GiantClamNPC
            }
        },
        {
            ProgressionUtils.Progression.PreSkeletron, new[]
            {
                ModUtils.Calamity.DesertScourgeNPC, ModUtils.Calamity.CrabulonNPC, ModUtils.Calamity.GiantClamNPC,
                ModUtils.Calamity.TheHiveMindNPC, ModUtils.Calamity.ThePerforatorsNPC
            }
        },
        {
            ProgressionUtils.Progression.PreWall, new[]
            {
                ModUtils.Calamity.TheHiveMindNPC, ModUtils.Calamity.ThePerforatorsNPC,
                ModUtils.Calamity.TheSlimeGodNPC
            }
        },
        {
            ProgressionUtils.Progression.PreMech, new[]
            {
                ModUtils.Calamity.GiantClamNPC,
                ModUtils.Calamity.CryogenNPC, ModUtils.Calamity.AquaticScourgeNPC, ModUtils.Calamity.BrimstoneElementalNPC,
                ModUtils.Calamity.EarthElementalNPC, ModUtils.Calamity.CloudElementalNPC
            }
        },
        {
            ProgressionUtils.Progression.PreGolem, new[]
            {
                ModUtils.Calamity.CryogenNPC, ModUtils.Calamity.AquaticScourgeNPC, ModUtils.Calamity.BrimstoneElementalNPC,
                ModUtils.Calamity.EarthElementalNPC,
                ModUtils.Calamity.CalamitasNPC, ModUtils.Calamity.AstrumAureusNPC,
                ModUtils.Calamity.GreatSandSharkNPC
            }
        },
        {
            ProgressionUtils.Progression.PreLunar, new[]
            {
                ModUtils.Calamity.CalamitasNPC, ModUtils.Calamity.AstrumAureusNPC,
                ModUtils.Calamity.ThePlaguebringerGoliathNPC, ModUtils.Calamity.RavagerNPC,
                ModUtils.Calamity.CragmawMireNPC
            }
        },
        {
            ProgressionUtils.Progression.PreMoonLord, new[]
            {
                ModUtils.Calamity.ThePlaguebringerGoliathNPC, ModUtils.Calamity.RavagerNPC,
                ModUtils.Calamity.AstrumDeusNPC
            }
        },
        {
            ProgressionUtils.Progression.PostGame, new[]
            {
                ModUtils.Calamity.NuclearTerrorNPC
            }
        }
    };

    #endregion

    #region Static Methods

    private static void Spawn(CrowdControlPlayer player, short npcType)
    {
        // Spawn above the player and to the side
        var boss = SpawnableNpc.Get(npcType);
        var spawnPos = player.Player.Center + new Vector2(16 * 22 * (Main.rand.Next(100) > 50 ? 1 : -1), -16 * 13);
        var npc = boss.Spawn(player, spawnPos);
        CrowdControlMod.GetInstance().GetFeature<DespawnNPCFeature>(FeatureID.DespawnNPC)?.RegisterNPC(npc.whoAmI);
    }

    #endregion

    #region Fields

    private readonly Dictionary<ProgressionUtils.Progression, List<short>> _allTypesByProgression = new();
    private SpawnableNpc? _chosenSpawnableNpc;

    #endregion

    #region Constructors

    public SpawnRandomBossEffect() : base(EffectID.RandomBoss, 0, EffectSeverity.Negative)
    {
        // Add vanilla types
        foreach (var (progress, types) in VanillaTypesByProgression)
        {
            _allTypesByProgression.Add(progress, types.ToList());
        }

        if (!ModUtils.TryGetMod(ModUtils.Calamity.Name, out var calamity))
        {
            // No calamity mod
            return;
        }

        // Add calamity types
        foreach (var (progression, calamityNpcNames) in CalamityTypesByProgression)
        {
            ModUtils.IterateTypes<ModNPC>(calamity, calamityNpcNames, x =>
            {
                if (!_allTypesByProgression.ContainsKey(progression))
                {
                    // Add a dictionary entry if one doesn't exist (unlikely)
                    _allTypesByProgression.Add(progression, new List<short>());
                }

                // Add the calamity boss to the dictionary
                _allTypesByProgression[progression].Add((short)x.Type);
            });
        }
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Boss;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        var candidates = _allTypesByProgression.GetValueOrDefault(ProgressionUtils.GetProgression(), new List<short>())
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

        if (NetUtils.IsSinglePlayer)
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

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var bossName = _chosenSpawnableNpc?.DisplayName ?? string.Empty;
        TerrariaUtils.WriteEffectMessage(ItemID.TrueNightsEdge, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, bossName), Severity);
    }

    #endregion
}