using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace StardewValley3D
{
    [HarmonyPatch]
    internal static class ObjectRenderer
    {
        static Harmony? harmony;
        public static void Init(Harmony h)
        {
            harmony = h;

            var baseType = typeof(SObject);
            var gameAssembly = baseType.Assembly;
            var derivedTypes = gameAssembly
                .GetTypes()
                .Where(t => t != null && t.IsClass && !t.IsAbstract
                    && baseType.IsAssignableFrom(t) && t != baseType)
                .ToList();

            // hook draw methods
            harmony.Patch(
                 AccessTools.Method(typeof(SObject), nameof(SObject.draw), [
                     typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)
                 ]),
                 prefix: new(AccessTools.Method(typeof(ObjectRenderer), nameof(Prefix_draw_spriteBatch_x_y_alpha))),
                 postfix: new(AccessTools.Method(typeof(ObjectRenderer), nameof(Postfix_draw_spriteBatch_x_y_alpha)))
            );
            foreach (var type in derivedTypes)
            {
                if (type.FullName.StartsWith("StardewValley.") is false)
                    continue;

                var drawMethod = type.GetMethod(nameof(SObject.draw),
                    BindingFlags.Public | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly,
                    [
                        typeof(SpriteBatch), typeof(int),
                        typeof(int), typeof(float)
                    ]);
                if (drawMethod is null)
                {
                    continue;
                }

                var drawMethodClassType = drawMethod.DeclaringType;
                var firstParamInfo = drawMethod.GetParameters().First();
                string? prefixDrawMethodName = null;
                string? postfixDrawMethodName = null;
                switch (firstParamInfo.Name)
                {
                    case "b":
                        prefixDrawMethodName = nameof(Prefix_draw_b_x_y_alpha);
                        postfixDrawMethodName = nameof(Postfix_draw_b_x_y_alpha);
                        break;

                    case "spriteBatch":
                        prefixDrawMethodName = nameof(Prefix_draw_spriteBatch_x_y_alpha);
                        postfixDrawMethodName = nameof(Postfix_draw_spriteBatch_x_y_alpha);
                        break;

                    default:
                        Console.WriteLine("not found match draw method on class: " + drawMethodClassType);
                        continue;
                }

                harmony.Patch(
                    drawMethod,
                    prefix: new(AccessTools.Method(typeof(ObjectRenderer), prefixDrawMethodName)),
                    postfix: new(AccessTools.Method(typeof(ObjectRenderer), postfixDrawMethodName))
                );
            }

            // setup events
            GameHookRenderer.eventOnSpriteBatchDraw1 += GameHookRenderer_eventOnSpriteBatchDraw;
        }

        static int spriteBatchDrawCallCounter = 0;

        private static void GameHookRenderer_eventOnSpriteBatchDraw(SpriteBatch spriteBatch, Texture2D texture, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Rectangle? sourceRectangle, Microsoft.Xna.Framework.Color color, float rotation, Microsoft.Xna.Framework.Vector2 origin, Microsoft.Xna.Framework.Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            if (currentDrawObject is null)
                return;

            spriteBatchDrawCallCounter++;
            System.Numerics.Vector2 tilePos = currentDrawObject.TileLocation.ToVec2System();
            string name = currentDrawObject.name;
            var server = ModEntry.server;
            Type objectClassType = currentDrawObject.GetType();
            string guid = $"{objectClassType.FullName}:{name}:{tilePos}";

            if (sourceRectangle.HasValue is false)
            {
                //Console.WriteLine("none src rect");
                //Console.WriteLine(" obj: " + name);
                return;
            }

            var srcRect = sourceRectangle.Value;

            Vector2 viewportTilePos = Game1.viewport.Location.ToVec2() / 64f;
            var drawTilePos = viewportTilePos + (position / 64f);

            var textureFileName = texture.GetUniquePath();
            server.SendEvent("Object:Render", [
                spriteBatchDrawCallCounter,
                guid,
                textureFileName,
                srcRect.ToRectSystem(),
                position.ToVec2System(),
                drawTilePos.ToVec2System(),
                scale.ToVec2System(),
                origin.ToVec2System(),
                color.ToColorSystem(),
                (int)effects,
                layerDepth,
            ], LiteNetLib.DeliveryMethod.Unreliable);

            // debug only
            if (name.Contains("Furnace"))
            {
            }
        }

        public static SObject? currentDrawObject;
        public static bool IsDisableDrawToGame;
        static int drawDepthStack = 0;
        static SpriteBatchBeginState backupDrawState = new();

        static bool Prefix_draw_b_x_y_alpha(
            SObject __instance, SpriteBatch b, int x, int y, float alpha = 1f)
            => Prefix_draw_spriteBatch_x_y_alpha(__instance, b, x, y, alpha);

        static bool Prefix_draw_spriteBatch_x_y_alpha(
            SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            var o = __instance;
            var name = o.name;

            // validate
            drawDepthStack++;

            if (drawDepthStack <= 0)
            {
                Console.WriteLine("incorrect draw stack: " + drawDepthStack);
                Console.WriteLine(" o: " + __instance + ", name: " + name);
                Console.WriteLine(" try restore!!");
                drawDepthStack = 0;
                return false;
            }

            //reset every time when begin new Object.draw();
            spriteBatchDrawCallCounter = 0;

            System.Numerics.Vector2 tilePos = o.TileLocation.ToVec2System();
            currentDrawObject = o;

            if (drawDepthStack == 1)
            {
            }

            if (IsDisableDrawToGame && drawDepthStack == 1)
            {
                backupDrawState.BackupAndEnd(spriteBatch);
                RenderTargetUtils.BeginUseForTemp();
                backupDrawState.Begin(spriteBatch);
            }

            return true;
        }


        static void Postfix_draw_b_x_y_alpha(
            SObject __instance, SpriteBatch b, int x, int y, float alpha = 1f)
            => Postfix_draw_spriteBatch_x_y_alpha(__instance, b, x, y, alpha);

        static void Postfix_draw_spriteBatch_x_y_alpha(
            SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            var tempObject = currentDrawObject;

            int prevDepth = drawDepthStack;
            drawDepthStack--;

            // end call stacks
            if (drawDepthStack == 0)
            {
                currentDrawObject = null;
                if (IsDisableDrawToGame)
                {
                    spriteBatch.End();
                    RenderTargetUtils.RestoreBack();
                    backupDrawState.Begin(spriteBatch);
                }
            }
            // error check
            else if (drawDepthStack < 0)
            {
                Console.WriteLine("incorrect postfix draw stack: " + drawDepthStack);
                Console.WriteLine("obj: " + tempObject);
                Console.WriteLine("try restore!");
                drawDepthStack = 0;
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(SObject), nameof(SObject.draw), [
            typeof(SpriteBatch),
            typeof(int),
            typeof(int),
            typeof(float),
            typeof(float),
        ])]
        static void Prefix_draw2(
            SObject __instance,
            SpriteBatch spriteBatch, int xNonTile, int yNonTile,
            float layerDepth, float alpha = 1f)
        {
            Console.WriteLine("prefix draw2: " + __instance);
        }
    }
}
