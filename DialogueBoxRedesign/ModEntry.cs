using System.Linq;
using System.Reflection;
using DialogueBoxRedesign.Patching;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace DialogueBoxRedesign;

/// <summary> The mod entry class loaded by SMAPI. </summary>
public class ModEntry : Mod
{
    public static IMonitor ModMonitor = null!;
    public static IModHelper ModHelper = null!;
    public static ModConfig Config = null!;

    public static Texture2D? GradientSample;

    internal static IHDPortraitsAPI? HdPortraitsApi;

    /// <summary> The mod entry point, called after the mod is first loaded. </summary>
    /// <param name="helper"> Provides simplified APIs for writing mods. </param>
    public override void Entry(IModHelper helper)
    {
        ModMonitor = Monitor;
        ModHelper = Helper;

        Config = Helper.ReadConfig<ModConfig>();

        PrepareAssets();

        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnFirstSaveLoaded;
    }

    public void SaveConfig(ModConfig newConfig)
    {
        Config = newConfig;
        Helper.WriteConfig(newConfig);
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"))
        {
            e.Edit(asset =>
            {
                var editor = asset.AsImage();

                Texture2D sourceImage;

                try
                {
                    sourceImage = Helper.ModContent.Load<Texture2D>("assets/friendshipJewel.png");
                }
                catch (Microsoft.Xna.Framework.Content.ContentLoadException)
                {
                    return;
                }

                editor.PatchImage(sourceImage, new Rectangle(0, 0, 44, 55), new Rectangle(140, 532, 44, 55));
                editor.PatchImage(sourceImage, new Rectangle(44, 0, 11, 11), new Rectangle(269, 495, 11, 11));
            });
        }
    }

    private void PrepareAssets()
    {
        GradientSample = Helper.ModContent.Load<Texture2D>("assets/gradientSample.png");
    }

    private static readonly MethodBase[] PatchedMethods =
    [
        AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.drawPortrait)),
        AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.drawBox)),
        AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.draw), new[] { typeof(SpriteBatch) })
    ];

    private void ApplyHarmonyPatches()
    {
        var harmony = new Harmony(ModManifest.UniqueID);

        harmony.Patch(
            PatchedMethods[0],
            new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.DrawPortrait))
        );

        harmony.Patch(
            PatchedMethods[1],
            new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.DrawBox))
        );

        harmony.Patch(
            PatchedMethods[2],
            new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Draw))
        );
    }

    private void OnFirstSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        Helper.Events.GameLoop.SaveLoaded -= OnFirstSaveLoaded;
        LogPatchConflicts();
    }

    /// <summary>
    /// mods affecting dialogue box/portraits seem to be common; this helps debugging
    /// </summary>
    private void LogPatchConflicts()
    {
        foreach (var method in PatchedMethods)
        {
            var patchInfo = Harmony.GetPatchInfo(method);
            if (patchInfo == null) continue;

            var otherOwners = patchInfo.Prefixes
                .Concat(patchInfo.Postfixes)
                .Concat(patchInfo.Transpilers)
                .Concat(patchInfo.Finalizers)
                .Select(patch => patch.owner)
                .Where(owner => owner != ModManifest.UniqueID)
                .Distinct()
                .ToList();

            if (otherOwners.Count == 0) continue;

            var ownerNames = otherOwners
                .Select(owner => Helper.ModRegistry.Get(owner)?.Manifest.Name ?? owner);
            Monitor.Log(
                $"Another mod also patches {method.DeclaringType?.Name}.{method.Name}: {string.Join(", ", ownerNames)}. " +
                "This may cause visual issues or conflicts with this mod's functionality.",
                LogLevel.Warn);
        }
    }

    /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        ApplyHarmonyPatches();
        ModConfig.SetUpModConfigMenu(Config, this);

        if (Helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits"))
        {
            HdPortraitsApi = Helper.ModRegistry.GetApi<IHDPortraitsAPI>("tlitookilakin.HDPortraits");
        }
    }
}