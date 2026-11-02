using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace FridgeLink;

internal sealed class ModEntry : Mod
{
    private const string MiniFridgeId = "(BC)216";

    private static ModEntry Instance = null!;
    private string LinkedKey = null!;

    /// <summary>Whether Better Crafting replaces the kitchen menu, in which case linked chests are added through its API instead.</summary>
    private bool UsingBetterCrafting;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        LinkedKey = $"{ModManifest.UniqueID}/Linked";

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.ActivateKitchen)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_ActivateKitchen))
        );
        // draw the button as part of the chest menu (right after the organize button), so tooltips, the held item and
        // anything drawn over the menu stay on top of it
        harmony.Patch(
            original: AccessTools.Method(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.draw), new[] { typeof(SpriteBatch) }),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_ClickableTextureComponent_Draw))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.performHoverAction)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_ItemGrabMenu_PerformHoverAction))
        );

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Better Crafting's event can't be mapped through a SMAPI API interface, so subscribe to it directly
        object? api = Helper.ModRegistry.GetApi("leclair.bettercrafting");
        EventInfo? populate = api?.GetType().GetEvent("MenuPopulateContainers");
        if (api is null || populate?.EventHandlerType is not { } handlerType)
            return;

        ParameterExpression arg = Expression.Parameter(handlerType.GetGenericArguments()[0], "e");
        Delegate handler = Expression.Lambda(
            handlerType,
            Expression.Call(Expression.Constant(this), typeof(ModEntry).GetMethod(nameof(OnBetterCraftingPopulateContainers), BindingFlags.Instance | BindingFlags.NonPublic)!, Expression.Convert(arg, typeof(object))),
            arg
        ).Compile();
        populate.AddEventHandler(api, handler);
        UsingBetterCrafting = true;
    }

    /// <summary>Add the linked chests when Better Crafting opens a cooking menu.</summary>
    private void OnBetterCraftingPopulateContainers(object e)
    {
        try
        {
            object? menu = GetProperty(e, "Menu");
            if (menu is null || GetProperty(menu, "Cooking") is not true)
                return;
            if (GetProperty(e, "Containers") is not IList<Tuple<object, GameLocation?>> containers)
                return;

            foreach (Chest chest in GetLinkedChests())
            {
                if (!containers.Any(c => ReferenceEquals(c.Item1, chest)))
                    containers.Add(new Tuple<object, GameLocation?>(chest, chest.Location));
            }
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed adding linked chests to Better Crafting:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Read a property, including one implemented explicitly for an interface.</summary>
    private static object? GetProperty(object obj, string name)
    {
        Type type = obj.GetType();
        PropertyInfo? property = type.GetProperty(name) ?? type.GetInterfaces().Select(i => i.GetProperty(name)).FirstOrDefault(p => p != null);
        return property?.GetValue(obj);
    }

    /// <summary>Get the chest whose menu is open, if it's a normal chest that can be linked.</summary>
    private static Chest? GetLinkableChest(IClickableMenu? menu)
    {
        if (menu is ItemGrabMenu { context: Chest chest } && chest.playerChest.Value && !chest.fridge.Value
            && chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.BigChest)
            return chest;
        return null;
    }

    /// <summary>The link button, below the organize button on the right of the chest menu.</summary>
    private static Rectangle GetButtonBounds(IClickableMenu menu)
    {
        if (menu is ItemGrabMenu { organizeButton: { } organize })
            return new Rectangle(organize.bounds.X, organize.bounds.Bottom + 16, 64, 64);
        return new Rectangle(menu.xPositionOnScreen + menu.width, menu.yPositionOnScreen + menu.height / 3 - 64 + 80, 64, 64);
    }

    private bool IsLinked(Chest chest)
    {
        return chest.modData.ContainsKey(LinkedKey);
    }

    private static bool IsMouseOver(Rectangle bounds)
    {
        return bounds.Contains(Game1.getMouseX(ui_scale: true), Game1.getMouseY(ui_scale: true));
    }

    /// <summary>Draw the link button right after the chest menu's organize button.</summary>
    private static void After_ClickableTextureComponent_Draw(ClickableTextureComponent __instance, SpriteBatch b)
    {
        try
        {
            IClickableMenu menu = Game1.activeClickableMenu;
            if (menu is not ItemGrabMenu grabMenu || !ReferenceEquals(__instance, grabMenu.organizeButton) || GetLinkableChest(menu) is not { } chest)
                return;

            Rectangle bounds = GetButtonBounds(menu);
            bool linked = Instance.IsLinked(chest);

            // the mini fridge sprite is 16x32, so draw it at half the usual scale to fit the 64x64 button
            ParsedItemData fridge = ItemRegistry.GetDataOrErrorItem(MiniFridgeId);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White, 4f, drawShadow: false);
            float scale = IsMouseOver(bounds) ? 1.6f : 1.4f;
            Rectangle source = fridge.GetSourceRect();
            Vector2 center = new(bounds.Center.X, bounds.Center.Y);
            b.Draw(fridge.GetTexture(), center, source, Color.White * (linked ? 1f : 0.4f), 0f, new Vector2(source.Width / 2f, source.Height / 2f), scale, default, 1f);
        }
        catch (Exception ex)
        {
            Instance.Monitor.LogOnce($"Failed drawing the link button:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Show the link button's tooltip through the menu, so it's drawn like the other buttons' tooltips.</summary>
    private static void After_ItemGrabMenu_PerformHoverAction(ItemGrabMenu __instance)
    {
        try
        {
            if (!ReferenceEquals(__instance, Game1.activeClickableMenu) || GetLinkableChest(__instance) is not { } chest)
                return;

            if (IsMouseOver(GetButtonBounds(__instance)))
                __instance.hoverText = Instance.Helper.Translation.Get(Instance.IsLinked(chest) ? "button.unlink" : "button.link");
        }
        catch (Exception ex)
        {
            Instance.Monitor.LogOnce($"Failed setting the link button's tooltip:\n{ex}", LogLevel.Error);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button != SButton.MouseLeft || GetLinkableChest(Game1.activeClickableMenu) is not { } chest)
            return;

        if (!IsMouseOver(GetButtonBounds(Game1.activeClickableMenu)))
            return;

        Helper.Input.Suppress(e.Button);
        if (IsLinked(chest))
            chest.modData.Remove(LinkedKey);
        else
            chest.modData[LinkedKey] = "true";
        Game1.playSound("drumkit6");
    }

    /// <summary>Find every linked chest in the world.</summary>
    private List<Chest> GetLinkedChests()
    {
        var chests = new List<Chest>();
        Utility.ForEachLocation(location =>
        {
            foreach (StardewValley.Object obj in location.objects.Values)
            {
                if (obj is Chest chest && !chest.fridge.Value && IsLinked(chest))
                    chests.Add(chest);
            }
            return true;
        });
        return chests;
    }

    /// <summary>Same as the game's kitchen, plus the linked chests. Falls back to the game's code if anything goes wrong.</summary>
    private static bool Before_ActivateKitchen(GameLocation __instance)
    {
        try
        {
            if (Instance.UsingBetterCrafting)
                return true;

            List<Chest> linked = Instance.GetLinkedChests();
            if (linked.Count == 0)
                return true;

            var mutexes = new List<NetMutex>();
            var miniFridges = new List<Chest>();
            foreach (StardewValley.Object obj in __instance.objects.Values)
            {
                if (obj != null && obj.bigCraftable.Value && obj is Chest chest && chest.fridge.Value)
                {
                    miniFridges.Add(chest);
                    mutexes.Add(chest.mutex);
                }
            }
            Chest? fridge = __instance.GetFridge();
            if (fridge != null)
                mutexes.Add(fridge.mutex);
            foreach (Chest chest in linked)
                mutexes.Add(chest.mutex);

            new MultipleMutexRequest(mutexes, request =>
            {
                var containers = new List<IInventory>();
                if (fridge != null)
                    containers.Add(fridge.Items);
                foreach (Chest chest in miniFridges)
                    containers.Add(chest.Items);
                foreach (Chest chest in linked)
                    containers.Add(chest.Items);

                Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
                Game1.activeClickableMenu = new CraftingPage((int)pos.X, (int)pos.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, cooking: true, standaloneMenu: true, containers);
                Game1.activeClickableMenu.exitFunction = request.ReleaseLocks;
            }, _ =>
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
            });
            return false;
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed in {nameof(Before_ActivateKitchen)}:\n{ex}", LogLevel.Error);
            return true;
        }
    }
}
