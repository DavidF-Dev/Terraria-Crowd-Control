namespace CrowdControlMod.Effects.Interfaces;

/// <summary>
///     Use for effects that explicitly require particular mods to be loaded and active.
/// </summary>
public interface IModEffect
{
    #region Properties

    /// <summary>
    ///     Name of the mod that must be loaded and active for this effect to function.
    /// </summary>
    public string ModName { get; }

    #endregion
}