using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Objects;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StardewValley3D;

[HarmonyPatch]
static class GameHookRenderer
{
    public delegate void SpriteBatchDrawDelegate(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth);

    public static event SpriteBatchDrawDelegate? eventOnSpriteBatchDraw1;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpriteBatch), "Draw", [
        typeof(Texture2D),
        typeof(Vector2),
        typeof(Rectangle?),
        typeof(Color),
        typeof(float),
        typeof(Vector2),
        typeof(Vector2),
        typeof(SpriteEffects),
        typeof(float)
    ])]
    static void Postfix_SpriteBatch_draw(
        SpriteBatch __instance,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth)
    {
        eventOnSpriteBatchDraw1?.Invoke(
            __instance,
            texture, position, sourceRectangle, color, rotation, origin,
            scale, effects, layerDepth);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpriteBatch), "Draw", [
        typeof(Texture2D),
        typeof(Rectangle),
        typeof(Rectangle?),
        typeof(Color),
        typeof(float),
        typeof(Vector2),
        typeof(SpriteEffects),
        typeof(float)
    ])]
    static void Draw2(
        SpriteBatch __instance,
        Texture2D texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        SpriteEffects effects,
        float layerDepth)
    {

#if true
        // debug
        var o = ObjectRenderer.currentDrawObject;
        if (o is not null)
        {
            //Console.WriteLine("on draw 2: " + o.name);
        }
#endif

        if (sourceRectangle.HasValue is false)
        {
            Console.WriteLine("none src rect");
            //return;
        }

        var drawScreenPos = new Vector2(destinationRectangle.X, destinationRectangle.Y);
        eventOnSpriteBatchDraw1?.Invoke(__instance,
            texture,
            drawScreenPos,
            sourceRectangle,
            color,
            rotation,
            origin,
            new Vector2(1, 1),
            effects,
            layerDepth
        );
    }

}
