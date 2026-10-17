using StardewModdingAPI.Utilities;

namespace StaircaseGuard;

internal sealed class ModConfig
{
    /// <summary>Whether a second staircase on the same mine level is blocked.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Hold this while placing to put down another staircase anyway.</summary>
    public KeybindList OverrideKey { get; set; } = KeybindList.Parse("LeftShift, RightShift");

    /// <summary>Show a short message when a staircase placement was blocked.</summary>
    public bool ShowHint { get; set; } = true;
}
