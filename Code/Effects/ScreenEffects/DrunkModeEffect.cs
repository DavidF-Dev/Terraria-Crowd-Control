using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects.Interfaces;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Shaders;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Induces a drunken feeling by swaying the screen using shaders for a short duration.
/// </summary>
public sealed class DrunkModeEffect : CrowdControlEffect, IMusicEffect
{
    #region Static Fields and Constants

    private const float SineIntensity = 0.05f;
    private const float GlitchIntensity = 24f;

    private static readonly short[] FoodIds =
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
        ItemID.GingerbreadCookie, ItemID.SugarCookie, ItemID.Marshmallow, ItemID.PadThai, ItemID.Sake
    };

    #endregion

    #region Static Methods

    private static void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (SteamUtils.IsThatGrayson)
        {
            // Random food name
            var nameLine = tooltips.FirstOrDefault(x => x.Name.Equals("ItemName"));
            if (nameLine != null)
            {
                nameLine.Text = Lang.GetItemNameValue(FoodIds[item.type % FoodIds.Length]);
            }
        }

        // Shuffle tooltip contents periodically
        var rng = new FastRandom(Main.GameUpdateCount / 30);
        foreach (var tooltipLine in tooltips)
        {
            if (rng.Next(3) == 0)
            {
                tooltipLine.OverrideColor = Main.DiscoColor;
            }

            if (!SteamUtils.IsThatGrayson)
            {
                tooltipLine.Text = string.Join(' ', tooltipLine.Text.Split(' ').OrderBy(x => x.GetHashCode()));
            }
        }
        
        // Shuffle tooltip order
        var shuffled = tooltips.OrderBy(x => x.Text.GetHashCode()).ToList();
        tooltips.Clear();
        tooltips.AddRange(shuffled);
    }

    private static bool PreDrawInInventory(Item item, SpriteBatch spritebatch, Vector2 position, Rectangle frame, Color drawColour, Color itemColour, Vector2 origin, float scale)
    {
        var id = FoodIds[item.type % FoodIds.Length];
        if (!TextureAssets.Item[id].IsLoaded)
        {
            // Ensure loaded
            Main.instance.LoadItem(id);
        }

        // Draw food in inventory
        Main.EntitySpriteDraw(
            TextureAssets.Item[id].Value,
            position,
            frame,
            drawColour,
            0f,
            origin,
            scale,
            SpriteEffects.None,
            0);

        return false;
    }

    #endregion

    #region Fields

    private readonly ScreenShader _sineShader;
    private readonly ScreenShader _glitchShader;

    #endregion

    #region Constructors

    public DrunkModeEffect(float duration) : base(EffectID.DrunkMode, duration, EffectSeverity.Negative)
    {
        _sineShader = new ScreenShader("SH_Sine", "CreateSine", $"{Id}_1");
        _glitchShader = new ScreenShader("SH_Glitch", "CreateGlitch", $"{Id}_2");
    }

    #endregion

    #region Properties

    int IMusicEffect.MusicId => MusicID.Mushrooms;

    int IMusicEffect.MusicPriority => 10;

    public override EffectCategory Category => EffectCategory.Screen;

    protected override int StartEmote => EmoteID.ItemBeer;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Enable the shader effects
        var sineShaderData = _sineShader.Enable();
        var glitchShaderData = _glitchShader.Enable();
        if (sineShaderData == null || glitchShaderData == null)
        {
            // Failed to enable one of the shader effects, so we failed
            _sineShader.Disable();
            _glitchShader.Disable();
            return CrowdControlResponseStatus.Failure;
        }

        // Set the intensity of the shader effects
        sineShaderData.UseIntensity(SineIntensity);
        glitchShaderData.UseIntensity(GlitchIntensity);

        CrowdControlItem.ModifyTooltipsHook += ModifyTooltips;
        if (SteamUtils.IsThatGrayson)
        {
            CrowdControlItem.PreDrawInInventoryHook += PreDrawInInventory;
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _sineShader.Disable();
        _glitchShader.Disable();

        CrowdControlItem.ModifyTooltipsHook -= ModifyTooltips;
        if (SteamUtils.IsThatGrayson)
        {
            CrowdControlItem.PreDrawInInventoryHook -= PreDrawInInventory;
        }
    }

    protected override void OnUpdate(float delta)
    {
        // Set the intensity of the shader effects
        _sineShader.GetShader()?.UseIntensity(SineIntensity);
        _glitchShader.GetShader()?.UseIntensity(GlitchIntensity);
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var item = SteamUtils.IsThatGrayson ? ItemID.ChefHat : ItemID.Ale;
        var locKey = SteamUtils.IsThatGrayson ? $"{Id}_egg" : Id;
        TerrariaUtils.WriteEffectMessage(item, LangUtils.GetEffectStartText(locKey, viewerString, playerString, durationString), Severity);
    }

    #endregion
}