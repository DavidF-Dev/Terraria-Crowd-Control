using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CrowdControlMod.Features;

public sealed class TheSuitedBirdEggFeature : IFeature
{
    #region Methods

    public void SessionStarted()
    {
    }

    public void SessionStopped()
    {
    }

    public void Dispose()
    {
    }

    #endregion

    #region Nested Types

    private sealed class TheSuitedBirdSystem : ModSystem
    {
        #region Static Methods

        private static void OnDrawSunAndMoon(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Color moonColor, Color sunColor, float tempMushroomInfluence)
        {
            /*if (!SteamUtils.IsTheSuitedBird)
            {
                orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
                return;
            }*/

            TheSuitedBirdSystem instance = ModContent.GetInstance<TheSuitedBirdSystem>();
            if (!instance.IsEventActive())
            {
                orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
                return;
            }

            var newTexture = ModContent.Request<Texture2D>(instance.Mod.Name + "/Assets/Textures/BirdMoon");
            var oldTexture = TextureAssets.Sun;
            TextureAssets.Sun = newTexture;
            try
            {
                orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
            }
            finally
            {
                TextureAssets.Sun = oldTexture;
            }
        }

        #endregion

        #region Fields

        private bool _isActive;
        private bool _wasDay;

        #endregion

        #region Methods

        public override void Load()
        {
            On_Main.DrawSunAndMoon += OnDrawSunAndMoon;
        }

        public override void ClearWorld()
        {
            _isActive = false;
            _wasDay = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag.Add("IsActive", _isActive);
            tag.Add("WasDay", _wasDay);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _isActive = tag.ContainsKey("IsActive") && tag.GetBool("IsActive");
            _wasDay = tag.ContainsKey("WasDay") && tag.GetBool("WasDay");
        }

        public override void PostUpdateTime()
        {
            if (Main.gameMenu)
            {
                // Ignore
                return;
            }

            bool isDay = Main.dayTime;
            if (isDay && !_wasDay) // -> Day
            {
                _isActive = Main._rand.NextBool(10);
#if DEBUG
                _isActive = true;
#endif
                if (_isActive)
                {
                    Main.NewText("The Bird Sun is rising...");
                }
            }
            else if (!isDay && _wasDay) // -> Night
            {
                _isActive = false;
            }

            _wasDay = isDay;
        }

        public bool IsEventActive()
        {
            return _isActive;
        }

        #endregion
    }

    #endregion
}