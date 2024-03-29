﻿using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace EventBlackBars
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        public static ModEntry Instance;
        public static IMonitor ModMonitor;

        private bool _barsMovingIn;
        private bool _barsMovingOut;
        private bool _renderBars;
        
        private Texture2D _blackRectangle;
        private GraphicsDevice _graphicsDevice;
        public ModConfig Config;

        private float _barHeight;

        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Instance = this;

            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.WindowResized += OnWindowResized;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            
            Config = Helper.ReadConfig<ModConfig>();
            _graphicsDevice = Game1.graphics.GraphicsDevice;
            PrepareAssets(_graphicsDevice);
        }

        public void SaveConfig(ModConfig newConfig)
        {
            Config = newConfig;
            Helper.WriteConfig(newConfig);
        }

        /// <summary>
        /// "Move" bars in direction.
        /// </summary>
        public void StartMovingBars(Direction direction)
        {
            // don't start to move out the bars if they are not moved in
            if(direction == Direction.MoveOut && _barHeight <= 0) return;
            
            _renderBars = true;
            
            if (Config.MoveBarsInSmoothly)
            {
                _barHeight = direction == Direction.MoveIn ? 0 : GetMaxBarHeight(_graphicsDevice);

                _barsMovingIn = direction == Direction.MoveIn;
                _barsMovingOut = direction == Direction.MoveOut;
            }
            else
            {
                _barHeight = direction == Direction.MoveIn ? GetMaxBarHeight(_graphicsDevice) : 0;
            }
        }
        
        /// <summary>
        /// Prepare a black square for the bars.
        /// </summary>
        private void PrepareAssets(GraphicsDevice graphicsDevice)
        {
            _blackRectangle = new Texture2D(graphicsDevice, 1, 1);
            _blackRectangle.SetData(new [] { Color.Black });
        }

        /// <summary>
        /// Draw the bars.
        /// </summary>
        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!_renderBars) return;
            
            var viewportWidth = _graphicsDevice.Viewport.Width;
            var viewportHeight = _graphicsDevice.Viewport.Height;
            
            // Top bar
            e.SpriteBatch.Draw(_blackRectangle, new Vector2(0, 0), null,
                Color.White, 0f, Vector2.Zero, new Vector2(viewportWidth, _barHeight),
                SpriteEffects.None, 0f);
            
            // Bottom bar
            e.SpriteBatch.Draw(_blackRectangle, new Vector2(0, viewportHeight - _barHeight), null,
                Color.White, 0f, Vector2.Zero, new Vector2(viewportWidth, _barHeight),
                SpriteEffects.None, 0f);
        }
        
        /// <summary>
        /// Smoothly "move" the bars when required.
        /// </summary>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_barsMovingIn && !_barsMovingOut || !Config.MoveBarsInSmoothly) return;

            var maxBarHeight = GetMaxBarHeight(_graphicsDevice);
            var desiredBarHeight = _barsMovingIn ? maxBarHeight : 0;
            const float speed = 1f;
            
            // Quit resizing the bars when the desired height is about to be reached.
            if (Math.Abs(_barHeight - desiredBarHeight) <= 1f)
            {
                _barsMovingIn = _barsMovingOut = false;
                _barHeight = desiredBarHeight;

                _renderBars = desiredBarHeight != 0;
                
                return;
            }
            
            // Gradually change the bar height.
            _barHeight = desiredBarHeight > _barHeight ? _barHeight += speed : _barHeight -= speed;
        }
        
        /// <summary>
        /// Adjust the bar height when resized.
        /// </summary>
        private void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            if(_barsMovingIn || _barsMovingOut || _barHeight <= 0) return;

            _barHeight = GetMaxBarHeight(_graphicsDevice);
        }
        
        private void ApplyHarmonyPatches()
        {
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                AccessTools.Method(typeof(Event), nameof(Event.exitEvent)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.EventEnd))
            );
            
            harmony.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.startEvent)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.EventStart))
            );
        }

        /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ApplyHarmonyPatches();
            ModConfig.SetUpModConfigMenu(Config, this);
        }

        /// <summary>
        /// Given a GraphicsDevice, return the maximum size each bar should be.
        /// </summary>
        private int GetMaxBarHeight(GraphicsDevice graphicsDevice)
        {
            return Convert.ToInt16(graphicsDevice.Viewport.Height *
                                   MathHelper.Clamp((float)Config.BarHeightPercentage / 100f, 0f, 1f));
        }
    }

    public enum Direction
    {
        MoveIn,
        MoveOut
    }
}