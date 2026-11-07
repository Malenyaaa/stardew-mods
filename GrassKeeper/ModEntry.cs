using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace GrassKeeper;

internal sealed class ModEntry : Mod
{
    private static ModEntry Instance = null!;
    private ModConfig Config = null!;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(Grass), nameof(Grass.performToolAction)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_PerformToolAction))
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
        gmcm.AddBoolOption(ModManifest, () => Config.UnderFences, v => Config.UnderFences = v,
            () => Helper.Translation.Get("config.under-fences.name"), () => Helper.Translation.Get("config.under-fences.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.UnderObjects, v => Config.UnderObjects = v,
            () => Helper.Translation.Get("config.under-objects.name"), () => Helper.Translation.Get("config.under-objects.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.UnderFurniture, v => Config.UnderFurniture = v,
            () => Helper.Translation.Get("config.under-furniture.name"), () => Helper.Translation.Get("config.under-furniture.tooltip"));
        gmcm.AddBoolOption(ModManifest, () => Config.AllWeapons, v => Config.AllWeapons = v,
            () => Helper.Translation.Get("config.all-weapons.name"), () => Helper.Translation.Get("config.all-weapons.tooltip"));
    }

    /// <summary>Leave grass alone when a weapon swing hits it on a tile that has something placed on it.</summary>
    private static bool Before_PerformToolAction(Grass __instance, Tool t, int explosion, Vector2 tileLocation, ref bool __result)
    {
        try
        {
            var config = Instance.Config;
            if (!config.Enabled || explosion > 0 || t is not MeleeWeapon weapon)
                return true;
            if (!weapon.isScythe() && !config.AllWeapons)
                return true;

            GameLocation? location = __instance.Location ?? Game1.currentLocation;
            if (location is null || !IsCovered(location, tileLocation, config))
                return true;

            __result = false;
            return false;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(Before_PerformToolAction)}:\n{ex}", LogLevel.Error);
            return true;
        }
    }

    /// <summary>Whether something placed by the player stands on this tile.</summary>
    private static bool IsCovered(GameLocation location, Vector2 tile, ModConfig config)
    {
        if (location.objects.TryGetValue(tile, out SObject? obj) && IsPlacedObject(obj))
        {
            if (obj is Fence ? config.UnderFences : config.UnderObjects)
                return true;
        }

        if (config.UnderFurniture && location.furniture.Count > 0)
        {
            var center = new Point((int)tile.X * Game1.tileSize + Game1.tileSize / 2, (int)tile.Y * Game1.tileSize + Game1.tileSize / 2);
            foreach (Furniture furniture in location.furniture)
            {
                if (furniture.GetBoundingBox().Contains(center))
                    return true;
            }
        }

        return false;
    }

    /// <summary>Whether an object was put there by a player, as opposed to weeds, stones, twigs, forage or dig spots the game spawned.</summary>
    private static bool IsPlacedObject(SObject obj)
    {
        if (obj.IsSpawnedObject || obj.Category == SObject.litterCategory)
            return false;
        if (obj.IsWeeds() || obj.IsBreakableStone() || obj.IsTwig())
            return false;
        return obj.QualifiedItemId is not ("(O)590" or "(O)SeedSpot");
    }
}
