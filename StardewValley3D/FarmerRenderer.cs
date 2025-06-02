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

    public static bool DisableDrawToGameScreen = false;

    static RenderTarget2D myRenderTarget;
    static RenderTarget2D gameRenderTarget;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Farmer), "draw", [typeof(SpriteBatch)])]
    static void Prefix_draw(
        Farmer __instance,
        SpriteBatch b)
    {
        lastFarmerDraw = __instance;
        OnDrawCounter = 0;

        var device = Game1.graphics.GraphicsDevice;

        gameRenderTarget = device.GetRenderTargets().First().RenderTarget as RenderTarget2D;


        if (myRenderTarget is null
            || myRenderTarget.width != gameRenderTarget.width
            || myRenderTarget.height != gameRenderTarget.height)
        {
            myRenderTarget = new RenderTarget2D(
                device,
                gameRenderTarget.width,
                gameRenderTarget.height
            );
        }

        b.End();
        Game1.SetRenderTarget(myRenderTarget);
        b.Begin(SpriteSortMode.Deferred, 
            BlendState.AlphaBlend, 
            SamplerState.PointClamp, 
            DepthStencilState.Default, 
            RasterizerState.CullNone);
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(Farmer), "draw", [typeof(SpriteBatch)])]
    static void Postfix_draw(
        Farmer __instance,
        SpriteBatch b)
    {
        lastFarmerDraw = null;

        if (DisableDrawToGameScreen)
        {
            b.End();

            Game1.SetRenderTarget(gameRenderTarget);

            b.Begin(SpriteSortMode.Deferred,
              BlendState.AlphaBlend,
              SamplerState.PointClamp,
              DepthStencilState.Default,
              RasterizerState.CullNone);
        }
    }

    static int OnDrawCounter = 0;
    internal static void OnSpriteBatchDraw(SpriteBatch b, Texture2D texture,
        Vector2 drawPos, Rectangle srcRect, Color color,
        float rotation, Vector2 origin, Vector2 scale,
        SpriteEffects effects, float layerDepth)
    {
        OnDrawCounter++;

        var server = ModEntry.server;

        Color[] pixels = new Color[srcRect.Width * srcRect.Height];
        TextureUtils.GetPixels(texture, srcRect, pixels);
        byte[] pixelBytes = new byte[pixels.Length * 4];
        TextureUtils.CopyColorsToBytes(pixels, ref pixelBytes);

        var colorDotnet = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        var srcRectSize = srcRect.Size;
        var originWithinDrawSrc = new System.Numerics.Vector2(
            (origin.X - srcRect.X) / (float)srcRectSize.X,
           (origin.Y - srcRect.Y) / (float)srcRectSize.Y);

        server.SendEvent("Farmer:Draw()", [
            lastFarmerDraw.uniqueMultiplayerID.Value,
            pixelBytes,
            new System.Numerics.Vector2(srcRect.Width, srcRect.Height),
            OnDrawCounter,
            drawPos.ToVec2(),
            origin.ToVec2(),
            colorDotnet,
            (int)effects,
            layerDepth,
        ]);

    }
}
