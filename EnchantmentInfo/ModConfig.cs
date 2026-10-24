using StardewModdingAPI.Utilities;

namespace EnchantmentInfo;

internal sealed class ModConfig
{
    /// <summary>Hold this while hovering an enchanted item to show what its enchantments do.</summary>
    public KeybindList ExpandKey { get; set; } = KeybindList.Parse("LeftShift, RightShift");

    /// <summary>Show a small "Shift: details" line while the key isn't held.</summary>
    public bool ShowHint { get; set; } = true;
}
