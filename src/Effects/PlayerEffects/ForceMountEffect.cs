using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Force the player to ride a mount for a short duration.
/// </summary>
public sealed class ForceMountEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private static readonly int[] VanillaMounts =
    {
        BuffID.SlimeMount, BuffID.BeeMount, BuffID.TurtleMount, BuffID.BunnyMount, BuffID.PogoStickMount, BuffID.GolfCartMount,
        BuffID.Flamingo, BuffID.DarkHorseMount, BuffID.MajesticHorseMount, BuffID.PaintedHorseMount, BuffID.LavaSharkMount,
        BuffID.BasiliskMount, BuffID.UnicornMount, BuffID.PigronMount, BuffID.QueenSlimeMount, BuffID.Rudolph, BuffID.ScutlixMount,
        BuffID.UFOMount, BuffID.WitchBroom, BuffID.CuteFishronMount, BuffID.DrillMount, BuffID.DarkMageBookMount, BuffID.WallOfFleshGoatMount,
        BuffID.PirateShipMount, BuffID.SpookyWoodMount, BuffID.SantankMount, BuffID.MinecartRightMech, BuffID.MeowmereMinecartRight
    };

    private static readonly string[] CalamityMounts =
    {
        "OnyxExcavatorBuff", "RimehoundBuff", "BrimroseMount", "BumbledogeMount", "SquishyBeanBuff", "AlicornBuff",
        "GazeOfCrysthamyrBuff", "DraedonGamerChairBuff"
    };

    #endregion

    #region Fields

    private readonly IReadOnlyList<int> _allMountOptions;
    private int _chosenMount;

    #endregion

    #region Constructors

    public ForceMountEffect(float duration) : base(EffectID.ForceMount, duration, EffectSeverity.Neutral)
    {
        var allMountOptions = VanillaMounts.ToList();
        if (ModLoader.TryGetMod(ModID.Calamity, out var calamity))
        {
            // Add calamity mounts
            foreach (var calamityPetName in CalamityMounts)
            {
                if(calamity.TryFind<ModBuff>(calamityPetName, out var calamityPetBuff))
                {
                    allMountOptions.Add(calamityPetBuff.Type);
                }
            }
        }

        _allMountOptions = allMountOptions;
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!_allMountOptions.Any())
        {
            return CrowdControlResponseStatus.Failure;
        }
        
        var player = GetLocalPlayer();

        // Choose random mount (that isn't already enabled)
        do
        {
            _chosenMount = Main.rand.Next((List<int>)_allMountOptions);
        } while (player.Player.HasBuff(_chosenMount));

        player.PreUpdateBuffsHook += PreUpdateBuffs;
        player.OnRespawnHook += OnRespawn;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.Player.ClearBuff(_chosenMount);
        player.PreUpdateBuffsHook -= PreUpdateBuffs;
        player.OnRespawnHook -= OnRespawn;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var mountName = Lang.GetBuffName(_chosenMount);
        TerrariaUtils.WriteEffectMessage(ItemID.MajesticHorseSaddle, $"{viewerString} is forcing {playerString} to ride a {mountName}", Severity);
    }

    private void PreUpdateBuffs()
    {
        var player = GetLocalPlayer();
        if (player.Player.HasBuff(_chosenMount))
        {
            return;
        }

        if (!player.Player.hideMisc[3])
        {
            // Remove existing mount buff
            player.Player.ClearBuff(player.Player.miscEquips[3].buffType);
        }

        // Hide existing buff
        player.Player.hideMisc[3] = true;

        // Add the mount buff
        player.Player.AddBuff(_chosenMount, 1);
    }

    private void OnRespawn()
    {
        if (_chosenMount == 0)
        {
            return;
        }

        var player = GetLocalPlayer();
        if (!player.Player.hideMisc[3])
        {
            // Disable the effect mount if the player has their normal mount enabled
            _chosenMount = 0;
            return;
        }

        // Re-apply mount upon respawning
        player.Player.AddBuff(_chosenMount, 1);
    }

    #endregion
}