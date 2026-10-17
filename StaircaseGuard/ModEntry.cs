using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common;
using StardewValley;
using StardewValley.Locations;
using SObject = StardewValley.Object;

namespace StaircaseGuard;

internal sealed class ModEntry : Mod
{
    private const string StaircaseId = "(BC)71";

    private static ModEntry Instance = null!;
    private ModConfig Config = null!;
    private HintBox Hint = null!;

    /// <summary>The mine level the local player already placed a staircase on, until they leave it.</summary>
    private readonly PerScreen<MineShaft?> PlacedOn = new();

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();
        Hint = new HintBox(helper.Events);

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_PlacementAction)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_PlacementAction))
        );

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Player.Warped += OnWarped;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        gmcm.Register(ModManifest, reset: () => Config = new ModConfig(), save: () => Helper.WriteConfig(Config));
        gmcm.AddBoolOption(ModManifest, () => Config.Enabled, v => Config.Enabled = v,
            () => Helper.Translation.Get("config.enabled.name"), () => Helper.Translation.Get("config.enabled.tooltip"));
        gmcm.AddKeybindList(ModManifest, () => Config.OverrideKey, v => Config.OverrideKey = v,
            () => Helper.Translation.Get("config.override-key.name"), () => Helper.Translation.Get("config.override-key.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.ShowHint, v => Config.ShowHint = v,
            () => Helper.Translation.Get("config.show-hint.name"), () => Helper.Translation.Get("config.show-hint.tooltip"));
    }

    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (e.IsLocalPlayer)
            PlacedOn.Value = null;
    }

    /// <summary>Block a staircase if the player already placed one on this level and the override key isn't held.</summary>
    private static bool Before_PlacementAction(SObject __instance, GameLocation location, Farmer who, ref bool __result)
    {
        try
        {
            var config = Instance.Config;
            if (!config.Enabled || __instance.QualifiedItemId != StaircaseId || location is not MineShaft mine)
                return true;
            if ((who ?? Game1.player) is not { IsLocalPlayer: true })
                return true;
            if (!ReferenceEquals(Instance.PlacedOn.Value, mine) || config.OverrideKey.IsDown())
                return true;

            if (config.ShowHint)
                Instance.Hint.Show(Instance.Helper.Translation.Get("hint.blocked", new { key = KeyNames.Describe(config.OverrideKey) }));

            __result = false;
            return false;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(Before_PlacementAction)}:\n{ex}", LogLevel.Error);
            return true;
        }
    }

    /// <summary>Remember the level once a staircase was actually placed there.</summary>
    private static void After_PlacementAction(SObject __instance, GameLocation location, Farmer who, bool __result)
    {
        if (__result && __instance.QualifiedItemId == StaircaseId && location is MineShaft mine && (who ?? Game1.player) is { IsLocalPlayer: true })
            Instance.PlacedOn.Value = mine;
    }
}
