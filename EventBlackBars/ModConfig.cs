namespace EventBlackBars
{
    /// <summary> The mod config class. More info here: https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config </summary>
    public class ModConfig
    {
        /// <summary>
        /// The percentage of the height of the screen for a bar to take up.
        /// </summary>
        public double BarHeightPercentage { get; set; } = 10;
        
        /// <summary>
        /// Whether to gradually move the bars in when an event starts, or have them fully out right away.
        /// </summary>
        public bool MoveBarsInSmoothly { get; set; } = true;
    }
}
