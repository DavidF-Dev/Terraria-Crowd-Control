using System;
using JetBrains.Annotations;
using Terraria;

namespace CrowdControlMod.Utilities;

public static class ProgressionUtils
{
    #region Enums

    [PublicAPI]
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
    [PublicAPI] [Pure]
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
    [PublicAPI] [CanBeNull] [Pure]
    public static T ChooseAtProgression<T>(Progression progression, T preEye, T preSkeletron, T preWall, T preMech, T preGolem, T preLunar, T preMoonLord, T postGame)
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
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    ///     Choose the appropriate method argument based on the current progression of the world.
    /// </summary>
    [PublicAPI] [CanBeNull] [Pure]
    public static T ChooseAtProgression<T>(T preEye, T preSkeletron, T preWall, T preMech, T preGolem, T preLunar, T preMoonLord, T postGame)
    {
        return ChooseAtProgression(GetProgression(), preEye, preSkeletron, preWall, preMech, preGolem, preLunar, preMoonLord, postGame);
    }

    #endregion
}