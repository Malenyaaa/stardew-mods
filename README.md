# Stardew Mods

Small quality-of-life [SMAPI](https://smapi.io/) mods for Stardew Valley 1.6.

| Mod | What it does |
|---|---|
| [Sign Lock](SignLock) | Stops signs from changing their item when you click them by accident. |
| [Staircase Guard](StaircaseGuard) | Stops you from wasting a second staircase on a mine level where you already placed one. |
| [Enchantment Info](EnchantmentInfo) | Hold Shift while hovering an enchanted tool or weapon to see what its enchantments do. |
| [Fridge Link](FridgeLink) | Link any chest to the kitchen, so cooking uses its ingredients too. |

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
