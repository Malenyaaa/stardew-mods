using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace QuietTownTrees;

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
            original: AccessTools.Method(typeof(Tree), nameof(Tree.performToolAction)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Transpile_PerformToolAction))
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
    }

    /// <summary>
    /// In <see cref="Tree.performToolAction"/>, the town tree check shows its message through
    /// <c>Game1.drawObjectDialogue(string)</c> and then returns without damaging the tree. Route that one call
    /// through <see cref="ShowTownTreeWarning"/>, so the tree stays protected but the message can be skipped.
    /// </summary>
    private static IEnumerable<CodeInstruction> Transpile_PerformToolAction(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo drawDialogue = AccessTools.Method(typeof(Game1), nameof(Game1.drawObjectDialogue), new[] { typeof(string) });
        MethodInfo replacement = AccessTools.Method(typeof(ModEntry), nameof(ShowTownTreeWarning));

        int patched = 0;
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(drawDialogue))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = replacement;
                patched++;
            }
            yield return instruction;
        }

        if (patched != 1)
            Instance.Monitor.Log($"Expected to find the town tree message once in Tree.performToolAction, but found it {patched} times. The message may still show; please report this.", LogLevel.Warn);
    }

    /// <summary>Show the town tree message unless the mod hides it.</summary>
    private static void ShowTownTreeWarning(string message)
    {
        try
        {
            if (!Instance.Config.Enabled)
                Game1.drawObjectDialogue(message);
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(ShowTownTreeWarning)}:\n{ex}", LogLevel.Error);
        }
    }
}
