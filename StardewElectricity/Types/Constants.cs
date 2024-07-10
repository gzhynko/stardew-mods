using StardewValley.Menus;

namespace StardewElectricity.Types;

public class Constants
{
    public static readonly string UtilityPoleBuildingTypeName = "Utility Pole";
    
    public static readonly string MetadataIsPlacedSideways = "IsPlacedSideways";
    public static readonly string ModDataIsOrigin = "IsOrigin";

    public static readonly string SkinUtilityPoleSide = $"{ModEntry.ModHelper.ModRegistry.ModID}_UtilityPoleSide";

    public static readonly int GameMenuElectricityTabIndex = GameMenu.exitTab + 1;
    public const string GameMenuElectricityTabName = "electricity";
}