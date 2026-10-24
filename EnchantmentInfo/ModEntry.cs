using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Tools;

namespace EnchantmentInfo;

internal sealed class ModEntry : Mod
{
    private const int Padding = 8;

    private static ModEntry Instance = null!;
    private ModConfig Config = null!;

    /// <summary>The text width of the tooltip being drawn, measured when the game asks for its size right before drawing it.</summary>
    private static int TextWidth = 272;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();

        var harmony = new Harmony(ModManifest.UniqueID);
        foreach (Type type in new[] { typeof(Tool), typeof(MeleeWeapon) })
        {
            harmony.Patch(
                original: AccessTools.DeclaredMethod(type, nameof(Tool.getExtraSpaceNeededForTooltipSpecialIcons)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_GetExtraSpace))
            );
            harmony.Patch(
                original: AccessTools.DeclaredMethod(type, nameof(Tool.drawTooltip)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_DrawTooltip))
            );
        }

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        gmcm.Register(ModManifest, reset: () => Config = new ModConfig(), save: () => Helper.WriteConfig(Config));
        gmcm.AddKeybindList(ModManifest, () => Config.ExpandKey, v => Config.ExpandKey = v,
            () => Helper.Translation.Get("config.expand-key.name"), () => Helper.Translation.Get("config.expand-key.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.ShowHint, v => Config.ShowHint = v,
            () => Helper.Translation.Get("config.show-hint.name"), () => Helper.Translation.Get("config.show-hint.tooltip"));
    }

    /// <summary>Make the tooltip tall enough for the extra text.</summary>
    private static void After_GetExtraSpace(Tool __instance, int minWidth, ref Point __result)
    {
        try
        {
            if (__result.Y == 0)
                return;

            TextWidth = (__result.X != 0 ? __result.X : minWidth) - 32;
            if (Instance.GetText(__instance, out _) is { } text)
                __result.Y += (int)Game1.smallFont.MeasureString(text).Y + Padding;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(After_GetExtraSpace)}:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Draw the extra text below the enchantment names.</summary>
    private static void After_DrawTooltip(Tool __instance, SpriteBatch spriteBatch, int x, ref int y, float alpha)
    {
        try
        {
            if (Instance.GetText(__instance, out bool expanded) is not { } text)
                return;

            Color color = Game1.textColor * (expanded ? 0.85f : 0.5f) * alpha;
            Utility.drawTextWithShadow(spriteBatch, text, Game1.smallFont, new Vector2(x + 16, y + 16 + 4), color);
            y += (int)Game1.smallFont.MeasureString(text).Y + Padding;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(After_DrawTooltip)}:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Get the wrapped text to show for a tool: its enchantment descriptions while the key is held, otherwise the hint.</summary>
    private string? GetText(Tool tool, out bool expanded)
    {
        expanded = false;
        var described = GetDescriptions(tool).ToList();
        if (described.Count == 0)
            return null;

        if (!Config.ExpandKey.IsDown())
            return Config.ShowHint ? Game1.parseText(Helper.Translation.Get("hint", new { key = KeyNames.Describe(Config.ExpandKey) }), Game1.smallFont, TextWidth) : null;

        expanded = true;
        return string.Join("\n", described.Select(d =>
            Game1.parseText(described.Count > 1 ? $"{d.Name}: {d.Text}" : d.Text, Game1.smallFont, TextWidth)));
    }

    /// <summary>Get the descriptions for the enchantments the tooltip shows by name.</summary>
    private IEnumerable<(string Name, string Text)> GetDescriptions(Tool tool)
    {
        foreach (BaseEnchantment enchantment in tool.enchantments)
        {
            bool hidden = enchantment.IsSecondaryEnchantment() ? BaseEnchantment.hideSecondaryEnchantName : BaseEnchantment.hideEnchantmentName;
            if (!enchantment.ShouldBeDisplayed() || hidden)
                continue;

            // a tool-specific text if the enchantment works differently per tool, e.g. "enchantment.GenerousEnchantment.Pan"
            string key = $"enchantment.{enchantment.GetType().Name}";
            Translation text = Helper.Translation.Get($"{key}.{tool.GetType().Name}");
            if (!text.HasValue())
                text = Helper.Translation.Get(key);
            if (text.HasValue())
                yield return (enchantment.GetDisplayName(), text);
        }
    }
}
