﻿using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlSceneEffect : ModSceneEffect
{
    #region Fields

    private int _musicId;

    #endregion

    #region Properties

    public override int Music => _musicId;

    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    #endregion

    #region Methods

    public override bool IsSceneEffectActive(Player player)
    {
        // Check if any of the effects want to play music
        return TerrariaUtils.IsLocalPlayer(player) && CrowdControlMod.GetInstance().TryGetEffectMusic(out _musicId) && _musicId > 0;
    }

    #endregion
}