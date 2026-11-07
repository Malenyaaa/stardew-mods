using StardewModdingAPI.Utilities;

namespace PonyHorses;

internal sealed class ModConfig
{
    /// <summary>Press this while riding or standing next to your horse to choose its look.</summary>
    public KeybindList ChooseKey { get; set; } = KeybindList.Parse("P");
}
