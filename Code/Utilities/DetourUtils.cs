using System.Reflection;
using Terraria;
using Terraria.DataStructures;

namespace CrowdControlMod.Code.Utilities;

/// <summary>
///     Temporary detour solutions until 1.4.4 is properly updated.
///     TODO: Remove temporary detour solutions.
/// </summary>
public static class DetourUtils
{
    #region Static Fields and Constants

    public static readonly MethodBase NewProjectileMethod;

    #endregion

    #region Constructors

    static DetourUtils()
    {
        NewProjectileMethod = typeof(Projectile).GetMethod("NewProjectile", BindingFlags.Public | BindingFlags.Static, new[]
        {
            typeof(IEntitySource),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(int),
            typeof(int),
            typeof(float),
            typeof(int),
            typeof(float),
            typeof(float),
            typeof(float)
        })!;
    }

    #endregion

    #region Nested Types

    public delegate int NewProjectileDelegate(
        IEntitySource spawnSource,
        float x,
        float y,
        float speedX,
        float speedY,
        int type,
        int damage,
        float knockBack,
        int owner,
        float ai0,
        float ai1,
        float ai2);

    #endregion
}