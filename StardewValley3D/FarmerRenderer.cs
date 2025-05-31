using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewValley3D;

[HarmonyPatch]
internal static class FarmerRenderer
{
    public static Farmer? lastFarmerDraw;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Farmer), "draw", [typeof(SpriteBatch)])]
    static void Prefix_draw(
        Farmer __instance,
        SpriteBatch b)
    {
        lastFarmerDraw = __instance;
        OnDrawCounter = 0;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(Farmer), "draw", [typeof(SpriteBatch)])]
    static void Postfix_draw(
        Farmer __instance,
        SpriteBatch b)
    {
        lastFarmerDraw = null;
    }

    static Type FarmerType = typeof(Farmer);

    static int OnDrawCounter = 0;
    internal static void OnDraw(SpriteBatch b, Texture2D texture,
        Vector2 drawPos, Rectangle srcRect, Color color,
        float rotation, Vector2 origin, Vector2 scale,
        SpriteEffects effects, float layerDepth)
    {
        OnDrawCounter++;

        var server = ModEntry.server;
        string entityIdentifier = $"Farmer:{lastFarmerDraw.GetHashCode()}:{OnDrawCounter}";

        //Console.WriteLine($" pos: {drawPos}");
        //Console.WriteLine($" origin: {origin}");
        //Console.WriteLine($" rotation: {rotation}");
        //Console.WriteLine($" scale: {scale}");
        //Console.WriteLine(" effects: " + effects);

        Color[] pixels = new Color[srcRect.Width * srcRect.Height];
        TextureUtils.GetPixels(texture, srcRect, pixels);
        byte[] pixelBytes = new byte[pixels.Length * 4];
        TextureUtils.CopyColorsToBytes(pixels, ref pixelBytes);

        var colorDotnet =  System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        server.SendEvent("Farmer:Draw()", [
            entityIdentifier,
            pixelBytes,
            new System.Numerics.Vector2(srcRect.Width, srcRect.Height),
            drawPos.ToVec2(),
            origin.ToVec2(),
            colorDotnet,
            (int)effects,
            layerDepth,
        ]);
    }
}
