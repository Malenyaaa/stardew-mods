# Enchantment Info

A small [SMAPI](https://smapi.io/) mod for Stardew Valley 1.6.

The game shows an enchantment's name in the tooltip, but not what it actually does. With this mod, hold **Shift** while hovering an enchanted tool or weapon, and the tooltip shows a short description of each enchantment on it.

Covers all tool enchantments (axe, pickaxe, hoe, watering can, pan, fishing rod) and the weapon enchantments from the Forge. Some enchantments do different things on different tools, for example *Generous* on a hoe or a pan, so the description matches the tool you're hovering.

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the latest version from [GitHub](https://github.com/Malenyaaa/stardew-mods/releases) and unzip it into your `Mods` folder.
3. Start the game through SMAPI.

## Config

Edit `config.json` (created on first launch) or use [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098):

| Option | Default | What it does |
|---|---|---|
| `ExpandKey` | `LeftShift, RightShift` | Hold this while hovering an enchanted item to show what its enchantments do. Any [SMAPI keybind](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings) works, including controller buttons. |
| `ShowHint` | `true` | Shows a small line in the tooltip reminding you of the key. |

## Compatibility

- Stardew Valley 1.6+, SMAPI 4.0+
- [Scythe Tool Enchantments](https://www.nexusmods.com/stardewvalley/mods/26217): its iridium scythe enchantments (Gatherer, Horticulturist, Palaeontologist, Reaper, Great Crescent) get descriptions too.
- Works in multiplayer. Each player only needs it if they want the descriptions for themselves.
- Translations: English, German.

## Building

Needs the .NET 6+ SDK. Build from the repo root, since the mod uses shared code from `Common`. The build uses [Pathoschild.Stardew.ModBuildConfig](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig), which finds the game folder automatically (or set the `GamePath` environment variable).

```
dotnet build EnchantmentInfo -c Release
```

## AI disclosure

This mod was written with the help of an AI coding assistant (Claude by Anthropic). I came up with the idea, decided how it should behave, tested it in-game and reviewed the result. The AI wrote most of the code.

## License

MIT
