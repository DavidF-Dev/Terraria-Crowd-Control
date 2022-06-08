using System.Collections.Generic;
using JetBrains.Annotations;

namespace CrowdControlMod.Effects;

public sealed class BuffEffect : CrowdControlEffect
{
    #region Fields

    [NotNull]
    private readonly HashSet<int> _buffs;

    #endregion

    #region Constructors

    public BuffEffect([NotNull] string id, float? duration, [NotNull] params int[] buffs) : base(id)
    {
        Duration = duration;
        _buffs = new HashSet<int>(buffs);
    }

    #endregion

    #region Methods

    protected override void OnStart()
    {
        CrowdControlMod.GetInstance().GetPlayer().PreUpdateBuffsHook += PreUpdateBuffs;
        base.OnStart();
    }

    protected override void OnStop()
    {
        CrowdControlMod.GetInstance().GetPlayer().PreUpdateBuffsHook -= PreUpdateBuffs;
        base.OnStop();
    }

    private void PreUpdateBuffs([NotNull] CrowdControlPlayer player)
    {
        // TODO: Ensure the buffs are applied (1 second?) (what if the max buff limit is reached?)
    }

    #endregion
}