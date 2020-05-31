using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Tools;
using System;
using xTile.Layers;
using xTile.Tiles;

namespace AnimalsNeedWater
{
    internal class Overrides
    {
        [HarmonyPriority(500)]
        public static void AnimalDayUpdate(ref FarmAnimal __instance, ref GameLocation environtment)
        {
            if (!(__instance.home != null && !(__instance.home.indoors.Value as AnimalHouse).animals.ContainsKey(__instance.myID.Value) && environtment is Farm && !__instance.home.animalDoorOpen.Value))
            {
                if (__instance.home.nameOfIndoors.ToLower().Contains("coop"))
                {
                    if (ModData.CoopsWithWateredTrough.Contains(__instance.home.nameOfIndoors.ToLower()))
                    {
                        __instance.friendshipTowardFarmer.Value += Math.Abs(ModEntry.instance.Config.FriendshipPointsForWateredTrough);
                    }
                }
                else if (__instance.home.nameOfIndoors.ToLower().Contains("barn"))
                {
                    if (ModData.BarnsWithWateredTrough.Contains(__instance.home.nameOfIndoors.ToLower()))
                    {
                        __instance.friendshipTowardFarmer.Value += Math.Abs(ModEntry.instance.Config.FriendshipPointsForWateredTrough);
                    }
                }
            }
        }

        [HarmonyPriority(500)]
        public static bool AnimalHouseToolAction(ref AnimalHouse __instance, ref Tool t, ref int tileX, ref int tileY)
        {
            GameLocation gameLocation = Game1.currentLocation;

            if (t.BaseName == "Watering Can" && (t as WateringCan).WaterLeft > 0)
            {
                if (Game1.currentLocation.Name.ToLower().Contains("coop") && !ModData.CoopsWithWateredTrough.Contains(__instance.NameOrUniqueName.ToLower()))
                {
                    if (__instance.getBuilding().nameOfIndoorsWithoutUnique.ToLower() == "coop")
                    {
                        if (tileX == 10 && tileY == 5)
                        {
                            gameLocation.removeTile(10, 5, "Buildings");
                            Layer layer = gameLocation.map.GetLayer("Buildings");
                            TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                            layer.Tiles[10, 5] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                            ModData.CoopsWithWateredTrough.Add(__instance.NameOrUniqueName.ToLower());
                            __instance.getBuilding().texture = new Lazy<Texture2D>(() => ModEntry.instance.Helper.Content.Load<Texture2D>("assets/Coop_fullWaterTrough.png", ContentSource.ModFolder));

                            foreach (FarmAnimal animal in __instance.animals.Values)
                            {
                                if (ModEntry.instance.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
                                {
                                    animal.doEmote(20, true);
                                }
                                animal.friendshipTowardFarmer.Value += Math.Abs(ModEntry.instance.Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
                            }
                        }
                    }
                    else if (__instance.getBuilding().nameOfIndoorsWithoutUnique.ToLower() == "coop2")
                    {
                        if (tileX == 14 && tileY == 5)
                        {
                            gameLocation.removeTile(14, 5, "Buildings");
                            Layer layer = gameLocation.map.GetLayer("Buildings");
                            TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                            layer.Tiles[14, 5] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                            ModData.CoopsWithWateredTrough.Add(__instance.NameOrUniqueName.ToLower());
                            __instance.getBuilding().texture = new Lazy<Texture2D>(() => ModEntry.instance.Helper.Content.Load<Texture2D>("assets/Coop2_fullWaterTrough.png", ContentSource.ModFolder));

                            foreach (FarmAnimal animal in __instance.animals.Values)
                            {
                                if (ModEntry.instance.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
                                {
                                    animal.doEmote(20, true);
                                }
                                animal.friendshipTowardFarmer.Value += Math.Abs(ModEntry.instance.Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
                            }
                        }
                    }
                    else if (__instance.getBuilding().nameOfIndoorsWithoutUnique.ToLower() == "coop3")
                    {
                        if ((tileX == 20 && tileY == 3) || (tileX == 1 && tileY == 6))
                        {
                            gameLocation.removeTile(20, 3, "Buildings");
                            gameLocation.removeTile(1, 6, "Buildings");
                            Layer layer = gameLocation.map.GetLayer("Buildings");
                            TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                            layer.Tiles[20, 3] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                            layer.Tiles[1, 6] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                            ModData.CoopsWithWateredTrough.Add(__instance.NameOrUniqueName.ToLower());

                            foreach (FarmAnimal animal in __instance.animals.Values)
                            {
                                if (ModEntry.instance.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
                                {
                                    animal.doEmote(20, true);
                                }
                                animal.friendshipTowardFarmer.Value += Math.Abs(ModEntry.instance.Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
                            }
                        }
                    }
                }
                else if (Game1.currentLocation.Name.ToLower().Contains("barn") && !ModData.BarnsWithWateredTrough.Contains(__instance.NameOrUniqueName.ToLower()))
                {
                    if (__instance.getBuilding().nameOfIndoorsWithoutUnique.ToLower() == "barn")
                    {
                        if ((tileX == 13 && tileY == 3) || (tileX == 14 && tileY == 3) || (tileX == 2 && tileY == 6))
                        {
                            gameLocation.removeTile(13, 3, "Buildings");
                            gameLocation.removeTile(13, 2, "Front");
                            gameLocation.removeTile(14, 3, "Buildings");
                            gameLocation.removeTile(14, 2, "Front");
                            gameLocation.removeTile(2, 6, "Buildings");
                            Layer buildingsLayer = gameLocation.map.GetLayer("Buildings");
                            Layer frontLayer = gameLocation.map.GetLayer("Front");
                            TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                            frontLayer.Tiles[13, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 4);
                            buildingsLayer.Tiles[13, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 8);
                            frontLayer.Tiles[14, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 5);
                            buildingsLayer.Tiles[14, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 9);
                            buildingsLayer.Tiles[2, 6] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);

                            ModData.BarnsWithWateredTrough.Add(__instance.NameOrUniqueName.ToLower());

                            foreach (FarmAnimal animal in __instance.animals.Values)
                            {
                                if (ModEntry.instance.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
                                {
                                    animal.doEmote(20, true);
                                }
                                animal.friendshipTowardFarmer.Value += Math.Abs(ModEntry.instance.Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
                            }
                        }
                    }
                    else if (__instance.getBuilding().nameOfIndoorsWithoutUnique.ToLower() == "barn2")
                    {
                        if ((tileX == 17 && tileY == 3) || (tileX == 18 && tileY == 3) || (tileX == 19 && tileY == 3) || (tileX == 3 && tileY == 7))
                        {
                            gameLocation.removeTile(17, 3, "Buildings");
                            gameLocation.removeTile(17, 2, "Front");
                            gameLocation.removeTile(18, 3, "Buildings");
                            gameLocation.removeTile(18, 2, "Front");
                            gameLocation.removeTile(19, 3, "Buildings");
                            gameLocation.removeTile(3, 7, "Buildings");
                            Layer buildingsLayer = gameLocation.map.GetLayer("Buildings");
                            Layer frontLayer = gameLocation.map.GetLayer("Front");
                            TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                            frontLayer.Tiles[17, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 4);
                            buildingsLayer.Tiles[17, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 8);
                            frontLayer.Tiles[18, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 5);
                            buildingsLayer.Tiles[18, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 9);
                            buildingsLayer.Tiles[19, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                            buildingsLayer.Tiles[3, 7] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);

                            ModData.BarnsWithWateredTrough.Add(__instance.NameOrUniqueName.ToLower());

                            foreach (FarmAnimal animal in __instance.animals.Values)
                            {
                                if (ModEntry.instance.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
                                {
                                    animal.doEmote(20, true);
                                }
                                animal.friendshipTowardFarmer.Value += Math.Abs(ModEntry.instance.Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
                            }
                        }
                    }
                    else if (__instance.getBuilding().nameOfIndoorsWithoutUnique.ToLower() == "barn3")
                    {
                        if ((tileX == 21 && tileY == 3) || (tileX == 22 && tileY == 3) || (tileX == 23 && tileY == 13) || (tileX == 1 && tileY == 7))
                        {
                            gameLocation.removeTile(21, 3, "Buildings");
                            gameLocation.removeTile(21, 2, "Front");
                            gameLocation.removeTile(22, 3, "Buildings");
                            gameLocation.removeTile(22, 2, "Front");
                            gameLocation.removeTile(23, 13, "Buildings");
                            gameLocation.removeTile(1, 7, "Buildings");
                            Layer buildingsLayer = gameLocation.map.GetLayer("Buildings");
                            Layer frontLayer = gameLocation.map.GetLayer("Front");
                            TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                            frontLayer.Tiles[21, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 4);
                            buildingsLayer.Tiles[21, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 8);
                            frontLayer.Tiles[22, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 5);
                            buildingsLayer.Tiles[22, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 9);
                            buildingsLayer.Tiles[23, 13] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                            buildingsLayer.Tiles[1, 7] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);

                            ModData.BarnsWithWateredTrough.Add(__instance.NameOrUniqueName.ToLower());

                            foreach (FarmAnimal animal in __instance.animals.Values)
                            {
                                if (ModEntry.instance.Config.ShowLoveBubblesOverAnimalsWhenWateredTrough)
                                {
                                    animal.doEmote(20, true);
                                }
                                animal.friendshipTowardFarmer.Value += Math.Abs(ModEntry.instance.Config.AdditionalFriendshipPointsForWateredTroughWithAnimalsInsideBuilding);
                            }
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPriority(500)]
        public static void WarpFarmer(Game1 __instance, ref string locationName, ref int tileX, ref int tileY, ref int facingDirectionAfterWarp, ref bool isStructure)
        {
            string locationNameWithoutUnique = Game1.getLocationFromName(locationName, isStructure).Name;
            Building building = null;

            if (locationName.ToLower().Contains("coop") || locationName.ToLower().Contains("barn"))
                building = ((AnimalHouse)Game1.getLocationFromName(locationName)).getBuilding();

            if ((ModData.BarnsWithWateredTrough.Contains(locationName.ToLower()) || ModData.CoopsWithWateredTrough.Contains(locationName.ToLower())) && building != null)
            {
                if (locationNameWithoutUnique.Contains("Coop"))
                {
                    if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        gameLocation.removeTile(10, 5, "Buildings");
                        Layer layer = gameLocation.map.GetLayer("Buildings");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        layer.Tiles[10, 5] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop2")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        gameLocation.removeTile(14, 5, "Buildings");
                        Layer layer = gameLocation.map.GetLayer("Buildings");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        layer.Tiles[14, 5] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop3")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        gameLocation.removeTile(20, 3, "Buildings");
                        gameLocation.removeTile(1, 6, "Buildings");
                        Layer layer = gameLocation.map.GetLayer("Buildings");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        layer.Tiles[20, 3] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                        layer.Tiles[1, 6] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                    }
                }
                else if (locationNameWithoutUnique.Contains("Barn"))
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
                        frontLayer.Tiles[13, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 4);
                        buildingsLayer.Tiles[13, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 8);
                        frontLayer.Tiles[14, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 5);
                        buildingsLayer.Tiles[14, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 9);
                        buildingsLayer.Tiles[2, 6] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);
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
                        frontLayer.Tiles[17, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 4);
                        buildingsLayer.Tiles[17, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 8);
                        frontLayer.Tiles[18, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 5);
                        buildingsLayer.Tiles[18, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 9);
                        buildingsLayer.Tiles[19, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                        buildingsLayer.Tiles[3, 7] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);
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
                        frontLayer.Tiles[21, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 4);
                        buildingsLayer.Tiles[21, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 8);
                        frontLayer.Tiles[22, 2] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, tileIndex: 5);
                        buildingsLayer.Tiles[22, 3] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 9);
                        buildingsLayer.Tiles[23, 13] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                        buildingsLayer.Tiles[1, 7] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tileIndex: 3);
                    }
                }
            }
            else if((!ModData.BarnsWithWateredTrough.Contains(locationName.ToLower()) || !ModData.CoopsWithWateredTrough.Contains(locationName.ToLower())) && building != null)
            {
                if (locationNameWithoutUnique.Contains("Coop"))
                {
                    if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop")
                    {
                        GameLocation gameLocation = building.indoors.Value;

                        gameLocation.removeTile(10, 5, "Buildings");
                        Layer layer = gameLocation.map.GetLayer("Buildings");
                        TileSheet tilesheet = gameLocation.map.GetTileSheet("z_waterTroughTilesheet");
                        layer.Tiles[10, 5] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 0);
                    }
                    else if (building.nameOfIndoorsWithoutUnique.ToLower() == "coop2")
                    {
                        GameLocation gameLocation = building.indoors.Value;

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
                else if (locationNameWithoutUnique.Contains("Barn"))
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
            }
        }
    }
}
