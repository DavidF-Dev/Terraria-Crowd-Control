using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Terraria;

namespace CrowdControlMod.Utilities;

public static class ProgressionUtils
{
    #region Enums

    public enum Progression
    {
        PreEye,
        PreSkeletron,
        PreWall,
        PreMech,
        PreGolem,
        PreLunar,
        PreMoonLord,
        PostGame
    }

    #endregion

    #region Static Methods

    /// <summary>
    ///     Get the current progression of the world.
    /// </summary>
    [Pure]
    public static Progression GetProgression()
    {
        if (NPC.downedMoonlord)
        {
            return Progression.PostGame;
        }

        if (NPC.downedAncientCultist)
        {
            return Progression.PreMoonLord;
        }

        if (NPC.downedGolemBoss)
        {
            return Progression.PreLunar;
        }

        if (NPC.downedMechBossAny)
        {
            return Progression.PreGolem;
        }

        if (Main.hardMode)
        {
            return Progression.PreMech;
        }

        if (NPC.downedBoss3)
        {
            return Progression.PreWall;
        }

        if (NPC.downedBoss1)
        {
            return Progression.PreSkeletron;
        }

        return Progression.PreEye;
    }

    /// <summary>
    ///     Choose the appropriate method argument based on the provided progression.
    /// </summary>
    [Pure]
    public static T? ChooseAtProgression<T>(Progression progression, T? preEye, T? preSkeletron, T? preWall, T? preMech, T? preGolem, T? preLunar, T? preMoonLord, T? postGame)
    {
        return progression switch
        {
            Progression.PreEye => preEye,
            Progression.PreSkeletron => preSkeletron,
            Progression.PreWall => preWall,
            Progression.PreMech => preMech,
            Progression.PreGolem => preGolem,
            Progression.PreLunar => preLunar,
            Progression.PreMoonLord => preMoonLord,
            Progression.PostGame => postGame,
            _ => throw new ArgumentOutOfRangeException(nameof(progression))
        };
    }

    /// <summary>
    ///     Choose the appropriate method argument based on the current progression of the world.
    /// </summary>
    [Pure]
    public static T? ChooseAtProgression<T>(T? preEye, T? preSkeletron, T? preWall, T? preMech, T? preGolem, T? preLunar, T? preMoonLord, T? postGame)
    {
        return ChooseAtProgression(GetProgression(), preEye, preSkeletron, preWall, preMech, preGolem, preLunar, preMoonLord, postGame);
    }

    /// <summary>
    ///     Choose the appropriate method arguments based on the current progression of the world.
    /// </summary>
    [Pure]
    public static IReadOnlyList<T> ChooseUpToProgression<T>(Progression progression, T? preEye, T? preSkeletron, T? preWall, T? preMech, T? preGolem, T? preLunar, T? preMoonLord, T? postGame)
    {
        List<T> list = new() {preEye!};

        if ((int)progression >= (int)Progression.PreSkeletron)
        {
            list.Add(preSkeletron!);
        }

        if ((int)progression >= (int)Progression.PreWall)
        {
            list.Add(preWall!);
        }

        if ((int)progression >= (int)Progression.PreMech)
        {
            list.Add(preMech!);
        }

        if ((int)progression >= (int)Progression.PreGolem)
        {
            list.Add(preGolem!);
        }

        if ((int)progression >= (int)Progression.PreLunar)
        {
            list.Add(preLunar!);
        }

        if ((int)progression >= (int)Progression.PreMoonLord)
        {
            list.Add(preMoonLord!);
        }

        if (progression == Progression.PostGame)
        {
            list.Add(postGame!);
        }

        return list;
    }

    /// <summary>
    ///     Choose the appropriate method arguments based on the provided progression.
    /// </summary>
    [Pure]
    public static IReadOnlyList<T> ChooseUpToProgression<T>(T? preEye, T? preSkeletron, T? preWall, T? preMech, T? preGolem, T? preLunar, T? preMoonLord, T? postGame)
    {
        return ChooseUpToProgression(GetProgression(), preEye, preSkeletron, preWall, preMech, preGolem, preLunar, preMoonLord, postGame);
    }

    #endregion
}