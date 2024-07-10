using System.Collections.Generic;
using StardewModdingAPI;

namespace StardewElectricity.ContentPacks;

public class StardewElectricityContentPack
{
    public IContentPack ContentPack { get; set; }
    public List<Consumer> Consumers { get; set; }
}