using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;
using xTile.Tiles;

namespace AnimalsNeedWater
{
    /// <summary> The mod config class. More info here: https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config </summary>
    public class ModConfig
    {
        public bool ShowLoveBubblesOverAnimalsWhenWateredTrough { get; set; } = true;
        public bool WateringSystemInDeluxeBuildings { get; set; } = true;
    }

    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        public static IMonitor ModMonitor;
        public static IModHelper ModHelper;
        public static ModEntry instance;
        public ModConfig Config;
        public List<AnimalLeftThirsty> AnimalsLeftThirstyYesterday;

        /*********
        ** Public methods
        *********/

        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;
            instance = this;

            AnimalsLeftThirstyYesterday = new List<AnimalLeftThirsty>();

            this.Config = Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayEnding += HandleDayUpdate;
        }

        public override object GetApi()
        {
            return new API();
        }

        public void EmptyWaterTroughs()
        {
            ModData.BarnsWithWateredTrough = new List<string>();
            ModData.CoopsWithWateredTrough = new List<string>();

            foreach (Building building in Game1.getFarm().buildings)
            {
                int animalCount = 0;

                foreach (FarmAnimal animal in Game1.getFarm().getAllFarmAnimals())
                {
                    if (animal.home.nameOfIndoors.ToLower().Equals(building.nameOfIndoors.ToLower())) animalCount++;
                }

                if(building.nameOfIndoorsWithoutUnique.ToLower().Contains("3") && Config.WateringSystemInDeluxeBuildings)
                {
                    if (building.nameOfIndoorsWithoutUnique.ToLower() == "barn3") 
                    { 
                        if (!ModData.BarnsWithWateredTrough.Contains(building.nameOfIndoors.ToLower()))
                            ModData.BarnsWithWateredTrough.Add(building.nameOfIndoors.ToLower());
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop3")
                    {
                        if (!ModData.CoopsWithWateredTrough.Contains(building.nameOfIndoors.ToLower()))
                            ModData.CoopsWithWateredTrough.Add(building.nameOfIndoors.ToLower());
                    }
                    continue;
                }

                if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("coop") && animalCount > 0)
                {
                    if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        building.texture = new Lazy<Texture2D>(() => Helper.Content.Load<Texture2D>("assets/Coop_emptyWaterTrough.png", ContentSource.ModFolder));
                        gameLocation.removeTile(10, 5, "Buildings");
                        Layer layer = gameLocation.map.GetLayer("Buildings");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        layer.Tiles[10, 5] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop2")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        building.texture = new Lazy<Texture2D>(() => Helper.Content.Load<Texture2D>("assets/Coop2_emptyWaterTrough.png", ContentSource.ModFolder));
                        gameLocation.removeTile(14, 5, "Buildings");
                        Layer layer = gameLocation.map.GetLayer("Buildings");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        layer.Tiles[14, 5] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop3")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        gameLocation.removeTile(20, 3, "Buildings");
                        gameLocation.removeTile(1, 6, "Buildings");
                        Layer layer = gameLocation.map.GetLayer("Buildings");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        layer.Tiles[20, 3] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                        layer.Tiles[1, 6] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                    }
                }
                else if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("barn") && animalCount > 0)
                {
                    if (building.nameOfIndoorsWithoutUnique.ToLower() == "barn")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        gameLocation.removeTile(13, 3, "Buildings");
                        gameLocation.removeTile(13, 2, "Front");
                        gameLocation.removeTile(14, 3, "Buildings");
                        gameLocation.removeTile(14, 2, "Front");
                        gameLocation.removeTile(2, 6, "Buildings");
                        Layer buildingsLayer = gameLocation.map.GetLayer("Buildings");
                        Layer frontLayer = gameLocation.map.GetLayer("Front");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        frontLayer.Tiles[13, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 1);
                        buildingsLayer.Tiles[13, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 6);
                        frontLayer.Tiles[14, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 2);
                        buildingsLayer.Tiles[14, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 7);
                        buildingsLayer.Tiles[2, 6] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower() == "barn2")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        gameLocation.removeTile(17, 3, "Buildings");
                        gameLocation.removeTile(17, 2, "Front");
                        gameLocation.removeTile(18, 3, "Buildings");
                        gameLocation.removeTile(18, 2, "Front");
                        gameLocation.removeTile(19, 3, "Buildings");
                        gameLocation.removeTile(3, 7, "Buildings");
                        Layer buildingsLayer = gameLocation.map.GetLayer("Buildings");
                        Layer frontLayer = gameLocation.map.GetLayer("Front");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        frontLayer.Tiles[17, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 1);
                        buildingsLayer.Tiles[17, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 6);
                        frontLayer.Tiles[18, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 2);
                        buildingsLayer.Tiles[18, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 7);
                        buildingsLayer.Tiles[19, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                        buildingsLayer.Tiles[3, 7] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower() == "barn3")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        gameLocation.removeTile(21, 3, "Buildings");
                        gameLocation.removeTile(21, 2, "Front");
                        gameLocation.removeTile(22, 3, "Buildings");
                        gameLocation.removeTile(22, 2, "Front");
                        gameLocation.removeTile(23, 13, "Buildings");
                        gameLocation.removeTile(1, 7, "Buildings");
                        Layer buildingsLayer = gameLocation.map.GetLayer("Buildings");
                        Layer frontLayer = gameLocation.map.GetLayer("Front");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        frontLayer.Tiles[21, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 1);
                        buildingsLayer.Tiles[21, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 6);
                        frontLayer.Tiles[22, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 2);
                        buildingsLayer.Tiles[22, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 7);
                        buildingsLayer.Tiles[23, 13] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                        buildingsLayer.Tiles[1, 7] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                    }
                }
                else if (animalCount == 0)
                {
                    if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("coop"))
                    {
                        ModData.CoopsWithWateredTrough.Add(building.nameOfIndoors.ToLower());
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("barn"))
                    {
                        ModData.BarnsWithWateredTrough.Add(building.nameOfIndoors.ToLower());
                    }
                }
            }
        }

        /*********
        ** Private methods
        *********/

        /// <summary> Looks for animals left thirsty and notifies player of them. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void HandleDayUpdate(object sender, DayEndingEventArgs e)
        {
            List<AnimalLeftThirsty> animalsLeftThirsty = new List<AnimalLeftThirsty>();

            foreach (Building building in Game1.getFarm().buildings)
            {
                if (building.nameOfIndoors.ToLower().Contains("coop"))
                {
                    foreach (FarmAnimal animal in ((AnimalHouse)building.indoors.Value).animals.Values)
                    {
                        if (ModData.CoopsWithWateredTrough.Contains(animal.home.nameOfIndoors.ToLower()) == false)
                        {
                            if (!animalsLeftThirsty.Any(item => item.DisplayName == animal.displayName))
                            {
                                animal.friendshipTowardFarmer.Value -= 20;
                                animalsLeftThirsty.Add(new AnimalLeftThirsty(animal.displayName, (animal.isMale() ? "male" : "female")));
                            }
                        }
                    }
                } 
                else if (building.nameOfIndoors.ToLower().Contains("barn"))
                {
                    foreach(FarmAnimal animal in ((AnimalHouse)building.indoors.Value).animals.Values)
                    {
                        if (ModData.BarnsWithWateredTrough.Contains(animal.home.nameOfIndoors.ToLower()) == false)
                        {
                            if (!animalsLeftThirsty.Any(item => item.DisplayName == animal.displayName))
                            {
                                animal.friendshipTowardFarmer.Value -= 20;
                                animalsLeftThirsty.Add(new AnimalLeftThirsty(animal.displayName, (animal.isMale() ? "male" : "female")));
                            }
                        }
                    }
                }
            }

            // Check for animals outside their buildings
            foreach (FarmAnimal animal in Game1.getFarm().animals.Values)
            {
                if (animal.home.nameOfIndoorsWithoutUnique.ToLower().Contains("coop"))
                {
                    if (ModData.CoopsWithWateredTrough.Contains(animal.home.nameOfIndoors.ToLower()) == false || animal.home.animalDoorOpen.Value == false)
                    {
                        if (!animalsLeftThirsty.Any(item => item.DisplayName == animal.displayName))
                        {
                            animal.friendshipTowardFarmer.Value -= 20;
                            animalsLeftThirsty.Add(new AnimalLeftThirsty(animal.displayName, (animal.isMale() ? "male" : "female")));
                        }
                    }
                } 
                else if(animal.home.nameOfIndoorsWithoutUnique.ToLower().Contains("barn"))
                {
                    if (ModData.BarnsWithWateredTrough.Contains(animal.home.nameOfIndoors.ToLower()) == false || animal.home.animalDoorOpen.Value == false)
                    {
                        if (!animalsLeftThirsty.Any(item => item.DisplayName == animal.displayName))
                        {
                            animal.friendshipTowardFarmer.Value -= 20;
                            animalsLeftThirsty.Add(new AnimalLeftThirsty(animal.displayName, (animal.isMale() ? "male" : "female")));
                        }
                    }
                }
            }

            if (animalsLeftThirsty.Count() > 0)
            {                
                if (animalsLeftThirsty.Count() == 1)
                {
                    if (animalsLeftThirsty[0].Gender == "male")
                    {
                        Game1.showGlobalMessage(ModHelper.Translation.Get("AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_Male", new { firstAnimalName = animalsLeftThirsty[0].DisplayName }));
                    } 
                    else if (animalsLeftThirsty[0].Gender == "female")
                    {
                        Game1.showGlobalMessage(ModHelper.Translation.Get("AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_Female", new { firstAnimalName = animalsLeftThirsty[0].DisplayName }));
                    }
                    else 
                    {
                        Game1.showGlobalMessage(ModHelper.Translation.Get("AnimalsLeftWithoutWaterYesterday.globalMessage.oneAnimal_UnknownGender", new { firstAnimalName = animalsLeftThirsty[0].DisplayName }));
                    }
                }
                else if (animalsLeftThirsty.Count() == 2)
                {
                    Game1.showGlobalMessage(ModHelper.Translation.Get("AnimalsLeftWithoutWaterYesterday.globalMessage.twoAnimals", new { firstAnimalName = animalsLeftThirsty[0].DisplayName, secondAnimalName = animalsLeftThirsty[1].DisplayName }));
                }
                else if (animalsLeftThirsty.Count() == 3)
                {
                    Game1.showGlobalMessage(ModHelper.Translation.Get("AnimalsLeftWithoutWaterYesterday.globalMessage.threeAnimals", new { firstAnimalName = animalsLeftThirsty[0].DisplayName, secondAnimalName = animalsLeftThirsty[1].DisplayName, thirdAnimalName = animalsLeftThirsty[2].DisplayName }));
                }
                else
                {
                    Game1.showGlobalMessage(ModHelper.Translation.Get("AnimalsLeftWithoutWaterYesterday.globalMessage.multipleAnimals", new { firstAnimalName = animalsLeftThirsty[0].DisplayName, secondAnimalName = animalsLeftThirsty[1].DisplayName, thirdAnimalName = animalsLeftThirsty[2].DisplayName, totalAmountExcludingFirstThree = animalsLeftThirsty.Count() - 3 }));
                }
            }

            AnimalsLeftThirstyYesterday = animalsLeftThirsty;

            List<object> nextDayAndSeasonList = GetNextDayAndSeason(Game1.dayOfMonth, Game1.currentSeason);

            if (!Utility.isFestivalDay((int)nextDayAndSeasonList[0], (string)nextDayAndSeasonList[1]))
            {
                EmptyWaterTroughs();
            }
            else
            {
                foreach (Building building in Game1.getFarm().buildings)
                {
                    if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("coop"))
                    {
                        ModData.CoopsWithWateredTrough.Add(building.nameOfIndoors.ToLower());
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("barn"))
                    {
                        ModData.BarnsWithWateredTrough.Add(building.nameOfIndoors.ToLower());
                    }
                }
            }
        }

        /// <summary> Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var harmony = HarmonyInstance.Create("GZhynko.AnimalsNeedWater");

            harmony.Patch(
                original: AccessTools.Method(typeof(AnimalHouse), nameof(AnimalHouse.performToolAction)),
                prefix: new HarmonyMethod(typeof(Overrides), nameof(Overrides.AnimalHouseToolAction))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate)),
                prefix: new HarmonyMethod(typeof(Overrides), nameof(Overrides.AnimalDayUpdate))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.warpFarmer), new Type[] {
                    typeof(string),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(bool)
                }),
                prefix: new HarmonyMethod(typeof(Overrides), nameof(Overrides.WarpFarmer))
            );
        }

        /// <summary> Raised after the save is loaded. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            foreach (Building building in Game1.getFarm().buildings)
            {
                if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("coop"))
                {
                    var coopMap = ((GameLocation)building.indoors.Value).map;

                    TileSheet tileSheet = new TileSheet(
                      id: "z_waterTroughTilesheet", 
                      map: coopMap,
                      imageSource: ModEntry.instance.Helper.Content.GetActualAssetKey("assets/waterTroughTilesheet.xnb", ContentSource.ModFolder),
                      sheetSize: new xTile.Dimensions.Size(160, 16), 
                      tileSize: new xTile.Dimensions.Size(16, 16)
                   );

                    coopMap.AddTileSheet(tileSheet);
                    coopMap.LoadTileSheets(Game1.mapDisplayDevice);

                    if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop3" && Config.WateringSystemInDeluxeBuildings)
                    {
                        var coop3Map = ((GameLocation)building.indoors.Value).map;
                        var gameLocation = building.indoors.Value;

                        TileSheet tileSheet3 = new TileSheet(
                            id: "z_wateringSystemTilesheet",
                            map: coop3Map,
                            imageSource: ModEntry.instance.Helper.Content.GetActualAssetKey("assets/wateringSystemTilesheet.xnb", ContentSource.ModFolder),
                            sheetSize: new xTile.Dimensions.Size(32, 16),
                            tileSize: new xTile.Dimensions.Size(16, 16)
                        );

                        coop3Map.AddTileSheet(tileSheet3);
                        coop3Map.LoadTileSheets(Game1.mapDisplayDevice);

                        gameLocation.removeTile(20, 2, "Front");
                        Layer frontLayer = coop3Map.GetLayer("Front");
                        TileSheet tilesheet = coop3Map.GetTileSheet("z_wateringSystemTilesheet");
                        frontLayer.Tiles[20, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 1);
                    }
                } 
                else if(building.nameOfIndoorsWithoutUnique.ToLower().Contains("barn"))
                {
                    var barnMap = ((GameLocation)building.indoors.Value).map;

                    TileSheet tileSheet = new TileSheet(
                        id: "z_waterTroughTilesheet",
                        map: barnMap,
                        imageSource: ModEntry.instance.Helper.Content.GetActualAssetKey("assets/waterTroughTilesheet.xnb", ContentSource.ModFolder),
                        sheetSize: new xTile.Dimensions.Size(160, 16),
                        tileSize: new xTile.Dimensions.Size(16, 16)
                    );

                    barnMap.AddTileSheet(tileSheet);
                    barnMap.LoadTileSheets(Game1.mapDisplayDevice);

                    if(building.nameOfIndoorsWithoutUnique.ToLower() == "barn3" && Config.WateringSystemInDeluxeBuildings)
                    {
                        var barn3Map = ((GameLocation)building.indoors.Value).map;
                        var gameLocation = building.indoors.Value;

                        TileSheet tileSheet3 = new TileSheet(
                            id: "z_wateringSystemTilesheet",
                            map: barn3Map,
                            imageSource: ModEntry.instance.Helper.Content.GetActualAssetKey("assets/wateringSystemTilesheet.xnb", ContentSource.ModFolder),
                            sheetSize: new xTile.Dimensions.Size(32, 16),
                            tileSize: new xTile.Dimensions.Size(16, 16)
                        );

                        barn3Map.AddTileSheet(tileSheet3);
                        barn3Map.LoadTileSheets(Game1.mapDisplayDevice);

                        gameLocation.removeTile(23, 3, "Buildings");
                        gameLocation.removeTile(23, 2, "Front");
                        Layer buildingsLayer = barn3Map.GetLayer("Buildings");
                        TileSheet tilesheet = barn3Map.GetTileSheet("z_wateringSystemTilesheet");
                        buildingsLayer.Tiles[23, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                    }
                }
            }

            HandleDayStart();
        }

        private void HandleDayStart()
        {
            if (!Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
            {
                EmptyWaterTroughs();
            } 
            else
            {
                foreach (Building building in Game1.getFarm().buildings)
                {
                    if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("coop"))
                    {
                        ModData.CoopsWithWateredTrough.Add(building.nameOfIndoors.ToLower());
                        if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop")
                        {
                            building.texture = new Lazy<Texture2D>(() => Helper.Content.Load<Texture2D>("assets/Coop_fullWaterTrough.png", ContentSource.ModFolder));
                        } 
                        else if(building.nameOfIndoorsWithoutUnique.ToLower() == "coop2")
                        {
                            building.texture = new Lazy<Texture2D>(() => Helper.Content.Load<Texture2D>("assets/Coop2_fullWaterTrough.png", ContentSource.ModFolder));
                        }
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower().Contains("barn"))
                    {
                        ModData.BarnsWithWateredTrough.Add(building.nameOfIndoors.ToLower());
                    }
                }
            }
        }

        /*********
        ** Utilities 
        *********/

        #region Utils

        public List<object> GetNextDayAndSeason(int currDay, string currSeason)
        {
            if (currDay + 1 <= 28)
            {
                List<object> returnList = new List<object>
                {
                    currDay + 1,
                    currSeason
                };
                return returnList;
            }
            else
            {
                List<object> returnList = new List<object>
                {
                    1,
                    NextSeason(currSeason)
                };
                return returnList;
            }
        }

        public string NextSeason(string season)
        {
            string newSeason = "";
            if (season == "spring") newSeason = "summer";
            if (season == "summer") newSeason = "fall";
            if (season == "fall") newSeason = "winter";
            if (season == "winter") newSeason = "spring";

            return newSeason;
        }

        public class AnimalLeftThirsty
        {
            public AnimalLeftThirsty(string displayName, string gender)
            {
                this.DisplayName = displayName;
                this.Gender = gender;
            }

            public string DisplayName { get; set; }
            public string Gender { get; set; }
        }
        #endregion
    }
}
