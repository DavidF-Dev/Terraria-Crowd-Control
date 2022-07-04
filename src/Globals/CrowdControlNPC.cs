using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public sealed class CrowdControlNPC : GlobalNPC
{
    #region Delegates

    /// <inheritdoc cref="EditSpawnRate" />
    public delegate void EditSpawnRateDelegate(Player player, ref int spawnRate, ref int maxSpawns);

    /// <inheritdoc cref="StrikeNPC" />
    public delegate bool StrikeNpcDelegate(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit);

    /// <inheritdoc cref="PreDraw" />
    [PublicAPI]
    public delegate bool PreDrawDelegate(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColour);

    #endregion

    #region Events

    /// <inheritdoc cref="EditSpawnRate" />
    [PublicAPI]
    public static event EditSpawnRateDelegate EditSpawnRateHook;

    /// <inheritdoc cref="StrikeNPC" />
    [PublicAPI]
    public static event StrikeNpcDelegate StrikeNpcHook;

    /// <inheritdoc cref="PreDraw" />
    [PublicAPI]
    public static event PreDrawDelegate PreDrawHook;

    #endregion

    #region Methods

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        EditSpawnRateHook?.Invoke(player, ref spawnRate, ref maxSpawns);
    }

    public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
    {
        return StrikeNpcHook?.Invoke(npc, ref damage, defense, ref knockback, hitDirection, ref crit) ?? base.StrikeNPC(npc, ref damage, defense, ref knockback, hitDirection, ref crit);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return PreDrawHook?.Invoke(npc, spriteBatch, screenPos, drawColor) ?? base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }

    #endregion
}