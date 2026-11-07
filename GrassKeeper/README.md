# Grass Keeper

A small [SMAPI](https://smapi.io/) mod for Stardew Valley 1.6.

Since 1.6, grass can grow under fences, machines and other things you place, and it looks nice there. But every swing of the scythe along a fence line mows it away again. With this mod, **your scythe leaves the grass under placed things alone**. Grass on open ground is cut as usual, so you still get your hay.

What counts as "under something":

- fences and gates
- placed objects: machines, chests, sprinklers, scarecrows, signs and so on
- furniture placed outside, like benches or lamps

Weeds, stones, twigs, forage and dig spots don't count, so grass under them is cut normally.

By default swords and daggers leave the grass alone too. Bombs still blow it away.

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the latest version from [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/55175) or [GitHub](https://github.com/Malenyaaa/stardew-mods/releases) and unzip it into your `Mods` folder.
3. Start the game through SMAPI.

## Config

Edit `config.json` (created on first launch) or use [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098):

| Option | Default | What it does |
|---|---|---|
| `Enabled` | `true` | Turns the mod on or off. |
| `UnderFences` | `true` | Keeps the grass under fences and gates. |
| `UnderObjects` | `true` | Keeps the grass under placed objects like machines, chests, sprinklers or signs. |
| `UnderFurniture` | `true` | Keeps the grass under furniture placed outside. |
| `AllWeapons` | `true` | Swords and daggers leave the grass alone too. When off, only scythes do. |

## Compatibility

- Stardew Valley 1.6+, SMAPI 4.0+
- Works in multiplayer and split-screen. Each player only needs it if they want it for their own scythe.
- Translations: English, German.

## Building

Needs the .NET 6+ SDK. Build from the repo root, since the mod uses shared code from `Common`. The build uses [Pathoschild.Stardew.ModBuildConfig](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig), which finds the game folder automatically (or set the `GamePath` environment variable).

```
dotnet build GrassKeeper -c Release
```

## AI disclosure

This mod was written with the help of an AI coding assistant (Claude by Anthropic). I came up with the idea, decided how it should behave, tested it in-game and reviewed the result. The AI wrote most of the code.

## License

MIT
