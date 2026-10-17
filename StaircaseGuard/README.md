# Staircase Guard

A small [SMAPI](https://smapi.io/) mod for Stardew Valley 1.6.

When you place a staircase in the Mines or Skull Cavern, the ladder takes a moment to appear. It's easy to click again and waste a second staircase on the same level. With this mod, once you placed a staircase on a level, further staircases there are blocked and stay in your inventory. Hold **Shift** to place another one anyway.

Every new level starts fresh.

You can also place a staircase on the tile you're standing on, which the game normally doesn't allow.

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the latest Staircase Guard release from [GitHub](https://github.com/Malenyaaa/stardew-mods/releases) and unzip it into your `Mods` folder.
3. Start the game through SMAPI.

## Config

Edit `config.json` (created on first launch) or use [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098):

| Option | Default | What it does |
|---|---|---|
| `Enabled` | `true` | Turns the guard on or off. |
| `OverrideKey` | `LeftShift, RightShift` | Hold this while placing to put down another staircase anyway. Any [SMAPI keybind](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings) works, including controller buttons. |
| `AllowUnderPlayer` | `true` | Lets you place a staircase on the tile you're standing on. |
| `ShowHint` | `true` | Shows a short message when a staircase was blocked. |

## Compatibility

- Stardew Valley 1.6+, SMAPI 4.0+
- Works in multiplayer and split-screen. Each player only needs it if they want the guard for themselves.
- Translations: English, German.

## Building

Needs the .NET 6+ SDK. Build from the repo root, since the mod uses shared code from `Common`. The build uses [Pathoschild.Stardew.ModBuildConfig](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig), which finds the game folder automatically (or set the `GamePath` environment variable).

```
dotnet build StaircaseGuard -c Release
```

## License

MIT
