namespace GreenRainToggle;

internal sealed class ModConfig
{
    /// <summary>Whether Green Rain is turned off.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Whether the day Green Rain would have come gets normal rain. If false, that day gets the normal random weather.</summary>
    public bool ReplaceWithRain { get; set; } = true;

    /// <summary>Keep Green Rain in the first year, so its one-time story moments still happen.</summary>
    public bool KeepFirstYear { get; set; } = false;
}
