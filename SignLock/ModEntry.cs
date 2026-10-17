using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common;
using StardewValley;
using StardewValley.Objects;

namespace SignLock;

internal sealed class ModEntry : Mod
{
    private static ModEntry Instance = null!;
    private ModConfig Config = null!;
    private HintBox Hint = null!;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();
        Hint = new HintBox(helper.Events);

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(Sign), nameof(Sign.checkForAction)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_CheckForAction))
        );

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        gmcm.Register(ModManifest, reset: () => Config = new ModConfig(), save: () => Helper.WriteConfig(Config));
        gmcm.AddBoolOption(ModManifest, () => Config.Enabled, v => Config.Enabled = v,
            () => Helper.Translation.Get("config.enabled.name"), () => Helper.Translation.Get("config.enabled.tooltip"));
        gmcm.AddKeybindList(ModManifest, () => Config.UnlockKey, v => Config.UnlockKey = v,
            () => Helper.Translation.Get("config.unlock-key.name"), () => Helper.Translation.Get("config.unlock-key.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.ShowHint, v => Config.ShowHint = v,
            () => Helper.Translation.Get("config.show-hint.name"), () => Helper.Translation.Get("config.show-hint.tooltip"));
    }

    /// <summary>Swallow the click if the sign already shows an item and the unlock key isn't held.</summary>
    private static bool Before_CheckForAction(Sign __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
    {
        try
        {
            var config = Instance.Config;
            if (!config.Enabled || justCheckingForActivity || !who.IsLocalPlayer)
                return true;
            if (__instance.displayItem.Value is null || who.ActiveItem is null)
                return true;
            if (config.UnlockKey.IsDown())
                return true;

            if (config.ShowHint)
                Instance.Hint.Show(Instance.Helper.Translation.Get("hint.locked", new { key = KeyNames.Describe(config.UnlockKey) }));

            __result = true;
            return false;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(Before_CheckForAction)}:\n{ex}", LogLevel.Error);
            return true;
        }
    }
}
