using System;
using System.Collections.Generic;
using System.Linq;
using StardewElectricity.Types;
using StardewValley;

namespace StardewElectricity.Managers
{
    public class ElectricityManager
    {
        private StardewElectricitySaveData _saveData;
        public int PricePerKwh = 30;

        public void ConsumeKwh(float amount)
        {
            _saveData.KwhConsumed += amount;
            SaveModData();
        }

        public float GetKwhConsumedThisCycle()
        {
            return _saveData.KwhConsumed;
        }

        public BillingCycle GetBillingCycle()
        {
            return _saveData.BillingCycle;
        }

        public bool PayAllBills()
        {
            var paid = new List<int>();
            foreach (var billDate in _saveData.PendingBills.Keys)
            {
                Game1.player.Money -= _saveData.PendingBills[billDate];
                paid.Add(billDate);
            }

            foreach (var paidDate in paid)
            {
                _saveData.PendingBills.Remove(paidDate);
            }

            return _saveData.PendingBills.Count == 0;
        }

        public bool AreBillsPending()
        {
            return _saveData.PendingBills.Count != 0;
        }

        public int GetTotalOwed()
        {
            return _saveData.PendingBills.Values.Sum();
        }

        public int GetDaysUntilNextBill()
        {
            return _saveData.KwhConsumed > 0 ? Utility.Utility.GetBillingCycleLengthDays(_saveData.BillingCycle) - _saveData.DaysSinceLastBill : -1;
        }

        public void SaveLoaded()
        {
            _saveData = ModEntry.ModHelper.Data.ReadSaveData<StardewElectricitySaveData>(ModEntry.ModHelper.ModRegistry.ModID) 
                        ?? new StardewElectricitySaveData();
        }

        public void DayEnding()
        {
            _saveData.DaysSinceLastBill++;
            if (_saveData.DaysSinceLastBill == Utility.Utility.GetBillingCycleLengthDays(_saveData.BillingCycle))
            {
                PostBill();
            }
            SaveModData();
        }

        private void PostBill()
        {
            if (_saveData.KwhConsumed == 0.0f)
                return;
            
            var date = Game1.Date;
            var billAmount = (int)Math.Ceiling(_saveData.KwhConsumed * PricePerKwh);

            _saveData.PendingBills[date.TotalDays] = billAmount;
            
            _saveData.KwhConsumed = 0;
            _saveData.DaysSinceLastBill = 0;
            SaveModData();
        }

        private void SaveModData()
        {
            ModEntry.ModHelper.Data.WriteSaveData(ModEntry.ModHelper.ModRegistry.ModID, _saveData);
        }
    }
}