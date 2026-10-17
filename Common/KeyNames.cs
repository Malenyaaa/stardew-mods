using System.Linq;
using System.Text.RegularExpressions;
using StardewModdingAPI.Utilities;

namespace StardewMods.Common;

internal static class KeyNames
{
    /// <summary>Turn e.g. "LeftShift, RightShift" into "Shift".</summary>
    public static string Describe(KeybindList keys)
    {
        var names = keys.Keybinds
            .Where(k => k.IsBound)
            .Select(k => string.Join(" + ", k.Buttons.Select(b => Regex.Replace(b.ToString(), "^(Left|Right)(?=[A-Z])", ""))))
            .Distinct()
            .ToArray();
        return names.Length > 0 ? string.Join(" / ", names) : keys.ToString();
    }
}
