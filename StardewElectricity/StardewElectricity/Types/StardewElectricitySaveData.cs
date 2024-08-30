using System.Collections.Generic;
using StardewValley;

namespace StardewElectricity.Types;

public class StardewElectricitySaveData
{
    public float KwhConsumed { get; set; }
    public int DaysSinceLastBill { get; set; }
    public Dictionary<int, int> PendingBills { get; set; } = new ();
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Daily;
}