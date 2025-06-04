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
    public static void Init()
    {
        GameHookRenderer.eventOnSpriteBatchDraw1 += GameHookRenderer_eventOnSpriteBatchDraw;
    }

    private static void GameHookRenderer_eventOnSpriteBatchDraw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        if (lastFarmerDraw is null)
            return;

        OnSpriteBatchDraw(spriteBatch, texture, position,
            sourceRectangle, color, rotation, origin,
            scale, effects, layerDepth);
    }

    public static Farmer? lastFarmerDraw;

    public static bool IsDisableDrawToGame = false;

    static RenderTarget2D myRenderTarget;
    static RenderTarget2D gameRenderTarget;

    static SpriteBatchBeginState backupBeginState = new();

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


        if (IsDisableDrawToGame)
        {
            backupBeginState.BackupAndEnd(b);

            Game1.SetRenderTarget(myRenderTarget);
            backupBeginState.Begin(b);
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(Farmer), "draw", [typeof(SpriteBatch)])]
    static void Postfix_draw(
        Farmer __instance,
        SpriteBatch b)
    {
        lastFarmerDraw = null;

        if (IsDisableDrawToGame)
        {
            b.End();
            Game1.SetRenderTarget(gameRenderTarget);
            backupBeginState.Begin(b);
        }
    }

    static int OnDrawCounter = 0;
    internal static void OnSpriteBatchDraw(SpriteBatch b, Texture2D texture,
        Vector2 drawPos, Rectangle? srcRectOrNull, Color color,
        float rotation, Vector2 origin, Vector2 scale,
        SpriteEffects effects, float layerDepth)
    {
        if (srcRectOrNull.HasValue is false)
        {
            return;
        }

        OnDrawCounter++;
        var server = ModEntry.server;
        var srcRect = srcRectOrNull.Value;

        var drawTilePos = Utils.ConvertDrawScreenPosToTilePos(drawPos);
        var textureUniqPath = texture.GetUniquePath();

        server.SendEvent("Farmer:Render", [
            Game1.ticks,
            lastFarmerDraw.uniqueMultiplayerID.Value,
            textureUniqPath,
            srcRect.ToRectSystem(),
            OnDrawCounter,
            drawTilePos.ToVec2System(),
            origin.ToVec2System(),
            color.ToColorSystem(),
            (int)effects,
            layerDepth,
        ], LiteNetLib.DeliveryMethod.Unreliable);

    }
}
