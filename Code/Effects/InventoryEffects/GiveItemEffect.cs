using System;
using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Spawns in an item at the player's position, chosen from a set depending on the world progression.
/// </summary>
public sealed class GiveItemEffect : CrowdControlEffect
{
    #region Enums

    public enum GiveItem
    {
        Pickaxe,
        Sword,
        Yoyo,
        Magic,
        Summon,
        Ranged,
        Armour,
        HealingPotion,
        Potion,
        Kite,
        Food
    }

    #endregion

    #region Static Fields and Constants

    private static readonly Dictionary<GiveItem, Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>> VanillaItems = new()
    {
        {
            GiveItem.Pickaxe, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new[] {ItemID.CopperPickaxe, ItemID.TinPickaxe, ItemID.IronPickaxe, ItemID.LeadPickaxe, ItemID.SilverPickaxe, ItemID.TungstenPickaxe, ItemID.CactusPickaxe, ItemID.PlatinumPickaxe}},
                {ProgressionUtils.Progression.PreSkeletron, new[] {ItemID.GoldPickaxe, ItemID.CnadyCanePickaxe, ItemID.BonePickaxe}},
                {ProgressionUtils.Progression.PreWall, new[] {ItemID.NightmarePickaxe, ItemID.DeathbringerPickaxe}},
                {ProgressionUtils.Progression.PreMech, new[] {ItemID.MoltenPickaxe, ItemID.CobaltPickaxe, ItemID.PalladiumPickaxe, ItemID.MythrilPickaxe, ItemID.OrichalcumPickaxe}},
                {ProgressionUtils.Progression.PreGolem, new[] {ItemID.AdamantitePickaxe, ItemID.TitaniumPickaxe, ItemID.SpectrePickaxe, ItemID.ChlorophytePickaxe, ItemID.PickaxeAxe, ItemID.ShroomiteDiggingClaw}},
                {ProgressionUtils.Progression.PreLunar, new[] {ItemID.Picksaw, ItemID.ShroomiteDiggingClaw, ItemID.PickaxeAxe}},
                {ProgressionUtils.Progression.PreMoonLord, new[] {ItemID.Picksaw, ItemID.ShroomiteDiggingClaw}},
                {ProgressionUtils.Progression.PostGame, new[] {ItemID.VortexPickaxe, ItemID.NebulaPickaxe, ItemID.SolarFlarePickaxe, ItemID.StardustPickaxe}}
            }
        },
        {
            GiveItem.Sword, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new[] {ItemID.BladedGlove, ItemID.IronBroadsword, ItemID.LeadBroadsword, ItemID.TungstenBroadsword, ItemID.ZombieArm, ItemID.AntlionClaw, ItemID.CactusSword}},
                {ProgressionUtils.Progression.PreSkeletron, new[] {ItemID.GoldBroadsword, ItemID.PlatinumBroadsword, ItemID.BoneSword, ItemID.CandyCaneSword, ItemID.Katana, ItemID.IceBlade, ItemID.LightsBane, ItemID.BloodButcherer}},
                {ProgressionUtils.Progression.PreWall, new[] {ItemID.Muramasa, ItemID.DyeTradersScimitar, ItemID.Starfury, ItemID.BeeKeeper, ItemID.BladeofGrass, ItemID.FieryGreatsword, ItemID.NightsEdge}},
                {ProgressionUtils.Progression.PreMech, new[] {ItemID.SlapHand, ItemID.CobaltSword, ItemID.PalladiumSword, ItemID.OrichalcumSword, ItemID.MythrilSword, ItemID.DD2SquireDemonSword, ItemID.IceSickle, ItemID.BreakerBlade, ItemID.Cutlass, ItemID.AdamantiteSword, ItemID.TitaniumSword, ItemID.Frostbrand, ItemID.BeamSword, ItemID.FetidBaghnakhs, ItemID.Bladetongue}},
                {ProgressionUtils.Progression.PreGolem, new[] {ItemID.Excalibur, ItemID.ChlorophyteSaber, ItemID.TrueExcalibur, ItemID.DeathSickle, ItemID.PsychoKnife, ItemID.Keybrand, ItemID.ChlorophyteClaymore, ItemID.TheHorsemansBlade, ItemID.ChristmasTreeSword, ItemID.TrueNightsEdge, ItemID.Seedler}},
                {ProgressionUtils.Progression.PreLunar, new[] {ItemID.Seedler, ItemID.DD2SquireBetsySword, ItemID.TerraBlade, ItemID.InfluxWaver}},
                {ProgressionUtils.Progression.PreMoonLord, new[] {ItemID.DD2SquireBetsySword}},
                {ProgressionUtils.Progression.PostGame, new[] {ItemID.StarWrath, ItemID.Meowmere}}
            }
        },
        {
            GiveItem.Yoyo, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new[] {ItemID.WoodYoyo, ItemID.Rally, ItemID.CorruptYoyo, ItemID.CrimsonYoyo, ItemID.JungleYoyo}},
                {ProgressionUtils.Progression.PreSkeletron, new[] {ItemID.CorruptYoyo, ItemID.CrimsonYoyo, ItemID.JungleYoyo, ItemID.Code1, ItemID.HiveFive}},
                {ProgressionUtils.Progression.PreWall, new[] {ItemID.Code1, ItemID.HiveFive, ItemID.Cascade, ItemID.Valor}},
                {ProgressionUtils.Progression.PreMech, new[] {ItemID.Chik, ItemID.FormatC, ItemID.HelFire, ItemID.Amarok}},
                {ProgressionUtils.Progression.PreGolem, new[] {ItemID.HelFire, ItemID.Amarok, ItemID.Gradient, ItemID.Code2, ItemID.Yelets, ItemID.ValkyrieYoyo, ItemID.RedsYoyo}},
                {ProgressionUtils.Progression.PreLunar, new[] {ItemID.Gradient, ItemID.Code2, ItemID.Yelets, ItemID.ValkyrieYoyo, ItemID.RedsYoyo, ItemID.Kraken, ItemID.TheEyeOfCthulhu}},
                {ProgressionUtils.Progression.PreMoonLord, new[] {ItemID.ValkyrieYoyo, ItemID.RedsYoyo, ItemID.Kraken, ItemID.TheEyeOfCthulhu, ItemID.Terrarian}},
                {ProgressionUtils.Progression.PostGame, new[] {ItemID.TheEyeOfCthulhu, ItemID.Terrarian}}
            }
        },
        {
            GiveItem.Magic, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new[] {ItemID.WandofSparking, ItemID.WandofFrosting, ItemID.AmethystStaff, ItemID.TopazStaff, ItemID.SapphireStaff, ItemID.EmeraldStaff, ItemID.AmberStaff, ItemID.RubyStaff, ItemID.DiamondStaff, ItemID.ThunderStaff}},
                {ProgressionUtils.Progression.PreSkeletron, new[] {ItemID.Vilethorn, ItemID.DiamondStaff, ItemID.ThunderStaff, ItemID.ZapinatorGray, ItemID.SpaceGun, ItemID.BeeGun, ItemID.WaterBolt, ItemID.CrimsonRod}},
                {ProgressionUtils.Progression.PreWall, new[] {ItemID.AquaScepter, ItemID.MagicMissile, ItemID.WeatherPain, ItemID.Flamelash, ItemID.FlowerofFire, ItemID.SpaceGun, ItemID.BeeGun, ItemID.DemonScythe, ItemID.BookofSkulls, ItemID.WaterBolt, ItemID.HoundiusShootius}},
                {ProgressionUtils.Progression.PreMech, new[] {ItemID.SkyFracture, ItemID.MeteorStaff, ItemID.CrystalSerpent, ItemID.CrystalVileShard, ItemID.SoulDrain, ItemID.PoisonStaff, ItemID.FrostStaff, ItemID.FlowerofFrost, ItemID.UnholyTrident, ItemID.LaserRifle, ItemID.ZapinatorOrange, ItemID.CursedFlames, ItemID.GoldenShower, ItemID.CrystalStorm, ItemID.MagicalHarp, ItemID.NimbusRod, ItemID.ShadowFlameHexDoll, ItemID.SpiritFlame, ItemID.ClingerStaff, ItemID.MedusaHead, ItemID.MagicDagger, ItemID.IceRod}},
                {ProgressionUtils.Progression.PreGolem, new[] {ItemID.RainbowRod, ItemID.BookStaff, ItemID.NettleBurst, ItemID.VenomStaff, ItemID.WaspGun, ItemID.LeafBlower, ItemID.RainbowGun, ItemID.MagnetSphere, ItemID.RazorbladeTyphoon, ItemID.ToxicFlask}},
                {ProgressionUtils.Progression.PreLunar, new[] {ItemID.ShadowbeamStaff, ItemID.InfernoFork, ItemID.SpectreStaff, ItemID.BatScepter, ItemID.Razorpine, ItemID.BlizzardStaff, ItemID.PrincessWeapon, ItemID.StaffofEarth, ItemID.HeatRay, ItemID.LaserMachinegun, ItemID.ChargedBlasterCannon, ItemID.BubbleGun, ItemID.SparkleGuitar, ItemID.FairyQueenMagicItem}},
                {ProgressionUtils.Progression.PreMoonLord, new[] {ItemID.ApprenticeStaffT3, ItemID.LunarFlareBook, ItemID.NebulaBlaze, ItemID.NebulaArcanum}},
                {ProgressionUtils.Progression.PostGame, new[] {ItemID.LastPrism, ItemID.PortalGun}}
            }
        },
        {
            GiveItem.Summon, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new[] {ItemID.AbigailsFlower, ItemID.BabyBirdStaff, ItemID.FlinxStaff, ItemID.SlimeStaff, ItemID.BlandWhip}},
                {ProgressionUtils.Progression.PreSkeletron, new[] {ItemID.HornetStaff, ItemID.DD2BallistraTowerT1Popper, ItemID.DD2ExplosiveTrapT1Popper, ItemID.DD2FlameburstTowerT1Popper, ItemID.DD2LightningAuraT1Popper, ItemID.ThornWhip}},
                {ProgressionUtils.Progression.PreWall, new[] {ItemID.ImpStaff, ItemID.VampireFrogStaff, ItemID.HoundiusShootius, ItemID.BoneWhip}},
                {ProgressionUtils.Progression.PreMech, new[] {ItemID.SpiderStaff, ItemID.PirateStaff, ItemID.SanguineStaff, ItemID.DD2BallistraTowerT2Popper, ItemID.DD2ExplosiveTrapT2Popper, ItemID.DD2FlameburstTowerT2Popper, ItemID.DD2LightningAuraT2Popper, ItemID.QueenSpiderStaff, ItemID.FireWhip, ItemID.CoolWhip}},
                {ProgressionUtils.Progression.PreGolem, new[] {ItemID.OpticStaff, ItemID.Smolstar, ItemID.DeadlySphereStaff, ItemID.PygmyStaff, ItemID.RavenStaff, ItemID.StaffoftheFrostHydra, ItemID.SwordWhip}},
                {ProgressionUtils.Progression.PreLunar, new[] {ItemID.StormTigerStaff, ItemID.TempestStaff, ItemID.XenoStaff, ItemID.DD2BallistraTowerT3Popper, ItemID.DD2ExplosiveTrapT3Popper, ItemID.DD2FlameburstTowerT3Popper, ItemID.DD2LightningAuraT3Popper, ItemID.StaffoftheFrostHydra, ItemID.ScytheWhip, ItemID.MaceWhip}},
                {ProgressionUtils.Progression.PreMoonLord, new[] {ItemID.StardustCellStaff, ItemID.StardustDragonStaff, ItemID.EmpressBlade, ItemID.RainbowWhip}},
                {ProgressionUtils.Progression.PostGame, new[] {ItemID.EmpressBlade, ItemID.RainbowCrystalStaff, ItemID.MoonlordTurretStaff}}
            }
        },
        {
            GiveItem.Ranged, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new[] {ItemID.WoodenBow, ItemID.CopperBow, ItemID.TinBow, ItemID.IronBow, ItemID.LeadBow, ItemID.SilverBow, ItemID.TungstenBow, ItemID.AshWoodBow, ItemID.GoldBow, ItemID.PlatinumBow, ItemID.Blowpipe, ItemID.FlintlockPistol, ItemID.SnowballCannon, ItemID.MolotovCocktail}},
                {ProgressionUtils.Progression.PreSkeletron, new[] {ItemID.DemonBow, ItemID.TendonBow, ItemID.BloodRainBow, ItemID.Minishark, ItemID.Boomstick, ItemID.Revolver, ItemID.RedRyder, ItemID.Sandgun, ItemID.Musket, ItemID.TheUndertaker, ItemID.AleThrowingGlove, ItemID.PainterPaintballGun, ItemID.Harpoon}},
                {ProgressionUtils.Progression.PreWall, new[] {ItemID.MoltenFury, ItemID.BeesKnees, ItemID.HellwingBow, ItemID.StarCannon, ItemID.Blowgun, ItemID.QuadBarrelShotgun, ItemID.Handgun, ItemID.PewMaticHorn, ItemID.PhoenixBlaster}},
                {ProgressionUtils.Progression.PreMech, new[] {ItemID.DaedalusStormbow, ItemID.IceBow, ItemID.ShadowFlameBow, ItemID.Marrow, ItemID.PulseBow, ItemID.CobaltRepeater, ItemID.PalladiumRepeater, ItemID.MythrilRepeater, ItemID.OrichalcumRepeater, ItemID.AdamantiteRepeater, ItemID.TitaniumRepeater, ItemID.ClockworkAssaultRifle, ItemID.Gatligator, ItemID.Shotgun, ItemID.OnyxBlaster, ItemID.CoinGun, ItemID.Uzi, ItemID.DartRifle, ItemID.DartPistol}},
                {ProgressionUtils.Progression.PreGolem, new[] {ItemID.DD2PhoenixBow, ItemID.Tsunami, ItemID.HallowedRepeater, ItemID.ChlorophyteShotbow, ItemID.SuperStarCannon, ItemID.Megashark, ItemID.Flamethrower, ItemID.VenusMagnum, ItemID.GrenadeLauncher, ItemID.PiranhaGun, ItemID.Toxikarp}},
                {ProgressionUtils.Progression.PreLunar, new[] {ItemID.DD2BetsyBow, ItemID.FairyQueenRangedItem, ItemID.StakeLauncher, ItemID.TacticalShotgun, ItemID.SniperRifle, ItemID.CandyCornRifle, ItemID.ChainGun, ItemID.ElfMelter, ItemID.Xenopopper, ItemID.ProximityMineLauncher, ItemID.RocketLauncher, ItemID.NailGun, ItemID.JackOLanternLauncher, ItemID.SnowmanCannon, ItemID.Stynger, ItemID.ElectrosphereLauncher, ItemID.FireworksLauncher}},
                {ProgressionUtils.Progression.PreMoonLord, new[] {ItemID.Phantasm, ItemID.VortexBeater}},
                {ProgressionUtils.Progression.PostGame, new[] {ItemID.SDMG, ItemID.Celeb2}}
            }
        },
        {
            GiveItem.Armour, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {
                    ProgressionUtils.Progression.PreEye, new[]
                    {
                        ItemID.MiningHelmet, ItemID.WoodHelmet, ItemID.WoodBreastplate, ItemID.WoodGreaves, ItemID.CactusHelmet, ItemID.CactusBreastplate, ItemID.CactusLeggings,
                        ItemID.CopperHelmet, ItemID.CopperChainmail, ItemID.CopperGreaves, ItemID.TinHelmet, ItemID.TinChainmail, ItemID.TinGreaves,
                        ItemID.IronHelmet, ItemID.IronChainmail, ItemID.IronGreaves, ItemID.PumpkinHelmet, ItemID.PumpkinBreastplate, ItemID.PumpkinLeggings,
                        ItemID.GladiatorHelmet, ItemID.GladiatorBreastplate, ItemID.GladiatorLeggings, ItemID.IronHelmet, ItemID.IronChainmail, ItemID.IronGreaves,
                        ItemID.LeadHelmet, ItemID.LeadChainmail, ItemID.LeadGreaves, ItemID.GoldHelmet, ItemID.GoldChainmail, ItemID.GoldGreaves,
                        ItemID.SilverHelmet, ItemID.SilverChainmail, ItemID.SilverGreaves, ItemID.TungstenHelmet, ItemID.TungstenChainmail, ItemID.TungstenGreaves,
                        ItemID.PlatinumHelmet, ItemID.PlatinumChainmail, ItemID.PlatinumGreaves
                    }
                },
                {
                    ProgressionUtils.Progression.PreSkeletron, new[]
                    {
                        ItemID.BeeHeadgear, ItemID.BeeBreastplate, ItemID.BeeGreaves,
                        ItemID.JungleHat, ItemID.JungleShirt, ItemID.JunglePants, ItemID.AncientCobaltHelmet, ItemID.AncientCobaltBreastplate, ItemID.AncientCobaltLeggings,
                        ItemID.MeteorHelmet, ItemID.MeteorSuit, ItemID.MeteorLeggings
                    }
                },
                {
                    ProgressionUtils.Progression.PreWall, new[]
                    {
                        ItemID.NecroHelmet, ItemID.NecroBreastplate, ItemID.NecroGreaves, ItemID.ShadowHelmet, ItemID.ShadowScalemail, ItemID.ShadowGreaves,
                        ItemID.AncientShadowHelmet, ItemID.AncientShadowScalemail, ItemID.AncientShadowGreaves, ItemID.CrimsonHelmet, ItemID.CrimsonScalemail, ItemID.CrimsonGreaves,
                        ItemID.MoltenHelmet, ItemID.MoltenBreastplate, ItemID.MoltenGreaves
                    }
                },
                {
                    ProgressionUtils.Progression.PreMech, new[]
                    {
                        ItemID.SpiderMask, ItemID.SpiderBreastplate, ItemID.SpiderGreaves, ItemID.PearlwoodHelmet, ItemID.PearlwoodBreastplate, ItemID.PearlwoodGreaves,
                        ItemID.CobaltHelmet, ItemID.CobaltBreastplate, ItemID.CobaltLeggings, ItemID.PalladiumHelmet, ItemID.PalladiumBreastplate, ItemID.PalladiumLeggings,
                        ItemID.MythrilHelmet, ItemID.MythrilChainmail, ItemID.MythrilGreaves, ItemID.OrichalcumHelmet, ItemID.OrichalcumBreastplate, ItemID.OrichalcumLeggings,
                        ItemID.AdamantiteHelmet, ItemID.AdamantiteBreastplate, ItemID.AdamantiteLeggings, ItemID.TitaniumHeadgear, ItemID.TitaniumHelmet, ItemID.TitaniumMask,
                        ItemID.TitaniumBreastplate, ItemID.TitaniumLeggings, ItemID.AdamantiteHeadgear, ItemID.AdamantiteMask
                    }
                },
                {
                    ProgressionUtils.Progression.PreGolem, new[]
                    {
                        ItemID.FrostHelmet, ItemID.FrostBreastplate, ItemID.FrostLeggings, ItemID.ApprenticeHat, ItemID.ApprenticeRobe, ItemID.ApprenticeTrousers,
                        ItemID.HallowedHelmet, ItemID.HallowedMask, ItemID.HallowedHeadgear, ItemID.HallowedPlateMail, ItemID.HallowedGreaves,
                        ItemID.ChlorophyteHelmet, ItemID.ChlorophyteMask, ItemID.ChlorophyteHeadgear, ItemID.ChlorophytePlateMail, ItemID.ChlorophyteGreaves,
                        ItemID.TurtleHelmet, ItemID.TurtleScaleMail, ItemID.TurtleLeggings, ItemID.TikiMask, ItemID.TikiShirt, ItemID.TikiPants,
                        ItemID.SpookyHelmet, ItemID.SpookyBreastplate, ItemID.SpookyLeggings, ItemID.ShroomiteHeadgear, ItemID.ShroomiteHelmet, ItemID.ShroomiteMask,
                        ItemID.ShroomiteBreastplate, ItemID.ShroomiteLeggings
                    }
                },
                {
                    ProgressionUtils.Progression.PreLunar, new[]
                    {
                        ItemID.SpectreHood, ItemID.SpectreMask, ItemID.SpectreRobe, ItemID.SpectrePants, ItemID.BeetleHelmet, ItemID.BeetleShell, ItemID.BeetleScaleMail,
                        ItemID.BeetleLeggings
                    }
                },
                {
                    ProgressionUtils.Progression.PreMoonLord, new[]
                    {
                        ItemID.SpectreHood, ItemID.SpectreMask, ItemID.SpectreRobe, ItemID.SpectrePants, ItemID.BeetleHelmet, ItemID.BeetleShell, ItemID.BeetleScaleMail,
                        ItemID.BeetleLeggings
                    }
                },
                {
                    ProgressionUtils.Progression.PostGame, new[]
                    {
                        ItemID.SolarFlareHelmet, ItemID.SolarFlareBreastplate, ItemID.SolarFlareLeggings, ItemID.VortexHelmet, ItemID.VortexBreastplate, ItemID.VortexLeggings,
                        ItemID.NebulaHelmet, ItemID.NebulaBreastplate, ItemID.NebulaLeggings, ItemID.StardustHelmet, ItemID.StardustBreastplate, ItemID.StardustLeggings
                    }
                }
            }
        },
        {
            GiveItem.HealingPotion, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new[] {ItemID.LesserHealingPotion}},
                {ProgressionUtils.Progression.PreSkeletron, new[] {ItemID.HealingPotion}},
                {ProgressionUtils.Progression.PreWall, new[] {ItemID.HealingPotion}},
                {ProgressionUtils.Progression.PreMech, new[] {ItemID.GreaterHealingPotion}},
                {ProgressionUtils.Progression.PreGolem, new[] {ItemID.GreaterHealingPotion}},
                {ProgressionUtils.Progression.PreLunar, new[] {ItemID.GreaterHealingPotion}},
                {ProgressionUtils.Progression.PreMoonLord, new[] {ItemID.SuperHealingPotion}},
                {ProgressionUtils.Progression.PostGame, new[] {ItemID.SuperHealingPotion}}
            }
        },
        {
            GiveItem.Potion, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {
                    ProgressionUtils.Progression.PreEye, new[]
                    {
                        ItemID.AmmoReservationPotion, ItemID.ArcheryPotion, ItemID.BattlePotion, ItemID.BuilderPotion, ItemID.CalmingPotion, (short)2329, ItemID.EndurancePotion,
                        ItemID.FeatherfallPotion, ItemID.FlipperPotion, ItemID.GillsPotion, ItemID.GravitationPotion, ItemID.HunterPotion, ItemID.InfernoPotion,
                        ItemID.IronskinPotion, ItemID.LifeforcePotion, ItemID.NightOwlPotion, ItemID.ObsidianSkinPotion, ItemID.RagePotion, ItemID.RegenerationPotion,
                        ItemID.ShinePotion, ItemID.SpelunkerPotion, ItemID.SummoningPotion, ItemID.SwiftnessPotion, ItemID.ThornsPotion, ItemID.TitanPotion, ItemID.WrathPotion,
                        ItemID.FlaskofCursedFlames, ItemID.FlaskofFire, ItemID.FlaskofGold, ItemID.FlaskofIchor, ItemID.FlaskofNanites, ItemID.FlaskofParty, ItemID.FlaskofPoison, ItemID.FlaskofVenom,
                        ItemID.BiomeSightPotion
                    }
                }
            }
        },
        {
            GiveItem.Kite, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {
                    ProgressionUtils.Progression.PreEye, new[]
                    {
                        ItemID.KiteBlue, ItemID.KiteRed, ItemID.KiteYellow, ItemID.KiteBunny, ItemID.KiteCrawltipede, ItemID.KiteGoldfish, ItemID.KiteKoi,
                        ItemID.KitePigron, ItemID.KiteShark, ItemID.KiteSpectrum, ItemID.KiteUnicorn, ItemID.KiteWyvern, ItemID.KiteAngryTrapper,
                        ItemID.KiteBoneSerpent, ItemID.KiteBunnyCorrupt, ItemID.KiteBunnyCrimson, ItemID.KiteJellyfishBlue, ItemID.KiteJellyfishPink,
                        ItemID.KiteManEater, ItemID.KiteSandShark, ItemID.KiteWanderingEye, ItemID.KiteWorldFeeder, ItemID.KiteBlueAndYellow, ItemID.KiteRedAndYellow
                    }
                }
            }
        },
        {
            GiveItem.Food, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {
                    ProgressionUtils.Progression.PreEye, new[]
                    {
                        ItemID.CookedMarshmallow, ItemID.AppleJuice, ItemID.BloodyMoscato, ItemID.BowlofSoup, ItemID.BunnyStew,
                        ItemID.CookedFish, ItemID.CookedShrimp, ItemID.Escargot, ItemID.FroggleBunwich, ItemID.BananaDaiquiri,
                        ItemID.FruitJuice, ItemID.FruitSalad, ItemID.GoldenDelight, ItemID.GrapeJuice, ItemID.GrilledSquirrel,
                        ItemID.GrubSoup, ItemID.Lemonade, ItemID.LobsterTail, ItemID.MonsterLasagna, ItemID.PeachSangria,
                        ItemID.PinaColada, ItemID.PrismaticPunch, ItemID.RoastedBird, ItemID.RoastedDuck, ItemID.SauteedFrogLegs,
                        ItemID.SeafoodDinner, ItemID.SmoothieofDarkness, ItemID.TropicalSmoothie, ItemID.PumpkinPie, ItemID.Ale,
                        ItemID.Teacup, ItemID.Sashimi, ItemID.Apple, ItemID.Apricot, ItemID.Banana, ItemID.BlackCurrant,
                        ItemID.BloodOrange, ItemID.Cherry, ItemID.Coconut, ItemID.Dragonfruit, ItemID.Elderberry, ItemID.Grapefruit,
                        ItemID.Lemon, ItemID.Mango, ItemID.Peach, ItemID.Pineapple, ItemID.Plum, ItemID.Rambutan, ItemID.Starfruit,
                        ItemID.ApplePie, ItemID.Bacon, ItemID.BananaSplit, ItemID.BBQRibs, ItemID.Burger, ItemID.MilkCarton,
                        ItemID.ChickenNugget, ItemID.ChocolateChipCookie, ItemID.CoffeeCup, ItemID.CreamSoda, ItemID.FriedEgg,
                        ItemID.Fries, ItemID.Grapes, ItemID.Hotdog, ItemID.IceCream, ItemID.Milkshake, ItemID.Nachos, ItemID.Pizza,
                        ItemID.PotatoChips, ItemID.ShrimpPoBoy, ItemID.ShuckedOyster, ItemID.Spaghetti, ItemID.Steak, ItemID.ChristmasPudding,
                        ItemID.GingerbreadCookie, ItemID.SugarCookie, ItemID.Marshmallow, ItemID.PadThai, ItemID.Sake,
                        ItemID.SpicyPepper, ItemID.Pomegranate
                    }
                }
            }
        }
    };

    private static readonly Dictionary<GiveItem, Dictionary<ProgressionUtils.Progression, IReadOnlyList<string>>> CalamityItems = new()
    {
        {
            GiveItem.Sword, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<string>>
            {
                {
                    ProgressionUtils.Progression.PreEye, new[]
                    {
                        "AmidiasTrident", "AquaticDischarge", "BallOFugu", "BladecrestOathsword", "FellerofEvergreens",
                        "FracturedArk", "MycelialClaws", "WulfrumBlade", "YateveoBloom", "MarniteSpear"
                    }
                },
                {
                    ProgressionUtils.Progression.PreSkeletron, new[]
                    {
                        "Basher", "Bonebreaker", "CausticEdge", "GoldplumeSpear",
                        "PerfectDark", "RedtideSpear", "SeashineSword", "TeardropCleaver", "VeinBurster",
                        "WindBlade", "SausageMaker", "UrchinFlail", "MonstrousKnives"
                    }
                },
                {ProgressionUtils.Progression.PreWall, new[] {"BloodyEdge", "GaussDagger", "GeliticBlade", "OldLordOathsword"}},
                {
                    ProgressionUtils.Progression.PreMech, new[]
                    {
                        "AbsoluteZero", "Aftershock", "Carnage", "CelestialClaymore", "EarthenPike", "EvilSmasher",
                        "FlarefrostBlade", "ForsakenSaber", "MajesticGuard", "Roxcalibur", "StormSaber", "TitanArm",
                        "ClamCrusher", "Nebulash"
                    }
                },
                {
                    ProgressionUtils.Progression.PreGolem, new[]
                    {
                        "AbyssBlade", "AnarchyBlade", "Avalanche", "TrueBiomeBlade", "Brimlance",
                        "Brimlash", "BrimstoneSword", "CatastropheClaymore", "CometQuasher", "DarklightGreatsword",
                        "DepthCrusher", "FeralthornClaymore", "Floodtide", "ForbiddenOathblade", "GalvanizingGlaive",
                        "Greentide", "InfernaCutter", "MantisClaws", "Tumbleweed"
                    }
                },
                {
                    ProgressionUtils.Progression.PreLunar, new[]
                    {
                        "AegisBlade", "BrinyBaron", "DiseasedPike", "TrueTyrantYharimsUltisword",
                        "SoulHarvester", "TrueBloodyEdge", "TrueForbiddenOathblade", "UltimusCleaver"
                    }
                },
                {
                    ProgressionUtils.Progression.PreMoonLord, new[]
                    {
                        "AstralBlade", "AstralPike", "AstralScythe", "FallenPaladinsHammer",
                        "StormRuler", "OmegaBiomeBlade", "Virulence"
                    }
                },
                {
                    ProgressionUtils.Progression.PostGame, new[]
                    {
                        "ArkoftheCosmos", "ArkoftheElements", "Ataraxia", "BansheeHook",
                        "CosmicDischarge", "CosmicShiv", "CrescentMoon", "Devastation", "DevilsDevastation",
                        "DevilsSunrise", "DraconicDestruction", "Earth", "ElementalExcalibur", "ElementalLance",
                        "ElementalShiv", "EmpyreanKnives", "EntropicClaymore", "EssenceFlayer", "Excelsus",
                        "Exoblade", "GaelsGreatsword", "GalactusBlade", "GalileoGladius", "GrandGuardian",
                        "GreatswordofBlah", "GreatswordofJudgement", "Grax"
                    }
                }
            }
        },
        {
            GiveItem.Yoyo, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<string>>
            {
                {
                    ProgressionUtils.Progression.PreEye, new[]
                    {
                        "Riptide", "SmokingComet"
                    }
                },
                {
                    ProgressionUtils.Progression.PreSkeletron, new[]
                    {
                        "AirSpinner", "Aorta"
                    }
                },
                {
                    ProgressionUtils.Progression.PreWall, new[]
                    {
                        "TheGodsGambit"
                    }
                },
                {
                    ProgressionUtils.Progression.PreMech, new[]
                    {
                        "Shimmerspark", "SulphurousGrabber", "YinYo"
                    }
                },
                {
                    ProgressionUtils.Progression.PreGolem, new[]
                    {
                        "Oblivion", "Quagmire"
                    }
                },
                {
                    ProgressionUtils.Progression.PreLunar, new[]
                    {
                        "FaultLine", "Pandemic", "TheMicrowave"
                    }
                },
                {
                    ProgressionUtils.Progression.PreMoonLord, new[]
                    {
                        "FaultLine", "Pandemic", "TheMicrowave"
                    }
                },
                {
                    ProgressionUtils.Progression.PostGame, new[]
                    {
                        "Azathoth", "Lacerator", "SolarFlare", "TheObliterator", "Oracle"
                    }
                }
            }
        },
        {
            GiveItem.Potion, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<string>>
            {
                {
                    ProgressionUtils.Progression.PreEye, new[]
                    {
                        "AstralInjection", "AureusCell", "Baguette", "BoundingPotion", "CalamitasBrew", "CalciumPotion",
                        "DeliciousMeat", "DraconicElixir", "HolyWrathPotion", "PhotosynthesisPotion", "PotionofOmniscience",
                        "TeslaPotion", "TitanScalePotion", "TriumphPotion", "ZenPotion", "ZergPotion", "Everclear",
                        "FabsolsVodka", "Moonshine", "Tequila", "TequilaSunrise", "Vodka", "Whiskey", "YharimsStimulants",
                        "Bloodfin", "GrapeBeer", "HadalStew"
                    }
                }
            }
        }
    };

    #endregion

    #region Static Methods

    private static string GetId(GiveItem giveItem)
    {
        return giveItem switch
        {
            GiveItem.Pickaxe => EffectID.GivePickaxe,
            GiveItem.Sword => EffectID.GiveSword,
            GiveItem.Yoyo => EffectID.GiveYoyo,
            GiveItem.Magic => EffectID.GiveMagicWeapon,
            GiveItem.Summon => EffectID.GiveSummonWeapon,
            GiveItem.Ranged => EffectID.GiveRangedWeapon,
            GiveItem.Armour => EffectID.GiveArmour,
            GiveItem.HealingPotion => EffectID.GiveHealingPotion,
            GiveItem.Potion => EffectID.GivePotion,
            GiveItem.Kite => EffectID.GiveKite,
            GiveItem.Food => EffectID.GiveFood,
            _ => throw new ArgumentOutOfRangeException(nameof(giveItem), giveItem, null)
        };
    }

    private static EffectSeverity GetSeverity(GiveItem giveItem)
    {
        return giveItem switch
        {
            GiveItem.Pickaxe => EffectSeverity.Positive,
            GiveItem.Sword => EffectSeverity.Positive,
            GiveItem.Magic => EffectSeverity.Positive,
            GiveItem.Yoyo => EffectSeverity.Positive,
            GiveItem.Summon => EffectSeverity.Positive,
            GiveItem.Ranged => EffectSeverity.Positive,
            GiveItem.Armour => EffectSeverity.Positive,
            GiveItem.HealingPotion => EffectSeverity.Positive,
            GiveItem.Potion => EffectSeverity.Positive,
            GiveItem.Kite => EffectSeverity.Neutral,
            GiveItem.Food => EffectSeverity.Positive,
            _ => throw new ArgumentOutOfRangeException(nameof(giveItem), giveItem, null)
        };
    }

    private static int GetStackSize(GiveItem giveItem)
    {
        return giveItem switch
        {
            GiveItem.HealingPotion => 2,
            _ => 1
        };
    }

    #endregion

    #region Fields

    private readonly GiveItem _giveItem;
    private readonly int _stack;
    private readonly bool _includePreviousTier;
    private Item? _item;

    #endregion

    #region Constructors

    public GiveItemEffect(GiveItem giveItem) : base(GetId(giveItem), 0, GetSeverity(giveItem))
    {
        _giveItem = giveItem;
        _stack = GetStackSize(_giveItem);
        _includePreviousTier = giveItem switch
        {
            GiveItem.Pickaxe => true,
            GiveItem.Sword => true,
            GiveItem.Magic => true,
            GiveItem.Summon => true,
            GiveItem.Ranged => true,
            _ => false
        };
        StartEmote = giveItem switch
        {
            GiveItem.Pickaxe => EmoteID.ItemPickaxe,
            GiveItem.Sword => EmoteID.ItemSword,
            GiveItem.Magic => EmoteID.ItemManaPotion,
            GiveItem.Ranged => EmoteID.ItemMinishark,
            GiveItem.HealingPotion => EmoteID.ItemLifePotion,
            GiveItem.Potion => EmoteID.ItemManaPotion,
            GiveItem.Food => EmoteID.ItemSoup,
            _ => -1
        };
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    protected override int StartEmote { get; }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Load the item id collection (try PreEye for cases that don't use progression)
        var progress = ProgressionUtils.GetProgression();
        List<short> availableOptions = new();

        if (!VanillaItems.TryGetValue(_giveItem, out var vanillaItemsByProgression) ||
            (!vanillaItemsByProgression.TryGetValue(progress, out var vanillaItems) &&
             !vanillaItemsByProgression.TryGetValue(progress = ProgressionUtils.Progression.PreEye, out vanillaItems)))
        {
            // Not supported
            return CrowdControlResponseStatus.Failure;
        }

        // Add vanilla items
        availableOptions.AddRange(vanillaItems);

        // Check if we should include the items from the previous tier of progression too
        if (_includePreviousTier && (int)progress - 1 >= 0 && vanillaItemsByProgression.TryGetValue((ProgressionUtils.Progression)((int)progress - 1), out vanillaItems))
        {
            availableOptions.AddRange(vanillaItems);
        }

        // Try to add calamity items
        if (ModUtils.TryGetMod(ModUtils.Calamity.Name, out var calamity) &&
            CalamityItems.TryGetValue(_giveItem, out var calamityItemsByProgression) &&
            calamityItemsByProgression.TryGetValue(progress, out var calamityItems))
        {
            // ReSharper disable once AccessToModifiedClosure
            ModUtils.IterateTypes<ModItem>(calamity, calamityItems, x => availableOptions.Add((short)x.Type));
        }

        // Create a distinct set and ensure it contains elements
        availableOptions = availableOptions.Distinct().ToList();
        if (availableOptions.Count == 0)
        {
            return CrowdControlResponseStatus.Failure;
        }

        // Choose the item type from a distinct set
        var chosenId = availableOptions[Main.rand.Next(availableOptions.Count)];

        // Create an instance of the item which will be cloned into the world
        _item = new Item(chosenId);
        if (_item.maxStack == 1 && !string.IsNullOrEmpty(Viewer))
        {
            // Set a custom name on the item using the viewer's name
            _item.SetItemOwner(Viewer);
        }

        // Spawn the item
        _item = GetLocalPlayer().Player.QuickSpawnItemDirect(null, _item, _stack);
        if (_item.whoAmI == Main.maxItems || !_item.active || _item.type != chosenId)
        {
            // Something went wrong - case where item from inventory was returned - how is this even possible!?
            // We'll try to catch that VERY odd case here...
            return CrowdControlResponseStatus.Retry;
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _item = null;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var itemName = _item != null ? Lang.GetItemName(_item.type).Value : string.Empty;
        TerrariaUtils.WriteEffectMessage(
            (short)(_item?.type ?? 0),
            LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, itemName),
            Severity);
    }

    #endregion
}