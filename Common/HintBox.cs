using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewMods.Common;

/// <summary>A short message box drawn bottom-left, a bit above where vanilla HUD messages sit so it clears the toolbar.</summary>
internal sealed class HintBox
{
    private const int Duration = 120;
    private const int Fade = 30;
    private const int Pad = 20;

    private string? Text;
    private int EndTick;

    public HintBox(IModEvents events)
    {
        events.Display.RenderedHud += OnRenderedHud;
    }

    public void Show(string text)
    {
        Text = text;
        EndTick = Game1.ticks + Duration;
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        int left = EndTick - Game1.ticks;
        if (Text is null || left <= 0)
            return;

        float alpha = Math.Min(1f, left / (float)Fade);
        Vector2 size = Game1.smallFont.MeasureString(Text);
        int width = (int)size.X + Pad * 2;
        int height = (int)size.Y + Pad * 2;
        int x = 16;
        int y = Game1.uiViewport.Height - 136 - height;

        IClickableMenu.drawTextureBox(e.SpriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White * alpha, drawShadow: false);
        Utility.drawTextWithShadow(e.SpriteBatch, Text, Game1.smallFont, new Vector2(x + Pad, y + Pad), Game1.textColor * alpha);
    }
}
