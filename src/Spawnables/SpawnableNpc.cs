using System;
using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Spawnables;

/// <summary>
///     Npc that can be spawned in the world.
/// </summary>
public sealed class SpawnableNpc : ISpawnable<NPC>
{
    #region Static Fields and Constants

    private static readonly Dictionary<short, SpawnableNpc> CachedNpcs = new();

    #endregion

    #region Static Methods

    /// <summary>
    ///     Get an existing spawnable npc instance, or create a new one.
    /// </summary>
    public static SpawnableNpc Get(short npcType)
    {
        // Check the cached npcs (might be cached between mod reloads)
        if (CachedNpcs.TryGetValue(npcType, out var spawnableNpc))
        {
            return spawnableNpc;
        }

        // Create a new instance and cache it
        ModNPC modNpc;
        if ((modNpc = ModContent.GetModNPC(npcType)) != null)
        {
            // Modded NPCs
            spawnableNpc = modNpc.Name switch
            {
                ModUtils.Calamity.NpcCalamitas => new SpawnableNpc(npcType, _ => !Main.dayTime),
                ModUtils.Calamity.NpcAstrumAureus => new SpawnableNpc(npcType, _ => !Main.dayTime),
                _ => new SpawnableNpc(npcType)
            };
        }
        else
        {
            // Vanilla NPCs
            spawnableNpc = npcType switch
            {
                NPCID.KingSlime => new SpawnableNpc(npcType, null, (_, npc) =>
                {
                    // Scale king slime life based on progression
                    npc.lifeMax = ProgressionUtils.GetProgression() switch
                    {
                        ProgressionUtils.Progression.PreEye => 1000,
                        ProgressionUtils.Progression.PreSkeletron => 1500,
                        ProgressionUtils.Progression.PreWall => 2000,
                        ProgressionUtils.Progression.PreMech => 2500,
                        ProgressionUtils.Progression.PreGolem => 3000,
                        ProgressionUtils.Progression.PreLunar => 3500,
                        ProgressionUtils.Progression.PreMoonLord => 4000,
                        ProgressionUtils.Progression.PostGame => 4500,
                        _ => 69
                    };
                    npc.life = npc.lifeMax;
                }),
                NPCID.EyeofCthulhu => new SpawnableNpc(NPCID.EyeofCthulhu, _ => !Main.dayTime),
                NPCID.EaterofWorldsHead => new SpawnableNpc(npcType, p => p.Player.ZoneCorrupt),
                NPCID.BrainofCthulhu => new SpawnableNpc(npcType, p => p.Player.ZoneCrimson),
                NPCID.SkeletronHead => new SpawnableNpc(npcType, _ => !Main.dayTime),
                NPCID.WallofFlesh => new SpawnableNpc(npcType, p => p.Player.ZoneUnderworldHeight && !Main.npc.Any(x => x.active && x.type == npcType)),
                NPCID.Retinazer => new SpawnableNpc(npcType, _ => !Main.dayTime, (p, npc) => Get(NPCID.Spazmatism).Spawn(p, npc.position)),
                NPCID.Spazmatism => new SpawnableNpc(npcType, _ => !Main.dayTime && Main.npc.Count(x => x.active && x.type == NPCID.Retinazer) % 2 != 0),
                NPCID.TheDestroyer => new SpawnableNpc(npcType, _ => !Main.dayTime),
                NPCID.SkeletronPrime => new SpawnableNpc(npcType, _ => !Main.dayTime),
                NPCID.MoonLordCore => new SpawnableNpc(npcType, _ => !Main.npc.Any(x => x.active && x.type == npcType)),
                NPCID.MourningWood => new SpawnableNpc(npcType, _ => !Main.dayTime),
                NPCID.Pumpking => new SpawnableNpc(npcType, _ => !Main.dayTime),
                NPCID.Everscream => new SpawnableNpc(npcType, _ => !Main.dayTime),
                NPCID.SantaNK1 => new SpawnableNpc(npcType, _ => !Main.dayTime),
                NPCID.IceQueen => new SpawnableNpc(npcType, _ => !Main.dayTime),
                NPCID.Mothron => new SpawnableNpc(npcType, _ => Main.dayTime),
                _ => new SpawnableNpc(npcType)
            };
        }
        
        CachedNpcs.Add(npcType, spawnableNpc);
        return spawnableNpc;
    }

    #endregion

    #region Fields

    /// <summary>
    ///     Npc type of the spawnable.
    /// </summary>
    public readonly short NpcType;

    private readonly Func<CrowdControlPlayer, bool>? _shouldSpawn;

    private readonly Action<CrowdControlPlayer, NPC>? _onSpawn;

    #endregion

    #region Constructors

    private SpawnableNpc(short npcType, Func<CrowdControlPlayer, bool>? shouldSpawn = null, Action<CrowdControlPlayer, NPC>? onSpawn = null)
    {
        NpcType = npcType;
        _shouldSpawn = shouldSpawn;
        _onSpawn = onSpawn;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Name of the npc.
    /// </summary>
    public string DisplayName => NpcType != NPCID.Retinazer ? Lang.GetNPCName(NpcType).Value : "Twins";

    #endregion

    #region Methods

    public bool CanSpawn(CrowdControlPlayer player)
    {
        return _shouldSpawn?.Invoke(player) ?? true;
    }

    public NPC Spawn(CrowdControlPlayer player, Vector2 position)
    {
        // Spawn the npc
        var index = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)position.X, (int)position.Y, NpcType, 0, 0, 0, 0, 0, player.Player.whoAmI);
        var npc = Main.npc[index];

        // Invoke the on spawn action
        _onSpawn?.Invoke(player, npc);

        if (Main.netMode == NetmodeID.Server)
        {
            // Notify the clients
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        }

        return npc;
    }

    #endregion
}