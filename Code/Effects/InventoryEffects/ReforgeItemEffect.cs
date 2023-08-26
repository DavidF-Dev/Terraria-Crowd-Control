using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Attempts to randomly reforge the held weapon.
/// </summary>
public sealed class ReforgeItemEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private static readonly Dictionary<int, IReadOnlyList<int>> PrefixIdsByClass = new()
    {
        {
            DamageClass.Melee.Type, new[]
            {
                PrefixID.Large, PrefixID.Massive, PrefixID.Dangerous, PrefixID.Savage, PrefixID.Sharp, PrefixID.Pointy, PrefixID.Tiny, PrefixID.Terrible,
                PrefixID.Small, PrefixID.Dull, PrefixID.Unhappy, PrefixID.Bulky, PrefixID.Shameful, PrefixID.Heavy, PrefixID.Light, PrefixID.Legendary
            }
        },
        {
            // Same as DamageClass.Melee
            DamageClass.MeleeNoSpeed.Type, new[]
            {
                PrefixID.Large, PrefixID.Massive, PrefixID.Dangerous, PrefixID.Savage, PrefixID.Sharp, PrefixID.Pointy, PrefixID.Tiny, PrefixID.Terrible,
                PrefixID.Small, PrefixID.Dull, PrefixID.Unhappy, PrefixID.Bulky, PrefixID.Shameful, PrefixID.Heavy, PrefixID.Light, PrefixID.Legendary
            }
        },
        {
            // Same as DamageClass.Melee
            DamageClass.SummonMeleeSpeed.Type, new[]
            {
                PrefixID.Large, PrefixID.Massive, PrefixID.Dangerous, PrefixID.Savage, PrefixID.Sharp, PrefixID.Pointy, PrefixID.Tiny, PrefixID.Terrible,
                PrefixID.Small, PrefixID.Dull, PrefixID.Unhappy, PrefixID.Bulky, PrefixID.Shameful, PrefixID.Heavy, PrefixID.Light, PrefixID.Legendary
            }
        },
        {
            DamageClass.Ranged.Type, new[]
            {
                PrefixID.Sighted, PrefixID.Rapid, PrefixID.Hasty, PrefixID.Intimidating, PrefixID.Deadly, PrefixID.Staunch, PrefixID.Awful, PrefixID.Lethargic,
                PrefixID.Awkward, PrefixID.Powerful, PrefixID.Unreal
            }
        },
        {
            DamageClass.Magic.Type, new[]
            {
                PrefixID.Mystic, PrefixID.Adept, PrefixID.Masterful, PrefixID.Inept, PrefixID.Ignorant, PrefixID.Deranged, PrefixID.Intense, PrefixID.Taboo,
                PrefixID.Celestial, PrefixID.Furious, PrefixID.Manic, PrefixID.Mythical
            }
        }
    };

    private static readonly IReadOnlyList<int> UniversalPrefixIds = new[]
    {
        PrefixID.Keen, PrefixID.Superior, PrefixID.Forceful, PrefixID.Broken, PrefixID.Damaged, PrefixID.Shoddy, PrefixID.Hurtful, PrefixID.Strong,
        PrefixID.Unpleasant, PrefixID.Weak, PrefixID.Ruthless, PrefixID.Frenzying, PrefixID.Godly, PrefixID.Demonic, PrefixID.Zealous
    };

    #endregion

    #region Fields

    private int _chosenPrefix;
    private Item? _item;

    #endregion

    #region Constructors

    public ReforgeItemEffect() : base(EffectID.ReforgeItem, 0, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.inventory[player.Player.selectedItem].type == ItemID.None)
        {
            // No item being held, so retry
            return CrowdControlResponseStatus.Retry;
        }

        var item = player.Player.inventory[player.Player.selectedItem];
        if (!PrefixIdsByClass.TryGetValue(item.DamageType.Type, out var prefixIds))
        {
            // Not a supported item to be reforged, so retry
            return CrowdControlResponseStatus.Retry;
        }

        // Choose a new random prefix (including universal prefixes)
        do
        {
            var index = Main.rand.Next(prefixIds.Count + UniversalPrefixIds.Count);
            _chosenPrefix = index < prefixIds.Count ? prefixIds[index] : UniversalPrefixIds[index - prefixIds.Count];
        } while (item.prefix == _chosenPrefix);

        _item = new Item(item.type);
        if (!_item.Prefix(_chosenPrefix))
        {
            // Unable to apply prefix, so retry
            _chosenPrefix = 0;
            _item = null;
            return CrowdControlResponseStatus.Retry;
        }

        // Replace the existing item with the new item
        _item.favorited = item.favorited;
        _item.stack = item.stack;
        // _item.SetItemOwner(item.GetItemOwner()); This renames the wrong item, maybe because it isn't a real item
        _chosenPrefix = _item.prefix;
        player.Player.inventory[player.Player.selectedItem] = _item;

        if (NetUtils.IsClient)
        {
            // Update the server on the changes
            NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.Player.whoAmI,
                player.Player.selectedItem, _item.stack, _item.prefix, _item.netID);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _chosenPrefix = 0;
        _item = null;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var itemName = _item != null ? Lang.GetItemName(_item.type).Value : string.Empty;
        var prefixName = Lang.prefix[_chosenPrefix].Value;
        TerrariaUtils.WriteEffectMessage(
            (short)(_item?.type ?? 0),
            LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, itemName, prefixName),
            Severity);
    }

    #endregion
}