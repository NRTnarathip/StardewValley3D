using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StardewValley3D;

[HarmonyPatch]
static class GameHookRenderer
{
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
        if (FurnitureRenderer.lastFurnitureDraw is not null)
        {
            FurnitureRenderer.OnSpriteBatchDraw(__instance, texture,
                position, sourceRectangle.Value,
                color, rotation,
                origin, scale,
                effects, layerDepth
                );
        }

        else if (FarmerRenderer.lastFarmerDraw is not null)
        {
            FarmerRenderer.OnSpriteBatchDraw(__instance, texture,
                position, sourceRectangle.Value,
                color, rotation,
                origin, scale,
                effects, layerDepth
                );
        }
    }
}
