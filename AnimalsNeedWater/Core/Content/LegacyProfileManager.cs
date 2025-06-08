using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnimalsNeedWater.Core.Models;
using StardewModdingAPI;

namespace AnimalsNeedWater.Core.Content;

public class LegacyProfileManager
{
    public static List<LegacyPlacementProfile>? LoadedProfiles;
    public static Dictionary<string, List<LegacyPlacementProfile>> ActiveProfiles = null!;

    public static void LoadProfiles()
    {
        var profiles = new List<LegacyPlacementProfile>();
        var availableFiles = Directory.GetFiles(Path.Combine(ModEntry.ModHelper.DirectoryPath, "assets/profiles")).Where(filename => filename.Contains(".json"));
        foreach (var file in availableFiles)
        {
            var profile = ModEntry.ModHelper.Data.ReadJsonFile<LegacyPlacementProfile>("assets/profiles/" + Path.GetFileName(file));
            if (profile == null)
                continue;
            
            profiles.Add(profile);
        }

        LoadedProfiles = profiles;
    }

    public static void FindActiveProfiles()
    {
        foreach (var modInfo in ModEntry.ModHelper.ModRegistry.GetAll())
        {
            var profile = GetProfileByUniqueId(modInfo.Manifest.UniqueID);
            if (profile == null || profile.TargetBuildings == null) continue;

            foreach (var profileTargetBuilding in profile.TargetBuildings)
            {
                if (ActiveProfiles.ContainsKey(profileTargetBuilding))
                    ActiveProfiles[profileTargetBuilding].Add(profile);
                else
                    ActiveProfiles.Add(profileTargetBuilding, [profile]);
            }
        }
    }
    
    public static LegacyPlacementProfile? GetProfileByUniqueId(string id)
    {
        LegacyPlacementProfile? result = null;
        foreach (var profile in LoadedProfiles!)
        {
            if (profile.ModUniqueId!.Contains(id, StringComparison.CurrentCultureIgnoreCase))
            {
                result = profile;
            }
        }
        
        return result;
    }
}
