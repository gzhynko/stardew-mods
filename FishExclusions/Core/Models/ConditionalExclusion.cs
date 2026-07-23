using System;

namespace FishExclusions.Core.Models;

public class ConditionalExclusion
{
    public string Season { get; set; } = "";

    public string Weather { get; set; } = "";

    public string Location { get; set; } = "";

    public string[] Exclusions { get; set; } = Array.Empty<string>();
}