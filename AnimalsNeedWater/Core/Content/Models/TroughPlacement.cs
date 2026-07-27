using System.Collections.Generic;
using AnimalsNeedWater.Core.Models;

namespace AnimalsNeedWater.Core.Content.Models;

public class TroughPlacement
{
    public List<TroughTile> TroughTiles = new List<TroughTile>();
    public List<WateringSystemTile> WateringSystemTiles = new List<WateringSystemTile>();
    public string? TroughTexture;
    public string? WateringSystemTexture;
    public ExteriorOverlay? ExteriorEmptyOverlay;
}