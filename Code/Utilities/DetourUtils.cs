using System.Reflection;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
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

    public static readonly MethodBase PlaySoundMethod;
    
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

        PlaySoundMethod = typeof(SoundPlayer).GetMethod("Play", BindingFlags.Public | BindingFlags.Instance, new[]
        {
            typeof(SoundStyle).MakeByRefType(),
            typeof(Vector2?),
            typeof(SoundUpdateCallback)
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

    public delegate SlotId PlaySoundDelegate(SoundPlayer self, ref SoundStyle soundType, Vector2? position, SoundUpdateCallback callback);

    #endregion
}