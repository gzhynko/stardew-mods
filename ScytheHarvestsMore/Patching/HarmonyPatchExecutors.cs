using Microsoft.Xna.Framework;
using ScytheHarvestsMore.Models;
using StardewValley;
using StardewValley.GameData.Machines;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ScytheHarvestsMore.Patching;

public class HarmonyPatchExecutors
{
    public static void FruitTreePerformToolAction(FruitTree __instance, Tool t, int explosion, Vector2 tileLocation)
    {
        if (t.QualifiedItemId != ModConstants.IridiumScytheQualifiedId)
            return;
        if (!ModEntry.Config.HarvestFruitTrees)
            return;
        
        __instance.shake(tileLocation, false);
    }

    public static void ObjectPerformToolAction(Object __instance, Tool? t)
    {
        if (t == null || t.QualifiedItemId != ModConstants.IridiumScytheQualifiedId)
            return;
        
        if (__instance.GetMachineData() != null && __instance.readyForHarvest.Value)
            HarvestMachine(__instance);
        if (__instance.isForage())
            HarvestForage(__instance);
    }

    private static void HarvestMachine(Object machine)
    {
        if (!ModEntry.Config.HarvestMachines)
            return;
        
        // Logic here follows Object::CheckForActionOnMachine
        
        var machineData = machine.GetMachineData();
        var machineOutput = machine.heldObject.Value;
        var machineTile = machine.TileLocation;

        Game1.createMultipleItemDebris(machineOutput, new Vector2(machineTile.X * 64, machineTile.Y * 64), -1);
        MachineDataUtility.UpdateStats(machineData.StatsToIncrementWhenHarvested, (Item) machineOutput, machineOutput.Stack);
        
        machine.heldObject.Value = null;
        machine.readyForHarvest.Value = false;
        machine.showNextIndex.Value = false;
        machine.ResetParentSheetIndex();
        
        if (MachineDataUtility.TryGetMachineOutputRule(machine, machineData, MachineOutputTrigger.OutputCollected, machineOutput.getOne(), Game1.player, machine.Location, out var rule, out MachineOutputTriggerRule _, out MachineOutputRule _, out MachineOutputTriggerRule _))
            machine.OutputMachine(machineData, rule, machine.lastInputItem.Value, Game1.player, machine.Location, false);
        
        if (machine.IsTapper() && machine.Location.terrainFeatures.TryGetValue(machine.TileLocation, out var terrainFeature) && terrainFeature is Tree tree)
            tree.UpdateTapperProduct(machine, machineOutput);
        
        if (machineData.ExperienceGainOnHarvest != null)
        {
            string[] array = machineData.ExperienceGainOnHarvest.Split(' ');
            for (int index = 0; index < array.Length; index += 2)
            {
                int skillNumberFromName = Farmer.getSkillNumberFromName(array[index]);
                if (skillNumberFromName != -1 && ArgUtility.TryGetInt(array, index + 1, out var howMuch, out string _, "int amount"))
                    Game1.player.gainExperience(skillNumberFromName, howMuch);
            }
        }
        
        machine.AttemptAutoLoad(Game1.player);
    }

    private static void HarvestForage(Object forage)
    {
        if (!ModEntry.Config.HarvestForage)
            return;

        // Logic here follows GameLocation::checkAction
        
        var location = forage.Location;
        var random = Utility.CreateDaySaveRandom(forage.TileLocation.X, forage.TileLocation.Y * 777f);

        // set forage quality
        if (Game1.player.professions.Contains(16))
        {
            forage.Quality = 4;
        }
        else
        {
            if (random.NextDouble() < (double)((float)Game1.player.ForagingLevel / 30f))
            {
                forage.Quality = 2;
            }
            else if (random.NextDouble() < (double)((float)Game1.player.ForagingLevel / 15f))
            {
                forage.Quality = 1;
            }
        }
        
        // gain experience
        if (location.isFarmBuildingInterior())
        {
            if (forage.SpecialVariable == 724519)
            {
                Game1.player.gainExperience(2, 2);
                Game1.player.gainExperience(0, 3);
            }
            else
            {
                Game1.player.gainExperience(2, 7);
            }
        }
        else
        {
            Game1.player.gainExperience(0, 5);
        }

        Game1.createItemDebris(forage.getOne(), new Vector2(forage.TileLocation.X * 64, forage.TileLocation.Y * 64), -1);
        Game1.stats.ItemsForaged++;
        
        // chance for extra item
        if (Game1.player.professions.Contains(13) && random.NextDouble() < 0.2 && !forage.questItem.Value && !location.isFarmBuildingInterior())
        {
            Game1.createItemDebris(forage.getOne(), new Vector2(forage.TileLocation.X * 64, forage.TileLocation.Y * 64), -1);
            Game1.player.gainExperience(2, 7);
        }
        
        location.removeObject(forage.TileLocation, false);
    }
}