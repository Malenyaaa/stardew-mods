# Green Rain Toggle

A small [SMAPI](https://smapi.io/) mod for Stardew Valley 1.6.

Once every summer, Stardew 1.6 sends a day of **Green Rain**: weeds and mossy bushes cover the paths, trees grow huge and green, and the next morning you're clearing up. If you'd rather skip it, this mod turns it off.

The day Green Rain would have come gets **normal rain** instead, so your crops still get watered. If you'd rather have that day's normal random weather, you can switch that in the config.

## Calendar switch

The calendar on the town billboard has a small weather symbol at the top right, in every season. Click it to turn Green Rain on or off. Hover over it to see whether it is on or off. It's the same setting as in the config.

## What changes

Nothing that's already in your world is removed. Mossy trees, moss on trees and trees that grew up during an earlier Green Rain all stay. The mod only stops new Green Rain days.

- No Green Rain, no Green Rain weeds or bushes, no temporary Green Rain trees.
- The TV weather forecast shows rain (or the normal forecast) for that day.
- Moss still grows on trees as usual, just without the big Green Rain boost. Moss and Mossy Seeds stay available.
- In year 1, Green Rain comes with a few one-time moments (a letter from Gus, the town meeting at the saloon). These are skipped too. Turn on `KeepFirstYear` if you want to keep them.
- Robin keeps building on that day. Weddings can now also be scheduled on that day.

A Green Rain that's already happening when you install the mod or change the setting finishes normally and is cleaned up the next morning, as usual. Changes take effect from the next day.

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the latest version from [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/55178) or [GitHub](https://github.com/Malenyaaa/stardew-mods/releases) and unzip it into your `Mods` folder.
3. Start the game through SMAPI.

## Config

Edit `config.json` (created on first launch) or use [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098):

| Option | Default | What it does |
|---|---|---|
| `Enabled` | `true` | Turns Green Rain off. |
| `ReplaceWithRain` | `true` | The day Green Rain would have come gets normal rain. If `false`, that day gets the normal random weather. |
| `KeepFirstYear` | `false` | Keeps Green Rain in the first year, so its one-time story moments still happen. |

Console command: `green_rain_day` shows which summer day Green Rain would come this year.

## Compatibility

- Stardew Valley 1.6+, SMAPI 4.0+
- Multiplayer: the weather is decided by the host, so the host needs the mod. Other players don't need it.
- Translations: English, German.

## Building

Needs the .NET 6+ SDK. Build from the repo root, since the mod uses shared code from `Common`. The build uses [Pathoschild.Stardew.ModBuildConfig](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig), which finds the game folder automatically (or set the `GamePath` environment variable).

```
dotnet build GreenRainToggle -c Release
```

## AI disclosure

This mod was written with the help of an AI coding assistant (Claude by Anthropic). I came up with the idea, decided how it should behave, tested it in-game and reviewed the result. The AI wrote most of the code.

## License

MIT
