# Stardew Mods

Small quality-of-life [SMAPI](https://smapi.io/) mods for Stardew Valley 1.6.

| Mod | What it does |
|---|---|
| [Sign Lock](SignLock) | Stops signs from changing their item when you click them by accident. |
| [Staircase Guard](StaircaseGuard) | Stops you from wasting a second staircase on a mine level where you already placed one. |
| [Enchantment Info](EnchantmentInfo) | Hold Shift while hovering an enchanted tool or weapon to see what its enchantments do. |
| [Fridge Link](FridgeLink) | Link any chest to the kitchen, so cooking uses its ingredients too. |

## Planned

Ideas players asked for that no mod covers yet (checked November 2026). Not started; order is not a promise.

| Idea | What it would do | Asked for in |
|---|---|---|
| Pony horses | Pony skins for the horse as a Content Patcher pack, remaking the 2016 [My Little Pony horse mod](https://www.nexusmods.com/stardewvalley/mods/123) by fumaryako, which still replaces game files directly. Credit goes to the original. | Nexus |
| Jas moves in | After marrying Shane, Jas comes to live on the farm. | [r/StardewValleyMods](https://www.reddit.com/r/StardewValleyMods/comments/1qz82fb) |
| Iridium monsters | Monsters get the double health and speed of the red Iridium Bat, and its red tint. | [mod-ideas #1468](https://github.com/StardewModders/mod-ideas/issues/1468) |
| Quiet town trees | Drops the "I shouldn't cut down trees in town" message without making town trees choppable. | [r/StardewValleyMods](https://www.reddit.com/r/StardewValleyMods/comments/1o4w7wx) |
| Green Rain toggle | Turns Green Rain off. | [Steam](https://steamcommunity.com/app/413150/discussions/0/844005624123510853/) |
| Grass under things | The scythe leaves grass under fences, furniture and machines alone. | [r/StardewValleyMods](https://www.reddit.com/r/StardewValleyMods/comments/1vsz8ja) |
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
