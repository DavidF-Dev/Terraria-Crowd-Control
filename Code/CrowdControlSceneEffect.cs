using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod;

// ReSharper disable once UnusedType.Global
public sealed class CrowdControlSceneEffect : ModSceneEffect
{
    #region Fields

    private int _musicId;

    #endregion

    #region Properties

    public override int Music => _musicId;

    public override SceneEffectPriority Priority => (SceneEffectPriority)int.MaxValue;

    #endregion

    #region Methods

    public override bool IsSceneEffectActive(Player player)
    {
        // Check if any of the effects want to play music
        return player.whoAmI == Main.myPlayer && CrowdControlMod.GetInstance().TryGetEffectMusic(out _musicId) && _musicId > 0;
    }

    #endregion
}