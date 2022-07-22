namespace CrowdControlMod.Effects.Interfaces;

/// <summary>
///     Use for effects the explicitly require particular mods to be loaded and active.
/// </summary>
public interface IModEffect
{
    /// <summary>
    ///     Name of the mod that must be loaded and active for this effect to function.
    /// </summary>
    public string ModName { get; }
}