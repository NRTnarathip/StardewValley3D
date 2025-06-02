using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewValley3D;

[HarmonyPatch]
internal static class FurnitureRenderer
{
    public static Furniture? lastFurnitureDraw;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Furniture), "draw")]
    static void Prefix_Furniture_draw(
           Furniture __instance,
           SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
    {
        // cache
        lastFurnitureDraw = __instance;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Furniture), "draw")]
    static void Postfix_Furniture_draw(
      Furniture __instance,
      SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
    {
        lastFurnitureDraw = null;
    }


    internal static void OnSpriteBatchDraw(SpriteBatch instance, Texture2D texture, Vector2 position,
        Rectangle srcRect, Color color, float rotation, Vector2 origin,
        Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        //setup
        var server = ModEntry.server;
        var furniture = lastFurnitureDraw;

        // ready
        Color[] pixels = new Color[srcRect.Width * srcRect.Height];
        TextureUtils.GetPixels(texture, srcRect, pixels);
        byte[] pixelBytes = new byte[pixels.Length * 4];
        TextureUtils.CopyColorsToBytes(pixels, ref pixelBytes);

        int entityID = lastFurnitureDraw.GetHashCode();
        var tilePos = lastFurnitureDraw.tileLocation.Value;

        return;

        server.SendEvent("Furniture:SpriteBatch:Draw()", [
            furniture.name,
            pixelBytes,
            srcRect.ToRect(),
            position.ToVec2(),
            scale.ToVec2(),
            origin.ToVec2(),
            entityID,
            tilePos.ToVec2(),
        ]);

    }
}
