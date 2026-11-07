# Quiet Town Trees

A small [SMAPI](https://smapi.io/) mod for Stardew Valley 1.6.

Since 1.6, hitting one of the trees along the paths in Pelican Town with your axe pops up a message box: *"I shouldn't cut down trees in town."* When you're swinging your axe around town, that box gets in the way every time.

This mod removes the message. The town trees still can't be cut down: your axe just bounces off with the usual chop sound and the tree shakes, like before. Nothing else changes.

If you want to actually cut the town trees down, use a mod like [Chop Town Trees](https://www.nexusmods.com/stardewvalley/mods/46849) instead.

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the latest version from [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/55177) or [GitHub](https://github.com/Malenyaaa/stardew-mods/releases) and unzip it into your `Mods` folder.
3. Start the game through SMAPI.

## Config

Edit `config.json` (created on first launch) or use [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098):

| Option | Default | What it does |
|---|---|---|
| `Enabled` | `true` | Hides the town tree message. Turn it off to get the message back. |

## Compatibility

- Stardew Valley 1.6+, SMAPI 4.0+
- Works in multiplayer and split-screen. Each player only needs it if they want the message gone for themselves.
- Translations: English, German.

## Building

Needs the .NET 6+ SDK. Build from the repo root, since the mod uses shared code from `Common`. The build uses [Pathoschild.Stardew.ModBuildConfig](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig), which finds the game folder automatically (or set the `GamePath` environment variable).

```
dotnet build QuietTownTrees -c Release
```

## AI disclosure

This mod was written with the help of an AI coding assistant (Claude by Anthropic). I came up with the idea, decided how it should behave, tested it in-game and reviewed the result. The AI wrote most of the code.

## License

MIT
