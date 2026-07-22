using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Mods;

namespace EventBlackBars;

/// <summary> The mod entry class loaded by SMAPI. </summary>
public class ModEntry : Mod
{
    public static ModEntry Instance = null!;
    public static IMonitor ModMonitor = null!;
    public static Texture2D BlackRectangle = null!;
    public static GraphicsDevice GraphicsDevice = null!;

    public static bool RenderBars;
    
    private static float _barHeight;
    private bool _barsMovingIn;
    private bool _barsMovingOut;
    private bool _wasInEvent;
    private ModConfig _config = null!;

    /// <summary> The mod entry point, called after the mod is first loaded. </summary>
    /// <param name="helper"> Provides simplified APIs for writing mods. </param>
    public override void Entry(IModHelper helper)
    {
        Instance = this;
        ModMonitor = Monitor;

        helper.Events.Display.RenderedStep += OnRenderedStep;
        helper.Events.Display.WindowResized += OnWindowResized;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;

        _config = Helper.ReadConfig<ModConfig>();
        GraphicsDevice = Game1.graphics.GraphicsDevice;
        PrepareAssets(GraphicsDevice);
    }

    public void SaveConfig(ModConfig newConfig)
    {
        _config = newConfig;
        Helper.WriteConfig(newConfig);
    }

    /// <summary>
    /// "Move" bars in direction.
    /// </summary>
    public void StartMovingBars(Direction direction)
    {
        // don't start to move out the bars if they are not moved in
        if (direction == Direction.MoveOut && _barHeight <= 0) return;

        RenderBars = true;

        if (_config.MoveBarsInSmoothly)
        {
            _barsMovingIn = direction == Direction.MoveIn;
            _barsMovingOut = direction == Direction.MoveOut;
        }
        else
        {
            _barHeight = direction == Direction.MoveIn ? GetMaxBarHeight(GraphicsDevice) : 0;
            RenderBars = direction == Direction.MoveIn;
        }
    }

    /// <summary>
    /// Prepare a black square for the bars.
    /// </summary>
    private void PrepareAssets(GraphicsDevice graphicsDevice)
    {
        BlackRectangle = new Texture2D(graphicsDevice, 1, 1);
        BlackRectangle.SetData(new[] { Color.Black });
    }

    /// <summary>
    /// Draw the bars. This handles drawing the bars sliding out after an event is done
    /// (the method we hook into for Event stops firing immediately after the event is done)
    /// </summary>
    private void OnRenderedStep(object? sender, RenderedStepEventArgs e)
    {
        if (e.Step != RenderSteps.World || !RenderBars || Game1.CurrentEvent != null) return;
        DrawBars(e.SpriteBatch);
    }

    public static void DrawBars(SpriteBatch b)
    {
        var viewport = Game1.graphics.GraphicsDevice.Viewport;

        // Top bar
        b.Draw(BlackRectangle, new Vector2(0, 0), null,
            Color.White, 0f, Vector2.Zero, new Vector2(viewport.Width, _barHeight),
            SpriteEffects.None, 0.0f);

        // Bottom bar
        b.Draw(BlackRectangle, new Vector2(0, viewport.Height - _barHeight), null,
            Color.White, 0f, Vector2.Zero, new Vector2(viewport.Width, _barHeight),
            SpriteEffects.None, 0.0f);
    }

    /// <summary>
    /// Smoothly "move" the bars when required.
    /// </summary>
    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        var inEvent = Game1.eventUp;
        if (_wasInEvent && !inEvent)
            StartMovingBars(Direction.MoveOut);
        _wasInEvent = inEvent;

        if (!_barsMovingIn && !_barsMovingOut || !_config.MoveBarsInSmoothly) return;

        var maxBarHeight = GetMaxBarHeight(GraphicsDevice);
        var desiredBarHeight = _barsMovingIn ? maxBarHeight : 0;
        var speed = maxBarHeight / 30f;

        // Quit resizing the bars when the desired height is about to be reached.
        if (Math.Abs(_barHeight - desiredBarHeight) <= speed)
        {
            _barsMovingIn = _barsMovingOut = false;
            _barHeight = desiredBarHeight;

            RenderBars = desiredBarHeight != 0;

            return;
        }

        // Gradually change the bar height.
        _barHeight += desiredBarHeight > _barHeight ? speed : -speed;
    }

    /// <summary>
    /// Adjust the bar height when resized.
    /// </summary>
    private void OnWindowResized(object? sender, WindowResizedEventArgs e)
    {
        if (_barsMovingIn || _barsMovingOut || _barHeight <= 0) return;

        _barHeight = GetMaxBarHeight(GraphicsDevice);
    }

    private void ApplyHarmonyPatches()
    {
        var harmony = new Harmony(ModManifest.UniqueID);

        harmony.Patch(
            AccessTools.Method(typeof(Event), nameof(Event.drawAfterMap)),
            prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.DrawAfterMap))
        );

        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), nameof(GameLocation.startEvent)),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.EventStart))
        );
    }

    /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        ApplyHarmonyPatches();
        ModConfig.SetUpModConfigMenu(_config, this);
    }

    /// <summary>
    /// Given a GraphicsDevice, return the maximum size each bar should be.
    /// </summary>
    private int GetMaxBarHeight(GraphicsDevice graphicsDevice)
    {
        return (int)Math.Round(graphicsDevice.Viewport.Height *
                               MathHelper.Clamp((float)_config.BarHeightPercentage / 100f, 0f, 1f));
    }
}

public enum Direction
{
    MoveIn,
    MoveOut
}