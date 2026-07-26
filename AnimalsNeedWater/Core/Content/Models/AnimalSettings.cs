namespace AnimalsNeedWater.Core.Content.Models;

/// <summary>
/// parsed from Data/FarmAnimals customfields
/// </summary>
public class AnimalSettings
{
    public bool NeedsWater = true;
    public int WaterIntervalDays = 1;
    public bool CanDrinkOutside = true;

    public int? DailyWaterFriendship;
    public int? WitnessedWaterFriendship;
    public int? ThirstyFriendshipPenalty;

    public int? ThirstyEmoteId;
    public int? DrinkOutsideEmoteId;
    public int? WaterBonusEmoteId;

    /// <summary>shared inst returned for any animal type without anw's CustomFields</summary>
    public static readonly AnimalSettings Default = new AnimalSettings();
}