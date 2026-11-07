namespace GrassKeeper;

internal sealed class ModConfig
{
    /// <summary>Whether grass under things is kept at all.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Keep grass under fences and gates.</summary>
    public bool UnderFences { get; set; } = true;

    /// <summary>Keep grass under placed objects like machines, chests, sprinklers or signs.</summary>
    public bool UnderObjects { get; set; } = true;

    /// <summary>Keep grass under furniture placed outside.</summary>
    public bool UnderFurniture { get; set; } = true;

    /// <summary>Also keep the grass when swinging swords or daggers, not just scythes.</summary>
    public bool AllWeapons { get; set; } = true;
}
