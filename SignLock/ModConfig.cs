using StardewModdingAPI.Utilities;

namespace SignLock;

internal sealed class ModConfig
{
    /// <summary>Whether signs that already show an item are locked.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Hold this while clicking a sign to change its item anyway.</summary>
    public KeybindList UnlockKey { get; set; } = KeybindList.Parse("LeftShift, RightShift");

    /// <summary>Show a short message when a click on a locked sign was blocked.</summary>
    public bool ShowHint { get; set; } = true;
}
