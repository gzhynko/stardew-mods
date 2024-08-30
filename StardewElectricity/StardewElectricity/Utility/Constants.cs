using StardewValley.Menus;

namespace StardewElectricity.Utility;

public class Constants
{
    public static readonly string UtilityPoleBuildingTypeName = "Utility Pole";
    
    public static readonly string MetadataIsPlacedSideways = "IsPlacedSideways";
    public static readonly string ModDataIsOrigin = "IsOrigin";
    public static readonly string ModDataKwhConsumedPer10Minutes =
        $"{ModEntry.ModHelper.ModRegistry.ModID}/KwhConsumedPer10Minutes";

    public static readonly string SkinUtilityPoleSide = $"{ModEntry.ModHelper.ModRegistry.ModID}_UtilityPoleSide";

    public static readonly int GameMenuElectricityTabIndex = GameMenu.exitTab + 1;
    public const string GameMenuElectricityTabName = "electricity";
}