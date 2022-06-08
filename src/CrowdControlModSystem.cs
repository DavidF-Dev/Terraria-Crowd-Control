using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlModSystem : ModSystem
{
    #region Methods

    public override void PreSaveAndQuit()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            // Stop the crowd control session upon exiting a world
            CrowdControlMod.GetInstance().StopCrowdControlSession();
        }
        
        base.PreSaveAndQuit();
    }

    #endregion
}