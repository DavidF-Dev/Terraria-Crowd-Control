﻿using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CrowdControlMod.Features;

/// <summary>
///     Allow editing the max stat (HP or MP) using new tModLoader API.
/// </summary>
public static class MaxStatUtils
{
    #region Static Methods

    /// <summary>
    ///     Shift the player's max life. Returns the left-over amount that couldn't be applied.
    /// </summary>
    public static int AddStatLifeMax(this Player player, int amount)
    {
        var editMaxStatPlayer = player.GetModPlayer<EditMaxStatPlayer>();
        switch (amount)
        {
            case > 0:
            {
                while (amount > 0)
                {
                    if (amount >= 20 && editMaxStatPlayer.LifeCrystalRemoved > 0)
                    {
                        editMaxStatPlayer.LifeCrystalRemoved--;
                        amount -= 20;
                    }
                    else if (amount >= 20 && player.ConsumedLifeCrystals < Player.LifeCrystalMax)
                    {
                        player.ConsumedLifeCrystals++;
                        amount -= 20;
                    }
                    else if (amount >= 5 && player.ConsumedLifeFruit < Player.LifeFruitMax)
                    {
                        player.ConsumedLifeFruit++;
                        amount -= 5;
                    }
                    else
                    {
                        break;
                    }
                }

                break;
            }
            case < 0:
            {
                while (amount <= 0)
                {
                    if (amount <= -5 && player.ConsumedLifeFruit > 0)
                    {
                        player.ConsumedLifeFruit--;
                        amount += 5;
                    }
                    else if (amount <= -20 && player.ConsumedLifeCrystals > 0)
                    {
                        player.ConsumedLifeCrystals--;
                        amount += 20;
                    }
                    else if (amount <= -20 && editMaxStatPlayer.LifeCrystalRemoved < 4)
                    {
                        editMaxStatPlayer.LifeCrystalRemoved++;
                        amount += 20;
                    }
                    else
                    {
                        break;
                    }
                }

                break;
            }
        }

        TerrariaUtils.WriteDebug($"{nameof(AddStatLifeMax)}: (Crystal={player.ConsumedLifeCrystals}) (Fruit={player.ConsumedLifeFruit}) (Residual={amount}) (Removed={editMaxStatPlayer.LifeCrystalRemoved})");
        return amount;
    }

    /// <summary>
    ///     Shift the player's max mana. Returns the left-over amount that couldn't be applied.
    /// </summary>
    public static int AddStatManaMax(this Player player, int amount)
    {
        switch (amount)
        {
            case > 0:
            {
                while (amount >= 20 && player.ConsumedManaCrystals < Player.ManaCrystalMax)
                {
                    player.ConsumedManaCrystals++;
                    amount -= 20;
                }

                break;
            }
            case < 0:
            {
                while (amount <= -20 && player.ConsumedManaCrystals > 0)
                {
                    player.ConsumedManaCrystals--;
                    amount += 20;
                }

                break;
            }
        }

        return amount;
    }

    #endregion

    #region Nested Types

    // ReSharper disable once UnusedType.Local
    private sealed class EditMaxStatItem : GlobalItem
    {
        #region Methods

        public override bool? UseItem(Item item, Player player)
        {
            // Check if a life crystal was blocked but should've been allowed
            var editMaxStatPlayer = player.GetModPlayer<EditMaxStatPlayer>();
            if (item.type == ItemID.LifeCrystal && player.ConsumedLifeCrystals == Player.LifeCrystalMax && editMaxStatPlayer.LifeCrystalRemoved > 0)
            {
                editMaxStatPlayer.LifeCrystalRemoved--;
                player.HealEffect(20);
                TerrariaUtils.WriteDebug($"{nameof(UseItem)}: (Crystal={player.ConsumedLifeCrystals}) (Fruit={player.ConsumedLifeFruit}) (Removed={editMaxStatPlayer.LifeCrystalRemoved})");
                return true;
            }

            return base.UseItem(item, player);
        }

        #endregion
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class EditMaxStatPlayer : ModPlayer
    {
        #region Fields

        /// <summary>
        ///     Number of life crystals (20HP) removed from the max hp.
        /// </summary>
        public int LifeCrystalRemoved;

        #endregion

        #region Methods

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            base.ModifyMaxStats(out health, out mana);
            health.Base = -LifeCrystalRemoved * 20;
        }

        public override void PreSavePlayer()
        {
            // Ensure we a saving the player in such a way that it can be used again without this mod enabled
            while (LifeCrystalRemoved > 0 && Player.ConsumedLifeCrystals > 0)
            {
                LifeCrystalRemoved--;
                Player.ConsumedLifeCrystals--;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("LifeCrystalRemoved", LifeCrystalRemoved);
        }

        public override void LoadData(TagCompound tag)
        {
            LifeCrystalRemoved = tag.GetInt("LifeCrystalRemoved");
        }

        #endregion
    }

    #endregion
}