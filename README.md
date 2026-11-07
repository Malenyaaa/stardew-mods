# Stardew Mods

Small quality-of-life [SMAPI](https://smapi.io/) mods for Stardew Valley 1.6.

| Mod | What it does |
|---|---|
| [Sign Lock](SignLock) | Stops signs from changing their item when you click them by accident. |
| [Staircase Guard](StaircaseGuard) | Stops you from wasting a second staircase on a mine level where you already placed one. |
| [Enchantment Info](EnchantmentInfo) | Hold Shift while hovering an enchanted tool or weapon to see what its enchantments do. |
| [Fridge Link](FridgeLink) | Link any chest to the kitchen, so cooking uses its ingredients too. |
| [My Little Pony Horse](PonyHorses) | Turn your horse into one of the Mane Six ponies and switch any time in-game. Sprites not included in this repo. |
| [Quiet Town Trees](QuietTownTrees) | Removes the "I shouldn't cut down trees in town" message. Town trees still can't be cut down. |
| [Grass Keeper](GrassKeeper) | Your scythe leaves the grass under fences, machines and furniture alone. Grass on open ground is cut as usual. |
| [Green Rain Toggle](GreenRainToggle) | Turns off Green Rain. The day it would have come gets normal rain instead. |

## Planned

Ideas players asked for that no mod covers yet (checked November 2026). Not started; order is not a promise.

| Idea | What it would do | Asked for in |
|---|---|---|
| Jas moves in | After marrying Shane, Jas comes to live on the farm. | [r/StardewValleyMods](https://www.reddit.com/r/StardewValleyMods/comments/1qz82fb) |
| Iridium monsters | Monsters get the double health and speed of the red Iridium Bat, and its red tint. | [mod-ideas #1468](https://github.com/StardewModders/mod-ideas/issues/1468) |
| Big shipping totals | Shows end-of-day totals above 10 million gold correctly. | [r/SMAPI](https://www.reddit.com/r/SMAPI/comments/1srmy55) |
| Trinket hats | Hats on the vanilla trinket companions (parrot, frog, fairy). | [r/StardewValleyMods](https://www.reddit.com/r/StardewValleyMods/comments/1vn02py) |

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the mod you want from the [releases](https://github.com/Malenyaaa/stardew-mods/releases) and unzip it into your `Mods` folder.
3. Start the game through SMAPI.

## Building

Needs the .NET 6+ SDK. Each mod folder is its own project; shared code lives in `Common` and is compiled into every mod, so the mods don't depend on each other. The build uses [Pathoschild.Stardew.ModBuildConfig](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig), which finds the game folder automatically (or set the `GamePath` environment variable).

```
dotnet build StaircaseGuard -c Release
```

## AI disclosure

These mods were written with the help of an AI coding assistant (Claude by Anthropic). I came up with the ideas, decided how things should behave, tested everything in-game and reviewed the result. The AI wrote most of the code.

## License

MIT
