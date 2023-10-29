using System;
using System.Collections.Generic;

namespace CropGrowthAdjustments.Types
{
    public interface IJsonAssetsApi
    {
        int GetObjectId(string name);
        
        IDictionary<string, int> GetAllCropIds();

        event EventHandler IdsAssigned;
    }
}