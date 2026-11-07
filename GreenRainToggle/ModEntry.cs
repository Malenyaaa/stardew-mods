using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;

namespace GreenRainToggle;

internal sealed class ModEntry : Mod
{
    private const string GreenRain = "GreenRain";

    private static ModEntry Instance = null!;
    private ModConfig Config = null!;

    /// <summary>Whether the game is picking a day's weather right now. While it does, <see cref="Utility.isGreenRainDay(int, Season)"/> answers truthfully so the weather code can swap Green Rain for something else.</summary>
    [ThreadStatic]
    private static bool PickingWeather;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(Utility), nameof(Utility.isGreenRainDay), new[] { typeof(int), typeof(Season) }),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_IsGreenRainDay))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Game1), nameof(Game1.getWeatherModificationsForDate)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_GetWeatherModificationsForDate)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_GetWeatherModificationsForDate)),
            finalizer: new HarmonyMethod(typeof(ModEntry), nameof(Finally_GetWeatherModificationsForDate))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(LocationWeather), nameof(LocationWeather.UpdateDailyWeather)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_UpdateDailyWeather))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Billboard), nameof(Billboard.performHoverAction)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_BillboardHover))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Billboard), nameof(Billboard.receiveLeftClick)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_BillboardClick))
        );

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
        helper.ConsoleCommands.Add("green_rain_day", "Shows which summer day Green Rain would come this year.", OnGreenRainDayCommand);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        gmcm.Register(ModManifest, reset: () => Config = new ModConfig(), save: () => Helper.WriteConfig(Config));
        gmcm.AddBoolOption(ModManifest, () => Config.Enabled, v => Config.Enabled = v,
            () => Helper.Translation.Get("config.enabled.name"), () => Helper.Translation.Get("config.enabled.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.ReplaceWithRain, v => Config.ReplaceWithRain = v,
            () => Helper.Translation.Get("config.replace-with-rain.name"), () => Helper.Translation.Get("config.replace-with-rain.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.KeepFirstYear, v => Config.KeepFirstYear = v,
            () => Helper.Translation.Get("config.keep-first-year.name"), () => Helper.Translation.Get("config.keep-first-year.tooltip"));
    }

    /// <summary>
    /// When loading a save, the game decides "is it Green Rain today?" from the calendar instead of the saved weather.
    /// Our patch would make that "no" even if the save really has Green Rain today (e.g. the mod was installed that day).
    /// Use the saved weather instead, so an ongoing Green Rain finishes normally and gets cleaned up the next morning.
    /// </summary>
    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (Context.IsMainPlayer)
            Game1.isGreenRain = Game1.netWorldState.Value.GetWeatherForLocation("Default").IsGreenRain;
    }

    private void OnGreenRainDayCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Monitor.Log("Load a save first.", LogLevel.Info);
            return;
        }

        int day = GetGreenRainDay();
        if (day < 1)
        {
            Monitor.Log($"No Green Rain day found for year {Game1.year}.", LogLevel.Info);
            return;
        }
        string state = IsActive() ? (Config.ReplaceWithRain ? "turned into normal rain" : "turned into normal weather") : "mod is off, so it stays Green Rain";
        Monitor.Log($"Green Rain would come on summer {day} of year {Game1.year} ({state}).", LogLevel.Info);
    }

    /// <summary>The summer day of this year the game picked for Green Rain, or 0 if none.</summary>
    private static int GetGreenRainDay()
    {
        bool wasPicking = PickingWeather;
        PickingWeather = true;
        try
        {
            for (int day = 1; day <= 28; day++)
            {
                if (Utility.isGreenRainDay(day, Season.Summer))
                    return day;
            }
            return 0;
        }
        finally
        {
            PickingWeather = wasPicking;
        }
    }

    /*********
    ** Calendar switch
    *********/
    private static readonly AccessTools.FieldRef<Billboard, string> BillboardHoverText = AccessTools.FieldRefAccess<Billboard, string>("hoverText");

    /// <summary>Get the switch area at the top right of the calendar (not the daily quest board).</summary>
    private static bool TryGetSwitchBounds(Billboard menu, out Rectangle bounds)
    {
        bounds = Rectangle.Empty;
        if (menu.calendarDays is not { Count: >= 7 } days)
            return false;

        // right-aligned with the last calendar column, on the same line as the season and year
        bounds = new Rectangle(days[6].bounds.Right - 56, menu.yPositionOnScreen + 76, 56, 40);
        return true;
    }

    /// <summary>Whether Green Rain will come (as far as this player knows).</summary>
    private static bool GreenRainOn()
    {
        return !IsActive();
    }

    private static string GetSwitchText()
    {
        var t = Instance.Helper.Translation;
        var config = Instance.Config;

        string text = GreenRainOn()
            ? t.Get("calendar.state-on")
            : t.Get(config.ReplaceWithRain ? "calendar.state-off-rain" : "calendar.state-off-weather");

        if (!Context.IsMainPlayer)
            text += "\n" + t.Get("calendar.host-only");
        return text;
    }

    /// <summary>Draw the switch on the calendar.</summary>
    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not Billboard menu || !TryGetSwitchBounds(menu, out Rectangle bounds))
            return;

        var b = e.SpriteBatch;
        var position = new Vector2(bounds.X + 4, bounds.Y + 4);
        if (GreenRainOn())
            b.Draw(Game1.mouseCursors_1_6, position, new Rectangle(243, 293, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        else
        {
            int icon = Config.ReplaceWithRain ? 4 : 2; // rain : sun
            b.Draw(Game1.mouseCursors, position, new Rectangle(317 + 12 * icon, 421, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        }

        // the switch is drawn after the menu, so draw the tooltip and cursor again on top
        string hoverText = BillboardHoverText(menu);
        if (!string.IsNullOrEmpty(hoverText))
            IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
        menu.drawMouse(b);
    }

    /// <summary>Show the switch tooltip.</summary>
    private static void After_BillboardHover(Billboard __instance, int x, int y)
    {
        try
        {
            if (TryGetSwitchBounds(__instance, out Rectangle bounds) && bounds.Contains(x, y))
                BillboardHoverText(__instance) = GetSwitchText();
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(After_BillboardHover)}:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Switch Green Rain on or off when the player clicks the switch.</summary>
    private static void After_BillboardClick(Billboard __instance, int x, int y)
    {
        try
        {
            if (!Context.IsMainPlayer || !TryGetSwitchBounds(__instance, out Rectangle bounds) || !bounds.Contains(x, y))
                return;

            var config = Instance.Config;
            if (IsActive())
                config.Enabled = false;
            else
            {
                config.Enabled = true;
                if (Game1.year == 1)
                    config.KeepFirstYear = false;
            }
            Instance.Helper.WriteConfig(config);
            Game1.playSound("drumkit6");

            // refresh the tooltip right away
            __instance.performHoverAction(x, y);
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(After_BillboardClick)}:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Whether Green Rain should be blocked right now.</summary>
    private static bool IsActive()
    {
        var config = Instance.Config;
        return config.Enabled && !(config.KeepFirstYear && Game1.year == 1);
    }

    /// <summary>Tell the rest of the game there's no Green Rain day (TV picture, wedding dates, game state queries, loading).</summary>
    private static void After_IsGreenRainDay(ref bool __result)
    {
        if (__result && !PickingWeather && IsActive())
            __result = false;
    }

    private static void Before_GetWeatherModificationsForDate()
    {
        PickingWeather = true;
    }

    /// <summary>Swap Green Rain for normal rain (or the day's normal weather). Also used for the TV weather forecast.</summary>
    private static void After_GetWeatherModificationsForDate(string default_weather, ref string __result)
    {
        try
        {
            if (__result != GreenRain || !IsActive())
                return;

            if (Instance.Config.ReplaceWithRain)
                __result = Game1.weather_rain;
            else
                __result = default_weather is null or GreenRain or Game1.weather_festival or Game1.weather_wedding
                    ? Game1.weather_sunny
                    : default_weather;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(After_GetWeatherModificationsForDate)}:\n{ex}", LogLevel.Error);
        }
    }

    private static Exception? Finally_GetWeatherModificationsForDate(Exception? __exception)
    {
        PickingWeather = false;
        return __exception;
    }

    /// <summary>Safety net: if Green Rain still gets through (e.g. from another mod's weather data), turn it into rain when the day starts.</summary>
    private static void After_UpdateDailyWeather(LocationWeather __instance)
    {
        try
        {
            if (!__instance.IsGreenRain || !IsActive())
                return;

            __instance.IsGreenRain = false;
            if (Instance.Config.ReplaceWithRain)
            {
                __instance.IsRaining = true;
                __instance.Weather = Game1.weather_rain;
            }
            else
                __instance.Weather = Game1.weather_sunny;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(After_UpdateDailyWeather)}:\n{ex}", LogLevel.Error);
        }
    }
}
