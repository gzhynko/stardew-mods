using System;
using System.Collections.Generic;
using System.Linq;
using AnimalsNeedWater.Patching;
using AnimalsNeedWater.Types;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace AnimalsNeedWater
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        public static IMonitor ModMonitor;
        public static IModHelper ModHelper;
        public static ModConfig Config;
        public static IManifest Manifest;
        
        public static Dictionary<string, TroughPlacementProfile> CurrentTroughPlacementProfiles;
        
        public static List<FarmAnimal> AnimalsLeftThirstyYesterday;

        // Initialize a dictionary to group buildings by their parent location
        public List<Building> AnimalBuildings;
        public IEnumerable<IGrouping<GameLocation, Building>> AnimalBuildingGroups;

        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;
            Manifest = ModManifest;

            AnimalsLeftThirstyYesterday = new List<FarmAnimal>();
            AnimalBuildings = new List<Building>();

            Config = Helper.ReadConfig<ModConfig>();
            HandleObsoleteConfigProperties();
            
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saved += HandleDayUpdate;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;

            InitTroughPlacementProfiles();
        }

        private void HandleObsoleteConfigProperties()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (Config.WateringSystemInDeluxeBuildings != null)
                Config.UseWateringSystems = Config.WateringSystemInDeluxeBuildings.Value;
            SaveConfig(Config);
#pragma warning restore CS0618 // Type or member is obsolete
        }
        
        public void SaveConfig(ModConfig newConfig)
        {
            Config = newConfig;
            Helper.WriteConfig(newConfig);
        }

        /// <summary> Get ANW's API </summary>
        /// <returns> API instance </returns>
        public override object GetApi()
        {
            return new API();
        }
        
        public static void SendTroughWateredMessage(string building)
        {
            SendMessageToSelf(new TroughWateredMessage(building), "TroughWateredMessage");
        }
        
        /// <summary> Look for known mods that modify coop/barn interiors and load the matching profile(s) (if any). </summary>
        private void InitTroughPlacementProfiles()
        {
            try
            {
                TroughPlacementProfiles.LoadProfiles(Helper);
            }
            catch (Exception e)
            {
                Monitor.Log($"Error while loading trough placement profiles: {e}", LogLevel.Warn);
            }

            if (TroughPlacementProfiles.DefaultProfile == null)
            {
                Monitor.Log("The default trough placement profile was not loaded. Animals Need Water will not work correctly. Make sure all files in the Animals Need Water/assets folder are in place.", LogLevel.Error);
                return;
            }

            CurrentTroughPlacementProfiles = new();
            foreach (var modInfo in Helper.ModRegistry.GetAll())
            {
                var profile = TroughPlacementProfiles.GetProfileByUniqueId(modInfo.Manifest.UniqueID);
                if (profile == null) continue;
                
                if (profile.TargetBuildings == null)
                {
                    Monitor.Log($"Skipping trough placement profile for mod {profile.ModUniqueId} because targetBuildings is not specified.", LogLevel.Warn);
                    continue;
                }

                foreach (var profileTargetBuilding in profile.TargetBuildings)
                {
                    if (!CurrentTroughPlacementProfiles.TryAdd(profileTargetBuilding, profile))
                    {
                        Monitor.Log(
                            $"Warning: target building {profileTargetBuilding} of profile for mod " +
                            $"{profile.ModUniqueId} is already covered by a previously loaded profile for mod " +
                            $"{CurrentTroughPlacementProfiles[profileTargetBuilding]}. Skipping this building."
                            , LogLevel.Warn);
                    }
                }
            }

            foreach (var defaultBuilding in TroughPlacementProfiles.DefaultProfile.TargetBuildings)
            {
                if (!CurrentTroughPlacementProfiles.ContainsKey(defaultBuilding))
                    CurrentTroughPlacementProfiles[defaultBuilding] = TroughPlacementProfiles.DefaultProfile;
            }
            
            Monitor.Log("Using the following trough placement profiles:", LogLevel.Info);
            foreach (var (buildingName, profile) in CurrentTroughPlacementProfiles)
            {
                Monitor.Log($"  {buildingName}: {(profile == TroughPlacementProfiles.DefaultProfile ? "default" : profile.ModUniqueId)}", LogLevel.Info);
            }
        }

        /// <summary> Empty water troughs in animal houses. </summary>
        private void EmptyWaterTroughs()
        {
            ModData.BuildingsWithWateredTrough = new List<string>();
            
            foreach (Building building in AnimalBuildings)
            {
                if (Config.UseWateringSystems)
                {
                    // check if this building has a watering system
                    if (CurrentTroughPlacementProfiles.TryGetValue(building.buildingType.Value.ToLower(), out var profile) && profile.BuildingHasWateringSystem(building.buildingType.Value.ToLower()))
                    {
                        if (!ModData.BuildingsWithWateredTrough.Contains(building.GetIndoorsName().ToLower()))
                            ModData.BuildingsWithWateredTrough.Add(building.GetIndoorsName().ToLower());
                        
                        // do not proceed with emptying water troughs
                        continue;
                    } 
                }

                EmptyWaterTroughsInBuilding(building);
            }
        }
        
        /// <summary>
        /// Empty water troughs in the specified animal house.
        /// </summary>
        /// <param name="building"></param>
        private void EmptyWaterTroughsInBuilding(Building building)
        {
            var buildingName = building.buildingType.Value;
            GameLocation indoorsGameLocation = building.indoors.Value;

            // if no animals live here, do not empty the water trough
            int animalCount = building.GetParentLocation().getAllFarmAnimals().Count(animal => animal.home.GetIndoorsName().Equals(indoorsGameLocation.NameOrUniqueName, StringComparison.CurrentCultureIgnoreCase));
            if (animalCount == 0)
            {
                ModData.BuildingsWithWateredTrough.Add(building.GetIndoorsName().ToLower());
                return;
            }

            foreach (var (profileBuildingName, profile) in CurrentTroughPlacementProfiles)
            {
                if (!buildingName.Equals(profileBuildingName, StringComparison.CurrentCultureIgnoreCase)) continue;

                switch (buildingName.ToLower())
                {
                    case "coop":
                        ChangeCoopTexture(building, true);
                        break;
                    case "big coop":
                        ChangeBigCoopTexture(building, true);
                        break;
                }

                var profileTroughTiles = profile.GetPlacementForBuildingName(profileBuildingName).TroughTiles;
                
                foreach (TroughTile tile in profileTroughTiles)
                {
                    indoorsGameLocation.removeTile(tile.TileX, tile.TileY, tile.Layer);
                }

                Layer buildingsLayer = indoorsGameLocation.Map.GetLayer("Buildings");
                Layer frontLayer = indoorsGameLocation.Map.GetLayer("Front");
                TileSheet tilesheet = indoorsGameLocation.Map.GetTileSheet("z_waterTroughTilesheet");

                foreach (TroughTile tile in profileTroughTiles)
                {
                    if (tile.Layer.Equals("Buildings", StringComparison.OrdinalIgnoreCase))
                        buildingsLayer.Tiles[tile.TileX, tile.TileY] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: tile.EmptyIndex);
                    else if (tile.Layer.Equals("Front", StringComparison.OrdinalIgnoreCase))
                        frontLayer.Tiles[tile.TileX, tile.TileY] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: tile.EmptyIndex);
                }
            }
        }

        private List<FarmAnimal> FindThirstyAnimals()
        { 
            List<FarmAnimal> animalsLeftThirsty = new List<FarmAnimal>();

            foreach (var locationGroup in AnimalBuildingGroups)
            {
                GameLocation parentLocation = locationGroup.Key;
                List<Building> buildingsInLocation = locationGroup.ToList();

                // Look for all animals inside buildings and check whether their troughs are watered.
                foreach (Building building in buildingsInLocation)
                {
                    foreach (var animal in ((AnimalHouse) building.indoors.Value).animals.Values
                        .Where(animal =>
                            ModData.BuildingsWithWateredTrough.Contains(animal.home.GetIndoorsName().ToLower()) == false &&
                            ModData.FullAnimals.Contains(animal) == false)
                        .Where(animal =>
                            !animalsLeftThirsty.Contains(animal)))
                    {
                        animal.friendshipTowardFarmer.Value -= Math.Abs(Config.NegativeFriendshipPointsForNotWateredTrough);
                        animalsLeftThirsty.Add(animal);
                    }
                }

                // Check for animals outside their buildings as well.
                foreach (var animal in parentLocation.animals.Values)
                {
                    if ((ModData.BuildingsWithWateredTrough.Contains(animal.home.GetIndoorsName().ToLower()) ||
                         ModData.FullAnimals.Contains(animal)) &&
                        animal.home.animalDoorOpen.Value) continue;
                
                    if (animalsLeftThirsty.Contains(animal)) continue;
                    
                    animal.friendshipTowardFarmer.Value -= Math.Abs(Config.NegativeFriendshipPointsForNotWateredTrough);
                    animalsLeftThirsty.Add(animal);
                }
            }

            return animalsLeftThirsty;
        }

        /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var harmony = new Harmony(ModManifest.UniqueID);
            
            ModMonitor.VerboseLog("Patching GameLocation.performToolAction.");
            harmony.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performToolAction)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.GameLocationToolAction))
            );
            
            ModMonitor.VerboseLog("Patching FarmAnimal.dayUpdate.");
            harmony.Patch(
                AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.AnimalDayUpdate))
            );

            ModMonitor.VerboseLog("Patching FarmAnimal.behaviors.");
            harmony.Patch(
                AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.behaviors)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.AnimalBehaviors))
            );

            ModMonitor.VerboseLog("Patching Game1.OnLocationChanged.");
            harmony.Patch(
                AccessTools.Method(typeof(Game1), nameof(Game1.OnLocationChanged)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.OnLocationChanged))
            );
            
            ModMonitor.VerboseLog("Done patching.");
            
            ModConfig.SetUpModConfigMenu(Config, this);
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModManifest.UniqueID || e.Type != "TroughWateredMessage") return;
            
            TroughWateredMessage message = e.ReadAs<TroughWateredMessage>();

            ModData.BuildingsWithWateredTrough.Add(message.BuildingUniqueName.ToLower());
            
            string locationName = message.BuildingUniqueName;
            Building building = Game1.getLocationFromName(locationName).GetContainingBuilding();

            switch (building.buildingType.Value.ToLower())
            {
                case "coop":
                    ChangeCoopTexture(building, false);
                    break;
                case "big coop":
                    ChangeBigCoopTexture(building, false);
                    break;
            }
                
            if (string.Equals(Game1.currentLocation.NameOrUniqueName, message.BuildingUniqueName,
                StringComparison.CurrentCultureIgnoreCase))
            {
                HarmonyPatchExecutors.UpdateBuildingTroughs(building);
            }
        }

        /// <summary> Raised after the save is loaded. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            GetAnimalBuildings();
            CheckHomeStatus();
            LoadNewTileSheets();
            PlaceWateringSystems();

            HandleDayStart();
        }
        
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            AnimalsLeftThirstyYesterday = FindThirstyAnimals();
        }

        private void HandleDayStart()
        {
            PrepareForNewDay();
        }
        
        /// <summary> Looks for animals left thirsty and notifies player of them. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void HandleDayUpdate(object sender, SavedEventArgs e)
        {
            PrepareForNewDay();
            // If enabled in config: notify player of animals left thirsty, if any.
            if (AnimalsLeftThirstyYesterday.Any() && Config.ShowAnimalsLeftThirstyMessage)
            {
                switch (AnimalsLeftThirstyYesterday.Count)
                {
                    case 1 when Helper.ModRegistry.IsLoaded("Paritee.GenderNeutralFarmAnimals"):
                        Game1.showGlobalMessage(ModHelper.Translation.Get(
                            "AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_UnknownGender",
                            new { firstAnimalName = AnimalsLeftThirstyYesterday[0].displayName }));
                        break;
                    case 1 when AnimalsLeftThirstyYesterday[0].isMale():
                        Game1.showGlobalMessage(ModHelper.Translation.Get(
                            "AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_Male",
                            new { firstAnimalName = AnimalsLeftThirstyYesterday[0].displayName }));
                        break;
                    case 1 when !AnimalsLeftThirstyYesterday[0].isMale():
                        Game1.showGlobalMessage(ModHelper.Translation.Get(
                            "AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_Female",
                            new { firstAnimalName = AnimalsLeftThirstyYesterday[0].displayName }));
                        break;
                    case 2:
                        Game1.showGlobalMessage(ModHelper.Translation.Get(
                            "AnimalsLeftWithoutWaterYesterday.globalMessage.twoAnimals",
                            new
                            {
                                firstAnimalName = AnimalsLeftThirstyYesterday[0].displayName,
                                secondAnimalName = AnimalsLeftThirstyYesterday[1].displayName
                            }));
                        break;
                    case 3:
                        Game1.showGlobalMessage(ModHelper.Translation.Get(
                            "AnimalsLeftWithoutWaterYesterday.globalMessage.threeAnimals",
                            new
                            {
                                firstAnimalName = AnimalsLeftThirstyYesterday[0].displayName,
                                secondAnimalName = AnimalsLeftThirstyYesterday[1].displayName,
                                thirdAnimalName = AnimalsLeftThirstyYesterday[2].displayName
                            }));
                        break;
                    default:
                        Game1.showGlobalMessage(ModHelper.Translation.Get(
                            "AnimalsLeftWithoutWaterYesterday.globalMessage.multipleAnimals",
                            new
                            {
                                firstAnimalName = AnimalsLeftThirstyYesterday[0].displayName,
                                secondAnimalName = AnimalsLeftThirstyYesterday[1].displayName,
                                thirdAnimalName = AnimalsLeftThirstyYesterday[2].displayName,
                                totalAmountExcludingFirstThree = AnimalsLeftThirstyYesterday.Count - 3
                            }));
                        break;
                }
            }
        }

        private void PrepareForNewDay()
        {
            LoadNewTileSheets();
            
            ModData.FullAnimals = new List<FarmAnimal>();
            
            // Check whether there is a festival today. If not, empty the troughs.
            if (!Utility.isFestivalDay(Game1.dayOfMonth, Game1.season))
            {
                EmptyWaterTroughs();
            }
            else
            {
                foreach (Building building in AnimalBuildings)
                {
                    var buildingName = building.buildingType.Value;
                    if (GetProfileForBuilding(buildingName) == null)
                        continue;
                    
                    if (!ModData.BuildingsWithWateredTrough.Contains(buildingName.ToLower()))
                        ModData.BuildingsWithWateredTrough.Add(building.GetIndoorsName().ToLower());

                    switch (building.buildingType.Value.ToLower())
                    {
                        case "coop":
                            ChangeCoopTexture(building, false);
                            break;
                        
                        case "big coop":
                            ChangeBigCoopTexture(building, false);
                            break;
                    }
                }
            }
            
            PlaceWateringSystems();
        }

        private void GetAnimalBuildings()
        {
            AnimalBuildings.Clear();
            foreach (GameLocation location in Game1.locations)
            {
                foreach (Building building in location.buildings)
                {
                    if (building.GetIndoors() is AnimalHouse)
                    {
                        AnimalBuildings.Add(building);
                    }
                }
            }

            AnimalBuildingGroups = AnimalBuildings.GroupBy(b => b.GetParentLocation());
        }

        private void CheckHomeStatus()
        {
            bool needToFixAnimals = false;

            foreach (var locationGroup in AnimalBuildingGroups)
            {
                GameLocation parentLocation = locationGroup.Key;

                if (parentLocation.getAllFarmAnimals().Any(animal => animal.home == null))
                {
                    needToFixAnimals = true;
                    break;
                }
            }

            if (needToFixAnimals)
            {
                Utility.fixAllAnimals();
            }
        }

        public static void ChangeBigCoopTexture(Building building, bool empty)
        {
            if (!Config.ReplaceCoopTextureIfTroughIsEmpty) return;

            if (empty)
            {
                building.texture = new Lazy<Texture2D>(() =>
                    ModHelper.ModContent.Load<Texture2D>("assets/Coop2_emptyWaterTrough.png"));
            }
            else
            {
                building.resetTexture();
            }
        }
        
        public static void ChangeCoopTexture(Building building, bool empty)
        {
            if (!Config.ReplaceCoopTextureIfTroughIsEmpty) return;
            
            if (empty)
            {
                building.texture = new Lazy<Texture2D>(() =>
                    ModHelper.ModContent.Load<Texture2D>("assets/Coop_emptyWaterTrough.png"));
            }
            else
            {
                building.resetTexture();
            }
        }
        
#nullable enable
        public static TroughPlacementProfile? GetProfileForBuilding(string buildingName)
        {
            foreach (var (profileBuildingName, profile) in CurrentTroughPlacementProfiles)
            {
                if (buildingName.Equals(profileBuildingName, StringComparison.CurrentCultureIgnoreCase))
                    return profile;
            }
            return null;
        }
#nullable disable

        private void LoadNewTileSheets()
        {
            foreach (Building building in AnimalBuildings)
            {
                var buildingName = building.buildingType.Value;
                var buildingProfile = GetProfileForBuilding(buildingName);
                if (buildingProfile == null)
                    continue;
                
                var indoorsMap = building.indoors.Value.Map;
                
                if (indoorsMap.TileSheets.All(ts => !ts.Id.Equals("z_waterTroughTilesheet")))
                {
                    var tileSheetImageSource = Helper.ModContent
                        .GetInternalAssetName(
                            $"assets/waterTroughTilesheet{(Config.CleanerTroughs ? "_clean" : "")}.png").Name;
                    var tileSheet = new TileSheet(
                        "z_waterTroughTilesheet",
                        indoorsMap,
                        tileSheetImageSource,
                        new Size(160, 16),
                        new Size(16, 16)
                    );

                    indoorsMap.AddTileSheet(tileSheet);
                    indoorsMap.LoadTileSheets(Game1.mapDisplayDevice);
                }
                
                if (!buildingProfile.BuildingHasWateringSystem(buildingName) ||
                    !Config.UseWateringSystems ||
                    indoorsMap.TileSheets.Any(ts => ts.Id.Equals("z_wateringSystemTilesheet"))) continue;
                
                var wateringSystemTilesheetImageSource = Helper.ModContent
                    .GetInternalAssetName("assets/wateringSystemTilesheet.png").Name;
                var wateringSystemTilesheet = new TileSheet(
                    "z_wateringSystemTilesheet",
                    indoorsMap,
                    wateringSystemTilesheetImageSource,
                    new Size(48, 16),
                    new Size(16, 16)
                );

                indoorsMap.AddTileSheet(wateringSystemTilesheet);
                indoorsMap.LoadTileSheets(Game1.mapDisplayDevice);
            }
        }

        private void PlaceWateringSystems()
        {
            if (!Config.UseWateringSystems) return;
            
            foreach (Building building in AnimalBuildings)
            {
                var buildingName = building.buildingType.Value;
                var buildingProfile = GetProfileForBuilding(buildingName);
                if (buildingProfile == null || !buildingProfile.BuildingHasWateringSystem(buildingName))
                    continue;
                
                var profileWateringSystemPlacement = buildingProfile.GetPlacementForBuildingName(buildingName).WateringSystem;
                var indoorsGameLocation = building.indoors.Value;

                foreach (SimplifiedTile tile in profileWateringSystemPlacement.TilesToRemove)
                {
                    indoorsGameLocation.removeTile(tile.TileX, tile.TileY, tile.Layer);
                }

                Layer buildingsLayer = indoorsGameLocation.Map.GetLayer("Buildings");
                Layer frontLayer = indoorsGameLocation.Map.GetLayer("Front");
                TileSheet tilesheet = indoorsGameLocation.Map.GetTileSheet("z_wateringSystemTilesheet");
                var wateringSystemLayer =
                    profileWateringSystemPlacement.Layer.Equals("Buildings", StringComparison.OrdinalIgnoreCase)
                        ? buildingsLayer
                        : profileWateringSystemPlacement.Layer.Equals("Front", StringComparison.OrdinalIgnoreCase) 
                            ? frontLayer 
                            : null;
                
                if (wateringSystemLayer == null) continue;
                wateringSystemLayer.Tiles[profileWateringSystemPlacement.TileX, profileWateringSystemPlacement.TileY] 
                    = new StaticTile(wateringSystemLayer, tilesheet, BlendMode.Alpha, tileIndex: profileWateringSystemPlacement.SystemIndex);
            }
        }

        private static void SendMessageToSelf(object message, string messageType)
        {
            ModHelper.Multiplayer.SendMessage(message, messageType, new[] { Manifest.UniqueID });
        }
    }
}
