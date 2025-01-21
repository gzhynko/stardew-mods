namespace AnimalsNeedWater.Core.Content;

public class AssetManager
{
    private const string BuildingsBasePath = "assets/buildings";
    private const string TroughsBasePath = "assets/troughs";
    private const string ItemsBasePath = "assets/items";
    
    public const string CoopEmptyWaterTrough = $"{BuildingsBasePath}/Coop_emptyWaterTrough.png";
    public const string Coop2EmptyWaterTrough = $"{BuildingsBasePath}/Coop2_emptyWaterTrough.png";

    public const string WateringSystemTilesheet = $"{TroughsBasePath}/wateringSystemTilesheet.png";
    public const string WaterTroughTilesheet = $"{TroughsBasePath}/waterTroughTilesheet.png";
    public const string WaterTroughTilesheetClean = $"{TroughsBasePath}/waterTroughTilesheet_clean.png";

    public const string WaterBowlTextureSpritesheet = $"{ItemsBasePath}/waterBowl.png";
}