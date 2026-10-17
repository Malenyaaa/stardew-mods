using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
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

    /// <summary>Whether the local player is placing a staircase right now (between the placementAction prefix and postfix).</summary>
    private static bool PlacingStaircase;

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
        harmony.Patch(
            original: AccessTools.Method(typeof(SObject), nameof(SObject.canBePlacedHere)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_CanBePlacedHere))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.recursiveTryToCreateLadderDown)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_RecursiveTryToCreateLadderDown))
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
        gmcm.AddBoolOption(ModManifest, () => Config.AllowUnderPlayer, v => Config.AllowUnderPlayer = v,
            () => Helper.Translation.Get("config.allow-under-player.name"), () => Helper.Translation.Get("config.allow-under-player.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.ShowHint, v => Config.ShowHint = v,
            () => Helper.Translation.Get("config.show-hint.name"), () => Helper.Translation.Get("config.show-hint.tooltip"));
    }

    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (e.IsLocalPlayer)
            PlacedOn.Value = null;
    }

    /// <summary>Let the local player place a staircase on the tile they're standing on.</summary>
    private static void Before_CanBePlacedHere(SObject __instance, GameLocation l, ref CollisionMask collisionMask)
    {
        if (Instance.Config.AllowUnderPlayer && __instance.QualifiedItemId == StaircaseId && l is MineShaft)
            collisionMask &= ~CollisionMask.Farmers;
    }

    /// <summary>The game skips tiles with a farmer on them when picking the ladder spot; use the clicked tile anyway if that's the only problem.</summary>
    private static bool Before_RecursiveTryToCreateLadderDown(MineShaft __instance, Vector2 centerTile, string sound, ref bool __result)
    {
        try
        {
            if (!PlacingStaircase || !Instance.Config.AllowUnderPlayer)
                return true;
            if (__instance.IsTileOccupiedBy(centerTile, CollisionMask.All & ~CollisionMask.Farmers)
                || !__instance.isTileOnClearAndSolidGround(centerTile)
                || __instance.doesTileHaveProperty((int)centerTile.X, (int)centerTile.Y, "Type", "Back") != "Stone")
                return true;

            __instance.createLadderAt(centerTile, sound);
            __result = true;
            return false;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(Before_RecursiveTryToCreateLadderDown)}:\n{ex}", LogLevel.Error);
            return true;
        }
    }

    /// <summary>Block a staircase if the player already placed one on this level and the override key isn't held.</summary>
    private static bool Before_PlacementAction(SObject __instance, GameLocation location, Farmer who, ref bool __result)
    {
        try
        {
            var config = Instance.Config;
            if (__instance.QualifiedItemId != StaircaseId || location is not MineShaft mine)
                return true;
            if ((who ?? Game1.player) is not { IsLocalPlayer: true })
                return true;

            PlacingStaircase = true;
            if (!config.Enabled || !ReferenceEquals(Instance.PlacedOn.Value, mine) || config.OverrideKey.IsDown())
                return true;
            PlacingStaircase = false;

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
        PlacingStaircase = false;
        if (__result && __instance.QualifiedItemId == StaircaseId && location is MineShaft mine && (who ?? Game1.player) is { IsLocalPlayer: true })
            Instance.PlacedOn.Value = mine;
    }
}
