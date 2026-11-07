using System;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;

namespace PonyHorses;

internal sealed class ModEntry : Mod
{
    /// <summary>The selectable ponies: asset file name (without .png) and display name.</summary>
    private static readonly (string Id, string Name)[] Ponies =
    {
        ("twilight", "Twilight Sparkle"),
        ("rarity", "Rarity"),
        ("rainbow", "Rainbow Dash"),
        ("fluttershy", "Fluttershy"),
        ("pinkie", "Pinkie Pie"),
        ("applejack", "Applejack")
    };

    /// <summary>Response key for the normal horse.</summary>
    private const string HorseChoice = "horse";

    /// <summary>Response key for closing the menu without a change.</summary>
    private const string CancelChoice = "cancel";

    private static ModEntry Instance = null!;
    private ModConfig Config = null!;
    private HintBox Hint = null!;

    /// <summary>The horse mod data key holding the chosen pony ID.</summary>
    private string PonyKey = null!;

    /// <summary>The asset name prefix of the pony textures.</summary>
    private string AssetPrefix = null!;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();
        Hint = new HintBox(helper.Events);
        PonyKey = $"{ModManifest.UniqueID}/Pony";
        AssetPrefix = $"Mods/{ModManifest.UniqueID}/";

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(Horse), nameof(Horse.checkAction)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_Horse_CheckAction))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Horse), nameof(Horse.draw), new[] { typeof(SpriteBatch) }),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_Horse_Draw))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.showRiding)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(After_Farmer_ShowRiding))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new[] { typeof(SpriteBatch) }),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Before_Farmer_Draw))
        );

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        gmcm.Register(ModManifest, reset: () => Config = new ModConfig(), save: () => Helper.WriteConfig(Config));
        gmcm.AddKeybindList(ModManifest, () => Config.ChooseKey, v => Config.ChooseKey = v,
            () => Helper.Translation.Get("config.choose-key.name"), () => Helper.Translation.Get("config.choose-key.tooltip"));
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (!e.NameWithoutLocale.StartsWith(AssetPrefix))
            return;

        string id = e.NameWithoutLocale.BaseName.Substring(AssetPrefix.Length);
        if (Ponies.Any(p => p.Id == id))
            e.LoadFromModFile<Microsoft.Xna.Framework.Graphics.Texture2D>($"assets/{id}.png", AssetLoadPriority.Exclusive);
    }

    /// <summary>
    /// Show each horse in the current location with its chosen pony texture. The texture is set locally only
    /// (<see cref="AnimatedSprite.overrideTextureName"/>), so players without the mod still see the normal horse.
    /// </summary>
    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.currentLocation is null)
            return;

        foreach (NPC npc in Game1.currentLocation.characters)
        {
            if (npc is Horse horse)
                ApplyTexture(horse);
        }
        if (Game1.player.mount is { } mount)
            ApplyTexture(mount);
    }

    private void ApplyTexture(Horse horse)
    {
        AnimatedSprite? sprite = horse.Sprite;
        if (sprite is null)
            return;

        string? pony = GetPony(horse);
        string? wanted = pony is null ? null : AssetPrefix + pony;
        if (sprite.overrideTextureName == wanted)
            return;

        if (wanted is null)
        {
            // back to the synced texture; it's reloaded the next time the texture is used
            sprite.overrideTextureName = null;
            _ = sprite.Texture;
        }
        else
            sprite.LoadTexture(wanted, syncTextureName: false);
    }

    /// <summary>The chosen pony ID of a horse, or null for a normal horse.</summary>
    private string? GetPony(Horse horse)
    {
        return horse.modData.TryGetValue(PonyKey, out string? id) && Ponies.Any(p => p.Id == id) ? id : null;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !Config.ChooseKey.JustPressed())
            return;

        Horse? horse = FindHorse();
        if (horse is null)
        {
            Hint.Show(Helper.Translation.Get("hint.no-horse"));
            return;
        }
        if (horse.ownerId.Value != 0 && horse.ownerId.Value != Game1.player.UniqueMultiplayerID)
        {
            Hint.Show(Helper.Translation.Get("hint.not-yours"));
            return;
        }

        Helper.Input.SuppressActiveKeybinds(Config.ChooseKey);
        ShowMenu(horse);
    }

    /// <summary>The horse the player rides, else the nearest horse within about two tiles.</summary>
    private static Horse? FindHorse()
    {
        if (Game1.player.mount is { } mount)
            return IsTractor(mount) ? null : mount;

        Vector2 me = Game1.player.getStandingPosition();
        return Game1.currentLocation.characters
            .OfType<Horse>()
            .Where(h => !IsTractor(h))
            .Select(h => (Horse: h, Distance: Vector2.Distance(me, h.getStandingPosition())))
            .Where(p => p.Distance <= Game1.tileSize * 2.5f)
            .OrderBy(p => p.Distance)
            .Select(p => p.Horse)
            .FirstOrDefault();
    }

    /// <summary>Tractor Mod's tractor is a horse internally; leave it alone.</summary>
    private static bool IsTractor(Horse horse)
    {
        return horse.modData.ContainsKey("Pathoschild.TractorMod");
    }

    private void ShowMenu(Horse horse)
    {
        string? current = GetPony(horse);
        string Mark(string key, string text) => key == (current ?? HorseChoice) ? $"> {text}" : text;

        var responses = Ponies
            .Select(p => new Response(p.Id, Mark(p.Id, p.Name)))
            .Append(new Response(HorseChoice, Mark(HorseChoice, Helper.Translation.Get("menu.horse"))))
            .Append(new Response(CancelChoice, Helper.Translation.Get("menu.cancel")))
            .ToArray();

        string name = string.IsNullOrWhiteSpace(horse.Name) ? Helper.Translation.Get("menu.your-horse") : horse.displayName;
        Game1.currentLocation.createQuestionDialogue(
            Helper.Translation.Get("menu.question", new { name }),
            responses,
            (who, answer) => OnChosen(horse, who, answer)
        );
    }

    private void OnChosen(Horse horse, Farmer who, string answer)
    {
        if (answer == CancelChoice)
            return;

        string? pony = answer == HorseChoice ? null : answer;
        if (pony == GetPony(horse))
            return;

        // change it under the horse's lock like the game does for hats, so two players can't change it at once
        horse.mutex.RequestLock(() =>
        {
            try
            {
                if (pony is null)
                    horse.modData.Remove(PonyKey);
                else
                {
                    horse.modData[PonyKey] = pony;
                    GiveBackHat(horse, who);
                }
            }
            finally
            {
                horse.mutex.ReleaseLock();
            }

            ApplyTexture(horse);
            Game1.playSound("dwop");
        });
    }

    /// <summary>Ponies don't wear hats: hand a worn hat back to the player.</summary>
    private static void GiveBackHat(Horse horse, Farmer who)
    {
        Hat? hat = horse.hat.Value;
        if (hat is null)
            return;

        horse.hat.Value = null;
        if (!who.addItemToInventoryBool(hat))
            Game1.createItemDebris(hat, horse.Position, horse.FacingDirection);
        Instance.Hint.Show(Instance.Helper.Translation.Get("hint.hat-returned"));
    }

    /// <summary>Rows of a front-view frame that make up the pony's head (ears to snout).</summary>
    private const int HeadRows = 21;

    /// <summary>
    /// Riding down, the rider sits behind the pony's big head. Draw the head of the current frame again over the
    /// rider, plus the middle of the chest, like the game does with the normal horse's neck. (The game's own piece for
    /// this is left empty in the pony sheets, since it moves with the rider instead of the pony.)
    /// </summary>
    private static void After_Horse_Draw(Horse __instance, SpriteBatch b)
    {
        try
        {
            if (__instance.rider is null || __instance.FacingDirection != Game1.down || Instance.GetPony(__instance) is null)
                return;

            AnimatedSprite sprite = __instance.Sprite;
            Rectangle frame = sprite.SourceRect;
            // a frame's top-left pixel is drawn at local position + (0, -80), see the game's carrot overlays
            Vector2 origin = __instance.getLocalPosition(Game1.viewport) + new Vector2(0f, -80f);
            float depth = (__instance.Position.Y + 64f) / 10000f + 2E-07f;

            var head = new Rectangle(frame.X, frame.Y, frame.Width, HeadRows);
            b.Draw(sprite.Texture, origin, head, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, depth);

            var chest = new Rectangle(frame.X + 12, frame.Y + HeadRows, 9, 29 - HeadRows);
            b.Draw(sprite.Texture, origin + new Vector2(12 * 4f, HeadRows * 4f), chest, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, depth);
        }
        catch (Exception ex)
        {
            Instance.Monitor.LogOnce($"Failed drawing a pony's head over its rider:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>How many pixels lower the rider sits on a pony than on a horse, by facing direction (up, right, down, left).</summary>
    /// <remarks>The pony's back is about 3-4 pixels lower than the horse's saddle. Facing down a bit less, so the pony's ears don't cover the rider's face.</remarks>
    private static readonly int[] SitLower = { 4, 4, 2, 4 };

    /// <summary>Farmers whose height was changed for riding a pony, so it can be reset after they get off.</summary>
    private static readonly ConditionalWeakTable<Farmer, object> LoweredRiders = new();

    /// <summary>How many pixels further back the rider sits on a pony when riding sideways.</summary>
    private const int SitBack = 3;

    /// <summary>Move the rider back onto the pony's back when riding sideways (the game sets the offset fresh each call).</summary>
    private static void After_Farmer_ShowRiding(Farmer __instance)
    {
        try
        {
            if (__instance.mount is not { } horse || Instance.GetPony(horse) is null)
                return;

            // xOffset is the draw origin in sprite pixels: a positive value draws the rider further LEFT
            if (__instance.FacingDirection == Game1.right)
                __instance.xOffset += SitBack;
            else if (__instance.FacingDirection == Game1.left)
                __instance.xOffset -= SitBack;
        }
        catch (Exception ex)
        {
            Instance.Monitor.LogOnce($"Failed placing the rider on a pony:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>
    /// Set the rider's height right before drawing (the game draws the mount from here too, so both use the same frame).
    /// The game bobs the rider to the normal horse's walk; the ponies instead lower their body by one pixel in walk
    /// frames 1 and 4. A negative offset draws the rider lower.
    /// </summary>
    private static void Before_Farmer_Draw(Farmer __instance)
    {
        try
        {
            if (__instance.mount is not { } horse || __instance.isAnimatingMount || horse.rider != __instance || Instance.GetPony(horse) is null)
            {
                // the game doesn't reset the height on dismounting
                if (LoweredRiders.Remove(__instance))
                    __instance.yOffset = 0f;
                return;
            }

            LoweredRiders.AddOrUpdate(__instance, new object());
            int walkFrame = horse.Sprite.currentFrame % 7;
            bool lowered = __instance.isMoving() && walkFrame is 1 or 4;
            int direction = Math.Clamp(__instance.FacingDirection, 0, 3);
            __instance.yOffset = -4f * (SitLower[direction] + (lowered ? 1 : 0));
        }
        catch (Exception ex)
        {
            Instance.Monitor.LogOnce($"Failed syncing the rider with a pony:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Don't let the player put a hat on a pony.</summary>
    private static bool Before_Horse_CheckAction(Horse __instance, Farmer who, ref bool __result)
    {
        try
        {
            if (__instance.rider is null && who.mount is null && who.CurrentItem is Hat && Instance.GetPony(__instance) is not null)
            {
                Instance.Hint.Show(Instance.Helper.Translation.Get("hint.no-hats"));
                __result = true;
                return false;
            }
        }
        catch (Exception ex)
        {
            Instance.Monitor.Log($"Failed checking a hat on a pony:\n{ex}", LogLevel.Error);
        }
        return true;
    }
}
