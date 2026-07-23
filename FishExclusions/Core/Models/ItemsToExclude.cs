using System;
using System.Collections.Generic;

namespace FishExclusions.Core.Models;

public class ItemsToExclude
{
    /// <summary>
    /// Season- and location-independent exclusions.
    /// </summary>
    public string[] CommonExclusions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Season- and/or location-dependent exclusions.
    /// </summary>
    public List<ConditionalExclusion> ConditionalExclusions { get; set; } = new();
}