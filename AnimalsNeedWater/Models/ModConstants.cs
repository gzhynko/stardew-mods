namespace AnimalsNeedWater.Models;

public static class ModConstants
{
    public static string ModDataSaveDataKey = "mod-data";
    public static int LoveEmote { get; } = 20;
    public static readonly string WaterBowlItemId = $"{ModEntry.ModHelper.ModContent.ModID}_Water_Bowl";
    public static readonly string WaterBowlItemModDataIsFullField = $"{ModEntry.ModHelper.ModContent.ModID}_Water_Bowl.IsFull";
}