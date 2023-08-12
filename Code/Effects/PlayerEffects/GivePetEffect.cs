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
///     Provide the player with a pet or light pet.
/// </summary>
public sealed class GivePetEffect : CrowdControlEffect
{
    #region Enums

    public enum PetType
    {
        Pet,
        LightPet
    }

    #endregion

    #region Static Fields and Constants

    private static readonly int[] VanillaPets =
    {
        BuffID.BabyDinosaur, BuffID.BabyEater, BuffID.BabyFaceMonster, BuffID.BabyGrinch, BuffID.BabyHornet, BuffID.BabyImp, BuffID.BabyPenguin, BuffID.BabyRedPanda,
        BuffID.BabySkeletronHead, BuffID.BabySnowman, BuffID.BabyTruffle, BuffID.BabyWerewolf, BuffID.BerniePet, BuffID.BlackCat, /*BuffID.PetBunny,*/ BuffID.ChesterPet,
        BuffID.CompanionCube, BuffID.CursedSapling, BuffID.DynamiteKitten, BuffID.UpbeatStar, BuffID.EyeballSpring, BuffID.FennecFox, BuffID.GlitteryButterfly,
        BuffID.GlommerPet, BuffID.PetDD2Dragon, BuffID.LilHarpy, BuffID.PetLizard, BuffID.MiniMinotaur, BuffID.PetParrot, BuffID.PigPet, BuffID.Plantero, BuffID.PetDD2Gato,
        BuffID.Puppy, BuffID.PetSapling, BuffID.PetSpider, BuffID.ShadowMimic, BuffID.SharkPup, BuffID.Squashling, BuffID.SugarGlider, BuffID.TikiSpirit,
        BuffID.PetTurtle, BuffID.VoltBunny, BuffID.ZephyrFish, BuffID.MartianPet, BuffID.DD2OgrePet, BuffID.DestroyerPet, BuffID.EaterOfWorldsPet, BuffID.EverscreamPet,
        BuffID.QueenBeePet, BuffID.IceQueenPet, BuffID.DD2BetsyPet, BuffID.SkeletronPrimePet, BuffID.MoonLordPet, BuffID.LunaticCultistPet, BuffID.PlanteraPet,
        BuffID.TwinsPet, BuffID.SkeletronPet, BuffID.KingSlimePet, BuffID.QueenSlimePet, BuffID.BrainOfCthulhuPet, BuffID.EyeOfCthulhuPet, BuffID.DeerclopsPet,
        BuffID.DukeFishronPet,
        BuffID.JunimoPet, BuffID.BlueChickenPet, BuffID.Spiffo, BuffID.CavelingGardener, BuffID.DirtiestBlock, BuffID.DualSlimePet
    };

    private static readonly string[] CalamityPets =
    {
        "AkatoBuff", "AstrophageBuff", "BearBuff", "BrimlingBuff", "ChibiiDoGBuff", "DannyDevito", "ElectricTroublemaker", "FlakHermitBuff",
        "FoxPetBuff", "FurtasticDuoBuff", "Kendra", "LadBuff", "LeviBuff", "PineappleBuff", "PlaguebringerBabBuff", "YharonSonBuff",
        "ClassicSCalPetBuff", "ThirdSageBuff", "BloodBound", "MiniMindBuff"
    };

    private static readonly int[] VanillaLightPets =
    {
        BuffID.ShadowOrb, BuffID.CrimsonHeart, BuffID.MagicLantern, BuffID.FairyBlue, BuffID.FairyGreen, BuffID.FairyRed, BuffID.PetDD2Ghost, BuffID.Wisp,
        BuffID.SuspiciousTentacle, BuffID.PumpkingPet, BuffID.GolemPet, BuffID.FairyQueenPet
    };

    private static readonly string[] CalamityLightPets =
    {
        "Dreamfog", "SparksBuff", "LittleLightBuff", "RadiatorBuff", "BabyGhostBellBuff", "OceanSpiritBuff"
    };

    #endregion

    #region Fields

    private readonly int _slot;
    private readonly IReadOnlyList<int> _allPetOptions;
    private List<int> _petOptions;
    private int _chosenId;

    #endregion

    #region Constructors

    public GivePetEffect(PetType petType) : base(petType == PetType.Pet ? EffectID.GivePet : EffectID.GiveLightPet, 0, EffectSeverity.Neutral)
    {
        _slot = petType == PetType.Pet ? 0 : 1;

        // Create a list of ALL the pets
        var allPetOptions = (petType == PetType.Pet ? VanillaPets : VanillaLightPets).ToList();
        if (ModUtils.TryGetMod(ModUtils.Calamity.Name, out var calamity))
        {
            // Add calamity pets
            ModUtils.IterateTypes<ModBuff>(calamity, petType == PetType.Pet ? CalamityPets : CalamityLightPets,
                x => allPetOptions.Add(x.Type));
        }

        _allPetOptions = allPetOptions;
        _petOptions = _allPetOptions.ToList();
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    #endregion

    #region Methods

    protected override void OnSessionStarted()
    {
        GetLocalPlayer().OnRespawnHook += OnRespawn;
    }

    protected override void OnSessionStopped()
    {
        GetLocalPlayer().OnRespawnHook -= OnRespawn;
    }

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!_petOptions.Any())
        {
            return CrowdControlResponseStatus.Failure;
        }

        var player = GetLocalPlayer();
        HideExistingPet(player.Player);

        // Choose a new pet id
        _chosenId = _petOptions[Main.rand.Next(_petOptions.Count)];

        // If there are no more options, regenerate the collection
        if (_petOptions.Count == 1)
        {
            _petOptions = _allPetOptions.ToList();
        }

        _petOptions.Remove(_chosenId);
        player.Player.AddBuff(_chosenId, 1);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(
            ItemID.DogWhistle,
            LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, Lang.GetBuffName(_chosenId)),
            Severity);
    }

    private void OnRespawn()
    {
        if (_chosenId == 0)
        {
            return;
        }

        var player = GetLocalPlayer();
        if (!player.Player.hideMisc[_slot])
        {
            // Disable the effect pet if the player has their normal pet enabled
            _chosenId = 0;
            return;
        }

        // Re-apply the pet upon respawning
        player.Player.AddBuff(_chosenId, 1);
    }

    private void HideExistingPet(Player player)
    {
        if (!player.hideMisc[_slot])
        {
            // Remove the existing pet buff
            player.ClearBuff(player.miscEquips[_slot].buffType);
        }

        // Hide any existing pet
        player.hideMisc[_slot] = true;
    }

    #endregion
}