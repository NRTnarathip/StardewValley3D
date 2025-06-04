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

//[HarmonyPatch]
//internal static class Remove_FurnitureRenderer
//{
//    public static Furniture? lastFurnitureDraw;

//    static SpriteBatchBeginState drawBeginState = new();
//    public static bool IsDisableDrawOnGame = false;

//    [HarmonyPrefix]
//    [HarmonyPatch(typeof(Furniture), "draw")]
//    static void Prefix_Furniture_draw(
//           Furniture __instance,
//           SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
//    {
//        // cache
//        lastFurnitureDraw = __instance;
//        if (IsDisableDrawOnGame)
//        {
//            drawBeginState.BackupAndEnd(spriteBatch);
//            RenderTargetUtils.BeginUseForTemp();
//            drawBeginState.Begin(spriteBatch);
//        }

//        // init
//        drawCounter = 0;
//    }

//    [HarmonyPostfix]
//    [HarmonyPatch(typeof(Furniture), "draw")]
//    static void Postfix_Furniture_draw(
//      Furniture __instance,
//      SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
//    {
//        lastFurnitureDraw = null;
//        if (IsDisableDrawOnGame)
//        {
//            spriteBatch.End();
//            RenderTargetUtils.RestoreBack();
//            drawBeginState.Begin(spriteBatch);
//        }
//    }

//    static int drawCounter = 0;
//    internal static void OnSpriteBatchDraw(SpriteBatch instance, Texture2D texture, Vector2 drawPos,
//        Rectangle srcRect, Color drawColor, float rotation, Vector2 origin,
//        Vector2 scale, SpriteEffects effects, float layerDepth)
//    {
//        //setup
//        var server = ModEntry.server;
//        var furniture = lastFurnitureDraw;

//        // ready
//        Color[] pixels = new Color[srcRect.Width * srcRect.Height];
//        TextureUtils.GetPixels(texture, srcRect, pixels);
//        byte[] pixelBytes = new byte[pixels.Length * 4];
//        TextureUtils.CopyColorsToBytes(pixels, ref pixelBytes);

//        var currentLocation = Game1.currentLocation;
//        string guide = currentLocation.furniture.GuidOf(furniture).ToString();

//        drawCounter++;
//        server.SendEvent("Furniture:Render", [
//            drawCounter,
//            guide,
//            pixelBytes,
//            srcRect.Size.ToVector2().ToVec2(),
//            drawPos.ToVec2(),
//            scale.ToVec2(),
//            origin.ToVec2(),
//            drawColor.ToColor(),
//            (int)effects,
//            layerDepth,
//        ], LiteNetLib.DeliveryMethod.ReliableUnordered);
//    }
//}
