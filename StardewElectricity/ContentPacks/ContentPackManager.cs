using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

namespace StardewElectricity.ContentPacks;

public class ContentPackManager
{
    public List<StardewElectricityContentPack> ContentPacks = new ();

    private HashSet<string> AffectedItems = new ();
    private const string ContentJsonName = "consumers.json";

    public void InitializeContentPacks(IModHelper helper, IMonitor monitor)
    {
        monitor.Log("Loading content packs...", LogLevel.Info);

        foreach (var pack in helper.ContentPacks.GetOwned())
        {
            if (!pack.HasFile(ContentJsonName))
            {
                monitor.Log(
                    $"  {pack.Manifest.Name} - Incorrect content pack folder structure. Expected {ContentJsonName} to be present in the folder.",
                    LogLevel.Error);
                continue;
            }

            var seContentPack = new StardewElectricityContentPack();
            // parse consumers.json
            try
            {
                seContentPack.Consumers = pack.ReadJsonFile<List<Consumer>>(ContentJsonName);
            }
            catch (Exception e)
            {
                monitor.Log($"  {pack.Manifest.Name} - Error while parsing {ContentJsonName}: {e}", LogLevel.Error);
                continue;
            }
            
            // populate AffectedItems
            foreach (var consumer in seContentPack.Consumers)
            {
                AffectedItems.Add(consumer.QualifiedItemId);
            }

            seContentPack.ContentPack = pack;
            ContentPacks.Add(seContentPack);
            // provide info about the loaded content pack
            monitor.Log(
                $"  Loaded {pack.Manifest.Name} {pack.Manifest.Version} by {pack.Manifest.Author}: {pack.Manifest.Description}", LogLevel.Info);
        }
    }

    public bool IsConsumer(string qualifiedItemId)
    {
        return AffectedItems.Contains(qualifiedItemId);
    }

    public float GetConsumptionRatePer10Minutes(string qualifiedItemId)
    {
        float result = 0.0f;
        foreach (var pack in ContentPacks)
        {
            foreach (var consumer in pack.Consumers)
            {
                result = consumer.KwhConsumedPer10Minutes;
            }
        }

        return result;
    }
}