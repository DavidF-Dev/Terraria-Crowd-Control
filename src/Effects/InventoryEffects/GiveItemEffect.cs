using System;
using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

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
        Armour,
        HealingPotion,
        Potion,
        Kite
    }

    #endregion

    #region Static Fields and Constants

    private static readonly Dictionary<GiveItem, Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>> Items = new()
    {
        {
            GiveItem.Pickaxe, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new short[] {3509, 3503, 1, 3497, 3515, 3491, 882, 3485}},
                {ProgressionUtils.Progression.PreSkeletron, new short[] {3521, 1917, 1320}},
                {ProgressionUtils.Progression.PreWall, new short[] {103, 798}},
                {ProgressionUtils.Progression.PreMech, new short[] {122, 776, 1188, 777, 1195}},
                {ProgressionUtils.Progression.PreGolem, new short[] {778, 1202, 1506, 1230, 990, 2176}},
                {ProgressionUtils.Progression.PreLunar, new short[] {1294, 2176, 990}},
                {ProgressionUtils.Progression.PreMoonLord, new short[] {1294, 2176}},
                {ProgressionUtils.Progression.PostGame, new short[] {2776, 2781, 2786, 3466}}
            }
        },
        {
            GiveItem.Sword, new Dictionary<ProgressionUtils.Progression, IReadOnlyList<short>>
            {
                {ProgressionUtils.Progression.PreEye, new short[] {1827, 4, 3496, 3490, 1304, 3772, 881}},
                {ProgressionUtils.Progression.PreSkeletron, new short[] {3520, 3484, 1166, 1909, 2273, 724, 46, 795}},
                {ProgressionUtils.Progression.PreWall, new short[] {155, 3349, 65, 1123, 190, 121, 273}},
                {ProgressionUtils.Progression.PreMech, new short[] {3258, 483, 1185, 1192, 484, 3823, 1306, 426, 672, 482, 1199, 676, 723, 3013, 3211}},
                {ProgressionUtils.Progression.PreGolem, new short[] {368, 1227, 674, 1327, 3106, 671, 1226, 1826, 1928, 675, 3018}},
                {ProgressionUtils.Progression.PreLunar, new short[] {3018, 3827, 757, 2880}},
                {ProgressionUtils.Progression.PreMoonLord, new short[] {3827, 757, 2880}},
                {ProgressionUtils.Progression.PostGame, new short[] {3065, 3063}}
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
                        ItemID.FlaskofCursedFlames, ItemID.FlaskofFire, ItemID.FlaskofGold, ItemID.FlaskofIchor, ItemID.FlaskofNanites, ItemID.FlaskofParty, ItemID.FlaskofPoison, ItemID.FlaskofVenom
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
            GiveItem.Armour => EffectID.GiveArmour,
            GiveItem.HealingPotion => EffectID.GiveHealingPotion,
            GiveItem.Potion => EffectID.GivePotion,
            GiveItem.Kite => EffectID.GiveKite,
            _ => throw new ArgumentOutOfRangeException(nameof(giveItem), giveItem, null)
        };
    }

    private static EffectSeverity GetSeverity(GiveItem giveItem)
    {
        return giveItem switch
        {
            GiveItem.Pickaxe => EffectSeverity.Positive,
            GiveItem.Sword => EffectSeverity.Positive,
            GiveItem.Armour => EffectSeverity.Positive,
            GiveItem.HealingPotion => EffectSeverity.Positive,
            GiveItem.Potion => EffectSeverity.Positive,
            GiveItem.Kite => EffectSeverity.Neutral,
            _ => throw new ArgumentOutOfRangeException(nameof(giveItem), giveItem, null)
        };
    }

    private static int GetStackSize(GiveItem giveItem)
    {
        return giveItem switch
        {
            GiveItem.Pickaxe => 1,
            GiveItem.Sword => 1,
            GiveItem.Armour => 1,
            GiveItem.HealingPotion => 2,
            GiveItem.Potion => 1,
            GiveItem.Kite => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(giveItem), giveItem, null)
        };
    }

    #endregion

    #region Fields

    private readonly GiveItem _giveItem;
    private readonly int _stack;
    private Item? _item;

    #endregion

    #region Constructors

    public GiveItemEffect(GiveItem giveItem) : base(GetId(giveItem), null, GetSeverity(giveItem))
    {
        _giveItem = giveItem;
        _stack = GetStackSize(_giveItem);
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Load the item id collection (try PreEye for cases that don't use progression)
        var progress = ProgressionUtils.GetProgression();
        if (!Items.TryGetValue(_giveItem, out var itemsByProgression) ||
            (!itemsByProgression.TryGetValue(progress, out var itemIds) &&
             !itemsByProgression.TryGetValue(ProgressionUtils.Progression.PreEye, out itemIds)))
        {
            // Not supported
            return CrowdControlResponseStatus.Failure;
        }

        // Choose the item and spawn it in
        var player = GetLocalPlayer();
        var chosenId = itemIds[Main.rand.Next(itemIds.Count)];
        var itemId = Item.NewItem(null, player.Player.position, player.Player.width, player.Player.height,
            chosenId, _stack, noGrabDelay: true);
        _item = Main.item[itemId];

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // Notify server of the item
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, chosenId, 1f);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _item = null;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage((short)(_item?.type ?? 0), $"{viewerString} gave {playerString} a {_item?.Name}", Severity);
    }

    #endregion
}