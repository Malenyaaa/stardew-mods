# Fridge Link

A small [SMAPI](https://smapi.io/) mod for Stardew Valley 1.6.

When you cook, the game only uses ingredients from your inventory, the fridge and mini fridges in the same house. With this mod you can link any chest to the kitchen, wherever it stands, and cooking uses its ingredients too.

Open a chest and click the fridge button below the organize button on the right. A linked chest shows the fridge brightly, an unlinked one shows it faded. Click again to unlink.

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the latest version from [GitHub](https://github.com/Malenyaaa/stardew-mods/releases) and unzip it into your `Mods` folder.
3. Start the game through SMAPI.

## Compatibility

- Stardew Valley 1.6+, SMAPI 4.0+
- Works with and without [Better Crafting](https://www.nexusmods.com/stardewvalley/mods/11115). With Better Crafting, linked chests are added to its cooking menu.
- Normal chests and big chests can be linked. Fridges, mini fridges and special chests (like the Junimo chest) can't.
- Links are saved with the chest, so they stay after reloading. In multiplayer, every player who wants to cook from linked chests needs the mod.
- Translations: English, German.

## Building

Needs the .NET 6+ SDK. Build from the repo root, since the mod uses shared code from `Common`. The build uses [Pathoschild.Stardew.ModBuildConfig](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig), which finds the game folder automatically (or set the `GamePath` environment variable).

```
dotnet build FridgeLink -c Release
```

## AI disclosure

This mod was written with the help of an AI coding assistant (Claude by Anthropic). I came up with the idea, decided how it should behave, tested it in-game and reviewed the result. The AI wrote most of the code.

## License

MIT
