using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
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

    // ReSharper disable once UnusedType.Local
    private sealed class TheSuitedBirdSystem : ModSystem
    {
        #region Static Methods

        public static bool IsEventActive()
        {
            return ModContent.GetInstance<TheSuitedBirdSystem>()._isActive;
        }

        private static void OnDrawSunAndMoon(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Color moonColor, Color sunColor, float tempMushroomInfluence)
        {
            if (Main.gameMenu || !NetUtils.IsSinglePlayer || !IsEventActive())
            {
                orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
                return;
            }

            var newTexture = ModContent.Request<Texture2D>(CrowdControlMod.GetInstance().Name + "/Assets/Textures/BirdMoon");
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
        private int _counter;

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
            _counter = 0;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag.Add("IsActive", _isActive);
            tag.Add("WasDay", _wasDay);
            tag.Add("Counter", _counter);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _isActive = tag.ContainsKey("IsActive") && tag.GetBool("IsActive");
            _wasDay = tag.ContainsKey("WasDay") && tag.GetBool("WasDay");
            _counter = tag.ContainsKey("Counter") ? tag.GetInt("Counter") : 0;
        }

        public override void PostUpdateTime()
        {
            if (Main.gameMenu || !NetUtils.IsSinglePlayer || !SteamUtils.IsTheSuitedBird)
            {
                // Ignore
                _isActive = false;
                return;
            }

            bool isDay = Main.dayTime;
            if (isDay && !_wasDay) // -> Day
            {
                if (_counter <= 0)
                {
                    _counter = Main.rand.Next(1, 3);
                }

#if DEBUG
                _counter = 1;
#endif

                _counter--;
                if (_counter == 0)
                {
                    _counter = Main.rand.Next(3, 8);
                    _isActive = true;
                    Main.LocalPlayer.Emote(EmoteID.CritterBird);
                    Main.NewText("The Bird Sun is rising...", new Color(50, 255, 130));
                }
            }
            else if (!isDay && _wasDay) // -> Night
            {
                _isActive = false;
            }

            _wasDay = isDay;
        }

        #endregion
    }

    // ReSharper disable once UnusedType.Local
    private sealed class TheSuitedBirdPlayer : ModPlayer
    {
        #region Methods

        public override void PostUpdateEquips()
        {
            if (!Main.gameMenu && NetUtils.IsSinglePlayer && TheSuitedBirdSystem.IsEventActive() && Main.GameUpdateCount % 30 == 0 && Main.rand.NextBool(Player.ZoneForest ? 25 : 40))
            {
                int birdN = Main.rand.Next(1, 4);
                if (Player.ZoneForest && Main.rand.NextBool(6))
                {
                    birdN += Main.rand.Next(1, 4);
                }

                for (int i = 0; i < birdN; i++)
                {
                    int x = (int)(Player.position.X + Main.rand.Next(Player.width));
                    int y = (int)(Player.position.Y + Main.rand.Next(Player.height));
                    NPC birdNPC = NPC.NewNPCDirect(null, x, y, NPCID.BirdBlue);
                    birdNPC.scale = 0.5f + Main.rand.NextFloat();
                }

                SoundEngine.PlaySound(SoundID.Bird with {Volume = 0.1f}, Player.position);

                int bubbleN = Main.rand.Next(25, 30);
                for (int i = 0; i < bubbleN; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.BlueCrystalShard);
                }
            }
        }

        #endregion
    }

    // ReSharper disable once UnusedType.Local
    private sealed class TheSuitedBirdNPC : GlobalNPC
    {
        #region Methods

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.type == NPCID.BirdBlue;
        }

        public override void AI(NPC npc)
        {
            if (!Main.gameMenu && NetUtils.IsSinglePlayer && TheSuitedBirdSystem.IsEventActive())
            {
                Lighting.AddLight(npc.Center, TorchID.UltraBright);
            }
        }

        #endregion
    }

    // ReSharper disable once UnusedType.Local
    private sealed class TheSuitedBirdScene : ModSceneEffect
    {
        #region Properties

        public override int Music => MusicID.OtherworldlyDay;

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

        #endregion

        #region Methods

        public override bool IsSceneEffectActive(Player player)
        {
            return !Main.gameMenu && NetUtils.IsSinglePlayer && TheSuitedBirdSystem.IsEventActive() && player.ZoneForest;
        }

        #endregion
    }

    #endregion
}